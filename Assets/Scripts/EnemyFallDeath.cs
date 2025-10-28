using UnityEngine;

public class EnemyFallDeath : MonoBehaviour
{
    public float deathY = -10f;

    void Update()
    {
        if (transform.position.y < deathY)
        {
            Destroy(gameObject); // Spawner list will drop the null and refill
        }
    }
}
