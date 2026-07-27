using System.Collections.Generic;
using UnityEngine;
using JellyGame;

/// <summary>
/// Handles pre-filling grid cells with jelly blocks at level start.
/// Adjacent pre-filled cells use different colors at touching edges to prevent auto-matching.
/// </summary>
public class GridPreFiller : MonoBehaviour {

    [SerializeField] private GridManager gridManager;
    [SerializeField] private JellyGroup jellyPrefab;

    public void Fill(int count, LevelTarget[] targets) {
        if (count <= 0 || gridManager == null || jellyPrefab == null) return;

        JellyColor[] goalColors = ColorUtils.ExtractGoalColors(targets);
        List<Cell> emptyCells = gridManager.GetAllEmptyCells();

        // Shuffle
        for (int i = emptyCells.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            (emptyCells[i], emptyCells[j]) = (emptyCells[j], emptyCells[i]);
        }

        int toFill = Mathf.Min(count, emptyCells.Count);
        HashSet<Cell> filledCells = new HashSet<Cell>();

        for (int i = 0; i < toFill; i++) {
            Cell cell = emptyCells[i];
            var forbidden = CollectForbiddenColors(cell, filledCells);

            JellyColor tl = ColorUtils.PickWeightedExcluding(goalColors, forbidden.tl);
            JellyColor tr = ColorUtils.PickWeightedExcluding(goalColors, forbidden.tr);
            JellyColor bl = ColorUtils.PickWeightedExcluding(goalColors, forbidden.bl);
            JellyColor br = ColorUtils.PickWeightedExcluding(goalColors, forbidden.br);

            JellyGroup jelly = Instantiate(jellyPrefab, cell.SurfacePosition, Quaternion.identity);
            jelly.Setup(tl, tr, bl, br);
            jelly.IsPlaced = true;
            cell.SetJelly(jelly);
            filledCells.Add(cell);
        }
    }

    private (HashSet<JellyColor> tl, HashSet<JellyColor> tr, HashSet<JellyColor> bl, HashSet<JellyColor> br)
        CollectForbiddenColors(Cell cell, HashSet<Cell> filledCells) {

        var forbidTL = new HashSet<JellyColor>();
        var forbidTR = new HashSet<JellyColor>();
        var forbidBL = new HashSet<JellyColor>();
        var forbidBR = new HashSet<JellyColor>();

        CheckNeighbor(gridManager.GetCell(cell.X - 1, cell.Y), filledCells,
            (1, 1, forbidTL), (1, 0, forbidBL));   // Left: TR→TL, BR→BL

        CheckNeighbor(gridManager.GetCell(cell.X + 1, cell.Y), filledCells,
            (0, 1, forbidTR), (0, 0, forbidBR));   // Right: TL→TR, BL→BR

        CheckNeighbor(gridManager.GetCell(cell.X, cell.Y - 1), filledCells,
            (0, 1, forbidBL), (1, 1, forbidBR));   // Bottom: TL→BL, TR→BR

        CheckNeighbor(gridManager.GetCell(cell.X, cell.Y + 1), filledCells,
            (0, 0, forbidTL), (1, 0, forbidTR));   // Top: BL→TL, BR→TR

        return (forbidTL, forbidTR, forbidBL, forbidBR);
    }

    private void CheckNeighbor(Cell neighbor, HashSet<Cell> filledCells,
        params (int lx, int ly, HashSet<JellyColor> forbidden)[] mappings) {

        if (neighbor == null || !filledCells.Contains(neighbor)) return;

        foreach (var (lx, ly, forbidden) in mappings) {
            JellyColor c = neighbor.OccupyingJelly.GetSubColor(lx, ly);
            if (c != JellyColor.None)
                forbidden.Add(c);
        }
    }
}
