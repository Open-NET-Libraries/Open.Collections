using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Open.Collections.Benchmarks;

/*
|                    Method |     Mean |     Error |    StdDev |     Gen 0 |   Allocated |
|-------------------------- |---------:|----------:|----------:|----------:|------------:|
|                TrieLookup | 2.161 ms | 0.0424 ms | 0.0471 ms |         - |           - |
|            TrieWalkLookup | 1.925 ms | 0.0062 ms | 0.0058 ms |         - |           - |
|     TrieLookupWithToArray | 2.522 ms | 0.0165 ms | 0.0138 ms |  265.6250 |   560,002 B |
|          DictionaryLookup | 1.064 ms | 0.0065 ms | 0.0057 ms |         - |           - |
| DictionaryLookupKeyConcat | 1.977 ms | 0.0063 ms | 0.0056 ms | 1527.3438 | 3,200,002 B |
|  DictionaryLookupWithJoin | 1.870 ms | 0.0045 ms | 0.0042 ms | 1529.2969 | 3,200,001 B |
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
    private List<(string[], string)> keys;

    [GlobalSetup]
    public void GlobalSetup()
    {
        int x = 0;
        trie = new();
        dictionary = new();
        ctrie = new();
        cdictionary = new();
        keys = new();

        foreach (string[] key in GenerateTree(Depth, NodeSize))
        {
            x++;
            trie.Add(key, x);
            ctrie.Add(key, x);

            string keyString = string.Join('/', key);
            dictionary.Add(keyString, x);
            cdictionary.TryAdd(keyString, x);

            keys.Add((key, keyString));
        }
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
        foreach(var key in keys)
        {
            trie.TryGetValue(key.Item1, out result);
        }
        return result;
    }

    [Benchmark]
    public int TrieWalkLookup()
    {
        int result = 0;
        foreach (var key in keys)
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
        foreach (var key in keys)
        {
            trie.TryGetValue(key.Item1.ToArray(), out result);
        }
        return result;
    }

    [Benchmark]
    public int DictionaryLookup()
    {
        int result = 0;
        foreach (var key in keys)
        {
            result = dictionary[key.Item2];

        }
        return result;
    }

    [Benchmark]
    public int DictionaryLookupKeyConcat()
    {
        int result = 0;
        foreach (var key in keys)
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
        foreach (var key in keys)
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
