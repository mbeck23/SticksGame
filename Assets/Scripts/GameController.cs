using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("UI")]
    public GameObject gameOverScreen;          // Shown when the player dies
    public GameObject gameCompletedScreen;     // Shown after last level is completed
    public TMP_Text survivedText;              // Displays "YOU SURVIVED X LEVEL(S)"
    public GameObject LoadCanvas;              // Optional loading overlay

    [Header("Gameplay")]
    public GameObject player;                  // Player to reposition on level load
    public List<GameObject> levels;            // Ordered list of level roots

    private int currentLevelIndex = 0;         // Index of the active level in 'levels'
    private int survivedLevelsCount;           // Count of levels successfully completed

    public static event Action OnReset;        // Fired on any reset (soft or to first level)
    public static event Action OnLevelChanged; // Fired whenever the active level changes

    // ------------------------------------------------------------------------

    void Start()
    {
        // Subscribe to player death -> show Game Over
        PlayerHealth.OnPlayerDied += GameOverScreen;

        // Ensure UI is hidden at start
        if (gameOverScreen) gameOverScreen.SetActive(false);
        if (gameCompletedScreen) gameCompletedScreen.SetActive(false);
        if (LoadCanvas) LoadCanvas.SetActive(false);

        // Detect which level is currently active at startup and sync index
        // (Assumes exactly one active level object at start)
        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i] != null && levels[i].activeSelf)
            {
                currentLevelIndex = i;
                break;
            }
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to avoid dangling handlers
        PlayerHealth.OnPlayerDied -= GameOverScreen;
    }

    void GameOverScreen()
    {
        if (gameOverScreen) gameOverScreen.SetActive(true);

        // Pause the game
        Time.timeScale = 0f;

        // Update the "YOU SURVIVED X LEVEL(S)" text
        if (survivedText)
        {
            survivedText.text = "YOU SURVIVED " + survivedLevelsCount + " LEVEL";
            if (survivedLevelsCount != 1)
                survivedText.text += "S";
        }
    }

    public void ResetGame()
    {
        // Resume time and hide Game Over UI
        Time.timeScale = 1f;
        if (gameOverScreen) gameOverScreen.SetActive(false);

        // Reload current level without increasing survived count
        loadLevel(currentLevelIndex, false);

        // Notify listeners that a reset occurred
        OnReset?.Invoke();
    }

    public void ResetToFirstLevel()
    {
        Time.timeScale = 1f;

        // Hide UI
        if (gameOverScreen) gameOverScreen.SetActive(false);
        if (gameCompletedScreen) gameCompletedScreen.SetActive(false);

        // Reset progress and load level 0 (no survive increment)
        survivedLevelsCount = 0;
        loadLevel(0, false);

        // Notify listeners that a reset occurred
        OnReset?.Invoke();
    }

    public void loadLevel(int level, bool wantSurvivedIncrease)
    {
        // Hide loading canvas if present
        if (LoadCanvas) LoadCanvas.SetActive(false);

        // Guard against missing level list
        if (levels == null || levels.Count == 0) return;

        // Clamp target index
        level = Mathf.Clamp(level, 0, levels.Count - 1);

        // Deactivate current level and activate target level (null-safe)
        if (levels[currentLevelIndex]) levels[currentLevelIndex].SetActive(false);
        if (levels[level]) levels[level].SetActive(true);

        // Reposition player to a known spawn for each level load (null-safe)
        if (player) player.transform.position = new Vector3(-4.57f, -3.34f, 0f);

        // Update index and notify listeners
        currentLevelIndex = level;
        OnLevelChanged?.Invoke();
        OnReset?.Invoke();

        // Optionally increment survived level count
        if (wantSurvivedIncrease) survivedLevelsCount++;
    }

    public void LoadNextLevel()
    {
        if (levels == null || levels.Count == 0) return;

        int nextLevelIndex = currentLevelIndex + 1;

        // If we've finished the last level, show the completion screen
        if (nextLevelIndex >= levels.Count)
        {
            ShowGameCompletedScreen();
            return;
        }

        // Load next level and count it as survived
        loadLevel(nextLevelIndex, true);
    }

    public void ShowGameCompletedScreen()
    {
        if (gameCompletedScreen) gameCompletedScreen.SetActive(true);
        Time.timeScale = 0f; // Pause game
    }
}
