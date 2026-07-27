using System.Collections.Generic;
using UnityEngine;
using JellyGame;

public class JellyMerger : MonoBehaviour {
    [SerializeField] private PlacementSystem placementSystem;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ScoreManager scoreManager;
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
        CheckAndMatchRecursive(placedCell);

        // Check lose after the entire chain finishes
        if (scoreManager != null) {
            scoreManager.CheckLoseCondition();
        }
    }

    private void CheckAndMatchRecursive(Cell placedCell) {
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
            HashSet<Cell> spreadCells = ExecutePop(matchedGroup);

            foreach (Cell cell in spreadCells) {
                CheckAndMatchRecursive(cell);
            }
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

    private HashSet<Cell> ExecutePop(List<SubPos> matchedGroup) {
        HashSet<Cell> affectedCells = new HashSet<Cell>();
        HashSet<Cell> spreadCells = new HashSet<Cell>();
        Dictionary<JellyColor, int> poppedCounts = new Dictionary<JellyColor, int>();

        foreach (SubPos pos in matchedGroup) {
            Cell cell = gridManager.GetCell(pos.cellX, pos.cellY);
            if (cell != null && cell.IsOccupied) {
                // Count and trigger VFX for the color before clearing it
                JellyColor color = cell.OccupyingJelly.GetSubColor(pos.localX, pos.localY);
                if (color != JellyColor.None) {
                    if (!poppedCounts.ContainsKey(color))
                        poppedCounts[color] = 0;
                    poppedCounts[color]++;

                    // Play Pop VFX 
                    Vector3 worldPos = cell.OccupyingJelly.GetSubWorldPosition(pos.localX, pos.localY);
                    if (JellyPopVFX.Instance != null) {
                        JellyPopVFX.Instance.PlayPopFX(worldPos, color);
                    }
                }

                cell.OccupyingJelly.SetSubColor(pos.localX, pos.localY, JellyColor.None);
                affectedCells.Add(cell);
            }
        }

        // Report popped colors to score manager
        if (scoreManager != null && poppedCounts.Count > 0) {
            scoreManager.RegisterPoppedBlocks(poppedCounts);
        }

        foreach (Cell cell in affectedCells) {
            JellyGroup jelly = cell.OccupyingJelly;
            if (jelly == null) continue; // Already cleared by another chain
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
                SpreadWithinCell(jelly);
                jelly.RefreshVisual();
                jelly.Jiggle(0.4f);
                spreadCells.Add(cell);
            }
        }

        // Jiggle neighbor cells that weren't popped (ripple effect)
        JiggleNeighbors(affectedCells);

        return spreadCells;
    }

    private void JiggleNeighbors(HashSet<Cell> affectedCells) {
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };
        HashSet<Cell> neighbors = new HashSet<Cell>();

        foreach (Cell cell in affectedCells) {
            for (int i = 0; i < 4; i++) {
                Cell neighbor = gridManager.GetCell(cell.X + dx[i], cell.Y + dy[i]);
                if (neighbor != null && neighbor.IsOccupied && !affectedCells.Contains(neighbor)) {
                    neighbors.Add(neighbor);
                }
            }
        }

        foreach (Cell neighbor in neighbors) {
            neighbor.OccupyingJelly.Jiggle(0.2f);
        }
    }

    private void SpreadWithinCell(JellyGroup jelly) {
        JellyColor[,] colors = new JellyColor[2, 2];
        bool hasEmpty = false;
        bool hasColor = false;

        for (int x = 0; x < 2; x++) {
            for (int y = 0; y < 2; y++) {
                colors[x, y] = jelly.GetSubColor(x, y);
                if (colors[x, y] == JellyColor.None) hasEmpty = true;
                else hasColor = true;
            }
        }

        if (!hasEmpty || !hasColor) return;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        for (int x = 0; x < 2; x++)
            for (int y = 0; y < 2; y++)
                if (colors[x, y] != JellyColor.None)
                    queue.Enqueue(new Vector2Int(x, y));

        int[] dx = {1, -1, 0, 0};
        int[] dy = {0, 0, 1, -1};

        while (queue.Count > 0) {
            Vector2Int current = queue.Dequeue();
            for (int i = 0; i < 4; i++) {
                int nx = current.x + dx[i];
                int ny = current.y + dy[i];
                if (nx >= 0 && nx < 2 && ny >= 0 && ny < 2 && colors[nx, ny] == JellyColor.None) {
                    colors[nx, ny] = colors[current.x, current.y];
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }

        // Safety - fix diagonal same-color patterns 
        if (colors[0, 1] == colors[1, 0] && colors[0, 1] != colors[1, 1] && colors[0, 1] != colors[0, 0]) {
            colors[1, 0] = colors[1, 1]; // BR takes TR's color
        }
        
        if (colors[1, 1] == colors[0, 0] && colors[1, 1] != colors[0, 1] && colors[1, 1] != colors[1, 0]) {
            colors[0, 0] = colors[0, 1]; // BL takes TL's color
        }

        for (int x = 0; x < 2; x++)
            for (int y = 0; y < 2; y++)
                jelly.SetSubColor(x, y, colors[x, y]);
    }
}
