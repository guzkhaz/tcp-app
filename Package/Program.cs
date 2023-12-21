using Package;
using Package.Serializator;

var t = new TestPacket 
{
    TestNumber = 12345,
    TestDouble = 123.45D,
    TestBoolean = true
};

var packet = PacketConverter.Serialize(0, 0, t);
var tDes = PacketConverter.Deserialize<TestPacket>(packet);

if (tDes.TestBoolean)
{
    Console.WriteLine($"Number = {tDes.TestNumber}\n" +
    $"Double = {tDes.TestDouble}");
}