using UnityEngine;

public enum JellyColor {
    Aquamarine = 0,
    Emerald = 1,
    Ruby = 2,
    Sapphire = 3,
    Topaz = 4,
    }

[System.Serializable]
public class SubBlock {
    public JellyColor color;

    public SubBlock(JellyColor color) {
        this.color = color;
    }
}

[System.Serializable]
public struct ColorMaterialMapping {
    public JellyColor color;
    public Material material;
}
