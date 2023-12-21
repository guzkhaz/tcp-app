using System.Reflection;

namespace Package.Serializator;

public class PacketConverter
{
    private static List<Tuple<FieldInfo, byte>> GetFields(Type t)
    {
        return t.GetFields(BindingFlags.Instance |
                           BindingFlags.NonPublic |
                           BindingFlags.Public)
            .Where(field => field.GetCustomAttribute<FieldAttribute>() != null)
            .Select(field => Tuple.Create(field, field.GetCustomAttribute<FieldAttribute>().FieldID))
            .ToList();
    }
    
    public static Packet Serialize(PacketType type, object obj, bool strict = false)
    {
        var t = PacketTypeManager.GetType(type);
        return Serialize(t.Item1, t.Item2, obj, strict);
    }
    public static Packet Serialize(byte type, byte subtype, object obj, bool strict = false)
    {
        var fields = GetFields(obj.GetType());

        if (strict)
        {
            var usedUp = new List<byte>();

            foreach (var field in fields)
            {
                if (usedUp.Contains(field.Item2))
                {
                    throw new Exception("One field used two times.");
                }

                usedUp.Add(field.Item2);
            }
        }

        var packet = Packet.Create(type, subtype);

        foreach (var field in fields)
        {
            packet.SetValue(field.Item2, field.Item1.GetValue(obj));
        }

        return packet;
    }

    public static T Deserialize<T>(Packet packet, bool strict = false)
    {
        var fields = GetFields(typeof(T));
        var instance = Activator.CreateInstance<T>();

        if (fields.Count == 0)
        {
            return instance;
        }

        foreach (var tuple in fields)
        {
            var field = tuple.Item1;
            var packetFieldId = tuple.Item2;

            if (!packet.HasField(packetFieldId))
            {
                if (strict)
                {
                    throw new Exception($"Couldn't get field[{packetFieldId}] for {field.Name}");
                }

                continue;
            }
            
             /* Метод GetValue<T>(byte) принимает тип как type-параметр
             * Наш же тип внутри field.FieldType
             * Используя Reflection, вызываем метод с нужным type-параметром
             */

            var value = typeof(Packet)
                .GetMethod("GetValue")?
                .MakeGenericMethod(field.FieldType)
                .Invoke(packet, new object[] { packetFieldId });

            if (value == null)
            {
                if (strict)
                {
                    throw new Exception($"Couldn't get value for field[ {packetFieldId}] for {field.Name}");
                }

                continue;
            }

            field.SetValue(instance, value);
        }

        return instance;
    }
}