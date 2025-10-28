using UnityEngine;
public class LevelEndCollectible : MonoBehaviour
{
    public LevelAdvanceUI advanceUI;
    public bool isFinalLevelCollectible = false;

    private GameController gameController;

    private void Awake()
    {
        // Attempt to auto-assign references if not set in Inspector
        if (advanceUI == null)
            advanceUI = FindObjectOfType<LevelAdvanceUI>(includeInactive: true);

        gameController = FindObjectOfType<GameController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only trigger when the player touches the collectible
        if (!other.CompareTag("Player")) return;

        // If this collectible belongs to the final level, end the game
        if (isFinalLevelCollectible && gameController != null)
        {
            gameController.ShowGameCompletedScreen();
        }
        else
        {
            // Otherwise, show the "Next Level?" popup UI
            if (advanceUI != null)
                advanceUI.Show();
        }

        // Disable this collectible so it can't be triggered again
        gameObject.SetActive(false);
    }
}
