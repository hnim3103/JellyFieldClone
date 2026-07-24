using Unity.Collections;
using UnityEngine;

public class GridManager : MonoBehaviour {
    [SerializeField] private Transform cellContainer;
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private CameraController cameraController;

    [SerializeField] float space = 0.01f;

    private Cell[,] cells;

    public void BuildGrid(LevelData levelData) {
        ClearBoard();

        cells = new Cell[levelData.width, levelData.height];

        float step = levelData.cellSize + space;

        float offsetX = (levelData.width - 1) * step * space;
        float offsetZ = (levelData.height - 1) * step * space;

        for (int y = 0; y < levelData.height; y++) {
            for (int x = 0; x < levelData.width; x++) {
                if (levelData.GetCell(x, y) == 0)
                    continue;

                Vector3 position = new Vector3(
                    x * step - offsetX,
                    0f,
                    y * step - offsetZ 
                );

                Quaternion rotation = cellPrefab.transform.rotation;

                Cell cell = Instantiate(cellPrefab, position, rotation, cellContainer);
                cell.Initialize(x, y);

                cells[x, y] = cell;
            }
        }
        cameraController.FitToGrid(cellContainer);
    }

    public void ClearBoard() {
        for (int i = cellContainer.childCount - 1; i >= 0; i--) {
            Destroy(cellContainer.GetChild(i).gameObject);
        }
    }

    public Cell GetCell(int x, int y) {
        if (cells == null)
            return null;

        if (x < 0 || x > cells.GetLength(0))
            return null;

        if (y < 0 || y > cells.GetLength(1))
            return null;

        return cells[x, y];
    }
}