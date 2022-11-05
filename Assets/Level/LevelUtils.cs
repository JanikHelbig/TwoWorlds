using UnityEngine;
using System.IO;
using Unity.Mathematics;

public static class LevelUtils
{
    public static Level LoadLevelDataFromFile(string fileName)
    {
        string[] lines = File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, "Levels", fileName + ".txt"));
        int width = 0;
        foreach(string line in lines)
        {
            if (line.Length > width)
                width = line.Length;
        }
        int height = lines.Length;

        Level lvl = new Level();
        lvl.tiles = new Tile[width, height];

        for (int y = 0; y < height; y++)
        {
            string line = lines[y];
            for (int x = 0; x<width;x++)
            {
                lvl.tiles[x, y] = new Tile();
                Tile.Type type = Tile.Type.Empty;
                World world = World.Dark;
                if (x >= line.Length)
                {
                    lvl.tiles[x, y].type = Tile.Type.Empty;
                    continue;
                }

                switch (line[x])
                {
                    case '#':
                        type = Tile.Type.Block;
                        world = World.Dark;
                        break;
                    case 'X':
                        type = Tile.Type.Block;
                        world = World.Dark;
                        lvl.spawnDark = new Vector2Int(x, y);
                        break;
                    case '.':
                        type = Tile.Type.Block;
                        world = World.Light;
                        break;
                    case 'i':
                        type = Tile.Type.Block;
                        world = World.Light;
                        lvl.spawnLight = new Vector2Int(x, y);
                        break;
                }
                lvl.tiles[x, y].type = type;
                lvl.tiles[x, y].world = world;
            }
        }

        return lvl;
    }

    public static int2 OffsetPosition(this int2 position, Direction dir)
    {
        switch(dir)
        {
            case Direction.NORTH:
                return position + new int2(0,1);
            case Direction.EAST:
                return position + new int2(1, 0);
            case Direction.SOUTH:
                return position + new int2(0, -1);
            case Direction.WEST:
                return position + new int2(-1, 0);
        }
        throw new System.Exception("Invalid Direction: "+dir);
    }
}
