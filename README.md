# Sharpino vs UmaDb Comparison

A side-by-side performance comparison between **Sharpino** and **UmaDb** when adding 10,000 elements.

## Overview

This project benchmarks the performance of:
- **Sharpino**: A lightweight event sourcing framework for F# / .NET.
- **UmaDb**: A high-performance event-sourced database.

The comparison is executed in the test suite defined in [WriteUmaTests.fs](file:///Users/antoniolucca/github/sharpinoVsUma/WriteUmaTests.fs), where we measure the time taken to append/add 10,000 elements.

## Performance Results

Our test runs show that **Sharpino is slightly faster** than UmaDb only in single task inserts.
Umadb prevails on parallel append tasks.

Below are the benchmark console outputs illustrating the comparison:

```text

Uma Append operation took 68 ms         
Add operation took 29 ms                
Massive Subscription of 10000 courses took 24 ms
Parallel Uma Append operation (10000 elements) took 504 ms
Parallel Sharpino Add operation (10000 elements) took 763 ms
Parallel tasks (30 tasks of 10000 elements) Uma Append operation took 832 ms
Parallel tasks (30 tasks of 10000 elements) Sharpino Add operation took 9190 ms

```

## Running the Tests

To run the comparison tests yourself, execute:

```bash
dotnet run
```
