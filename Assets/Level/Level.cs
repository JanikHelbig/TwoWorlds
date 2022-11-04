using UnityEngine;

public class Level
{
    public Tile[,] tiles;
    public Vector2Int spawnDark;
    public Vector2Int spawnLight;

    public int Width => tiles.GetLength(0);
    public int Height => tiles.GetLength(1);
}
