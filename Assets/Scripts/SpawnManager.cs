using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns enemies in waves with progressive difficulty scaling.
/// Spawn interval decreases over time, and harder enemy types
/// become available as the player's score increases.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    private float spawnInterval = 1.5f;
    private const float MinSpawnInterval = 0.4f;
    private float difficultyTimer;
    private const float DifficultyInterval = 10f;
    private bool isSpawning;
    private Coroutine spawnCoroutine;

    /// <summary>Begins the enemy spawn loop.</summary>
    public void StartSpawning()
    {
        isSpawning = true;
        difficultyTimer = 0f;
        spawnInterval = 1.5f;
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    /// <summary>Halts enemy spawning (e.g. on game over).</summary>
    public void StopSpawning()
    {
        isSpawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(1f); // Brief initial delay

        while (isSpawning)
        {
            SpawnEnemy();

            // Gradually increase difficulty
            difficultyTimer += spawnInterval;
            if (difficultyTimer >= DifficultyInterval)
            {
                difficultyTimer = 0f;
                spawnInterval = Mathf.Max(MinSpawnInterval, spawnInterval - 0.1f);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        float x = Random.Range(-5f, 5f);
        float y = 11f;

        // Enemy type distribution scales with score
        EnemyType type;
        int score = GameManager.Instance != null ? GameManager.Instance.Score : 0;

        if (score < 500)
        {
            type = EnemyType.Straight;
        }
        else if (score < 1500)
        {
            type = Random.value < 0.6f ? EnemyType.Straight : EnemyType.Zigzag;
        }
        else
        {
            float r = Random.value;
            if (r < 0.4f) type = EnemyType.Straight;
            else if (r < 0.75f) type = EnemyType.Zigzag;
            else type = EnemyType.Shooter;
        }

        CreateEnemy(type, new Vector3(x, y, 0f));
    }

    private void CreateEnemy(EnemyType type, Vector3 position)
    {
        GameObject enemyObj = new GameObject("Enemy_" + type);
        enemyObj.transform.position = position;
        enemyObj.tag = "Enemy";

        SpriteRenderer sr = enemyObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 3;

        Sprite enemySprite;
        int hp;
        int scoreValue;
        float speed;

        switch (type)
        {
            case EnemyType.Zigzag:
                enemySprite = SpriteGenerator.CreateEnemyZigzag();
                hp = 1;
                scoreValue = 150;
                speed = 2.5f;
                break;

            case EnemyType.Shooter:
                enemySprite = SpriteGenerator.CreateEnemyShooter();
                hp = 2;
                scoreValue = 250;
                speed = 1.5f;
                break;

            default: // Straight
                enemySprite = SpriteGenerator.CreateEnemyStraight();
                hp = 1;
                scoreValue = 100;
                speed = 3f;
                break;
        }

        sr.sprite = enemySprite;

        // Collider
        BoxCollider2D col = enemyObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 0.8f);

        // Kinematic RB2D for trigger detection
        Rigidbody2D rb = enemyObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Enemy behavior
        Enemy enemy = enemyObj.AddComponent<Enemy>();
        enemy.enemyType = type;
        enemy.hp = hp;
        enemy.scoreValue = scoreValue;
        enemy.moveSpeed = speed;

        // Neon glow pulsation
        NeonGlow glow = enemyObj.AddComponent<NeonGlow>();
        glow.pulseSpeed = Random.Range(1.5f, 3f);
    }
}
