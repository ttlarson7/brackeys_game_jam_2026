using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class WinManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject winScreen;
    public TMP_Text finalScoreText;

    bool shown;

    void Update()
    {
        if (shown) return;

        var targets = FindObjectsByType<PhotoTarget>(FindObjectsSortMode.None);
        if (targets.Length == 0) return; // nothing spawned yet

        for (int i = 0; i < targets.Length; i++)
        {
            if (!targets[i].captured)
                return; // still something left
        }

        ShowWin();
    }

    void ShowWin()
    {
        shown = true;

        if (winScreen != null)
            winScreen.SetActive(true);

        if (finalScoreText != null && ScoreManager.Instance != null)
            finalScoreText.text = $"Final Score: {ScoreManager.Instance.Score}";

        // Optional: pause gameplay
        Time.timeScale = 0f;
    }

    // Optional buttons
    public void BackToMenu(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quit"); // works only in build
    }
}