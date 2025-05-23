﻿//
// JwzMboxBenchmarks.cs
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
	public class JwzMboxBenchmarks
	{
		static readonly string BenchmarkData = Path.Combine (BenchmarkHelper.ProjectDir, "BenchmarkData", "jwz.mbox");
		const int iterations = 10;

		[Benchmark]
		public void MimeKit ()
		{
			using (var stream = File.OpenRead (BenchmarkData)) {
				using (var looped = new LoopedInputStream (stream, iterations)) {
					var parser = new MimeParser (looped, MimeFormat.Mbox);

					while (!parser.IsEndOfStream) {
						var message = parser.ParseMessage ();
						message.Dispose ();
					}
				}
			}
		}

		[Benchmark]
		public void MimeKitPersistent ()
		{
			using (var stream = File.OpenRead (BenchmarkData)) {
				using (var looped = new LoopedInputStream (stream, iterations)) {
					var parser = new MimeParser (looped, MimeFormat.Mbox, true);

					while (!parser.IsEndOfStream) {
						var message = parser.ParseMessage ();
						message.Dispose ();
					}
				}
			}
		}

		[Benchmark]
		public void LimiLabs ()
		{
			using (var stream = File.OpenRead (BenchmarkData)) {
				using (var looped = new LoopedInputStream (stream, iterations)) {
					var reader = new Limilabs.Mail.Tools.MBox.MBoxReader (looped);

					while (reader.ReadNext () != null)
						;
				}
			}
		}
	}
}
