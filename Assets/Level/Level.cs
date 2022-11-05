using System;
using UnityEngine;
using Unity.Mathematics;

public class Level
{
    public Tile[,] tiles;
    public int2 spawnDark;
    public int2 spawnLight;
    public int2 goalDark;
    public int2 goalLight;
    public bool activeWorld;

    private World raisedWorld = World.Dark;

    public event Action<World> OnRaisedWorldChanged;
    public event Action<int2, int2> OnTileSwitched;

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
        private set
        {

            if (x < 0 || y < 0 || x >= Width || y >= Height)
            {
                return;
            }
            tiles[x, y] = value;
        }
}

    public int Width => tiles.GetLength(0);
    public int Height => tiles.GetLength(1);

    public bool TryMoveTile(int2 position, Direction dir)
    {
        if (!CanMoveTile(position, dir))
            return false;

        int2 nextPosition = position.OffsetPosition(dir);
        Tile t1 = this[position.x, position.y];
        Tile t2 = this[nextPosition.x, nextPosition.y];
        this[position.x, position.y] = t2;
        this[nextPosition.x, nextPosition.y] = t1;

        OnTileSwitched?.Invoke(position, nextPosition);

        return true;
    }

    public bool CanMoveTile(int2 position, Direction dir)
    {
        int2 nextPosition = position.OffsetPosition(dir);
        Tile blockToMove = this[position.x, position.y];
        Tile targetTile = this[nextPosition.x, nextPosition.y];
        
        if (!blockToMove.IsPushable() || blockToMove.world != this.raisedWorld)
            return false; //Is not a raised block
        if (!blockToMove.CanBePushedOn() || targetTile.world == this.raisedWorld)
            return false; //Is not a tile to be moved to

        return true;
    }

    public void ToggleRaisedWorld()
    {
        if (RaisedWorld == World.Dark)
            RaisedWorld = World.Light;
        else
            RaisedWorld = World.Dark;
    }
}
