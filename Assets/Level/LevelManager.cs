using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;
using System.Collections;
using System;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject lightBoxPrefab;
    [SerializeField] private GameObject darkBoxPrefab;
    [SerializeField] private GameObject lightGoalPrefab;
    [SerializeField] private GameObject darkGoalPrefab;
    [SerializeField] private GameObject emptyPrefab;
    [SerializeField] private Transform lightContainer;
    [SerializeField] private Transform darkContainer;
    [SerializeField] private Transform emptyContainer;

    [Header("Player")]
    [SerializeField] private GameObject p1;
    [SerializeField] private GameObject p2;

    [Header("Camera")]
    [SerializeField] private Transform minPosition;
    [SerializeField] private Transform maxPosition;

    public event Action OnLevelLoaded;
    public event Action OnLevelCompleted;

    public bool blockInput = true;

    private int currentLevel = 1;
    public int CurrentLevel => currentLevel;

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
                    if (instances[x, y] != null)
                    {
                        Destroy(instances[x, y]);
                    }
                }
            }
        }
        foreach (Transform child in emptyContainer)
            Destroy(child.gameObject);

        DOTween.KillAll();
    }

    public bool IsOccuipiedByOtherPlayer(GameObject player, int2 targetPosition)
    {
        GameObject p = p1 == player ? p2 : p1;
        float3 pos = p.transform.position;
        int2 otherPlayerPos = (int2)math.round(pos.xz);
        return math.all(targetPosition == otherPlayerPos);
    }

    public void LoadLevel(string fileName)
    {
        ClearLevel();
        blockInput = true;
        level = LevelUtils.LoadLevelDataFromFile(fileName);
        instances = new GameObject[level.Width, level.Height];
        for(int x = -1; x <= level.Width; x++)
        {
            for(int y=-1; y <= level.Height; y++)
            {
                GameObject prefab = null;
                Transform container = this.transform;
                switch (level[x,y].type)
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
                    case Tile.Type.Empty:
                        prefab = emptyPrefab;
                        container = emptyContainer;
                        break;
                }

                if(prefab)
                {
                    var go = Instantiate(prefab, container);
                    go.transform.localPosition = new Vector3(x, 0, y);
                    if(x >= 0 && y >= 0 && x < level.Width && y < level.Height)
                        instances[x, y] = go;
                }
            }
        }

        minPosition.transform.position = new Vector3(-0.5f, 0, -0.5f);
        maxPosition.transform.position = new Vector3(level.Width+0.5f, 0, level.Height+0.5f);

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
        PlayFadeInLevel();
        OnLevelLoaded?.Invoke();
    }

    public void PlayFadeInLevel()
    {
        blockInput = true;
        DOTween.KillAll(true);
        float offset = 3;
        float d = 0.1f;
        float t = 0.5f;

        Sequence s = DOTween.Sequence();
        for(int x = 0; x < level.Width; x++)
        for(int y = 0; y < level.Height; y++)
        {
            GameObject g = instances[x, y];
            if (!g) continue;
            float target = g.transform.localPosition.y;
            g.transform.localPosition += Vector3.down * offset;
            float delay = (x+y) * d;
            Sequence a = DOTween.Sequence();
            a.AppendInterval(delay);
            a.Append(g.transform.DOLocalMoveY(target, t));
            if(math.all(level.spawnLight == new int2(x, y)))
            {
                p1.transform.localPosition += Vector3.down * offset;
                a.Join(p1.transform.DOLocalMoveY(target, t));
            }
            if (math.all(level.spawnDark == new int2(x, y)))
            {
                p2.transform.localPosition += Vector3.down * offset;
                a.Join(p2.transform.DOLocalMoveY(target,t));
            }
            
            s.Join(a);
        }
        s.OnComplete(() =>
        {
            blockInput = false;
        });
        s.Play();
    }

    public void RestartLevel()
    {
        LoadLevel("lvl" + currentLevel);
    }

    private void Update()
    {
        if (blockInput)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
            level?.ToggleRaisedWorld();

        if (Input.GetKeyDown(KeyCode.R))
            RestartLevel();
        
        CheckWinCondition();
    }

    void CheckWinCondition()
    {
        if (!p1 || !p2)
            return;

        float R = 0.5f;
        if (Mathf.Abs(p1.transform.position.x - level.goalLight.x) < R &&
            Mathf.Abs(p1.transform.position.z - level.goalLight.y) < R &&
            Mathf.Abs(p2.transform.position.x - level.goalDark.x) < R &&
            Mathf.Abs(p2.transform.position.z - level.goalDark.y) < R)
        {
            blockInput = true;
            OnLevelCompleted?.Invoke();
            StartCoroutine(DelayedStartNextLevel(1.3f));
        }
    }

    private IEnumerator DelayedStartNextLevel(float delay)
    {
        UberAudio.AudioManager.Instance.Play("Victory");
        yield return new WaitForSeconds(delay);
        currentLevel++;
        RestartLevel();
    }

    void OnRaisedWorldChanged(World raisedWorld)
    {
        float duration = 0.2f;
        Sequence s = DOTween.Sequence();
        s.Join(darkContainer.transform.DOLocalMoveY(raisedWorld == World.Dark ? 1 : 0, duration));
        s.Join(lightContainer.transform.DOLocalMoveY(raisedWorld == World.Light ? 1 : 0, duration));
        s.Play();

        UberAudio.AudioManager.Instance.Play("SwitchWorld");
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
        UberAudio.AudioManager.Instance.Play("MoveTile");
    }
}