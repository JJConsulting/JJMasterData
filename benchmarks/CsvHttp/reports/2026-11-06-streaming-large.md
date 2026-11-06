```

BenchmarkDotNet v0.15.8, Linux CachyOS
Intel Core 7 150U 0.65GHz, 1 CPU, 12 logical and 10 physical cores
.NET SDK 10.0.403
  [Host]     : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v3
  Job-MKWYNG : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v3

IterationCount=12  LaunchCount=2  WarmupCount=8  

```
| Method    | Rows  | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0     | Gen1    | Gen2    | Allocated | Alloc Ratio |
|---------- |------ |---------:|---------:|---------:|------:|--------:|---------:|--------:|--------:|----------:|------------:|
| Old       | 10000 | 12.93 ms | 0.440 ms | 0.556 ms |  1.00 |    0.06 | 156.2500 | 62.5000 | 62.5000 |   6.12 MB |        1.00 |
| New       | 10000 | 40.40 ms | 1.329 ms | 1.728 ms |  3.13 |    0.19 | 272.7273 | 90.9091 |       - |  15.54 MB |        2.54 |
| Optimized | 10000 | 16.72 ms | 0.640 ms | 0.786 ms |  1.30 |    0.08 | 156.2500 | 31.2500 | 31.2500 |   5.33 MB |        0.87 |
