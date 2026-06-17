using UnityEngine;

/// <summary>
/// Generates neon/vector-style sprites procedurally at runtime.
/// All sprites are drawn on textures using SDF-based line and circle rendering
/// to achieve a glowing wireframe aesthetic without external art assets.
/// </summary>
public static class SpriteGenerator
{
    private static readonly float DefaultLineWidth = 1.5f;
    private static readonly float DefaultGlowWidth = 4f;

    // ───────────────────────── Public API ─────────────────────────

    /// <summary>Creates a cyan triangular player ship pointing upward.</summary>
    public static Sprite CreatePlayerShip()
    {
        int size = 64;
        Texture2D tex = CreateTexture(size, size);
        Color color = new Color(0f, 1f, 1f, 1f); // Cyan

        // Triangle vertices
        Vector2 top = new Vector2(size / 2f, size - 6);
        Vector2 botL = new Vector2(8, 8);
        Vector2 botR = new Vector2(size - 8, 8);
        Vector2 center = new Vector2(size / 2f, size / 2f - 4);

        // Outer hull
        DrawLineGlow(tex, top, botL, color);
        DrawLineGlow(tex, top, botR, color);
        DrawLineGlow(tex, botL, botR, color);

        // Center spine detail
        DrawLineGlow(tex, center, top, color, 1f, 2.5f);

        // Engine glow at bottom center
        DrawCircleGlow(tex, new Vector2(size / 2f, 6), 4f, new Color(0f, 0.8f, 1f, 0.8f), 1f, 6f);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64);
    }

    /// <summary>Creates a magenta diamond enemy (Straight type).</summary>
    public static Sprite CreateEnemyStraight()
    {
        int size = 48;
        Texture2D tex = CreateTexture(size, size);
        Color color = new Color(1f, 0f, 0.5f, 1f); // Magenta

        Vector2 top = new Vector2(size / 2f, size - 6);
        Vector2 left = new Vector2(6, size / 2f);
        Vector2 right = new Vector2(size - 6, size / 2f);
        Vector2 bottom = new Vector2(size / 2f, 6);

        // Diamond outline
        DrawLineGlow(tex, top, right, color);
        DrawLineGlow(tex, right, bottom, color);
        DrawLineGlow(tex, bottom, left, color);
        DrawLineGlow(tex, left, top, color);

        // Cross detail
        DrawLineGlow(tex, top, bottom, color, 0.8f, 2f);
        DrawLineGlow(tex, left, right, color, 0.8f, 2f);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 48);
    }

    /// <summary>Creates a yellow hexagonal enemy (Zigzag type).</summary>
    public static Sprite CreateEnemyZigzag()
    {
        int size = 48;
        Texture2D tex = CreateTexture(size, size);
        Color color = new Color(1f, 1f, 0f, 1f); // Yellow

        float cx = size / 2f;
        float cy = size / 2f;
        float radius = size / 2f - 6;

        // Hexagon vertices
        Vector2[] hex = new Vector2[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Deg2Rad * (60 * i - 90);
            hex[i] = new Vector2(cx + Mathf.Cos(angle) * radius, cy + Mathf.Sin(angle) * radius);
        }

        // Hexagon edges
        for (int i = 0; i < 6; i++)
            DrawLineGlow(tex, hex[i], hex[(i + 1) % 6], color);

        // Inner circle detail
        DrawCircleGlow(tex, new Vector2(cx, cy), 5f, color, 1f, 3f);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 48);
    }

    /// <summary>Creates a green circle-with-crosshair enemy (Shooter type).</summary>
    public static Sprite CreateEnemyShooter()
    {
        int size = 48;
        Texture2D tex = CreateTexture(size, size);
        Color color = new Color(0f, 1f, 0.3f, 1f); // Green

        float cx = size / 2f;
        float cy = size / 2f;
        float radius = size / 2f - 6;

        // Outer circle
        DrawCircleGlow(tex, new Vector2(cx, cy), radius, color, DefaultLineWidth, DefaultGlowWidth);

        // Crosshair lines
        DrawLineGlow(tex, new Vector2(cx, cy - radius - 2), new Vector2(cx, cy + radius + 2), color, 0.8f, 2f);
        DrawLineGlow(tex, new Vector2(cx - radius - 2, cy), new Vector2(cx + radius + 2, cy), color, 0.8f, 2f);

        // Inner ring
        DrawCircleGlow(tex, new Vector2(cx, cy), radius * 0.4f, color, 1f, 2.5f);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 48);
    }

    /// <summary>Creates a small elongated bullet glow in the given color.</summary>
    public static Sprite CreateBullet(Color color)
    {
        int w = 8, h = 16;
        Texture2D tex = CreateTexture(w, h);

        DrawLineGlow(tex, new Vector2(w / 2f, 2), new Vector2(w / 2f, h - 2), color, 1.5f, 3.5f);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16);
    }

    /// <summary>Creates a tiny dot used for background stars.</summary>
    public static Sprite CreateStar()
    {
        int size = 4;
        Texture2D tex = CreateTexture(size, size);

        DrawCircleGlow(tex, new Vector2(size / 2f, size / 2f), 0.5f, Color.white, 0.5f, 1.5f);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 4);
    }

    // ───────────────────────── Drawing Helpers ─────────────────────────

    private static Texture2D CreateTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] clear = new Color[width * height];
        for (int i = 0; i < clear.Length; i++)
            clear[i] = Color.clear;
        tex.SetPixels(clear);

        return tex;
    }

    private static void DrawLineGlow(Texture2D tex, Vector2 a, Vector2 b, Color color,
        float coreWidth = -1f, float glowWidth = -1f)
    {
        if (coreWidth < 0f) coreWidth = DefaultLineWidth;
        if (glowWidth < 0f) glowWidth = DefaultGlowWidth;

        int w = tex.width;
        int h = tex.height;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dist = DistToSegment(new Vector2(x + 0.5f, y + 0.5f), a, b);
                Color c = GetNeonColor(dist, color, coreWidth, glowWidth);
                if (c.a > 0f)
                {
                    Color existing = tex.GetPixel(x, y);
                    tex.SetPixel(x, y, BlendOver(c, existing));
                }
            }
        }
    }

    private static void DrawCircleGlow(Texture2D tex, Vector2 center, float radius, Color color,
        float coreWidth, float glowWidth)
    {
        int w = tex.width;
        int h = tex.height;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dist = Mathf.Abs(Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center) - radius);
                Color c = GetNeonColor(dist, color, coreWidth, glowWidth);
                if (c.a > 0f)
                {
                    Color existing = tex.GetPixel(x, y);
                    tex.SetPixel(x, y, BlendOver(c, existing));
                }
            }
        }
    }

    /// <summary>
    /// Returns a neon-styled color based on distance from a shape edge.
    /// Core pixels are bright white-tinted; glow fades out with quadratic falloff.
    /// </summary>
    private static Color GetNeonColor(float dist, Color color, float coreWidth, float glowWidth)
    {
        if (dist <= coreWidth)
        {
            float t = 1f - (dist / Mathf.Max(coreWidth, 0.001f));
            return new Color(
                Mathf.Lerp(color.r, 1f, t * 0.5f),
                Mathf.Lerp(color.g, 1f, t * 0.5f),
                Mathf.Lerp(color.b, 1f, t * 0.5f),
                1f
            );
        }
        else if (dist <= glowWidth)
        {
            float t = 1f - (dist - coreWidth) / (glowWidth - coreWidth);
            t = t * t; // Quadratic falloff for softer glow
            return new Color(color.r, color.g, color.b, t * 0.6f);
        }
        return Color.clear;
    }

    /// <summary>Minimum distance from point p to line segment a-b.</summary>
    private static float DistToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        Vector2 ap = p - a;
        float lenSq = ab.sqrMagnitude;
        float t = lenSq > 0f ? Mathf.Clamp01(Vector2.Dot(ap, ab) / lenSq) : 0f;
        Vector2 closest = a + t * ab;
        return Vector2.Distance(p, closest);
    }

    /// <summary>Alpha-composite src over dst.</summary>
    private static Color BlendOver(Color src, Color dst)
    {
        float outA = src.a + dst.a * (1f - src.a);
        if (outA <= 0f) return Color.clear;
        return new Color(
            (src.r * src.a + dst.r * dst.a * (1f - src.a)) / outA,
            (src.g * src.a + dst.g * dst.a * (1f - src.a)) / outA,
            (src.b * src.a + dst.b * dst.a * (1f - src.a)) / outA,
            outA
        );
    }
}
