using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public float bounceForce = 10f;
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerBounce(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            // If an enemy touches the trap, destroy or bounce it away
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                rb.AddForce(Vector2.up * (bounceForce / 2f), ForceMode2D.Impulse);
            }
        }
    }


    private void HandlePlayerBounce(GameObject player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb)
        {
            // Reset player velocity
            rb.velocity = new Vector2(rb.velocity.x, 0f);

            // Apply bounce force
            rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
            SoundEffectManager.Play("BounceTrap");
        }
    }
}
