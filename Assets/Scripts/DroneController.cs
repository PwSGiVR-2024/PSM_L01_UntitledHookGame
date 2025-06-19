using UnityEngine;

[RequireComponent(typeof(DroneAttack))]
public class DroneController : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] Rigidbody rb;
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float hoverAmplitude = 0.5f;
    [SerializeField] float hoverFrequency = 1f;

    private IEnemyAttack laserAttack;

    private Vector3 startPosition;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        startPosition = transform.position;
        laserAttack = GetComponent<IEnemyAttack>();
    }


    private void FixedUpdate()
    {
        if (player == null) return;

        Vector3 hoverOffset = Vector3.up * Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;

        Vector3 direction = (player.position - rb.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, lookRotation, rotationSpeed * Time.fixedDeltaTime));

        Vector3 targetPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime + hoverOffset * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);

        laserAttack.TryAttack();
    }

}
