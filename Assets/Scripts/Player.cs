using UnityEngine;

public class Player : MonoBehaviour
{
    private DungeonGenerator dungeonGenerator;


    [SerializeField] private float moveSpeed = 3f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private SpriteRenderer sprite;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        dungeonGenerator = FindFirstObjectByType<DungeonGenerator>();

        Vector2Int spawnPos = dungeonGenerator.SpawnRoomCenter;
        transform.position = new Vector3(spawnPos.x + 0.5f, spawnPos.y + 0.5f, 0);
    }

    private void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        movement = movement.normalized;
        if (movement == new Vector2(0, 0))
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsWalkingUp", false);
            animator.SetBool("IsWalkingDown", false);
        }
        else
        {
            if (movement == new Vector2(-1, 0))
            {
                transform.localScale = new Vector3(-1, 1, 1);
                animator.SetBool("IsWalking", true);
                animator.SetBool("IsWalkingUp", false);
                animator.SetBool("IsWalkingDown", false);
            }
            else if (movement == new Vector2(1, 0))
            {
                transform.localScale = new Vector3(1, 1, 1);
                animator.SetBool("IsWalking", true);
                animator.SetBool("IsWalkingUp", false);
                animator.SetBool("IsWalkingDown", false);

            }
            else if (movement == new Vector2(0, 1))
            {
                transform.localScale = new Vector3(1, 1, 1);
                animator.SetBool("IsWalkingUp", true);
                animator.SetBool("IsWalkingDown", false);
                animator.SetBool("IsWalking", false);
            }
            else if (movement == new Vector2(0, -1))
            {
                transform.localScale = new Vector3(1, 1, 1);
                animator.SetBool("IsWalkingDown", true);
                animator.SetBool("IsWalkingUp", false);
                animator.SetBool("IsWalking", false);
            }
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movement * moveSpeed;
    }
}
