using UnityEngine;
using System.IO;

public static class LevelUtils
{
    public static Level LoadLevelDataFromFile(string fileName)
    {
        TextAsset file =  Resources.Load<TextAsset>(Path.Combine("Levels",fileName));
        string[] lines = file.text.Split("\r\n");
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
                if(x >= line.Length)
                {
                    lvl.tiles[x, y].type = Tile.Type.Empty;
                    continue;
                }

                switch (line[x])
                {
                    case '#':
                        type = Tile.Type.Dark;
                        break;
                    case 'X':
                        type = Tile.Type.Dark;
                        lvl.spawnDark = new Vector2Int(x, y);
                        break;
                    case '.':
                        type = Tile.Type.Light;
                        break;
                    case 'i':
                        type = Tile.Type.Light;
                        lvl.spawnLight = new Vector2Int(x, y);
                        break;
                }
                lvl.tiles[x, y].type = type;
            }
        }

        return lvl;
    }
}
