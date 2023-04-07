using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Open.Collections.Benchmarks;

/*
|                    Method |     Mean |     Error |    StdDev |     Gen 0 |   Allocated |
|-------------------------- |---------:|----------:|----------:|----------:|------------:|
|                TrieLookup | 1.672 ms | 0.0317 ms | 0.0325 ms |         - |           - |
|            TrieWalkLookup | 1.866 ms | 0.0366 ms | 0.0697 ms |         - |           - |
|     TrieLookupWithToArray | 2.243 ms | 0.0340 ms | 0.0391 ms |  265.6250 |   560,002 B |
|          DictionaryLookup | 1.330 ms | 0.0140 ms | 0.0124 ms |         - |           - |
| DictionaryLookupKeyConcat | 2.033 ms | 0.0244 ms | 0.0228 ms | 1527.3438 | 3,200,004 B |
|  DictionaryLookupWithJoin | 2.007 ms | 0.0052 ms | 0.0046 ms | 1527.3438 | 3,200,002 B |
*/

[MemoryDiagnoser]
public class TrieBenchmark
{
    private const int NodeSize = 10;
    private const int Depth = 4; // Changing this will break some of the tests.

    private ConcurrentTrie<string, int> ctrie;
    private ConcurrentDictionary<string, int> cdictionary;

    private Trie<string, int> trie;
    private Dictionary<string, int> dictionary;
    private (string[], string)[] keys;

    [GlobalSetup]
    public void GlobalSetup()
    {
        int x = 0;
        trie = new();
        dictionary = new();
        ctrie = new();
        cdictionary = new();
        keys = GenerateTree(Depth, NodeSize)
            .Select(key=>
            {
                x++;
                trie.Add(key, x);
                ctrie.Add(key, x);

                string keyString = string.Join('/', key);
                dictionary.Add(keyString, x);
                cdictionary.TryAdd(keyString, x);

                return (key, keyString);
            }).ToArray();
    }

    public static IEnumerable<string[]> GenerateTree(int depth, int nodeCount, Stack<string> stack = null)
    {
        stack ??= new Stack<string>();

        if (depth == 0)
        {
            yield return stack.Reverse().ToArray();
            yield break;
        }

        for(int i = 0; i < nodeCount; i++)
        {
            stack.Push(Guid.NewGuid().ToString());
            foreach(string[] key in GenerateTree(depth - 1, nodeCount, stack))
            {
                yield return key;
            }
            stack.Pop();
        }
    }

    [Benchmark]
    public int TrieLookup()
    {
        int result = 0;
        foreach(var key in keys.AsSpan())
        {
            trie.TryGetValue(key.Item1, out result);
        }
        return result;
    }

    [Benchmark]
    public int TrieWalkLookup()
    {
        int result = 0;
        foreach (var key in keys.AsSpan())
        {
            string[] k = key.Item1;
            result = trie.GetChild(k[0]).GetChild(k[1]).GetChild(k[2]).GetChild(k[3]).Value;
        }
        return result;
    }

    [Benchmark]
    public int TrieLookupWithToArray()
    {
        int result = 0;
        foreach (var key in keys.AsSpan())
        {
            trie.TryGetValue(key.Item1.ToArray(), out result);
        }
        return result;
    }

    [Benchmark]
    public int DictionaryLookup()
    {
        int result = 0;
        foreach (var key in keys.AsSpan())
        {
            result = dictionary[key.Item2];
        }
        return result;
    }

    [Benchmark]
    public int DictionaryLookupKeyConcat()
    {
        int result = 0;
        foreach (var key in keys.AsSpan())
        {
            string[] k = key.Item1;
            result = dictionary[$"{k[0]}/{k[1]}/{k[2]}/{k[3]}"];
        }
        return result;
    }

    [Benchmark]
    public int DictionaryLookupWithJoin()
    {
        int result = 0;
        foreach (var key in keys.AsSpan())
        {
            result = dictionary[string.Join("/", key.Item1)];
        }
        return result;
    }

    //[Benchmark]
    //public void CTrieLookup()
    //{
    //    Parallel.ForEach(keys, key
    //        => ctrie.TryGetValue(key.Item1, out _));
    //}

    //[Benchmark]
    //public void CTrieLookupWithToArray()
    //{
    //    Parallel.ForEach(keys, key
    //        => ctrie.TryGetValue(key.Item1.ToArray(), out _));
    //}

    //[Benchmark]
    //public void CDictionaryLookup()
    //{
    //    Parallel.ForEach(keys, key
    //        => cdictionary.TryGetValue(key.Item2, out _));
    //}

    //[Benchmark]
    //public void CDictionaryLookupWithJoin()
    //{
    //    Parallel.ForEach(keys, key
    //        => cdictionary.TryGetValue(string.Join("/", key.Item1), out _));
    //}
}
