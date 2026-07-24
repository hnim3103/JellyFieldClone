using UnityEngine;

public class Cell : MonoBehaviour {
    public int X { get; private set; }
    public int Y { get; private set; }

    public void Initialize(int x, int y) {
        X = x;
        Y = y;

        gameObject.name = $"Cell ({x}, {y})";
    }
}