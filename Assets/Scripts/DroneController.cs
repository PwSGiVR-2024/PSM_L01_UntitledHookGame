using UnityEngine;

[RequireComponent(typeof(DroneAttack))]
public class DroneController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float hoverAmplitude = 0.5f;
    [SerializeField] private float hoverFrequency = 1f;

    private IEnemyAttack laserAttack;

    private Vector3 startPosition;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        startPosition = transform.position;
        laserAttack = GetComponent<IEnemyAttack>();
    }

    private void Update()
    {
        if (player == null) return;

        Hover();
        MoveAndRotate();
        laserAttack.TryAttack();
    }

    private void Hover()
    {
        float hoverOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        transform.position += Vector3.up * hoverOffset * Time.deltaTime;
    }

    private void MoveAndRotate()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
    }
}
