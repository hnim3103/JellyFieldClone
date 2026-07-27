using UnityEngine;

public class UIManager : MonoBehaviour {

    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    public void ShowWinPopup() {
        if (winPanel != null)
            winPanel.SetActive(true);
    }

    public void ShowLosePopup() {
        if (losePanel != null)
            losePanel.SetActive(true);
    }
}
