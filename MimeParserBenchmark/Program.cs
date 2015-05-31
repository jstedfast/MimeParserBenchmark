//
// Program.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc. (www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.IO;
using System.Diagnostics;

namespace MimeParserBenchmark {
	class Program
	{
		public static void Main (string[] args)
		{
			var messages = new [] { "startrek.msg", "xamarin3.msg" };

			foreach (var message in messages) {
				using (var stream = new MemoryStream ()) {
					// Note: we copy into a memory stream to try and make things fair for
					// MailSystem.NET which can only parse MemoryStreams or byte[] data.
					using (var file = File.OpenRead (message)) {
						// Note: convert to DOS line endings in case the benchmark
						// is being run on Mac or Linux
						using (var filtered = new MimeKit.IO.FilteredStream (stream)) {
							filtered.Add (new MimeKit.IO.Filters.Unix2DosFilter ());
							file.CopyTo (filtered);
							filtered.Flush ();
						}
					}
					stream.Position = 0;

					int count = (1024 * 1024 * 1024) / (int) stream.Length;

					Console.WriteLine ("Parsing {0} ({1} iterations):", message, count);
					
					Console.WriteLine ("MimeKit:        {0}", MeasureMimeKit (stream, count).TotalSeconds);

					// Note: OpenPop fails trying to base64 decode the attachments so I had to
					// patch it.
					try {
						Console.WriteLine ("OpenPop:        {0}", MeasureOpenPOP (stream, count).TotalSeconds);
					} catch (Exception ex) {
						Console.WriteLine ("OpenPop:     Failed.    {0}", ex.Message);
					}

					Console.WriteLine ("AE.Net.Mail:    {0}", MeasureAENetMail (stream, count).TotalSeconds);

					Console.WriteLine ("MailSystem.NET: {0}", MeasureMailSystemNET (stream, count).TotalSeconds);

					Console.WriteLine ("MIMER:          {0}", MeasureMIMER (stream, count).TotalSeconds);
				}

				Console.WriteLine ();
			}
		}

		static TimeSpan MeasureMimeKit (Stream stream, int count)
		{
			var stopwatch = new Stopwatch ();

			stopwatch.Start ();
			for (int i = 0; i < count; i++) {
				var parser = new MimeKit.MimeParser (stream, MimeKit.MimeFormat.Default, true);
				parser.ParseMessage ();
				stream.Position = 0;
			}
			stopwatch.Stop ();

			return stopwatch.Elapsed;
		}

		static TimeSpan MeasureOpenPOP (Stream stream, int count)
		{
			var stopwatch = new Stopwatch ();

			stopwatch.Start ();
			for (int i = 0; i < count; i++) {
				OpenPop.Mime.Message.Load (stream);
				stream.Position = 0;
			}
			stopwatch.Stop ();

			return stopwatch.Elapsed;
		}

		class EndOfLineCriteriaStrategy : MIMER.IEndCriteriaStrategy
		{
			public bool IsEndReached (char[] data, int size)
			{
				return data[0] == '\n';
			}
		}

		static TimeSpan MeasureMIMER (Stream stream, int count)
		{
			var eoln = new EndOfLineCriteriaStrategy ();
			var stopwatch = new Stopwatch ();

			stopwatch.Start ();
			for (int i = 0; i < count; i++) {
				var reader = new MIMER.RFC822.MailReader ();
				reader.Read (ref stream, eoln);
				stream.Position = 0;
			}
			stopwatch.Stop ();

			return stopwatch.Elapsed;
		}

		static TimeSpan MeasureAENetMail (Stream stream, int count)
		{
			var stopwatch = new Stopwatch ();

			stopwatch.Start ();
			for (int i = 0; i < count; i++) {
				var message = new AE.Net.Mail.MailMessage ();
				message.Load (stream, false, (int) stream.Length);
				stream.Position = 0;
			}
			stopwatch.Stop ();

			return stopwatch.Elapsed;
		}

		static TimeSpan MeasureMailSystemNET (MemoryStream stream, int count)
		{
			var stopwatch = new Stopwatch ();

			stopwatch.Start ();
			for (int i = 0; i < count; i++) {
				ActiveUp.Net.Mail.Parser.ParseMessage (stream);
				stream.Position = 0;
			}
			stopwatch.Stop ();

			return stopwatch.Elapsed;
		}
	}
}
