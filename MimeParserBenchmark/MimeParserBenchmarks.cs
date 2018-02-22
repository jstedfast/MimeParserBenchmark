using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;

namespace MimeParserBenchmark
{
    [MemoryDiagnoser] /* this enables memeory diagnostics */
    public class MimeParserBenchmarks
    {
        private readonly string[] _messages = { "startrek.msg", "xamarin3.msg" };
        private readonly List<MemoryStream> _streams = new List<MemoryStream>();

        [GlobalSetup]
        public void GlobalSetup()
        {
            foreach (var message in _messages)
            {
                var stream = new MemoryStream();
                // Note: we copy into a memory stream to try and make things fair for
                // MailSystem.NET which can only parse MemoryStreams or byte[] data.
                using (var file = File.OpenRead(message))
                {
                    // Note: convert to DOS line endings in case the benchmark
                    // is being run on Mac or Linux
                    using (var filtered = new MimeKit.IO.FilteredStream(stream))
                    {
                        filtered.Add(new MimeKit.IO.Filters.Unix2DosFilter());
                        file.CopyTo(filtered);
                        filtered.Flush();
                    }
                }
                stream.Position = 0;
                _streams.Add(stream);
            }
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            foreach (var stream in _streams)
            {
                stream.Dispose();
            }
        }

        [Benchmark]
        public bool MimeKitParser()
        {
            var parser = new MimeKit.MimeParser(Stream.Null);
            foreach (var stream in _streams)
            {
                parser.SetStream(stream, true);
                parser.ParseMessage();
                stream.Position = 0;
            }
            return true;
        }

        [Benchmark]
        public bool Mime4NeParsert()
        {
            var parser = new MimeKit.MimeParser(Stream.Null);
            foreach (var stream in _streams)
            {
                var message = new NI.Email.Mime.Message.MimeMessage(stream, false);
                stream.Position = 0;
            }
            return true;
        }

        [Benchmark]
        public bool OpenPopParser()
        {
            foreach (var stream in _streams)
            {
                OpenPop.Mime.Message.Load(stream);
                stream.Position = 0;
            }
            return true;
        }
        [Benchmark]
        public bool AENetMailParser()
        {
            foreach (var stream in _streams)
            {
                var message = new AE.Net.Mail.MailMessage();
                message.Load(stream, false, (int)stream.Length);
                stream.Position = 0;
            }
            return true;
        }
        [Benchmark]
        public bool MailSystemNETParser()
        {
            foreach (var stream in _streams)
            {
                ActiveUp.Net.Mail.Parser.ParseMessage(stream);
                stream.Position = 0;
            }
            return true;
        }
        [Benchmark]
        public bool MIMERParser()
        {
            var eoln = new EndOfLineCriteriaStrategy();
            foreach (var stream in _streams)
            {
                Stream localStream = stream;
                var reader = new MIMER.RFC822.MailReader();
                reader.Read(ref localStream, eoln);
                localStream.Position = 0;
            }

            return true;
        }
    }
}