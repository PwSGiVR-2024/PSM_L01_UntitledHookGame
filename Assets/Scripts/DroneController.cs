using UnityEngine;

public class DroneController : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] Transform player;
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float hoverAmplitude = 0.5f;
    [SerializeField] float hoverFrequency = 1f;

    [Header("Laser Attack")]
    [SerializeField] float chargeTime = 0.5f;         // Charging time before firing
    [SerializeField] float laserCooldown = 3f;        // Time between shots
    [SerializeField] float laserRange = 100f;
    [SerializeField] LayerMask hitMask;
    [SerializeField] LineRenderer laserBeam;
    [SerializeField] float laserDuration = 0.1f;
    [SerializeField] ParticleSystem chargeEffect;

    private Vector3 startPosition;
    private float lastFireTime;
    private Vector3 lastKnownPlayerPos;
    private bool isCharging;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        startPosition = transform.position;
        lastKnownPlayerPos = player.position;

        laserBeam.enabled = false;
    }

    void Update()
    {
        if (player == null)
            return;

        Hover();
        MoveAndRotate();

        // Simulate a delayed aiming by lagging behind player movement
        if (Time.time - lastFireTime >= laserCooldown && !isCharging)
        {
            lastKnownPlayerPos = player.position;
            StartCoroutine(FireLaserWithDelay());
        }
    }

    void Hover()
    {
        float hoverOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        transform.position += Vector3.up * hoverOffset * Time.deltaTime;
    }

    void MoveAndRotate()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
    }

    System.Collections.IEnumerator FireLaserWithDelay()
    {
        isCharging = true;

        if (chargeEffect != null)
            chargeEffect.Play();

        yield return new WaitForSeconds(chargeTime); // Simulate charging

        // Stop charging effect
        if (chargeEffect != null)
            chargeEffect.Stop();

        Vector3 fireDirection = (lastKnownPlayerPos - transform.position).normalized;
        Vector3 hitPoint = transform.position + fireDirection * laserRange;

        if (Physics.Raycast(transform.position, fireDirection, out RaycastHit hit, laserRange, hitMask))
        {
            hitPoint = hit.point;
            Debug.Log("Laser hit: " + hit.collider.name);
            ITimeDamageable damageable = hit.collider.GetComponent<ITimeDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(5f);
            }
        }
        else
        {
            Debug.Log("Laser missed.");
        }

        StartCoroutine(ShowLaserBeam(transform.position, hitPoint));

        lastFireTime = Time.time;
        isCharging = false;
    }

    System.Collections.IEnumerator ShowLaserBeam(Vector3 start, Vector3 end)
    {
        laserBeam.SetPosition(0, start);
        laserBeam.SetPosition(1, end);
        laserBeam.enabled = true;

        yield return new WaitForSeconds(laserDuration);

        laserBeam.enabled = false;
    }

}