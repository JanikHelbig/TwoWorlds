using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialDisplay : MonoBehaviour
{
    public int targetLevel;
    private LevelManager manager;
    private SpriteRenderer[] sprites;
    [SerializeField] AudioSource souce;


    private void Awake()
    {
        bool visible = false; ;
        manager = FindObjectOfType<LevelManager>();
        sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (var s in sprites)
            s.color = new Color(1, 1, 1, 0);

        manager.OnLevelCompleted += () => {
            if (!visible)
                return;
            foreach (var s in sprites)
                s.DOFade(0, 0.3f).Play();
        };

        manager.OnLevelLoaded += ()=> {
            if (souce && manager.CurrentLevel == targetLevel && !manager.IsCustomLevel)
            {
                foreach (var s in sprites)
                    s.DOFade(1, 0.3f).Play();

                if (!souce.isPlaying)
                    souce.Play();
                visible = true;
            }
        };
    }
}
