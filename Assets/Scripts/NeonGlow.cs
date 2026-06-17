using UnityEngine;

/// <summary>
/// Pulses the SpriteRenderer brightness over time for a neon glow effect.
/// Each instance starts with a random phase offset to avoid synchronized pulsing.
/// </summary>
public class NeonGlow : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float minBrightness = 0.6f;
    public float maxBrightness = 1f;

    private SpriteRenderer sr;
    private float timer;
    private Color baseColor;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            baseColor = sr.color;
        timer = Random.Range(0f, Mathf.PI * 2f); // Random phase
    }

    void Update()
    {
        if (sr == null) return;

        timer += Time.deltaTime * pulseSpeed;
        float t = (Mathf.Sin(timer) + 1f) / 2f; // Normalized 0→1
        float brightness = Mathf.Lerp(minBrightness, maxBrightness, t);
        sr.color = new Color(
            baseColor.r * brightness,
            baseColor.g * brightness,
            baseColor.b * brightness,
            baseColor.a
        );
    }
}
