using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int bulletDamage = 1;
    public float speed = 10f;

    private Vector2 direction;

    // Called right after Instantiate to set direction and rotate sprite
    public void Initialize(Vector2 shootDirection)
    {
        direction = shootDirection.normalized;

        // Set the rotation so that the bullet's "forward" faces the travel direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply rotation — flip by 180° so the sprite's back faces the player
        transform.rotation = Quaternion.Euler(0, 0, angle + 180f);
    }

    void Update()
    {
        // Move in its set direction
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy)
        {
            enemy.TakeDamage(bulletDamage);
            Destroy(gameObject);
        }
    }
}
