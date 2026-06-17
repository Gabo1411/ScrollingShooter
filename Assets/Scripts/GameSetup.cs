using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Auto-bootstraps the entire game when Play is pressed.
/// Uses [RuntimeInitializeOnLoadMethod] so no manual scene setup is needed.
/// Creates camera, managers, player, background, and UI programmatically.
/// </summary>
public class GameSetup : MonoBehaviour
{
    public static GameSetup Instance { get; private set; }

    private PlayerController player;
    private SpawnManager spawnManager;

    /// <summary>
    /// Automatically runs when the game starts — no component needs to be
    /// manually placed in any scene. Just press Play in Unity.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoBootstrap()
    {
        // Prevent double initialization
        if (FindObjectOfType<GameSetup>() != null) return;

        GameObject setupObj = new GameObject("[GameSetup]");
        setupObj.AddComponent<GameSetup>();
    }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Enable auto-sync so Transform.Translate works with trigger detection
        Physics2D.autoSyncTransforms = true;

        SetupCamera();
        SetupManagers();
        CreatePlayer();
        SetupBackground();
        SetupUI();

        // Start the game
        GameManager.Instance.StartGame();
        spawnManager.StartSpawning();
    }

    // ───────────────────────── Setup Methods ─────────────────────────

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }

        cam.orthographic = true;
        cam.orthographicSize = 10f;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.06f, 1f); // Deep dark blue-black
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.transform.position = new Vector3(0f, 0f, -10f);
    }

    private void SetupManagers()
    {
        // GameManager singleton — Awake sets Instance immediately on AddComponent
        GameObject gmObj = new GameObject("[GameManager]");
        gmObj.AddComponent<GameManager>();

        // SpawnManager lives on the same object
        spawnManager = gmObj.AddComponent<SpawnManager>();

        // Wire up game flow events
        GameManager.Instance.OnGameRestart += OnGameRestart;
        GameManager.Instance.OnGameOver += OnGameOver;
    }

    private void CreatePlayer()
    {
        GameObject playerObj = new GameObject("Player");
        playerObj.tag = "Player";
        playerObj.transform.position = new Vector3(0f, -7f, 0f);

        // Visual
        SpriteRenderer sr = playerObj.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreatePlayerShip();
        sr.sortingOrder = 5;

        // Physics (kinematic — we move via Transform.Translate)
        BoxCollider2D col = playerObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.8f);

        Rigidbody2D rb = playerObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Behavior
        player = playerObj.AddComponent<PlayerController>();

        // Subtle neon glow pulsation
        NeonGlow glow = playerObj.AddComponent<NeonGlow>();
        glow.pulseSpeed = 1.5f;
        glow.minBrightness = 0.85f;
    }

    private void SetupBackground()
    {
        GameObject bgObj = new GameObject("[Background]");
        BackgroundScroller bg = bgObj.AddComponent<BackgroundScroller>();
        bg.Setup();
    }

    private void SetupUI()
    {
        // EventSystem required for UI interaction
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("[EventSystem]");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }

        GameObject uiObj = new GameObject("[UIManager]");
        UIManager uiManager = uiObj.AddComponent<UIManager>();
        uiManager.Setup();
    }

    // ───────────────────────── Event Handlers ─────────────────────────

    private void OnGameRestart()
    {
        if (player != null)
            player.ResetPlayer();
        spawnManager.StartSpawning();
    }

    private void OnGameOver()
    {
        spawnManager.StopSpawning();
    }
}
