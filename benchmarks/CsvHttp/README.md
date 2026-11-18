# CSV SQL/HTTP benchmark

Compares the CSV paths before/after commit `042ad69d` (parent `d3562767`)
and the current reader-to-HTTP streaming implementation.

- `Old`: benchmark-only `LegacyPlainTextReader.cs`, frozen from `042ad69d`.
  The original production method and formatter have now been removed.
- `New`: the real dictionary repository plus `OriginalListResponse.cs`,
  frozen from `042ad69d` (including its `SetData` and `ToCsv`).
- `Optimized`: current production `IEntityRepository.WriteCsvAsync` ->
  `DataAccess.WriteCsvAsync` -> `DbDataReader` -> CsvHelper -> HTTP writer.
  No row dictionaries or full CSV string are created on the server.

No simulated SQL reader, mocked repository or TestServer.

Prerequisites: .NET 10 SDK and Docker with permission to run Linux containers.

```sh
dotnet run -c Release --project benchmarks/CsvHttp -- --filter '*' --warmupCount 8 --iterationCount 12 --launchCount 2
```

Each BenchmarkDotNet process starts an isolated SQL Server 2022 CU14 container
with Testcontainers, creates and seeds 10,000 rows, starts Kestrel on a dynamic
loopback TCP port, validates all three response bodies and warms all paths. Setup,
image pull, seeding and cleanup are outside the measurement. Each operation
opens a pooled SQL connection, executes a stored procedure, writes CSV, sends
it over HTTP and consumes the entire response with HttpClient. Requests are
sequential, with persistent HTTP and warm database/connection caches.

The table has int, nvarchar, datetime2 and decimal columns, with identical
ordered results and no special CSV characters. Page sizes: 5, 1,000, 10,000.
The procedure implements the count flag: the old path requests a count and
the `New` path uses the default `tot=0` behavior (requests a count too).
`Optimized` does not request a count, matching the new production CSV contract.
The benchmark uses platform line endings on all paths for byte equality;
production streaming defaults to CRLF. Setup also validates special characters,
quotes, nulls, decimals and dates against the original CsvHelper output.
The host has dedicated benchmark endpoints that reproduce the changed data
paths; it excludes dictionary loading, authentication, user filters and the
rest of MasterApiService. This measures SQL + CSV + HTTP, not the entire
production application or concurrent throughput. Real production stored
procedures, wide tables and special-character payloads can change results.

MemoryDiagnoser measures allocations in the benchmark process, which hosts
both HTTP client and Kestrel; SQL Server memory is excluded. Keep full BDN
reports including standard deviation, confidence interval and warnings;
do not infer a winner from differences within measurement noise.

## Measured on 2026-11-06

Full output and environment are in `reports/2026-11-06.md`, `.csv` and `.log`.
Two launches, 8 warmups, 12 measured iterations per launch; all 6 cases passed
setup validation. Mean HTTP latency (old/new): 5 rows 1.506/1.127 ms;
1,000 rows 2.956/5.481 ms; 10,000 rows 13.949/39.975 ms.
The new path allocated approximately 2.0–2.54 times as much managed memory.
BDN flagged a possibly multimodal distribution and removed several outliers;
see the full log for those details. These are local warm sequential-request
measurements, not production capacity estimates.

## After removing the CSV row copy

`reports/2026-11-06-optimized.md` and `.csv` compare all three paths in the
same run (2 launches, 8 warmups, 12 measurements each). All 9 cases passed
SQL/HTTP response equality validation. The original `New` formatter is frozen
at commit `042ad69d`; `Optimized` uses current production code.

| Rows | New | Optimized | New allocated | Optimized allocated |
| ---: | ---: | ---: | ---: | ---: |
| 5 | 1.479 ms | 1.627 ms | 40.92 KiB | 38.32 KiB |
| 1,000 | 5.321 ms | 5.430 ms | 1,586.86 KiB | 1,070.77 KiB |
| 10,000 | 39.703 ms | 26.760 ms | 15,912.14 KiB | 10,656.48 KiB |

At 10,000 rows: about 33% less latency and allocated memory. Smaller page
latency confidence intervals overlap; no speed improvement is established
there. The legacy implementation remains faster for larger pages. BDN flagged
multimodal distributions and removed outliers; see the retained reports/logs.

## Reader-to-HTTP streaming

The current `Optimized` benchmark streams from the database reader via the new
repository API. Original formatters are kept only as benchmark snapshots.
`reports/2026-11-06-streaming.md` contains the full 9-case run. Builds and tests
ran during parts of that run; latency at 10,000 rows was noisy. Use the dedicated
repeat in `reports/2026-11-06-streaming-large.md` for the large-page comparison:
no builds/tests ran concurrently during measurement. Both reports retain BDN
confidence intervals, dispersion and warnings; full logs are saved locally.

| 10,000 rows | Mean | Managed allocation (client + server) |
| --- | ---: | ---: |
| Old | 12.93 ms | 6.12 MiB |
| New original | 40.40 ms | 15.54 MiB |
| Reader-to-HTTP | 16.72 ms | 5.33 MiB |

Streaming reduces mean latency by about 59% and managed allocations by about
66% versus the original new path. The legacy formatter remains faster in this
scenario, but streaming allocates about 13% less. All response validations,
including special characters and SQL date/decimal/null values, passed.

To reproduce just the large-page comparison:

```sh
CSV_BENCH_ROWS=10000 dotnet run -c Release --project benchmarks/CsvHttp -- --filter '*' --warmupCount 8 --iterationCount 12 --launchCount 2
```

The production defaults are CRLF line endings and no total-count query. The
benchmark matches platform line endings across implementations for comparison.
