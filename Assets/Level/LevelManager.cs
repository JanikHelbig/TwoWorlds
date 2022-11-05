using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject lightBoxPrefab;
    [SerializeField] private GameObject darkBoxPrefab;
    [SerializeField] private Transform lightContainer;
    [SerializeField] private Transform darkContainer;

    public Level level;
    private GameObject[,] instances;

    private void Awake()
    {
        LoadLevel("test");
    }

    private void ClearLevel()
    {
        if (instances != null)
        {
            for (int x = 0; x < instances.GetLength(0); x++)
            {

                for (int y = 0; y < instances.GetLength(1); y++)
                {
                    if(instances[x,y] != null) 
                    {
                        Destroy(instances[x, y]);
                    }
                }
            }
        }
    }

    public void LoadLevel(string fileName)
    {
        ClearLevel();

        level = LevelUtils.LoadLevelDataFromFile(fileName);
        instances = new GameObject[level.Width,level.Height];
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
                    instances[x, y] = go;
                }
            }
        }

        lightContainer.transform.position = Vector3.zero;
        darkContainer.transform.position = Vector3.zero + Vector3.up;

        level.OnRaisedWorldChanged += OnRaisedWorldChanged;
        level.OnTileSwitched += OnTilesSwitched;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            level?.ToggleRaisedWorld();

        if (Input.GetKeyDown(KeyCode.A))
            level?.TryMoveTile(new int2(2,2), Direction.NORTH);
    }

    void OnRaisedWorldChanged(World raisedWorld)
    {
        float duration = 0.2f;
        Sequence s = DOTween.Sequence();
        s.Join(darkContainer.transform.DOLocalMoveY(raisedWorld == World.Dark ? 1 : 0, duration));
        s.Join(lightContainer.transform.DOLocalMoveY(raisedWorld == World.Light ? 1 : 0, duration));
        s.Play();
    }

    void OnTilesSwitched(int2 p1, int2 p2 )
    {
        GameObject g1 = instances[p1.x, p1.y];
        GameObject g2 = instances[p2.x, p2.y];
        instances[p1.x, p1.y] = g2;
        instances[p2.x, p2.y] = g1;

        GameObject temp = Instantiate(g2, this.transform);
        temp.transform.localPosition = new Vector3(p1.x, 0, p1.y);
        temp.name = "Animation Placeholder";

        Sequence s = DOTween.Sequence();
        s.Append(g1.transform.DOLocalMove(new Vector3(p2.x, 0, p2.y), 0.3f));
        s.OnComplete(() => {
            g2.transform.localPosition = new Vector3(p1.x, 0, p1.y);
            Destroy(temp);
        });
        s.Play();
    }
}