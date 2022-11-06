using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Menu : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private GameObject mainContainer;
    [SerializeField] private Button startGame;
    [SerializeField] private Button lvlEditor;

    [Header("Editor")]
    [SerializeField] private GameObject editorContainer;
    [SerializeField] private Button startCustomGame;
    [SerializeField] private Button exitLevelEditor;
    [SerializeField] private TMP_InputField editor;

    [Header("Ingame")]
    [SerializeField] private GameObject ingameContainer;

    LevelManager _lvlManager;

    private void Awake()
    {
        OpenMenu();
        _lvlManager = FindObjectOfType<LevelManager>();
        startGame.onClick.AddListener(() => {
            mainContainer.SetActive(false);
            ingameContainer.SetActive(true);
            _lvlManager.StartGame();
        });
        lvlEditor.onClick.AddListener(() => {
            mainContainer.SetActive(false);
            editorContainer.SetActive(true);
        });

        startCustomGame.onClick.AddListener(()=> {
            editorContainer.SetActive(false);
            _lvlManager.PlayCustomLevel(editor.text);
        });
        exitLevelEditor.onClick.AddListener(() => {
            mainContainer.SetActive(true);
            editorContainer.SetActive(false);
        });
    }


    public void OpenMenu()
    {

        mainContainer.SetActive(true);
        editorContainer.SetActive(false);
        ingameContainer.SetActive(false);
    }
}
