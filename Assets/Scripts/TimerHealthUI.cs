using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerHealthUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    public void UpdateTimerUI(float currentTime)
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}