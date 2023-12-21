using System.Net.Sockets;
using Package;
using Package.Serializator;
using ScrambleModels;

namespace TCPServer;

public class ConnectedClient
{
    public Socket Client { get; }
    public char[] Letters { get; set; }

    private readonly Queue<byte[]> _packetSendingQueue = new Queue<byte[]>();

    public ConnectedClient(Socket client)
    {
        Client = client;

        Task.Run(ProcessIncomingPackets);
        Task.Run(SendPackets);
    }

    private void ProcessIncomingPackets()
    {
        while (true) // Слушаем пакеты, пока клиент не отключится.
        {
            var buff = new byte[256]; // Максимальный размер пакета - 256 байт.
            Client.Receive(buff);

            buff = buff.TakeWhile((b, i) =>
            {
                if (b != 0xFF) return true;
                return buff[i + 1] != 0;
            }).Concat(new byte[] {0xFF, 0}).ToArray();

            var parsed = Packet.Parse(buff);

            if (parsed != null)
            {
                ProcessIncomingPacket(parsed);
            }
        }
    }

    private void ProcessIncomingPacket(Packet packet)
    {
        var type = PacketTypeManager.GetTypeFromPacket(packet);

        switch (type)
        {
            case PacketType.BeginGame:
                ProcessBeginGame(packet);
                break;
            case PacketType.UpdateGame:
                Server.UpdateBoard(packet);
                break;
            case PacketType.Unknown:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void ProcessBeginGame(Packet packet)
    {
        Letters = GenerateRandomLetters();
        var sendModel = new SendModel()
        {
            Board = Server.Board,
            Letters = Letters,
            Log = "Игра началась"
        };
        QueuePacketSend(PacketConverter.Serialize(PacketType.SendModel, sendModel).ToPacket());
    }
    public void UpdateGame()
    {
        Letters = GenerateRandomLetters();
        var sendModel = new SendModel()
        {
            Board = Server.Board,
            Letters = Letters,
            Log = "Успешное обновление"
        };
        QueuePacketSend(PacketConverter.Serialize(PacketType.SendModel, sendModel).ToPacket());
    }

    public void SendErrorMessage()
    {
        var sendModel = new SendModel()
        {
            Board = Server.Board,
            Letters = Letters,
            Log = "Ошибка"
        };

        QueuePacketSend(PacketConverter.Serialize(PacketType.SendModel, sendModel).ToPacket());
    }
    private char[] GenerateRandomLetters()
    {
            var words = File.ReadLines("russian_nouns.txt").ToArray();
            var word1 = words[new Random().Next(0, words.Length)][..2];
            var word2 = words[new Random().Next(0, words.Length)][..2];
            var word3 = words[new Random().Next(0, words.Length)][..3];
            return (word1 + word2 + word3).ToCharArray();
    }
    public void QueuePacketSend(byte[] packet)
    {
        if (packet.Length > 256)
        {
            throw new Exception("Max packet size is 256 bytes.");
        }

        _packetSendingQueue.Enqueue(packet);
    }

    private void SendPackets()
    {
        while (true)
        {
            if (_packetSendingQueue.Count == 0)
            {
                Thread.Sleep(100);
                continue;
            }

            var packet = _packetSendingQueue.Dequeue();
            Client.Send(packet);

            Thread.Sleep(100);
        }
    }
}