
public enum World { Dark, Light }
public struct Tile
{
    public enum Type { Block, Empty, Goal }
    public World world;
    public Type type;

    /// <summary>
    /// Returns true if a character can wolk on that tile
    /// </summary>
    /// <returns></returns>
    public bool IsMovable()
    {
        if (type == Type.Empty)
            return false;

        return true;
    }

    /// <summary>
    /// Returns true, if the tile can in theory be pushed.
    /// This does not account for blocking objects
    /// </summary>
    /// <returns></returns>
    public bool IsPushable()
    {
        if (type == Type.Empty)
            return false;

        return true;
    }
    /// <summary>
    /// Returns true if other tiles can be pushed on it.
    /// This does not account for blocking objects
    /// </summary>
    /// <returns></returns>
    public bool CanBePushedOn()
    {
        if (type != Type.Block)
            return false;

        return true;
    }
}
