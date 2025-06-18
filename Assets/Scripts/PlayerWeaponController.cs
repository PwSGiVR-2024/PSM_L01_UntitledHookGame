using UnityEngine;
using UnityEngine.InputSystem.LowLevel;


public class PlayerWeaponController : MonoBehaviour
{
    [SerializeField] private Transform weaponHolder;

    private IWeapon currentWeapon;
    public KeyCode fireKey = KeyCode.Mouse0;
    public KeyCode reloadKey = KeyCode.R;

    private void Start()
    {
        if (weaponHolder == null)
        {
            Debug.LogError("no weapon holder");
            return;
        }

        currentWeapon = weaponHolder.GetComponentInChildren<IWeapon>();
    }

    private void Update()
    {
        if (currentWeapon == null) return;

        if (Input.GetKeyDown(fireKey))
        {
            Debug.Log("fire!!");
            currentWeapon.Fire();
        }

        if (Input.GetKeyDown(reloadKey))
        {
            if (currentWeapon is IReloadable reloadable)
                reloadable.Reload();
        }
    }
}
