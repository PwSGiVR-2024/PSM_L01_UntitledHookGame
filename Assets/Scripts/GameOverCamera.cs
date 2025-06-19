using UnityEngine;

public class GameOverCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 40f;
    [SerializeField] private float height = 10f;    
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float tiltAngle = -25f;

    private float currentAngle;

    private void LateUpdate()
    {
        if (target == null) return;

        currentAngle += rotationSpeed * Time.deltaTime;
        if (currentAngle > 360f) currentAngle -= 360f;

        float radians = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians)) * distance;

        transform.position = target.position + offset + Vector3.up * height;
        Vector3 lookTarget = target.position + Vector3.up * height * 0.5f;
        Quaternion tilt = Quaternion.Euler(tiltAngle, 0f, 0f);
        transform.rotation = Quaternion.LookRotation(tilt * (lookTarget - transform.position));
    }
}
