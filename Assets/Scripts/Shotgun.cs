using UnityEngine;

public class Shotgun : MonoBehaviour, IWeapon, IReloadable
{
    [Header("References")]
    [SerializeField] Transform firePoint;
    [SerializeField] Animator animator;
    [SerializeField] AudioSource reloadAudio, shootAudio;
    [SerializeField] ParticleSystem pelletParticles, blastParticles;

    [Header("Stats")]
    [SerializeField] int maxAmmo = 6;
    [SerializeField] float reloadTime = 1f;
    [SerializeField] float range = 30f;
    [SerializeField] float spread = 0.1f;
    [SerializeField] float fireDelay = 0.8f;
    float lastFireTime = -999f;

    private int currentAmmo;
    private bool isReloading;
    [SerializeField] LayerMask enemyLayer;

    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => maxAmmo;

    private void Start()
    {
        enemyLayer = LayerMask.GetMask("Enemy");
        currentAmmo = maxAmmo;
    }


    public void Fire()
    {

        if (isReloading || currentAmmo <= 0) return;

        if (Time.time - lastFireTime < fireDelay)
            return;

        Vector3[] pelletDirections = PelletSpread.Generate(firePoint.forward, firePoint.up, firePoint.right, spread);
        foreach (var dir in pelletDirections)
        {
            //Debug.DrawRay(firePoint.position, dir * range, Color.green, 1f);
            ShotgunPellet.Fire(firePoint.position, dir, range, 1, enemyLayer);
        }

        shootAudio.Play();
        animator.SetTrigger("Shoot");
        blastParticles.Play();
        pelletParticles.Play();
        lastFireTime = Time.time;
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
        animator.SetTrigger("Reload");
        reloadAudio.Play();

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
    }

}
