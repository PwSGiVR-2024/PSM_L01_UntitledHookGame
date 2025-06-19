using UnityEngine;
using TMPro; // Or UnityEngine.UI if you're not using TextMeshPro
using UnityEngine.SceneManagement;

public class GameOverScreenController : MonoBehaviour
{
    [SerializeField] private TimerHealth timerHealth;
    [SerializeField] private TextMeshProUGUI timeSurvivedText;
    [SerializeField] private GameObject gameOver;

    public void ShowGameOverScreen()
    {
        float time = timerHealth.totalTime;
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        timeSurvivedText.text = $"You survived {minutes:00}:{seconds:00}";
        gameOver.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}

