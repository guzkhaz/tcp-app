using Package.Serializator;

namespace ScrambleModels;

public class SendModel
{
    [Field(1)]
    public PacketBoard Board;

    [Field(2)]
    public char[] Letters;
    
    [Field(3)]
    public string Log;
    
}