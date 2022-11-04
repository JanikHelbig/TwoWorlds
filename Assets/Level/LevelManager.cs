using UnityEngine;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject lightBoxPrefab;
    [SerializeField] private GameObject darkBoxPrefab;

    public Level level;

    private void Awake()
    {
        LoadLevel("test.txt");
    }

    public void LoadLevel(string path)
    {
        level = LevelUtils.LoadLevelDataFromFile(path);
        for(int x = 0; x < level.Width; x++)
        {
            for(int y=0; y < level.Height; y++)
            {
                if(level.tiles[x,y].type == Tile.Type.Dark)
                {

                }
                else if(level.tiles[x,y].type == Tile.Type.Light)
                {

                }
            }
        }
    }
}
