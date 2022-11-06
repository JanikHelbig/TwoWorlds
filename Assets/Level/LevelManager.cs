using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;
using System.Collections;
using System;
using System.Linq;
using Utility;

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

    public event Action OnLevelLoaded;public event Action OnLevelCompleted;

    public bool blockInput = true;

    private string customLevel = null;
    private int currentLevel = 1;
    public int CurrentLevel => currentLevel;

    public Level level;
    private GameObject[,] instances;

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
    }

    public bool IsOccuipiedByOtherPlayer(GameObject player, int2 targetPosition)
    {
        GameObject p = p1 == player ? p2 : p1;
        float3 pos = p.transform.position;
        int2 otherPlayerPos = (int2)math.round(pos.xz);
        return math.all(targetPosition == otherPlayerPos);
    }

    private IEnumerator TransitionToLevel(Level lvl)
    {
        if (level != null)
            yield return FadeOutLevelRoutine();

        yield return LoadLevel(lvl);

        blockInput = false;
    }

    private IEnumerator LoadLevel(Level lvl)
    {
        ClearLevel();

        level = lvl;
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
        yield return FadeInLevelRoutine();
        OnLevelLoaded?.Invoke();
    }

    private IEnumerator FadeInLevelRoutine()
    {
        const float hideOffset = -5f;

        float[,] distances = CalculateDistancesOfLevel();

        for (var y = 0; y < level.Height; y++)
        for (var x = 0; x < level.Width; x++)
        {
            if (instances[x, y] is { } instance)
                instance.transform.localPosition = instance.transform.localPosition.With(y: hideOffset);
        }

        var t = 0f;
        var tMin = 0f;
        while (tMin < 1f)
        {
            tMin = float.MaxValue;

            for (var y = 0; y < level.Height; y++)
            for (var x = 0; x < level.Width; x++)
            {
                GameObject instance = instances[x, y];

                if (instance == null)
                    continue;

                float distance = distances[x, y];
                float tInstance = math.clamp(t * 2 - distance * 0.25f, 0, 1);
                tMin = math.min(tMin, tInstance);

                float tInstanceEased = EaseOutBack(tInstance);
                float yInstance = math.lerp(hideOffset, 0, tInstanceEased);

                instance.transform.localPosition = instance.transform.localPosition.With(y: yInstance);
            }

            t += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeOutLevelRoutine()
    {
        const float hideOffset = -5f;

        float[,] distances = CalculateDistancesOfLevel();

        var t = 0f;
        var tMin = 0f;
        while (tMin < 1f)
        {
            tMin = float.MaxValue;

            for (var y = 0; y < level.Height; y++)
            for (var x = 0; x < level.Width; x++)
            {
                GameObject instance = instances[x, y];

                if (instance == null)
                    continue;

                float distance = distances[x, y];
                float tInstance = math.clamp(t * 2 - distance * 0.25f, 0, 1);
                tMin = math.min(tMin, tInstance);

                float tInstanceEased = EaseInQuad(tInstance);
                float yInstance = math.lerp(0, hideOffset, tInstanceEased);

                instance.transform.localPosition = instance.transform.localPosition.With(y: yInstance);
            }

            t += Time.deltaTime;
            yield return null;
        }
    }

    private float[,] CalculateDistancesOfLevel()
    {
        var distances = new float[level.Width, level.Height];
        float2[] focusPoints = level.goalsLight.Concat(level.goalsDark)
            .Select(x => (float2) x)
            .ToArray();

        for (var y = 0; y < level.Height; y++)
        for (var x = 0; x < level.Width; x++)
        {
            var distance = float.MaxValue;
            foreach (float2 focusPoint in focusPoints)
            {
                float distanceToFocusPoint = math.distance(new float2(x, y), focusPoint);
                distance = SmoothMin(distance, distanceToFocusPoint, 1);
            }
            distances[x, y] = distance;
        }

        return distances;
    }

    private static float SmoothMin(float a, float b, float k)
    {
        float h = math.max(k - math.abs(a - b), 0) / k;
        return math.min(a, b) - h * h * k * (1.0f / 4.0f);
    }

    private float EaseInQuad(float x) {
        return x * x;
    }

    private static float EaseOutBack(float x) {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;

        return 1 + c3 * math.pow(x - 1, 3) + c1 * math.pow(x - 1, 2);
    }

    public void PlayFadeInLevel()
    {
        blockInput = true;
        DOTween.KillAll(true);
        float offset = 6f;
        float initDelay = 0.2f;
        float d = 0.1f;
        float t = 0.6f;

        Sequence s = DOTween.Sequence();
        for(int x = 0; x < level.Width; x++)
        for(int y = 0; y < level.Height; y++)
        {
            GameObject g = instances[x, y];
            if (!g) continue;
            float target = g.transform.localPosition.y;
            g.transform.localPosition += Vector3.down * offset;
            float delay = initDelay+ (x+y) * d;
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
        if (customLevel != null)
        {
            var lines = customLevel.Split("\n");
            Level lvl = LevelUtils.LoadLevelFromText(lines);
            blockInput = true;
            StartCoroutine(TransitionToLevel(lvl));
        }
        else
        {
            blockInput = true;
            Level lvl = LevelUtils.LoadLevelDataFromFile("lvl" + currentLevel);
            StartCoroutine(TransitionToLevel(lvl));
        }
    }

    public void PlayCustomLevel(string customLevel)
    {
        this.customLevel = customLevel;
        RestartLevel();
    }

    public void StartGame()
    {
        customLevel = null;
        RestartLevel();
    }

    private void Update()
    {
        if (blockInput)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
            level?.ToggleRaisedWorld();

        if (Input.GetKeyDown(KeyCode.R))
            RestartLevel();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            FindObjectOfType<Menu>().OpenMenu();
        }

        CheckWinCondition();
    }

    void CheckWinCondition()
    {
        if (!p1 || !p2 || level == null)
            return;

        float R = 0.5f;
        bool isP1OnGoal = false;
        bool isP2OnGoal = false;

        foreach(int2 goalPos in level.goalsLight)
        {
            if (Mathf.Abs(p1.transform.position.x - goalPos.x) < R &&
                Mathf.Abs(p1.transform.position.z - goalPos.y) < R)
                isP1OnGoal = true;
        }
        foreach (int2 goalPos in level.goalsDark)
        {
            if (Mathf.Abs(p2.transform.position.x - goalPos.x) < R &&
                Mathf.Abs(p2.transform.position.z - goalPos.y) < R)
                isP2OnGoal = true;
        }

        if (isP1OnGoal && isP2OnGoal)
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
        if (customLevel == null)
            currentLevel++;
        RestartLevel();
    }

    void OnRaisedWorldChanged(World raisedWorld)
    {
        blockInput = true;
        float duration = 0.2f;
        Sequence s = DOTween.Sequence();
        s.AppendCallback(()=> {
            GameObject p = raisedWorld == World.Dark ? p1 : p2;
            p.GetComponentInChildren<Animator>().SetTrigger("Jump");
        });
        s.AppendInterval(0.6f);
        s.Append(darkContainer.transform.DOLocalMoveY(raisedWorld == World.Dark ? 1 : 0, duration));
        s.Join(lightContainer.transform.DOLocalMoveY(raisedWorld == World.Light ? 1 : 0, duration));
        s.AppendCallback(() => { blockInput = false; });
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
        g1.transform.localScale = new Vector3(1.01f, 1, 1.01f);
        s.Append(g1.transform.DOLocalMove(new Vector3(p2.x, 0, p2.y), 0.3f));
        s.OnComplete(() => {
            g2.transform.localPosition = new Vector3(p1.x, 0, p1.y);
            g1.transform.localScale = Vector3.one;
            Destroy(temp);
        });
        s.Play();
        UberAudio.AudioManager.Instance.Play("MoveTile");
    }
}