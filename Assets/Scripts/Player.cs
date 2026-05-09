using UnityEngine;

public class Player : MonoBehaviour
{
    private DungeonGenerator dungeonGenerator;


    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dungeonGenerator = FindFirstObjectByType<DungeonGenerator>();

        Vector2Int spawnPos = dungeonGenerator.SpawnRoomCenter;
        transform.position = new Vector3(spawnPos.x, spawnPos.y, 0);
    }

    private void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        movement = movement.normalized;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movement * moveSpeed;
    }
}
