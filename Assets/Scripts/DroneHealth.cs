using UnityEngine;
using UnityEngine.Events;

public class DroneHealth : MonoBehaviour, IDamageable
{
    [SerializeField] float maxHealth = 3f;
    [SerializeField] float timeGained = 5f;
    private bool isDead = false;

    private UnityAction<float> cachedListener;

    private float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;

        var player = GameObject.FindGameObjectWithTag("Player");
        var receiver = player?.GetComponent<TimerHealth>();
        if (receiver != null)
        {
            cachedListener = receiver.ModifyTime;
        }

    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        if (cachedListener != null)
        {
            cachedListener.Invoke(timeGained);
        }

        Destroy(gameObject);
    }
}
