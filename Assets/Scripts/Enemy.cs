using UnityEngine;
using System.Collections;

/// <summary>
/// Defines enemy types and their movement/behavior patterns.
/// Straight: moves straight down. Zigzag: oscillates horizontally.
/// Shooter: moves slowly and fires bullets at intervals.
/// </summary>
public enum EnemyType { Straight, Zigzag, Shooter }

public class Enemy : MonoBehaviour
{
    public EnemyType enemyType = EnemyType.Straight;
    public int hp = 1;
    public int scoreValue = 100;
    public float moveSpeed = 3f;

    // Zigzag parameters
    private float zigzagTimer;
    private float zigzagFrequency = 2f;
    private float zigzagAmplitude = 3f;
    private float startX;

    // Shooter parameters
    private float shootTimer;
    private float shootInterval = 2f;

    void Start()
    {
        startX = transform.position.x;
        shootTimer = Random.Range(0.5f, shootInterval);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        Vector3 pos = transform.position;

        // All enemies move downward
        pos.y -= moveSpeed * Time.deltaTime;

        // Zigzag: sinusoidal horizontal oscillation
        if (enemyType == EnemyType.Zigzag)
        {
            zigzagTimer += Time.deltaTime;
            pos.x = startX + Mathf.Sin(zigzagTimer * zigzagFrequency) * zigzagAmplitude;
        }

        transform.position = pos;

        // Shooter: periodically fire a bullet downward
        if (enemyType == EnemyType.Shooter)
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                shootTimer = shootInterval;
                FireBullet();
            }
        }

        // Self-destruct when past bottom of screen
        if (pos.y < -12f)
        {
            Destroy(gameObject);
        }
    }

    private void FireBullet()
    {
        GameObject bulletObj = new GameObject("EnemyBullet");
        bulletObj.transform.position = transform.position + Vector3.down * 0.5f;
        bulletObj.tag = "EnemyBullet";

        SpriteRenderer sr = bulletObj.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateBullet(new Color(1f, 0.3f, 0.3f, 1f));
        sr.sortingOrder = 5;

        BoxCollider2D col = bulletObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.3f, 0.6f);

        // Kinematic RB2D for trigger detection
        Rigidbody2D rb = bulletObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        Bullet bullet = bulletObj.AddComponent<Bullet>();
        bullet.speed = 6f;
        bullet.direction = Vector2.down;
        bullet.isPlayerBullet = false;
    }

    /// <summary>Applies damage. Destroys the enemy and awards score when HP reaches 0.</summary>
    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.AddScore(scoreValue);

            // Spawn explosion with the enemy's visual color
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            Color explosionColor = sr != null ? sr.color : Color.white;
            ExplosionEffect.Spawn(transform.position, explosionColor);

            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(FlashWhite());
        }
    }

    /// <summary>Brief white flash to indicate damage taken but not destroyed.</summary>
    private IEnumerator FlashWhite()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color originalColor = sr.color;
        sr.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        if (sr != null)
            sr.color = originalColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
                player.TakeDamage();

            // Enemy also dies on contact with player
            TakeDamage(hp);
        }
    }
}
