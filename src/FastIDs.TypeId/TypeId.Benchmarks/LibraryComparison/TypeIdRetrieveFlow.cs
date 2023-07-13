﻿using System.Text;
using BenchmarkDotNet.Attributes;

namespace FastIDs.TypeId.Benchmarks.LibraryComparison;

[MemoryDiagnoser]
[MarkdownExporter]
public class TypeIdRetrieveFlow
{
    [Params(10_000_000)]
    public int Iterations;

    [Params(0, 5, 10, 30, 63)]
    public int PrefixLength;

    private string[] _typeIdStrings;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        var sb = new StringBuilder(PrefixLength);
        for (var i = 0; i < PrefixLength; i++)
        {
            var letter = (char) random.Next('a', 'z');
            sb.Append(letter);
        }
        var prefix = PrefixLength > 0 ? sb.ToString() : "";

        _typeIdStrings = new string[Iterations];
        for (var i = 0; i < Iterations; i++)
        {
            _typeIdStrings[i] = TypeId.New(prefix, false).ToString();
        }        
    }
    
    [Benchmark(Baseline = true)]
    public string FastIdsBenchmark()
    {
        var result = "";
        foreach (var str in _typeIdStrings)
        {
            var typeId = TypeId.Parse(str);
            result = typeId.ToString();
        }
        return result;
    }
    
    [Benchmark]
    public string FastIdsWithoutValidationBenchmark()
    {
        var result = "";
        foreach (var str in _typeIdStrings)
        {
            var typeId = TypeId.Parse(str);
            result = typeId.ToString();
        }

        return result;
    }

    [Benchmark]
    public string TcKsBenchmark()
    {
        var result = "";
        foreach (var str in _typeIdStrings)
        {
            var typeId = TcKs.TypeId.TypeId.Parse(str);
            result = typeId.ToString();
        }
        return result;
    }

    [Benchmark]
    public string EvgregBenchmark()
    {
        var result = "";
        foreach (var str in _typeIdStrings)
        {
            var typeId = global::TypeId.TypeId.Parse(str);
            result = typeId.ToString();
        }
        return result;
    }
}