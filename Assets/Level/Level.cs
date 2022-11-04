using UnityEngine;

public class Level
{
    public Tile[,] tiles;
    public Vector2Int spawnDark;
    public Vector2Int spawnLight;
    public bool activeWorld;

    public World RaisedWorld = World.Dark;

    public int Width => tiles.GetLength(0);
    public int Height => tiles.GetLength(1);
}
