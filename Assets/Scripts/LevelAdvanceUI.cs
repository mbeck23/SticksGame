using UnityEngine;

public class LevelAdvanceUI : MonoBehaviour
{
    public GameObject panel;

    void Awake()
    {
        if (panel) panel.SetActive(false);
    }

    public void Show()
    {
        if (panel) panel.SetActive(true);
        Time.timeScale = 0f; // Pause while asking
    }

    public void Hide()
    {
        if (panel) panel.SetActive(false);
        Time.timeScale = 1f;
    }

    // Hook to Yes button
    public void OnYes()
    {
        Hide();
        var gc = FindObjectOfType<GameController>();
        if (gc != null) gc.LoadNextLevel(); 
    }

    // Hook to No button
    public void OnNo()
    {
        Hide();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
