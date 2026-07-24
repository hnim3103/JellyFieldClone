using UnityEngine;

public class GameManager : MonoBehaviour {

    [SerializeField] private GridManager gridManager;

    private void Start() {
        LevelData level = LevelLoader.LoadLevel(3);
        
        if (level == null)
            return;

        gridManager.BuildGrid(level);

    }
}