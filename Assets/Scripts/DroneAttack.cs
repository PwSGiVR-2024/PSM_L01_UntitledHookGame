using UnityEngine;

public class DroneAttack : MonoBehaviour, IEnemyAttack
{
    [SerializeField] float chargeTime = 0.5f;
    [SerializeField] float laserCooldown = 3f;
    [SerializeField] float laserRange = 100f;
    [SerializeField] float laserDuration = 0.1f;
    [SerializeField] float laserDamage = 5f;
    [SerializeField] LayerMask hitMask;
    [SerializeField] LineRenderer laserBeam;
    [SerializeField] ParticleSystem chargeEffect;

    private float lastFireTime;
    private Transform player;
    private Vector3 lastKnownPlayerPos;
    private bool isCharging;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        laserBeam.enabled = false;
    }

    public void TryAttack()
    {
        if (player == null || isCharging || Time.time - lastFireTime < laserCooldown)
            return;

        lastKnownPlayerPos = player.position;
        StartCoroutine(FireLaserWithDelay());
    }

    private System.Collections.IEnumerator FireLaserWithDelay()
    {
        isCharging = true;

        if (chargeEffect != null) chargeEffect.Play();
        yield return new WaitForSeconds(chargeTime);
        if (chargeEffect != null) chargeEffect.Stop();

        Vector3 fireDirection = (lastKnownPlayerPos - transform.position).normalized;
        Vector3 hitPoint = transform.position + fireDirection * laserRange;

        if (Physics.Raycast(transform.position, fireDirection, out RaycastHit hit, laserRange, hitMask))
        {
            hitPoint = hit.point;
            if (hit.collider.TryGetComponent(out ITimeDamageable damageable))
            {
                damageable.TakeDamage(laserDamage);
            }
        }

        StartCoroutine(ShowLaserBeam(transform.position, hitPoint));
        lastFireTime = Time.time;
        isCharging = false;
    }

    private System.Collections.IEnumerator ShowLaserBeam(Vector3 start, Vector3 end)
    {
        laserBeam.SetPosition(0, start);
        laserBeam.SetPosition(1, end);
        laserBeam.enabled = true;
        yield return new WaitForSeconds(laserDuration);
        laserBeam.enabled = false;
    }
}
