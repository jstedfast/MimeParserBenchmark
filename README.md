# MimeParserBenchmark

This project benchmarks various .NET MIME parsers for comparison purposes.

## Results

```
Parsing startrek.msg (1000 iterations):
MimeKit:        0.6989221 seconds
OpenPop:        25.3056064 seconds
AE.Net.Mail:    17.5971438 seconds
MailSystem.NET: 26.3891218 seconds
MIMER:          76.4538978 seconds

Parsing xamarin3.msg (1000 iterations):
MimeKit:        3.4215505 seconds
OpenPop:        159.3308053 seconds
AE.Net.Mail:    132.3044291 seconds
MailSystem.NET: 133.5832078 seconds
MIMER:          784.433441 seconds
```

How does your MIME parser compare?

