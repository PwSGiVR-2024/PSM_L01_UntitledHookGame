using UnityEngine;
using UnityEngine.Events;

public class DroneHealth : MonoBehaviour, IDamageable
{
    [SerializeField] float maxHealth = 3f;
    [SerializeField] float timeGained = 5f;
    [SerializeField] ParticleSystem deathExplosion;
    [SerializeField] float deathDelay = 1f; // Time to wait before destroying the GameObject

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

        cachedListener?.Invoke(timeGained);

        if (deathExplosion != null)
        {
            deathExplosion.transform.parent = null;
            deathExplosion.Play();
            Destroy(deathExplosion.gameObject, deathExplosion.main.duration + deathExplosion.main.startLifetime.constantMax);
        }

        DisableDroneComponents();

        Destroy(gameObject, deathDelay);
    }

    private void DisableDroneComponents()
    {
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        var ai = GetComponent<DroneController>();
        if (ai != null) ai.enabled = false;
    }
}
