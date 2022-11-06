using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialDisplay : MonoBehaviour
{
    public int targetLevel;
    private LevelManager manager;
    [SerializeField] AudioSource souce;


    private void Awake()
    {

        manager = FindObjectOfType<LevelManager>(); 
        manager.OnLevelCompleted += () => {
            this.gameObject.SetActive(false);
        };
        manager.OnLevelLoaded += ()=> {
            this.gameObject.SetActive(manager.CurrentLevel == targetLevel && !manager.IsCustomLevel);
            if (souce && manager.CurrentLevel == targetLevel && !manager.IsCustomLevel)
                souce.Play();
        };
    }
}