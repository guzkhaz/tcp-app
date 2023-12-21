using Package.Serializator;

namespace ScrambleModels;

public class PacketBoard
{
    [Field(1)]
    public PacketTile[,] Tiles;
}