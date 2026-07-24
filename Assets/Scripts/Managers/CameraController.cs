using UnityEngine;

public class CameraController : MonoBehaviour {
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float padding = 0.5f;

    public void FitToGrid(Transform gridContainer) {
        Renderer[] renderers = gridContainer.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        Fit(bounds);
    }

    public void Fit(Bounds bounds) {
        Transform cam = targetCamera.transform;

        Vector3 ext = bounds.extents;

        Vector3[] corners = {
            bounds.center + new Vector3(-ext.x,-ext.y,-ext.z),
            bounds.center + new Vector3(-ext.x,-ext.y, ext.z),
            bounds.center + new Vector3(-ext.x, ext.y,-ext.z),
            bounds.center + new Vector3(-ext.x, ext.y, ext.z),

            bounds.center + new Vector3( ext.x,-ext.y,-ext.z),
            bounds.center + new Vector3( ext.x,-ext.y, ext.z),
            bounds.center + new Vector3( ext.x, ext.y,-ext.z),
            bounds.center + new Vector3( ext.x, ext.y, ext.z),
        };

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        Matrix4x4 worldToCamera = cam.worldToLocalMatrix;

        foreach (Vector3 corner in corners) {
            Vector3 p = worldToCamera.MultiplyPoint(corner);

            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);
            minY = Mathf.Min(minY, p.y);
            maxY = Mathf.Max(maxY, p.y);
        }

        float width = maxX - minX;
        float height = maxY - minY;

        float verticalSize = height * 0.5f;
        float horizontalSize = width * 0.5f / targetCamera.aspect;

        targetCamera.orthographicSize = Mathf.Max(verticalSize, horizontalSize) + padding;

        float centerX = (minX + maxX) * 0.5f;
        float centerY = (minY + maxY) * 0.5f;

        Vector3 worldOffset = cam.right * centerX + cam.up * centerY;
        cam.position += worldOffset;
    }
}