using TMPro;
using UnityEngine;

public class AmmoDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private MonoBehaviour weaponReference; // must implement IAmmoInfo

    private IWeapon weapon;

    private void Start()
    {
        weapon = weaponReference as IWeapon;

        if (weapon == null)
        {
            Debug.LogError("weapon with no interface?");
        }
    }

    private void Update()
    {
        if (weapon != null)
        {
            ammoText.text = $"{weapon.CurrentAmmo} / {weapon.MaxAmmo}";
        }
    }
}
