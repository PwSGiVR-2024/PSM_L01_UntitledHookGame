using UnityEngine;

public enum EnemyMovementType
{
    Grounded,
    Flying
}

[RequireComponent(typeof(BoxCollider))]
public class SpawnArea : MonoBehaviour
{
    [SerializeField] private EnemyMovementType movementType;

    public EnemyMovementType MovementType => movementType;

    public Vector3 GetRandomPointInArea()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        Vector3 center = col.center + transform.position;
        Vector3 size = col.size * 0.5f;

        float x = Random.Range(-size.x, size.x);
        float y = Random.Range(-size.y, size.y);
        float z = Random.Range(-size.z, size.z);

        return center + new Vector3(x, y, z);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = movementType == EnemyMovementType.Flying ? Color.cyan : Color.green;
        BoxCollider col = GetComponent<BoxCollider>();
        if (col)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.center, col.size);
        }
    }
}
