using System.Net;
using System.Net.Sockets;
using Package;
using Package.Serializator;
using ScrambleModels;

namespace TCPServer;

public static class Server
{
    private static readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private static readonly List<ConnectedClient> _clients = new();
    public static PacketBoard Board = new();
    private static readonly List<string> _words = new List<string>();

    private static bool _listening;
    private static bool _stopListening;

    public static void Start()
    {
        if (_listening)
        {
            throw new Exception("Server is already listening incoming requests.");
        }
        InitBoard();
        _socket.Bind(new IPEndPoint(IPAddress.Any, 4910));
        _socket.Listen(10);

        _listening = true;
    }

    public static void Stop()
    {
        if (!_listening)
        {
            throw new Exception("Server is already not listening incoming requests.");
        }

        _stopListening = true;
        _socket.Shutdown(SocketShutdown.Both);
        _listening = false;
    }

    public static void AcceptClients()
    {
        while (true)
        {
            if (_stopListening)
            {
                return;
            }

            Socket client;

            try
            {
                client = _socket.Accept();
            } catch { return; }

            Console.WriteLine($"[!] Accepted client from {(IPEndPoint) client.RemoteEndPoint}");

            var c = new ConnectedClient(client);
            _clients.Add(c);
        }
    }

    public static void InitBoard()
    {
        AddWordToBoard(Board);
    }

    public static void UpdateBoard(Packet packet)
    {
        var newBoard = PacketConverter.Deserialize<PacketBoard>(packet);
        var message1 = SortVerticalWords(newBoard);
        var message2 = SortHorizontalWords(newBoard);
        if (message1 == "Успех" && message2 == "Успех")
        {
            foreach (var client in _clients)
            {
                client.UpdateGame();
            }

            Board = newBoard;
        }
        else
        {
            foreach (var client in _clients)
            {
                client.SendErrorMessage( );
            }
        }
    }

    private static string SortHorizontalWords(PacketBoard board)
    {
        var message = "Ошибка";
        var newWords = new List<string>();
        var flag = true;
        for (int i = 0; i < 9; i++)
        {
            var wordList = "";
            for (int j = 0; j < 9; j++)
            {
                wordList += board.Tiles[i, j].Letter;
            }
            var wordsArray = wordList.Split(" ");
            foreach (var word in wordsArray)
            {
                if (_words.Contains(word))
                {
                    flag = false;
                    break;
                }
                if (!WordsValidator.IsValid(word))
                {
                    flag = false;
                    break;
                }
            }
        }
        if (flag)
        {
            _words.AddRange(newWords);
            message = "Успех";
        }
        return message;
    }
    private static string SortVerticalWords(PacketBoard board)
    {
        var message = "Ошибка";
        var newWords = new List<string>();
        var flag = true;
        for (int i = 0; i < 9; i++)
        {
            var wordList = "";
            for (int j = 0; j < 9; j++)
            {
                wordList += board.Tiles[j, i].Letter;
            }
            var wordsArray = wordList.Split(" ");
            foreach (var word in wordsArray)
            {
                if (_words.Contains(word))
                {
                    flag = false;
                    break;
                }
                if (!WordsValidator.IsValid(word))
                {
                    flag = false;
                    break;
                }
            }
        }
        if (flag)
        {
            _words.AddRange(newWords);
            message = "Успех";
        }
        return message;
            
    }
    private static void AddWordToBoard(PacketBoard board)
    {
        const int i = 4;
        var word = GenerateRandomWord();
        for (var j = 2; j < 7; j++)
        {
            board.Tiles[i, j].Letter = word[j - 2];
        }
        _words.Add(word);
    }

    private static string GenerateRandomWord()
    {
        var word = "";
        while (word.Length != 5)
        {
            var words = File.ReadLines("russian_nouns.txt").ToArray();
            word = words[new Random().Next(0, words.Length)];
        }
        return word;
    }
}