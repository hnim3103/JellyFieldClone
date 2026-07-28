using UnityEngine;
using JellyGame;

public class JellySpawner : MonoBehaviour {

    [SerializeField] private JellyGroup jellyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private PlacementSystem placementSystem;

    [SerializeField] private float spawnGap = 1.5f;
    [SerializeField] private float spawnSpread = 0.6f;

    [Header("Weighted Spawning")]
    [SerializeField, Range(0f, 1f)] private float goalColorWeight = 0.7f;

    private JellyGroup[] spawnedJellies;
    private JellyColor[] goalColors;

    private void Awake() {
        spawnedJellies = new JellyGroup[spawnPoints.Length];
    }

    public void Initialize(Bounds gridBounds, LevelTarget[] targets) {
        goalColors = ColorUtils.ExtractGoalColors(targets);

        Vector3 belowCenter = new Vector3(
            gridBounds.center.x, 0f, gridBounds.min.z - spawnGap
        );

        float halfWidth = Mathf.Max(gridBounds.extents.x * spawnSpread, 0.8f);

        if (spawnPoints.Length >= 1)
            spawnPoints[0].position = belowCenter + Vector3.left * halfWidth;
        if (spawnPoints.Length >= 2)
            spawnPoints[1].position = belowCenter + Vector3.right * halfWidth;

        SpawnAll();
    }

    private void OnEnable() {
        if (placementSystem != null)
            placementSystem.OnJellyPlaced += OnJellyPlaced;
    }

    private void OnDisable() {
        if (placementSystem != null)
            placementSystem.OnJellyPlaced -= OnJellyPlaced;
    }

    private void OnJellyPlaced(Cell cell) {
        for (int i = 0; i < spawnedJellies.Length; i++) {
            if (spawnedJellies[i] != null && spawnedJellies[i].IsPlaced)
                SpawnAt(i);
        }
    }

    private void SpawnAll() {
        for (int i = 0; i < spawnPoints.Length; i++)
            SpawnAt(i);
    }

    private void SpawnAt(int slotIndex) {
        Vector3 spawnPos = spawnPoints[slotIndex].position;
        Quaternion spawnRot = spawnPoints[slotIndex].rotation;

        JellyGroup jelly = Instantiate(jellyPrefab, spawnPos, spawnRot);

        JellyColor tl, tr, bl, br;
        do {
            tl = ColorUtils.PickWeighted(goalColors, goalColorWeight);
            tr = ColorUtils.PickWeighted(goalColors, goalColorWeight);
            bl = ColorUtils.PickWeighted(goalColors, goalColorWeight);
            br = ColorUtils.PickWeighted(goalColors, goalColorWeight);
        } while (!ColorUtils.IsValidColorConfig(tl, tr, bl, br));

        jelly.Setup(tl, tr, bl, br);
        spawnedJellies[slotIndex] = jelly;
    }

    public void DestroyUnplacedJellies() {
        if (spawnedJellies == null) return;
        for (int i = 0; i < spawnedJellies.Length; i++) {
            if (spawnedJellies[i] != null && !spawnedJellies[i].IsPlaced) {
                Destroy(spawnedJellies[i].gameObject);
                spawnedJellies[i] = null;
            }
        }
    }
}
