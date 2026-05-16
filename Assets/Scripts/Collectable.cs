using UnityEngine;

public class Collectable : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().collectables++;
            collision.GetComponent<Player>().health += 10;

            if (collision.GetComponent<Player>().health > 100)
            {
            collision.GetComponent<Player>().health = 100; 
            }
            Destroy(gameObject);
        }
    }
}
