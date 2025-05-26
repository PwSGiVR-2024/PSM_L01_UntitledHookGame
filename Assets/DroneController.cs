using UnityEngine;

public class DroneController : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float hoverAmplitude = 0.5f;
    [SerializeField] float hoverFrequency = 1f;

    private Vector3 startPosition;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        startPosition = transform.position;
    }

    void Update()
    {
        if (player == null)
            return;

        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

        transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

        float hoverOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        transform.position += Vector3.up * hoverOffset * Time.deltaTime;
    }
}
