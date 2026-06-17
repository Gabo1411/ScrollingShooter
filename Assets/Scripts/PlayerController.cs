using UnityEngine;

/// <summary>
/// Handles player movement (WASD/arrows), shooting (Fire1/Jump),
/// damage with temporary invincibility, and blinking visual feedback.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float fireRate = 0.15f;

    private float fireTimer;
    private float invincibilityTimer;
    private const float InvincibilityDuration = 2f;
    private bool isInvincible;
    private SpriteRenderer spriteRenderer;
    private float blinkTimer;

    // Play area bounds
    private const float MinX = -5.5f;
    private const float MaxX = 5.5f;
    private const float MinY = -9f;
    private const float MaxY = 9f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            // Allow restart with R key during game over
            if (Input.GetKeyDown(KeyCode.R))
            {
                GameManager.Instance.RestartGame();
            }
            return;
        }

        HandleMovement();
        HandleShooting();
        HandleInvincibility();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, v, 0f) * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);

        // Clamp within play area
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, MinX, MaxX);
        pos.y = Mathf.Clamp(pos.y, MinY, MaxY);
        transform.position = pos;
    }

    private void HandleShooting()
    {
        fireTimer -= Time.deltaTime;

        if ((Input.GetButton("Fire1") || Input.GetButton("Jump")) && fireTimer <= 0f)
        {
            fireTimer = fireRate;
            SpawnBullet();
        }
    }

    private void SpawnBullet()
    {
        GameObject bulletObj = new GameObject("PlayerBullet");
        bulletObj.transform.position = transform.position + Vector3.up * 0.6f;
        bulletObj.tag = "PlayerBullet";

        SpriteRenderer sr = bulletObj.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateBullet(new Color(0f, 1f, 1f, 1f));
        sr.sortingOrder = 5;

        BoxCollider2D col = bulletObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.3f, 0.6f);

        // Kinematic RB2D needed for trigger detection
        Rigidbody2D rb = bulletObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        Bullet bullet = bulletObj.AddComponent<Bullet>();
        bullet.speed = 14f;
        bullet.direction = Vector2.up;
        bullet.isPlayerBullet = true;
    }

    private void HandleInvincibility()
    {
        if (!isInvincible) return;

        invincibilityTimer -= Time.deltaTime;
        blinkTimer -= Time.deltaTime;

        // Blink the sprite for visual feedback
        if (blinkTimer <= 0f)
        {
            blinkTimer = 0.1f;
            spriteRenderer.enabled = !spriteRenderer.enabled;
        }

        if (invincibilityTimer <= 0f)
        {
            isInvincible = false;
            spriteRenderer.enabled = true;
        }
    }

    /// <summary>Called when hit by an enemy or enemy bullet.</summary>
    public void TakeDamage()
    {
        if (isInvincible) return;

        if (GameManager.Instance != null)
            GameManager.Instance.LoseLife();

        // Become temporarily invincible
        isInvincible = true;
        invincibilityTimer = InvincibilityDuration;
        blinkTimer = 0.1f;

        // Visual feedback explosion
        ExplosionEffect.Spawn(transform.position, new Color(0f, 1f, 1f, 1f));
    }

    /// <summary>Resets the player to starting position with brief invincibility.</summary>
    public void ResetPlayer()
    {
        transform.position = new Vector3(0f, -7f, 0f);
        isInvincible = true;
        invincibilityTimer = InvincibilityDuration;
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
    }
}
