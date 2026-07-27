using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using JellyGame;

public class GameManager : MonoBehaviour {

    [Header("Core References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GridPreFiller gridPreFiller;
    [SerializeField] private JellySpawner jellySpawner;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GoalDisplay goalDisplay;
    [SerializeField] private DragSystem dragSystem;
    [SerializeField] private UIManager uiManager;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI levelTitleText;

    [Header("Level Settings")]
    [SerializeField] private int maxLevel = 3;

    [Header("World Container")]
    [SerializeField] private GameObject worldContainer;

    [Header("End-of-Level Pop")]
    [SerializeField] private float popStaggerDelay = 0.06f;
    [SerializeField] private float postPopWait = 0.5f;

    private const string CURRENT_LEVEL_KEY = "CurrentLevel";
    private int currentLevel;

    public static int GetSavedLevel() {
        return PlayerPrefs.GetInt(CURRENT_LEVEL_KEY, 1);
    }

    public static void SetSavedLevel(int level) {
        PlayerPrefs.SetInt(CURRENT_LEVEL_KEY, level);
        PlayerPrefs.Save();
    }

    private void Start() {
        currentLevel = GetSavedLevel();
        LoadLevel(currentLevel);
    }

    private void LoadLevel(int levelId) {
        LevelData level = LevelLoader.LoadLevel(levelId);

        if (level == null) {
            Debug.LogError($"Failed to load level {levelId}");
            return;
        }

        if (levelTitleText != null)
            levelTitleText.text = $"Level {levelId}";

        gridManager.BuildGrid(level);

        // Pre-fill cells
        if (level.preFilledCount > 0 && gridPreFiller != null)
            gridPreFiller.Fill(level.preFilledCount, level.targets);

        Bounds gridBounds = gridManager.GetGridBounds();
        jellySpawner.Initialize(gridBounds, level.targets);

        cameraController.FitToGrid(gridManager.CellContainer);

        if (scoreManager != null)
            scoreManager.Initialize(level.targets);

        if (goalDisplay != null)
            goalDisplay.Initialize(level.targets);

        if (dragSystem != null)
            dragSystem.enabled = true;
    }

    public void HandleWin() {
        Debug.Log($"LEVEL {currentLevel} WON!");
        DisableGameplay();

        int nextLevel = currentLevel + 1;
        SetSavedLevel(nextLevel <= maxLevel ? nextLevel : maxLevel);

        StartCoroutine(EndLevelSequence(isWin: true));
    }

    public void HandleLose() {
        Debug.Log($"LEVEL {currentLevel} LOST");
        DisableGameplay();
        StartCoroutine(EndLevelSequence(isWin: false));
    }

    private void DisableGameplay() {
        if (dragSystem != null) dragSystem.enabled = false;
        if (jellySpawner != null) jellySpawner.enabled = false;
    }

    private IEnumerator EndLevelSequence(bool isWin) {
        yield return null;

        if (jellySpawner != null)
            jellySpawner.DestroyUnplacedJellies();

        // Pop all remaining blocks
        List<Cell> occupiedCells = gridManager.GetAllOccupiedCells();

        foreach (Cell cell in occupiedCells) {
            if (!cell.IsOccupied) continue;

            JellyGroup jelly = cell.OccupyingJelly;
            for (int lx = 0; lx < 2; lx++) {
                for (int ly = 0; ly < 2; ly++) {
                    JellyColor color = jelly.GetSubColor(lx, ly);
                    if (color == JellyColor.None) continue;

                    Vector3 worldPos = jelly.GetSubWorldPosition(lx, ly);
                    if (JellyPopVFX.Instance != null)
                        JellyPopVFX.Instance.PlayPopFX(worldPos, color);
                }
            }

            Destroy(jelly.gameObject);
            cell.ClearJelly();
            yield return new WaitForSeconds(popStaggerDelay);
        }

        yield return new WaitForSeconds(postPopWait);

        if (worldContainer != null)
            worldContainer.SetActive(false);

        if (uiManager != null) {
            if (isWin) uiManager.ShowWinPopup();
            else uiManager.ShowLosePopup();
        }
    }

    public void LoadNextLevel() {
        currentLevel = GetSavedLevel();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RetryLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}