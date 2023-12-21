using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ScrambleModels;

namespace TCPServer;

public static class WordsValidator
{
    public static bool IsValid(string word)
    {
        var words = File.ReadLines("russian_nouns.txt").ToArray();
        return words.Any(str => str == word);
    }
}
