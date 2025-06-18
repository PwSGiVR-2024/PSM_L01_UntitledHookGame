using UnityEngine;

public static class ShotgunPellet
{
    public static void Fire(Vector3 origin, Vector3 direction, float range, int damage, LayerMask enemyLayer)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, range, enemyLayer))
        {
            Debug.Log(hit.transform.gameObject.name);
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            Debug.DrawRay(origin, direction * hit.distance, Color.red, 1f);
        }
    }
}
