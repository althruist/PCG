using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : Entity
{
    private DungeonGenerator dungeonGenerator;
    public int kills = 0;
    public int collectables = 0;
    private bool isAttacking = false;
    public GameObject sword;
    [SerializeField] private Tilemap liquidTilemap;
    [SerializeField] private Tilemap trapsTilemap;
    private bool onPressurePlate = false;

    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float arrowSpeed = 8f;

    private Rigidbody2D rb;
    private Vector2 movement;

    public override void TakeDamage(int Damage)
    {
        base.TakeDamage(Damage);
        Debug.Log("take damage");
    }

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
        if (isDead) return;
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        CheckCurrentTile();

        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(SwordSwing());
        }

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

    IEnumerator SwordSwing()
    {
        if (isAttacking) yield break;
        isAttacking = true;
        GameObject swordInstance = Instantiate(sword, transform.position, Quaternion.identity);

        float duration = 0.3f;
        float elapsed = 0f;

        float radius = 1.2f;
        animator.SetTrigger("Attack");

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float angle = (elapsed / duration) * 360f;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;

            swordInstance.transform.position = transform.position + offset;

            yield return null;
        }

        Destroy(swordInstance);
        isAttacking = false;
    }

    void SpawnArrow()
    {
        Vector3 spawnPos = transform.position + Vector3.up * 5f;

        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);

        Vector3 direction = (transform.position - spawnPos).normalized;

        arrow.GetComponent<Rigidbody2D>().linearVelocity = direction * arrowSpeed;
    }

    IEnumerator TriggerArrows()
    {
        int arrowCount = 5;

        for (int i = 0; i < arrowCount; i++)
        {
            SpawnArrow();
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void CheckCurrentTile()
    {
        Vector3Int tilePos = liquidTilemap.WorldToCell(transform.position);
        Vector3Int tTilePos = trapsTilemap.WorldToCell(transform.position);
        TileBase currentTile = liquidTilemap.GetTile(tilePos);
        TileBase tCurrentTile = trapsTilemap.GetTile(tTilePos);

        if (tCurrentTile != null)
        {
            if (tCurrentTile.name == "PoisonFlower")
            {
                TakeDamage(4);
            }
            else if (tCurrentTile.name == "Spikes")
            {
                TakeDamage(15);
            }
            else if (tCurrentTile.name == "PressurePlate")
            {
                if (!onPressurePlate)
                {
                    onPressurePlate = true;
                    StartCoroutine(TriggerArrows());
                }
            }
        }
        else
        {
            onPressurePlate = false;
        }

        if (currentTile != null)
        {
            if (currentTile.name.Contains("Water"))
            {
                moveSpeed = 1;
            }
            else if (currentTile.name.Contains("Lava"))
            {
                TakeDamage(5);
            }
        }
        else
        {
            moveSpeed = 3;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movement * moveSpeed;
    }
}
