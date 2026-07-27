using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour {

    [SerializeField] private GridManager gridManager;

    [Header("Events (wire in Inspector)")]
    public UnityEvent OnGoalUpdated;
    public UnityEvent OnLevelWon;
    public UnityEvent OnLevelLost;

    private Dictionary<JellyColor, int> remaining = new Dictionary<JellyColor, int>();
    private bool levelEnded;

    public void Initialize(LevelTarget[] targets) {
        remaining.Clear();
        levelEnded = false;

        if (targets == null) return;

        foreach (LevelTarget target in targets) {
            JellyColor color = (JellyColor)target.color;
            remaining[color] = target.count;
        }
    }

    /// <summary>
    /// Called by JellyMerger after sub-blocks are popped.
    /// </summary>
    public void RegisterPoppedBlocks(Dictionary<JellyColor, int> poppedCounts) {
        if (levelEnded) return;

        foreach (var kvp in poppedCounts) {
            if (remaining.ContainsKey(kvp.Key)) {
                remaining[kvp.Key] = Mathf.Max(0, remaining[kvp.Key] - kvp.Value);
            }
        }

        OnGoalUpdated?.Invoke();

        if (IsGoalComplete()) {
            levelEnded = true;
            OnLevelWon?.Invoke();
        }
    }

    /// <summary>
    /// Called after placement + pop chain finishes to check lose condition.
    /// </summary>
    public void CheckLoseCondition() {
        if (levelEnded) return;

        if (!IsGoalComplete() && !gridManager.HasEmptyCell()) {
            levelEnded = true;
            OnLevelLost?.Invoke();
        }
    }

    public bool IsGoalComplete() {
        foreach (var kvp in remaining) {
            if (kvp.Value > 0) return false;
        }
        return true;
    }

    public int GetRemaining(JellyColor color) {
        return remaining.TryGetValue(color, out int count) ? count : 0;
    }

    public Dictionary<JellyColor, int> GetAllRemaining() {
        return remaining;
    }
}
