using UnityEngine;
using UnityEngine.Events;

public class DroneAttack : MonoBehaviour, IEnemyAttack
{
    [SerializeField] float chargeTime = 0.2f;
    [SerializeField] float laserCooldown = 3f;
    [SerializeField] float laserRange = 100f;
    [SerializeField] float laserDuration = 0.1f;
    [SerializeField] float laserDamage = 10f;
    [SerializeField] LayerMask hitMask;
    [SerializeField] LineRenderer laserBeam;
    [SerializeField] ParticleSystem chargeEffect;
    private UnityAction<float> cachedListener;

    private float lastFireTime;
    private GameObject player;
    private Vector3 lastKnownPlayerPos;
    private bool isCharging;
    private int playerLayer;

    private void Start()
    {
        playerLayer = LayerMask.NameToLayer("Player");
        player = GameObject.FindGameObjectWithTag("Player");
        laserBeam.enabled = false;
        var receiver = player?.GetComponent<TimerHealth>();
        if (receiver != null)
        {
            cachedListener = receiver.ModifyTime;
        }
    }

    public void TryAttack()
    {
        if (player == null || isCharging || Time.time - lastFireTime < laserCooldown)
            return;

        lastKnownPlayerPos = player.transform.position;
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
            if (hit.collider.gameObject.layer == playerLayer && cachedListener != null)
            {
                Debug.Log("Laser hit the player.");
                cachedListener.Invoke(-laserDamage);
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
