using System;
using UnityEngine;

[Serializable]
public class LevelData {
    public int id;
    public int width;
    public int height;
    public float cellSize;

    public int[] cells;

    public int GetCell(int x, int y) {
        return cells[y * width + x];
    }
}


public class LevelLoader : MonoBehaviour {
    public static LevelData LoadLevel(int levelId) {
        string fileName = $"Levels/level_{levelId:D3}";

        TextAsset json = Resources.Load<TextAsset>(fileName);

        if (json == null) {
            Debug.LogError($"Cannot find {fileName}");
            return null;
        }

        return JsonUtility.FromJson<LevelData>(json.text);
    }
}