using UnityEngine;
using JellyGame;

public class Cell : MonoBehaviour {
    [SerializeField] private float surfaceOffset = 0.25f; 

    public int X { get; private set; }
    public int Y { get; private set; }

    public bool IsOccupied => OccupyingJelly != null;
    public JellyGroup OccupyingJelly { get; private set; }

    public Vector3 SurfacePosition => transform.position + Vector3.up * surfaceOffset;

    public void Initialize(int x, int y) {
        X = x;
        Y = y;
        gameObject.name = $"Cell ({x}, {y})";
    }

    public void SetJelly(JellyGroup jelly) {
        OccupyingJelly = jelly;
        if (jelly != null) {
            jelly.transform.position = SurfacePosition;
        }
    }

}
