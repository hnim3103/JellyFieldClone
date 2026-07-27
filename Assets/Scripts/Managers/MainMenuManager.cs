using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {

    [Header("UI References")]
    [SerializeField] private Slider loadingBar;
    [SerializeField] private GameObject startButton;

    [Header("Scene")]
    [SerializeField] private string gameplaySceneName = "Gameplay";

    private AsyncOperation asyncLoad;

    private void Start() {
        // Hide start button, show loading bar
        if (startButton != null)
            startButton.SetActive(false);

        StartCoroutine(LoadGameplaySceneAsync());
    }

    private IEnumerator LoadGameplaySceneAsync() {
        asyncLoad = SceneManager.LoadSceneAsync(gameplaySceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone) {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            if (loadingBar != null)
                loadingBar.value = progress;

            // Loading complete 
            if (asyncLoad.progress >= 0.9f) {
                // Fill bar to 100%
                if (loadingBar != null)
                    loadingBar.value = 1f;

                // Hide loading bar
                if (loadingBar != null)
                    loadingBar.gameObject.SetActive(false);

                // Show start button
                if (startButton != null)
                    startButton.SetActive(true);

                // Wait for player to press start
                yield break;
            }

            yield return null;
        }
    }

    public void OnStartButtonClicked() {
        if (asyncLoad != null) {
            asyncLoad.allowSceneActivation = true;
        }
    }
}
