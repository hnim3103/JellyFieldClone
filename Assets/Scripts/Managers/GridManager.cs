using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour {

    [SerializeField] private Transform cellContainer;
    [SerializeField] private Cell cellPrefab;
    [SerializeField] float space = 0.01f;

    public Transform CellContainer => cellContainer;

    private Cell[,] cells;

    public void BuildGrid(LevelData levelData) {
        ClearBoard();

        cells = new Cell[levelData.width, levelData.height];
        float step = levelData.cellSize + space;
        float offsetX = (levelData.width - 1) * step * 0.5f;
        float offsetZ = (levelData.height - 1) * step * 0.5f;

        for (int y = 0; y < levelData.height; y++) {
            for (int x = 0; x < levelData.width; x++) {
                if (levelData.GetCell(x, y) == 0) continue;

                Vector3 position = new Vector3(
                    x * step - offsetX, 0f, y * step - offsetZ
                );

                Cell cell = Instantiate(cellPrefab, position, cellPrefab.transform.rotation, cellContainer);
                cell.Initialize(x, y);
                cells[x, y] = cell;
            }
        }
    }

    public void ClearBoard() {
        for (int i = cellContainer.childCount - 1; i >= 0; i--)
            Destroy(cellContainer.GetChild(i).gameObject);
    }

    public Cell GetCell(int x, int y) {
        if (cells == null) return null;
        if (x < 0 || x >= cells.GetLength(0)) return null;
        if (y < 0 || y >= cells.GetLength(1)) return null;
        return cells[x, y];
    }

    public Bounds GetGridBounds() {
        Renderer[] renderers = cellContainer.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(Vector3.zero, Vector3.zero);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    public List<Cell> GetAllOccupiedCells() {
        var list = new List<Cell>();
        if (cells == null) return list;
        for (int x = 0; x < cells.GetLength(0); x++)
            for (int y = 0; y < cells.GetLength(1); y++)
                if (cells[x, y] != null && cells[x, y].IsOccupied)
                    list.Add(cells[x, y]);
        return list;
    }

    public List<Cell> GetAllEmptyCells() {
        var list = new List<Cell>();
        if (cells == null) return list;
        for (int x = 0; x < cells.GetLength(0); x++)
            for (int y = 0; y < cells.GetLength(1); y++)
                if (cells[x, y] != null && !cells[x, y].IsOccupied)
                    list.Add(cells[x, y]);
        return list;
    }

    public bool HasEmptyCell() {
        if (cells == null) return false;
        for (int x = 0; x < cells.GetLength(0); x++)
            for (int y = 0; y < cells.GetLength(1); y++)
                if (cells[x, y] != null && !cells[x, y].IsOccupied)
                    return true;
        return false;
    }
}