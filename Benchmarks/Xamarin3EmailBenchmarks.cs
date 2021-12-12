//
// Xamarin3EmailBenchmarks.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2021 .NET Foundation and Contributors
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

using System.IO;

using BenchmarkDotNet.Attributes;

using MimeKit;

namespace Benchmarks {
	public class Xamarin3EmailBenchmarks
	{
		static readonly string BenchmarkData = Path.Combine (BenchmarkHelper.ProjectDir, "BenchmarkData", "xamarin3.eml");
		const int iterations = 1000;

		[Benchmark]
		public void MimeKit ()
		{
			using (var stream = File.OpenRead (BenchmarkData)) {
				var parser = new MimeParser (stream, MimeFormat.Entity);

				for (int i = 0; i < iterations; i++) {
					var message = parser.ParseMessage ();
					message.Dispose ();

					stream.Position = 0;
					parser.SetStream (stream, MimeFormat.Entity);
				}
			}
		}

		[Benchmark]
		public void MimeKitPersistent ()
		{
			using (var stream = File.OpenRead (BenchmarkData)) {
				var parser = new MimeParser (stream, MimeFormat.Entity, true);

				for (int i = 0; i < iterations; i++) {
					var message = parser.ParseMessage ();
					message.Dispose ();

					stream.Position = 0;
					parser.SetStream (stream, MimeFormat.Entity, true);
				}
			}
		}

		[Benchmark]
		public void LimiLabs ()
		{
			var builder = new Limilabs.Mail.MailBuilder ();

			for (int i = 0; i < iterations; i++) {
				builder.CreateFromEmlFile (BenchmarkData);
			}
		}

		[Benchmark]
		public void Mime4Net ()
		{
			using (var stream = File.OpenRead (BenchmarkData)) {
				for (int i = 0; i < iterations; i++) {
					new NI.Email.Mime.Message.MimeMessage (stream, false);
					stream.Position = 0;
				}
			}
		}

		[Benchmark]
		public void OpenPOP ()
		{
			using (var stream = File.OpenRead (BenchmarkData)) {
				for (int i = 0; i < iterations; i++) {
					OpenPop.Mime.Message.Load (stream);
					stream.Position = 0;
				}
			}
		}

		[Benchmark]
		public void AENetMail ()
		{
			using (var stream = File.OpenRead (BenchmarkData)) {
				for (int i = 0; i < iterations; i++) {
					var message = new AE.Net.Mail.MailMessage ();
					message.Load (stream, false, (int) stream.Length);
					stream.Position = 0;
				}
			}
		}

		[Benchmark]
		public void MailSystemNET ()
		{
			// Note: We let MailSystem.NET cheat here because it doesn't support parsing Stream, it can only take MemoryStream, byte[], and string inputs.
			var rawData = File.ReadAllBytes (BenchmarkData);

			for (int i = 0; i < iterations; i++) {
				ActiveUp.Net.Mail.Parser.ParseMessage (rawData);
			}
		}
	}
}
