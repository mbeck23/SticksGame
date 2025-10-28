using UnityEngine;

public class FallDeath : MonoBehaviour
{
    [Header("Death Zone Settings")]
    public float deathY = -10f;

    [Tooltip("Optional reference to PlayerHealth")]
    public PlayerHealth playerHealth;

    void Start()
    {
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (transform.position.y < deathY)
        {
            DieFromFall();
        }
    }

    private void DieFromFall()
    {
        Debug.Log("Player fell off the map!");
        if (playerHealth != null)
        {
            // Reduce all health instantly
            while (true)
            {
                playerHealth.SendMessage("TakeDamage", 9999, SendMessageOptions.DontRequireReceiver);
                break;
            }
        }
        else
        {
            // Fallback if PlayerHealth is missing
            Destroy(gameObject);
        }
    }
}
