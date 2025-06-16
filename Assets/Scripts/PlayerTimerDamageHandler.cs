using UnityEngine;

public class PlayerTimerDamageHandler : MonoBehaviour, ITimeDamageable
{
    [SerializeField] private TimeManager timeManager;

    public void TakeDamage(float amount)
    {
        if (timeManager != null)
        {
            timeManager.ReduceTime(amount);
        }
    }
}
