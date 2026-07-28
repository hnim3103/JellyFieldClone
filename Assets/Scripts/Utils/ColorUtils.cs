using System.Collections.Generic;
using UnityEngine;

public static class ColorUtils {

    public static readonly JellyColor[] AllColors = {
        JellyColor.Aquamarine,
        JellyColor.Emerald,
        JellyColor.Ruby,
        JellyColor.Sapphire,
        JellyColor.Topaz
    };

    public static JellyColor[] ExtractGoalColors(LevelTarget[] targets) {
        var colors = new List<JellyColor>();
        if (targets != null) {
            foreach (var t in targets) {
                JellyColor c = (JellyColor)t.color;
                if (c != JellyColor.None && !colors.Contains(c))
                    colors.Add(c);
            }
        }
        return colors.Count > 0 ? colors.ToArray() : AllColors;
    }

    public static JellyColor PickWeighted(JellyColor[] goalColors, float goalWeight = 0.7f) {
        if (goalColors != null && goalColors.Length > 0 && Random.value < goalWeight) {
            return goalColors[Random.Range(0, goalColors.Length)];
        }
        return AllColors[Random.Range(0, AllColors.Length)];
    }

    public static JellyColor PickWeightedExcluding(JellyColor[] goalColors, HashSet<JellyColor> forbidden, float goalWeight = 0.7f) {
        const int maxAttempts = 20;
        for (int i = 0; i < maxAttempts; i++) {
            JellyColor pick = PickWeighted(goalColors, goalWeight);
            if (!forbidden.Contains(pick))
                return pick;
        }

        // Fallback - first non-forbidden color
        foreach (JellyColor c in AllColors) {
            if (!forbidden.Contains(c))
                return c;
        }

        return AllColors[Random.Range(0, AllColors.Length)];
    }

    public static bool IsValidColorConfig(JellyColor tl, JellyColor tr, JellyColor bl, JellyColor br) {
        int colorCount = 1;
        if (tr != tl) colorCount++;
        if (bl != tl && bl != tr) colorCount++;
        if (br != tl && br != tr && br != bl) colorCount++;

        if (colorCount == 3) return false;
        if (colorCount == 1 || colorCount == 4) return true;

        // 2 colors: reject diagonal same-color pairs
        if (tl == br) return false;
        if (tr == bl) return false;
        return true;
    }
}
