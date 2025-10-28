using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("Heart Settings")]
    public Image heartPrefab;           // Prefab for each heart image
    public Sprite fullHeartSprite;      // Sprite for a filled (active) heart
    public Sprite emptyHeartSprite;     // Sprite for an empty (lost health) heart

    private List<Image> hearts = new List<Image>();  // List of instantiated heart images

    public void SetMaxHearts(int maxHearts)
    {
        // Remove any existing hearts
        foreach (Image heart in hearts)
        {
            Destroy(heart.gameObject);
        }

        hearts.Clear();

        // Create new hearts up to the max count
        for (int i = 0; i < maxHearts; i++)
        {
            Image newHeart = Instantiate(heartPrefab, transform);
            newHeart.sprite = fullHeartSprite;
            newHeart.color = Color.red;
            hearts.Add(newHeart);
        }
    }

    public void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            if (i < currentHealth)
            {
                // Heart is active
                hearts[i].sprite = fullHeartSprite;
                hearts[i].color = Color.red;
            }
            else
            {
                // Heart is empty
                hearts[i].sprite = emptyHeartSprite;
                hearts[i].color = Color.white;
            }
        }
    }
}
