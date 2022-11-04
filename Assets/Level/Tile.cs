
public enum World { Dark, Light }
public struct Tile
{
    public enum Type { Block, Empty }
    public World world;
    public Type type;
}
