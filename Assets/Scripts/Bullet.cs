using UnityEngine;

/// <summary>
/// Generic projectile that moves in a fixed direction and handles
/// collision with enemies (player bullets) or the player (enemy bullets).
/// </summary>
public class Bullet : MonoBehaviour
{
    public float speed = 12f;
    public Vector2 direction = Vector2.up;
    public bool isPlayerBullet = true;

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Self-destruct when off screen
        Vector3 pos = transform.position;
        if (pos.y > 12f || pos.y < -12f || pos.x > 8f || pos.x < -8f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet && other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage(1);
            Destroy(gameObject);
        }
        else if (!isPlayerBullet && other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
                player.TakeDamage();
            Destroy(gameObject);
        }
    }
}
