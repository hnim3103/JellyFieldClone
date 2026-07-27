using UnityEngine;

public class JellySquish : MonoBehaviour {
    [Header("Spring Physics")]
    [SerializeField] private float frequency = 15f;
    [SerializeField] private float damping = 0.5f;

    [Header("Idle Wobble")]
    [SerializeField] private bool enableIdleWobble = true;
    [SerializeField] private float idleAmplitude = 0.025f;
    [SerializeField] private float idleSpeed = 1.8f;

    private Vector3 targetScale = Vector3.one;
    private Vector3 currentScale = Vector3.one;
    private Vector3 scaleVelocity = Vector3.zero;
    private float idlePhase;

    private void Awake() {
        idlePhase = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update() {
        Vector3 effectiveTarget = targetScale;

        // Idle wobble: subtle breathing/pulsing using sine waves
        if (enableIdleWobble) {
            float t = Time.time * idleSpeed + idlePhase;
            // Y axis wobble (up/down breathing)
            float wobbleY = Mathf.Sin(t) * idleAmplitude;
            // XZ wobble (slightly out of phase for organic feel)
            float wobbleXZ = Mathf.Sin(t * 0.73f + 2.1f) * idleAmplitude * 0.6f;
            effectiveTarget += new Vector3(-wobbleXZ, wobbleY, -wobbleXZ);
        }

        // Spring-damper physics
        Vector3 force = (effectiveTarget - currentScale) * (frequency * frequency);
        scaleVelocity += force * Time.deltaTime;
        scaleVelocity *= Mathf.Clamp01(1f - (damping * frequency * Time.deltaTime));
        currentScale += scaleVelocity * Time.deltaTime;
        transform.localScale = currentScale;
    }

    /// <summary>
    /// Squash down (flatten + widen). Used when jelly lands on a cell.
    /// </summary>
    [ContextMenu("Test Squish")]
    public void ApplySquish(float squishY = 0.65f, float stretchXZ = 1.3f) {
        currentScale = new Vector3(stretchXZ, squishY, stretchXZ);
    }

    /// <summary>
    /// Stretch up (elongate + narrow). Used when jelly is picked up / lifted.
    /// </summary>
    [ContextMenu("Test Stretch")]
    public void ApplyStretch(float stretchY = 1.15f, float squishXZ = 0.9f) {
        currentScale = new Vector3(squishXZ, stretchY, squishXZ);
    }

    /// <summary>
    /// Random velocity impulse causing a wobbly jiggle. 
    /// Used for neighbor reactions (e.g. when nearby blocks pop).
    /// </summary>
    [ContextMenu("Test Jiggle")]
    public void ApplyJiggle(float intensity = 0.3f) {
        scaleVelocity += new Vector3(
            Random.Range(-1f, 1f) * intensity,
            Random.Range(0.5f, 1.5f) * intensity * 1.5f,
            Random.Range(-1f, 1f) * intensity
        );
    }

    public void SetTargetScale(Vector3 scale) {
        targetScale = scale;
    }

    public void ResetScale() {
        targetScale = Vector3.one;
        currentScale = Vector3.one;
        scaleVelocity = Vector3.zero;
    }
}
