using UnityEngine;
using DG.Tweening;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject lightBoxPrefab;
    [SerializeField] private GameObject darkBoxPrefab;
    [SerializeField] private Transform lightContainer;
    [SerializeField] private Transform darkContainer;

    public Level level;

    private void Awake()
    {
        LoadLevel("test");
    }

    public void LoadLevel(string fileName)
    {
        level = LevelUtils.LoadLevelDataFromFile(fileName);
        for(int x = 0; x < level.Width; x++)
        {
            for(int y=0; y < level.Height; y++)
            {
                GameObject prefab = null;
                Transform container = this.transform;
                if(level.tiles[x,y].type == Tile.Type.Block)
                {
                    switch(level.tiles[x,y].world)
                    {
                        case World.Dark:
                            prefab = darkBoxPrefab;
                            container = darkContainer;
                            break;
                        case World.Light:
                            prefab = lightBoxPrefab;
                            container = lightContainer;
                            break;
                    }
                }

                if(prefab)
                {
                    var go = Instantiate(prefab, container);
                    go.transform.localPosition = new Vector3(x, 0, y);
                }
            }
        }

        lightContainer.transform.position = Vector3.zero;
        darkContainer.transform.position = Vector3.zero + Vector3.up;
        level.OnRaisedWorldChanged += OnRaisedWorldChanged;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            level?.ToggleRaisedWorld();
    }

    void OnRaisedWorldChanged(World raisedWorld)
    {
        float duration = 0.2f;
        Sequence s = DOTween.Sequence();
        s.Join(darkContainer.transform.DOLocalMoveY(raisedWorld == World.Dark ? 1 : 0, duration));
        s.Join(lightContainer.transform.DOLocalMoveY(raisedWorld == World.Light ? 1 : 0, duration));
        s.Play();
    }
}