using UnityEngine;
using System;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private float maxTime = 60f;
    private float currentTime;

    public event Action OnTimeOver;
    public float CurrentTime => currentTime;

    void Start()
    {
        currentTime = maxTime;
    }

    void Update()
    {
        if (currentTime > 0f)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                OnTimeOver?.Invoke();
            }
        }
    }

    public void ReduceTime(float amount)
    {
        currentTime = Mathf.Max(currentTime - amount, 0f);
        Debug.Log($"Time reduced by {amount}. Remaining time: {currentTime}");

        if (currentTime <= 0f)
        {
            OnTimeOver?.Invoke();
        }
    }
}
