using UnityEngine;

public class Enemy : Entity
{
    public enum enemyType { Patrol, Chaser }
    public enemyType enemy;

    public int damage = 10;
    public float chaseRange = 5f;

    private int direction = 1;

    private Rigidbody2D rb;
    private Transform player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        Player p = FindFirstObjectByType<Player>();

        if (p != null)
        {
            player = p.transform;
        }
    }

    void Update()
    {
        if (enemy == enemyType.Patrol)
        {
            Patrol();
        }
        else if (enemy == enemyType.Chaser)
        {
            Chaser();
        }
    }

    void Patrol()
    {
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        // Flip sprite
        if (direction == 1)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void Chaser()
    {
        if (player == null)
            return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= chaseRange)
        {
            Vector2 dir = (player.position - transform.position).normalized;

            rb.linearVelocity = dir * moveSpeed;

            if (dir.x > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else if (dir.x < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (
            enemy == enemyType.Patrol &&
            collision.gameObject.name != "Sword(Clone)" &&
            collision.gameObject.name != "Player"
        )
        {
            direction *= -1;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().TakeDamage(damage);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().TakeDamage(damage);
        }
        else if (collision.gameObject.name.Contains("Sword"))
        {
            TakeDamage(10);
            if (health <= 0)
            {
                FindFirstObjectByType<Player>().kills++;
            }
        }
    }
}