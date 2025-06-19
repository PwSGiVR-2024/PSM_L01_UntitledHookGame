using UnityEngine;
using UnityEngine.Events;

public class TimerHealth : MonoBehaviour
{
    [SerializeField] private float startTime = 60f;

    [Header("Events")]
    public UnityEvent<float> OnTimeChanged;
    public UnityEvent OnTimeDepleted;
    public float totalTime=0f;
    public float CurrentTime { get; private set; }
    public bool IsDead => CurrentTime <= 0f;
    

    private void Start()
    {
        CurrentTime = startTime;
        OnTimeChanged?.Invoke(CurrentTime);
    }

    private void Update()
    {
        if (IsDead) return;
        totalTime += Time.deltaTime;
        CurrentTime -= Time.deltaTime;
        CurrentTime = Mathf.Max(0f, CurrentTime);
        OnTimeChanged?.Invoke(CurrentTime);

        if (IsDead)
        {
            OnTimeDepleted?.Invoke();
            gameObject.SetActive(false);
        }
    }

    public void ModifyTime(float amount)
    {
        if (IsDead) return;

        CurrentTime = Mathf.Max(0f, CurrentTime + amount);
        OnTimeChanged?.Invoke(CurrentTime);

        if (IsDead)
        {
            OnTimeDepleted?.Invoke();
        }
    }
}
