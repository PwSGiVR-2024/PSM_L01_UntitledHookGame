public interface IWeapon
{
    void Fire();
    int CurrentAmmo { get; }
    int MaxAmmo { get; }
}