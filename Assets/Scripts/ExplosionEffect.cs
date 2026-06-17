using UnityEngine;

/// <summary>
/// Spawns a one-shot particle burst explosion at a given position with neon-colored particles.
/// The explosion GameObject self-destructs after the effect completes.
/// </summary>
public class ExplosionEffect : MonoBehaviour
{
    /// <summary>Creates an explosion effect at the specified world position.</summary>
    public static void Spawn(Vector3 position, Color color)
    {
        GameObject obj = new GameObject("Explosion");
        obj.transform.position = position;

        ParticleSystem ps = obj.AddComponent<ParticleSystem>();

        // Main module
        var main = ps.main;
        main.duration = 0.5f;
        main.startLifetime = 0.4f;
        main.startSpeed = 5f;
        main.startSize = 0.15f;
        main.maxParticles = 30;
        main.loop = false;
        main.playOnAwake = true;
        main.startColor = color;
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Emission: single burst
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 20, 30)
        });

        // Shape: small circle origin
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;

        // Color over lifetime: white flash → neon color → fade out
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(color, 0.3f),
                new GradientColorKey(color, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        // Size over lifetime: shrink to nothing
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0f)
        ));

        // Renderer setup
        var psRenderer = obj.GetComponent<ParticleSystemRenderer>();
        psRenderer.sortingOrder = 10;
        // Use Sprites/Default shader which is always available in built-in pipeline
        psRenderer.material = new Material(Shader.Find("Sprites/Default"));

        // Self-destruct after particles finish
        Destroy(obj, 1f);
    }
}
