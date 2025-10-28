using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;   // Prefab of the bullet to instantiate
    public float bulletSpeed = 50f;   // Speed at which the bullet travels

    private void Update()
    {
        // Left mouse click triggers shooting
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        SoundEffectManager.Play("Bullet");
        // Get the mouse position in world space
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 shootDir = (mousePos - transform.position).normalized;

        // Instantiate the bullet at player position
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // Rotate the bullet so that its back faces the player
        float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle + 180f);
        // Remove "+180f" if your sprite already faces right and you want its front toward the target

        // Apply velocity to the bullet's Rigidbody2D
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = shootDir * bulletSpeed;
        }

        // Automatically destroy bullet after 2 seconds to clean up
        Destroy(bullet, 2f);
    }
}
