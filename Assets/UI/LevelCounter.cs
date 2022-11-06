using UnityEngine;
using TMPro;

public class LevelCounter : MonoBehaviour
{
    LevelManager manager;
    TextMeshProUGUI label;

    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
        manager = FindObjectOfType<LevelManager>();
        manager.OnLevelLoaded += UpdateDisplay;
    }

    // Update is called once per frame
    void UpdateDisplay()
    {
        label.text = $"{manager.CurrentLevel}/12";
    }
}
