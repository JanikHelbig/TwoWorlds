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

            if (manager.CurrentLevel == targetLevel && !manager.IsCustomLevel)
            {
                visible = true;
                foreach (var s in sprites)
                    s.DOFade(1, 0.3f).Play();

                if (souce && !souce.isPlaying)
                    souce.Play();
            }
        };
    }
}
