using Package.Serializator;

namespace Package;

public class TestPacket
{
    [Field(0)]
    public int TestNumber;

    [Field(1)]
    public double TestDouble;

    [Field(2)]
    public bool TestBoolean;
}