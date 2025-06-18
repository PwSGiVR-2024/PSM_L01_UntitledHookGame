using UnityEngine;

public class Shotgun : MonoBehaviour, IWeapon, IReloadable
{
    [Header("References")]
    [SerializeField] Transform firePoint;
    //[SerializeField] Animator animator;
    [SerializeField] AudioSource reloadAudio;

    [Header("Stats")]
    [SerializeField] int maxAmmo = 6;
    [SerializeField] float reloadTime = 1f;
    [SerializeField] float range = 30f;
    [SerializeField] float spread = 0.1f;

    private int currentAmmo;
    private bool isReloading;
    [SerializeField] LayerMask enemyLayer = LayerMask.GetMask("Enemy");


    private void Start()
    {
        currentAmmo = maxAmmo;
    }


    public void Fire()
    {

        if (isReloading || currentAmmo <= 0) return;

        Vector3[] pelletDirections = PelletSpread.Generate(firePoint.forward, firePoint.up, firePoint.right, spread);
        foreach (var dir in pelletDirections)
        {
            //Debug.DrawRay(firePoint.position, dir * range, Color.green, 1f);
            ShotgunPellet.Fire(firePoint.position, dir, range, 1, enemyLayer);
        }


        currentAmmo--;
    }
    public void Reload()
    {
        if (isReloading || currentAmmo == maxAmmo) return;
        StartCoroutine(ReloadRoutine());
    }

    private System.Collections.IEnumerator ReloadRoutine()
    {
        isReloading = true;
        //animator.SetTrigger("Reload");
        reloadAudio.Play();

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
    }

}
