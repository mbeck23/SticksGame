using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;         // Base maximum health (may be increased by abilities)
    private int currentHealth;        // Runtime health value

    [Header("UI")]
    public HealthUI healthUI;         // Reference to heart UI

    [SerializeField] private SpriteRenderer spriteRenderer;

    public static event Action OnPlayerDied;  // Fired when health reaches zero

    private void Awake()
    {
        // Auto-assign a SpriteRenderer found in children if not already set
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        // Read selected ability and apply health bonus if EnhancedHealth is chosen (ability == 1)
        int ability = PlayerPrefs.GetInt("ability", 0);
        if (ability == 1) // 1 = EnhancedHealth
        {
            maxHealth += 4; // Adds two extra hearts
        }

        // Initialize health & UI; subscribe to reset events
        ResetHealth();
        GameController.OnReset += ResetHealth;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If collided with an enemy, take enemy damage and kill that enemy after attack
        Enemy enemy = collision.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            TakeDamage(enemy.damage);
            SoundEffectManager.Play("Enemy");
            enemy.DieAfterAttack();
        }

        // If collided with a trap, apply its damage
        Trap trap = collision.GetComponent<Trap>();
        if (trap && trap.damage > 0)
        {
            TakeDamage(trap.damage);
        }
    }

    private void ResetHealth()
    {
        currentHealth = maxHealth;
        healthUI.SetMaxHearts(maxHealth);
    }

    private void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthUI.UpdateHearts(currentHealth);

        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            // Player is dead
            OnPlayerDied.Invoke();
        }
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = Color.white;
    }
}
