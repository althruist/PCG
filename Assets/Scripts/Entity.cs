using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public int health = 100;
    private bool isTakingDamage = false;
    public Animator animator;
    public bool isDead;
    public float moveSpeed = 3f;

    public void DestroyOnDeath()
    {
        Destroy(gameObject);
    }

    IEnumerator DefaultDamage(int Damage)
    {
        if (isTakingDamage || isDead) yield break;
        isTakingDamage = true;
        animator.SetTrigger("Hurt");

        health -= Damage;

        if (health <= 0)
        {
            isDead = true;
            moveSpeed = 0;
            animator.SetTrigger("Dead");
        }
        yield return new WaitForSeconds(0.5f);
        isTakingDamage = false;
    }

    public virtual void TakeDamage(int Damage)
    {
        StartCoroutine(DefaultDamage(Damage));
    }
}
