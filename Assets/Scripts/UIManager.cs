using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates and manages the in-game UI: score display, lives hearts,
/// and the game over overlay panel. All UI is built programmatically.
/// </summary>
public class UIManager : MonoBehaviour
{
    private Text scoreText;
    private Text livesText;
    private GameObject gameOverPanel;
    private Text gameOverScoreText;
    private Canvas canvas;

    /// <summary>
    /// Builds the entire UI hierarchy programmatically.
    /// Must be called after GameManager.Instance is available.
    /// </summary>
    public void Setup()
    {
        CreateCanvas();
        CreateHUD();
        CreateGameOverPanel();
        SubscribeToEvents();
    }

    // ───────────────────────── Canvas ─────────────────────────

    private void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("UICanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
    }

    // ───────────────────────── HUD ─────────────────────────

    private void CreateHUD()
    {
        // Score — top right
        scoreText = CreateText("ScoreText", "SCORE: 0", 36,
            TextAnchor.UpperRight,
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-20f, -20f),
            new Color(0f, 1f, 1f, 1f));

        // Lives — top left (red hearts)
        livesText = CreateText("LivesText", "\u2665 \u2665 \u2665", 40,
            TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(20f, -20f),
            new Color(1f, 0.3f, 0.3f, 1f));
    }

    // ───────────────────────── Game Over Panel ─────────────────────────

    private void CreateGameOverPanel()
    {
        // Semi-transparent black overlay
        GameObject panelObj = new GameObject("GameOverPanel");
        panelObj.transform.SetParent(canvas.transform, false);

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.85f);

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        gameOverPanel = panelObj;

        // "GAME OVER" title
        CreateChildText(panelObj, "GameOverTitle", "GAME OVER", 72,
            TextAnchor.MiddleCenter,
            new Vector2(0f, 100f), new Vector2(600f, 100f),
            new Color(1f, 0f, 0.3f, 1f));

        // Final score display
        gameOverScoreText = CreateChildText(panelObj, "FinalScore", "SCORE: 0", 48,
            TextAnchor.MiddleCenter,
            new Vector2(0f, 0f), new Vector2(600f, 80f),
            new Color(0f, 1f, 1f, 1f));

        // Restart instruction
        CreateChildText(panelObj, "RestartText", "PRESS R TO RESTART", 32,
            TextAnchor.MiddleCenter,
            new Vector2(0f, -80f), new Vector2(600f, 60f),
            new Color(1f, 1f, 1f, 0.7f));

        gameOverPanel.SetActive(false);
    }

    // ───────────────────────── Text Helpers ─────────────────────────

    private Text CreateText(string name, string content, int fontSize,
        TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPosition, Color color)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(canvas.transform, false);

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.font = GetFont(fontSize);
        text.alignment = alignment;
        text.color = color;
        text.fontStyle = FontStyle.Bold;

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(400f, 60f);

        // Shadow for glow effect
        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(color.r, color.g, color.b, 0.5f);
        shadow.effectDistance = new Vector2(2f, -2f);

        return text;
    }

    private Text CreateChildText(GameObject parent, string name, string content, int fontSize,
        TextAnchor alignment, Vector2 anchoredPosition, Vector2 sizeDelta, Color color)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.font = GetFont(fontSize);
        text.alignment = alignment;
        text.color = color;
        text.fontStyle = FontStyle.Bold;

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(color.r, color.g, color.b, 0.5f);
        shadow.effectDistance = new Vector2(2f, -2f);

        return text;
    }

    private static Font GetFont(int size)
    {
        // Try OS font first, fallback to built-in
        Font font = Font.CreateDynamicFontFromOSFont("Arial", size);
        return font;
    }

    // ───────────────────────── Event Handlers ─────────────────────────

    private void SubscribeToEvents()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnScoreChanged += UpdateScore;
        GameManager.Instance.OnLivesChanged += UpdateLives;
        GameManager.Instance.OnGameOver += ShowGameOver;
        GameManager.Instance.OnGameRestart += HideGameOver;
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "SCORE: " + score;
    }

    private void UpdateLives(int lives)
    {
        if (livesText == null) return;

        string hearts = "";
        for (int i = 0; i < lives; i++)
        {
            if (i > 0) hearts += " ";
            hearts += "\u2665"; // ♥
        }
        livesText.text = hearts;
    }

    private void ShowGameOver()
    {
        if (gameOverPanel == null) return;

        gameOverPanel.SetActive(true);
        if (gameOverScoreText != null && GameManager.Instance != null)
            gameOverScoreText.text = "SCORE: " + GameManager.Instance.Score;
    }

    private void HideGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnLivesChanged -= UpdateLives;
            GameManager.Instance.OnGameOver -= ShowGameOver;
            GameManager.Instance.OnGameRestart -= HideGameOver;
        }
    }
}
