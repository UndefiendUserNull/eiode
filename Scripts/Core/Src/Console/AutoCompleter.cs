using System;
using System.Collections.Generic;

namespace EIODE.Core.Console;

public sealed class AutoCompleter
{
    private readonly string[] _sortedWords;
    private readonly int _maxSuggestions;

    // to avoid GC pressure
    private readonly List<string> _resultBuffer;

    public AutoCompleter(IEnumerable<string> words, int maxSuggestions = 5)
    {
        // Create and sort the array once during initialization
        _sortedWords = [.. words];
        Array.Sort(_sortedWords, StringComparer.OrdinalIgnoreCase);

        _maxSuggestions = maxSuggestions;
        _resultBuffer = new List<string>(maxSuggestions);
    }

    public IReadOnlyList<string> GetSuggestions(string prefix)
    {
        _resultBuffer.Clear();

        int index = Array.BinarySearch(
            _sortedWords,
            prefix,
            StringComparer.OrdinalIgnoreCase
        );

        if (index < 0)
        {
            index = ~index;
        }

        // Collect matching words
        for (int i = index; i < _sortedWords.Length && _resultBuffer.Count < _maxSuggestions; i++)
        {
            if (_sortedWords[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                _resultBuffer.Add(_sortedWords[i]);
            }
            else
            {
                break;
            }
        }

        return _resultBuffer;
    }
}