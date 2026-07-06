# Sharpino vs UmaDb Comparison

A side-by-side performance comparison between **Sharpino** and **UmaDb** when adding 10,000 elements.

## Overview

This project benchmarks the performance of:
- **Sharpino**: A lightweight event sourcing framework for F# / .NET.
- **UmaDb**: A high-performance event-sourced database.

The comparison is executed in the test suite defined in [WriteUmaTests.fs](file:///Users/antoniolucca/github/sharpinoVsUma/WriteUmaTests.fs), where we measure the time taken to append/add 10,000 elements.

## Performance Results

Our test runs show that **Sharpino is slightly faster** than UmaDb when processing these batch inserts.

Below are the benchmark console outputs illustrating the comparison:

```text
UMa db Uma Append operation (10000 elements) took 86 ms         
Sharpino Add initial states operation (10000 elements) took 30 ms                
Sharpino Massive Subscription of 10000 courses took 31 ms
```

## Running the Tests

To run the comparison tests yourself, execute:

```bash
dotnet test
```
