using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject lightBoxPrefab;
    [SerializeField] private GameObject darkBoxPrefab;
    [SerializeField] private GameObject lightGoalPrefab;
    [SerializeField] private GameObject darkGoalPrefab;
    [SerializeField] private Transform lightContainer;
    [SerializeField] private Transform darkContainer;

    [Header("Player")]
    [SerializeField] private GameObject p1;
    [SerializeField] private GameObject p2;

    [Header("Camera")]
    [SerializeField] private Transform minPosition;
    [SerializeField] private Transform maxPosition;

    private int currentLevel = 1;

    public Level level;
    private GameObject[,] instances;

    private void Awake()
    {
        RestartLevel();
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
                switch (level.tiles[x,y].type)
                {
                    case Tile.Type.Block:
                        switch (level.tiles[x,y].world)
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
                    break;
                    case Tile.Type.Goal:
                        switch (level.tiles[x, y].world)
                        {
                            case World.Dark:
                                prefab = darkGoalPrefab;
                                container = darkContainer;
                                break;
                            case World.Light:
                                prefab = lightGoalPrefab;
                                container = lightContainer;
                                break;
                        }
                        break;
                }

                if(prefab)
                {
                    var go = Instantiate(prefab, container);
                    go.transform.localPosition = new Vector3(x, 0, y);
                    instances[x, y] = go;
                }
            }
        }

        minPosition.transform.position = new Vector3(-0.5f, 2, -0.5f);
        maxPosition.transform.position = new Vector3(level.Width+0.5f, 2,level.Height+0.5f);

        lightContainer.transform.position = Vector3.zero;
        darkContainer.transform.position = Vector3.zero + Vector3.up;
        if(p1)
        {
            p1.transform.SetParent(lightContainer);
            p1.transform.localPosition = new Vector3(level.spawnLight.x, 0, level.spawnLight.y);
        }
        if(p2)
        {
            p2.transform.SetParent(darkContainer);
            p2.transform.localPosition = new Vector3(level.spawnDark.x, 0, level.spawnDark.y);
        }

        level.OnRaisedWorldChanged += OnRaisedWorldChanged;
        level.OnTileSwitched += OnTilesSwitched;
    }

    public void RestartLevel()
    {
        LoadLevel("lvl" + currentLevel);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            level?.ToggleRaisedWorld();


        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentLevel = 1;
            RestartLevel();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentLevel = 2;
            RestartLevel();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentLevel = 3;
            RestartLevel();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currentLevel = 4;
            RestartLevel();
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            currentLevel =5;
            RestartLevel();
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            currentLevel = 6;
            RestartLevel();
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            currentLevel = 7;
            RestartLevel();
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            currentLevel = 8;
            RestartLevel();
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            currentLevel = 9;
            RestartLevel();
        }


        if (Input.GetKeyDown(KeyCode.R))
            RestartLevel();
        
        CheckWinCondition();
    }

    void CheckWinCondition()
    {
        if (!p1 || !p2)
            return;

        float R = 0.5f;
        if(Mathf.Abs(p1.transform.position.x - level.goalLight.x) < R &&
            Mathf.Abs(p1.transform.position.y - level.goalLight.y) < R && 
            Mathf.Abs(p2.transform.position.x - level.goalDark.x) < R &&
            Mathf.Abs(p2.transform.position.y - level.goalDark.y) < R)
        {
            currentLevel++;
            RestartLevel();
        }
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