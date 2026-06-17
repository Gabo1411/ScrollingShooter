using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Creates and scrolls a multi-layer star field background.
/// Faster stars appear larger and brighter (parallax effect).
/// A small percentage of stars have subtle neon coloring.
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    private readonly List<Transform> stars = new List<Transform>();
    private readonly List<float> starSpeeds = new List<float>();

    private const int StarCount = 80;
    private const float MinSpeed = 1f;
    private const float MaxSpeed = 5f;
    private const float FieldHalfWidth = 7f;
    private const float FieldHalfHeight = 12f;

    private Sprite starSprite;

    private static readonly Color[] NeonColors = new Color[]
    {
        new Color(0f, 1f, 1f, 0.3f),    // Cyan
        new Color(1f, 0f, 0.5f, 0.3f),   // Magenta
        new Color(0f, 0.5f, 1f, 0.3f),   // Blue
        new Color(0.5f, 0f, 1f, 0.3f),   // Purple
    };

    /// <summary>Generates all initial stars. Call once after instantiation.</summary>
    public void Setup()
    {
        starSprite = SpriteGenerator.CreateStar();

        for (int i = 0; i < StarCount; i++)
        {
            CreateStar(
                Random.Range(-FieldHalfWidth, FieldHalfWidth),
                Random.Range(-FieldHalfHeight, FieldHalfHeight)
            );
        }
    }

    private void CreateStar(float x, float y)
    {
        GameObject starObj = new GameObject("Star");
        starObj.transform.SetParent(transform);
        starObj.transform.position = new Vector3(x, y, 10f); // z=10 behind everything

        SpriteRenderer sr = starObj.AddComponent<SpriteRenderer>();
        sr.sprite = starSprite;
        sr.sortingOrder = -10;

        float speed = Random.Range(MinSpeed, MaxSpeed);
        float depthFactor = Mathf.InverseLerp(MinSpeed, MaxSpeed, speed); // 0=far, 1=near

        // Faster (nearer) stars are bigger
        starObj.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.2f, depthFactor);

        // Most stars are white/gray, ~15% get a subtle neon tint
        if (Random.value < 0.15f)
        {
            Color c = NeonColors[Random.Range(0, NeonColors.Length)];
            sr.color = new Color(c.r, c.g, c.b, Mathf.Lerp(0.2f, 0.6f, depthFactor));
        }
        else
        {
            float brightness = Mathf.Lerp(0.2f, 0.8f, depthFactor);
            sr.color = new Color(brightness, brightness, brightness, brightness);
        }

        stars.Add(starObj.transform);
        starSpeeds.Add(speed);
    }

    void Update()
    {
        for (int i = 0; i < stars.Count; i++)
        {
            if (stars[i] == null) continue;

            Vector3 pos = stars[i].position;
            pos.y -= starSpeeds[i] * Time.deltaTime;

            // Wrap to top when past bottom edge
            if (pos.y < -FieldHalfHeight)
            {
                pos.y = FieldHalfHeight;
                pos.x = Random.Range(-FieldHalfWidth, FieldHalfWidth);
            }

            stars[i].position = pos;
        }
    }
}
