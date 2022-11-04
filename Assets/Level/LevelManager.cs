using UnityEngine;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject lightBoxPrefab;
    [SerializeField] private GameObject darkBoxPrefab;

    private void Awake()
    {
        LoadLevel("test.txt");
    }

    public void LoadLevel(string path)
    {
         LevelUtils.LoadLevelDataFromFile(path);
    }
}
