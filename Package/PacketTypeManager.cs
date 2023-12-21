using System.Reflection;

namespace Package;

public static class PacketTypeManager
{
    private static readonly Dictionary<PacketType, Tuple<byte, byte>> TypeDictionary = new Dictionary<PacketType, Tuple<byte, byte>>();
    
    public static void RegisterType(PacketType type, byte btype, byte bsubtype)
    {
        if (TypeDictionary.ContainsKey(type))
        {
            throw new Exception($"Packet type {type:G} is already registered.");
        }

        TypeDictionary.Add(type, Tuple.Create(btype, bsubtype));
    }
    
    public static Tuple<byte, byte> GetType(PacketType type)
    {
        if (!TypeDictionary.ContainsKey(type))
        {
            throw new Exception($"Packet type {type:G} is not registered.");
        }

        return TypeDictionary[type];
    }
    
    public static PacketType GetTypeFromPacket(Packet packet)
    {
        var type = packet.PacketType;
        var subtype = packet.PacketSubtype;

        foreach (var tuple in TypeDictionary)
        {
            var value = tuple.Value;

            if (value.Item1 == type && value.Item2 == subtype)
            {
                return tuple.Key;
            }
        }

        return PacketType.Unknown;
    }
}