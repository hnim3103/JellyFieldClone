using UnityEngine;

public class JellySquish : MonoBehaviour {
    [SerializeField] private float frequency = 12f;
    [SerializeField] private float damping = 0.55f;

    private Vector3 targetScale = Vector3.one;
    private Vector3 currentScale = Vector3.one;
    private Vector3 scaleVelocity = Vector3.zero;
    
    private void Update() {
        Vector3 force = (targetScale - currentScale) * (frequency * frequency);
        scaleVelocity += force * Time.deltaTime;
        scaleVelocity *= Mathf.Clamp01(1f - (damping * frequency * Time.deltaTime));
        currentScale += scaleVelocity * Time.deltaTime;
        transform.localScale = currentScale;
    }

    [ContextMenu("Test Squish")]
    public void ApplySquish(float squishY = 0.65f, float stretchXZ = 1.3f) {
        currentScale = new Vector3(stretchXZ, squishY, stretchXZ);
    }
}
