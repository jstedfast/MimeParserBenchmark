//
// LoopedInputStream.cs
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

using System;
using System.IO;

namespace Benchmarks
{
	class LoopedInputStream : Stream
	{
		readonly int iterationCount;
		Stream innerStream;
		int iteration;
		long position;
		long length;

		public LoopedInputStream (Stream innerStream, int iterationCount)
		{
			this.iterationCount = iterationCount;
			this.innerStream = innerStream;

			length = innerStream.Length;
		}

		public override bool CanRead => true;

		public override bool CanSeek => true;

		public override bool CanWrite => false;

		public override long Length {
			get {
				return iterationCount * length;
			}
		}

		public override long Position {
			get {
				return (iteration * length) + position;
			}
			set {
				Seek (value, SeekOrigin.Begin);
			}
		}

		public override void Flush ()
		{
			throw new NotImplementedException ();
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			int nread = 0;

			do {
				int n;

				if ((n = innerStream.Read (buffer, offset + nread, count - nread)) > 0) {
					position += n;
					nread += n;
				}

				if (position == length) {
					if (iteration < iterationCount) {
						innerStream.Position = position = 0;
						iteration++;
					} else {
						break;
					}
				}
			} while (nread < count);

			return nread;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			switch (origin) {
			case SeekOrigin.Begin:
				position = offset % length;
				iteration = (int) (offset / length);
				innerStream.Seek (position, SeekOrigin.Begin);
				break;
			case SeekOrigin.Current:
				offset = Position + offset;
				if (offset < 0 || offset > Length)
					throw new IOException ();

				goto case SeekOrigin.Begin;
			case SeekOrigin.End:
				offset = Length + offset;
				if (offset < 0 || offset > Length)
					throw new IOException ();

				goto case SeekOrigin.Begin;
			}

			return Position;
		}

		public override void SetLength (long value)
		{
			throw new NotImplementedException ();
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException ();
		}
	}
}
