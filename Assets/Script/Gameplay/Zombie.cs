using System;
using ArcadeVP;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    private static readonly int Attack = Animator.StringToHash("Attack");
    private ArcadeVehicleController ObjectToAttack;
    private ArcadeVehicleController ObjectToFlip;

    [Header("Health")] public float health;
    public float maxHealth;

    [Header("Attack")] public float damage = 10f;
    public float attackSpeed = 1f; // attacks per second
    private float attackTimer;

    [SerializeField] private Animator animator;

    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Update()
    {
        
        
        
        TryFlip();
        
        // Always tick the timer
        attackTimer += Time.deltaTime;

        // Safety check before attacking
        if (ObjectToAttack == null)
            return;

        TryAttack();


    }

    private void TryFlip()
    {


        if (ObjectToFlip == null)
        {
            ObjectToFlip = GameManager.Instance.player;
            return;
        }

        // Player is on the right
        if (ObjectToFlip.transform.position.x > transform.position.x)
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
        if (ObjectToAttack == null)
            return;

        animator.SetTrigger(Attack);
        ObjectToAttack.TakeDamage(damage);
        
        //Debug.Log("Zombie attacked the player!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        ObjectToAttack = other.GetComponent<ArcadeVehicleController>();
        attackTimer = attackSpeed; // allows instant attack on enter (optional)
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        ObjectToAttack = null;
    }
}
