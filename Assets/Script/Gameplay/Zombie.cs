using System;
using ArcadeVP;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    private ArcadeVehicleController player;

    [Header("Health")] public float health;
    public float maxHealth;

    [Header("Attack")] public float damage = 10f;
    public float attackSpeed = 1f; // attacks per second
    private float attackTimer;

    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Update()
    {
        
        TryFlip();
        
        // Always tick the timer
        attackTimer += Time.deltaTime;

        // Safety check before attacking
        if (player == null)
            return;

        TryAttack();


    }

    private void TryFlip()
    {
        var player = GameManager.Instance.player;
        if (player == null) return;

        // Player is on the right
        if (player.transform.position.x > transform.position.x)
            spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;
    }

private void TryAttack()
    {
        if (attackTimer < attackSpeed)
            return;

        attackTimer = 0f;

        AttackPlayer();
    }

    private void AttackPlayer()
    {
        if (player == null)
            return;

        player.TakeDamage(damage);
        
        //Debug.Log("Zombie attacked the player!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        player = other.GetComponent<ArcadeVehicleController>();
        attackTimer = attackSpeed; // allows instant attack on enter (optional)
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        player = null;
    }
}
