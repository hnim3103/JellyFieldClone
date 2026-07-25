using System.Collections.Generic;
using UnityEngine;
using JellyGame;

public class JellyMerger : MonoBehaviour {
    [SerializeField] private PlacementSystem placementSystem;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private int minMatchToPop = 2;

    private void OnEnable() {
        if (placementSystem != null) {
            placementSystem.OnJellyPlaced += CheckAndMatchAt;
        }
    }

    private void OnDisable() {
        if (placementSystem != null) {
            placementSystem.OnJellyPlaced -= CheckAndMatchAt;   
        }
    }

    private struct SubPos {
        public int cellX, cellY;
        public int localX, localY;

        public SubPos(int cx, int cy, int lx, int ly) {
            cellX = cx; cellY = cy; localX = lx; localY = ly;
        }
    }

    public void CheckAndMatchAt(Cell placedCell) {
        if (placedCell == null || !placedCell.IsOccupied) return;

        List<SubPos> matchedGroup = new List<SubPos>();
        HashSet<string> visited = new HashSet<string>();

        for (int lx = 0; lx < 2; lx++) {
            for (int ly = 0; ly < 2; ly++) {

                JellyColor color = placedCell.OccupyingJelly.GetSubColor(lx, ly);
                if (color == JellyColor.None) continue;

                string key = $"{placedCell.X}_{placedCell.Y}_{lx}_{ly}";
                if (visited.Contains(key)) continue;

                List<SubPos> currentConnectedGroup = FindConnectedSubBlocks(placedCell.X, placedCell.Y, lx, ly, color, visited);

                HashSet<Cell> distinctCells = new HashSet<Cell>();
                foreach(SubPos pos in currentConnectedGroup) {
                    Cell cell = gridManager.GetCell(pos.cellX, pos.cellY);
                    if(cell != null)
                        distinctCells.Add(cell);
                }

                if (distinctCells.Count >= minMatchToPop) {
                    matchedGroup.AddRange(currentConnectedGroup);
                }
            }
        }

        if (matchedGroup.Count > 0) {
            ExecutePop(matchedGroup);
        }
    }

    private List<SubPos> FindConnectedSubBlocks(int startCX, int startCY, int startLX, int startLY, JellyColor targetColor, HashSet<string> visited) {
        List<SubPos> group = new List<SubPos>();
        Queue<SubPos> queue = new Queue<SubPos>();

        SubPos startPos = new SubPos(startCX, startCY, startLX, startLY);
        queue.Enqueue(startPos);
        visited.Add($"{startCX}_{startCY}_{startLX}_{startLY}");

        int[] dirX = { 1, -1, 0, 0 };
        int[] dirY = { 0, 0, 1, -1 };

        while (queue.Count > 0) {
            SubPos current = queue.Dequeue();
            group.Add(current);

            int globalX = current.cellX * 2 + current.localX;
            int globalY = current.cellY * 2 + current.localY;

            for (int i = 0; i < 4; i++) {
                int nextGlobalX = globalX + dirX[i];
                int nextGlobalY = globalY + dirY[i];

                int nCX = nextGlobalX / 2;
                int nLX = nextGlobalX % 2;
                if (nLX < 0) { nCX--; nLX += 2; }

                int nCY = nextGlobalY / 2;
                int nLY = nextGlobalY % 2;
                if (nLY < 0) { nCY--; nLY += 2; }

                string neighborKey = $"{nCX}_{nCY}_{nLX}_{nLY}";
                if (visited.Contains(neighborKey)) continue;

                Cell neighborCell = gridManager.GetCell(nCX, nCY);
                if (neighborCell != null && neighborCell.IsOccupied) {

                    JellyColor neighborColor = neighborCell.OccupyingJelly.GetSubColor(nLX, nLY);
                    if (neighborColor == targetColor) {
                        visited.Add(neighborKey);
                        queue.Enqueue(new SubPos(nCX, nCY, nLX, nLY));
                    }
                }
            }
        }

        return group;
    }

    private void ExecutePop(List<SubPos> matchedGroup) {
        HashSet<Cell> affectedCells = new HashSet<Cell>();

        foreach (SubPos pos in matchedGroup) {
            Cell cell = gridManager.GetCell(pos.cellX, pos.cellY);
            if (cell != null && cell.IsOccupied) {

                cell.OccupyingJelly.SetSubColor(pos.localX, pos.localY, JellyColor.None);
                affectedCells.Add(cell);
            }
        }

        foreach (Cell cell in affectedCells) {
            JellyGroup jelly = cell.OccupyingJelly;
            
            bool isEmptyAll = true;
            for (int x = 0; x < 2; x++) {
                for (int y = 0; y < 2; y++) {
                    if (jelly.GetSubColor(x, y) != JellyColor.None) {
                        isEmptyAll = false;
                        break;
                    }
                }
            }

            if (isEmptyAll) {
                Destroy(jelly.gameObject);
                cell.ClearJelly();
            } 
            else {
                jelly.RefreshVisual();
            }
        }
    }
}
