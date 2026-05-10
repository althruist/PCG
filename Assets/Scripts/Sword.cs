using UnityEngine;

public class Sword : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        Entity entity = collision.transform.GetComponent<Entity>();

        if (entity != null)
        {
            entity.TakeDamage(10);
            if (entity.health == 0)
            {
                FindFirstObjectByType<Player>().kills++;
            }
        }
    }
}
