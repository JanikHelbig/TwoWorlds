using System;
using UnityEngine;

public class Level
{
    public Tile[,] tiles;
    public Vector2Int spawnDark;
    public Vector2Int spawnLight;
    public bool activeWorld;

    private World raisedWorld = World.Dark;

    public event Action<World> OnRaisedWorldChanged;

    public World RaisedWorld
    {
        get => raisedWorld;
        set
        {
            raisedWorld = value;
            OnRaisedWorldChanged?.Invoke(raisedWorld);
        }
    }

    public Tile this[int x, int y]
    {
        get
        {

            if (x < 0 || y < 0 || x >= Width || y >= Height)
            {
                Tile empty = new Tile();
                empty.type = Tile.Type.Empty;
                return empty;
            }
            return tiles[x, y];
        }
}

    public int Width => tiles.GetLength(0);
    public int Height => tiles.GetLength(1);

    public void ToggleRaisedWorld()
    {
        if (RaisedWorld == World.Dark)
            RaisedWorld = World.Light;
        else
            RaisedWorld = World.Dark;




    }
}
