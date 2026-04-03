using UnityEngine;
using System;

namespace ShooterGame.WeaponSystem
{
    /// <summary>
    /// Base class for all weapons in the game
    /// Handles ammo management, firing logic, and state
    /// </summary>
    public abstract class Weapon : MonoBehaviour
    {
        [Header("Weapon Info")]
        [SerializeField] private string weaponName;
        [SerializeField] private WeaponType weaponType;
        [SerializeField] private Sprite weaponIcon;
        
        [Header("Ammo Configuration")]
        [SerializeField] private int magazineSize = 30;
        [SerializeField] private int maxTotalAmmo = 180;
        [SerializeField] private int currentAmmo;
        [SerializeField] private int reserveAmmo;
        
        [Header("Fire Configuration")]
        [SerializeField] private float fireRate = 0.1f; // Time between shots
        [SerializeField] private float damage = 25f;
        [SerializeField] private float range = 100f;
        [SerializeField] private FireMode fireMode = FireMode.Automatic;
        [SerializeField] private float reloadTime = 2f;
        
        [Header("Recoil & Accuracy")]
        [SerializeField] private float recoilAmount = 0.5f;
        [SerializeField] private float spreadAngle = 2f;
        
        // State tracking
        private float lastFireTime;
        private bool isReloading;
        private bool isEquipped;
        
        // Events for UI and other systems to listen to
        public event Action<int, int> OnAmmoChanged; // currentAmmo, reserveAmmo
        public event Action OnWeaponFired;
        public event Action OnReloadStarted;
        public event Action OnReloadCompleted;
        public event Action OnWeaponEquipped;
        public event Action OnWeaponUnequipped;
        
        // Properties
        public string WeaponName => weaponName;
        public WeaponType Type => weaponType;
        public Sprite Icon => weaponIcon;
        public int CurrentAmmo => currentAmmo;
        public int ReserveAmmo => reserveAmmo;
        public int MagazineSize => magazineSize;
        public bool IsReloading => isReloading;
        public bool IsEquipped => isEquipped;
        public FireMode Mode => fireMode;
        
        protected virtual void Awake()
        {
            // Initialize ammo
            currentAmmo = magazineSize;
            reserveAmmo = maxTotalAmmo - magazineSize;
        }
        
        /// <summary>
        /// Attempts to fire the weapon
        /// </summary>
        public virtual bool TryFire()
        {
            if (!CanFire())
                return false;
            
            Fire();
            return true;
        }
        
        /// <summary>
        /// Checks if the weapon can fire
        /// </summary>
        protected virtual bool CanFire()
        {
            if (!isEquipped)
                return false;
            
            if (isReloading)
                return false;
            
            if (currentAmmo <= 0)
                return false;
            
            // Check fire rate
            if (Time.time - lastFireTime < fireRate)
                return false;
            
            return true;
        }
        
        /// <summary>
        /// Executes the fire logic
        /// </summary>
        protected virtual void Fire()
        {
            lastFireTime = Time.time;
            currentAmmo--;
            
            // Execute weapon-specific fire behavior
            ExecuteFireBehavior();
            
            // Notify listeners
            OnWeaponFired?.Invoke();
            OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
            
            // Auto reload if empty
            if (currentAmmo <= 0 && reserveAmmo > 0)
            {
                StartReload();
            }
        }
        
        /// <summary>
        /// Override this in derived classes for specific weapon behavior
        /// </summary>
        protected abstract void ExecuteFireBehavior();
        
        /// <summary>
        /// Attempts to reload the weapon
        /// </summary>
        public virtual void StartReload()
        {
            if (isReloading)
                return;
            
            if (currentAmmo >= magazineSize)
                return;
            
            if (reserveAmmo <= 0)
                return;
            
            isReloading = true;
            OnReloadStarted?.Invoke();
            
            // Start reload coroutine
            StartCoroutine(ReloadCoroutine());
        }
        
        private System.Collections.IEnumerator ReloadCoroutine()
        {
            yield return new WaitForSeconds(reloadTime);
            
            CompleteReload();
        }
        
        /// <summary>
        /// Completes the reload process
        /// </summary>
        protected virtual void CompleteReload()
        {
            int ammoNeeded = magazineSize - currentAmmo;
            int ammoToReload = Mathf.Min(ammoNeeded, reserveAmmo);
            
            currentAmmo += ammoToReload;
            reserveAmmo -= ammoToReload;
            
            isReloading = false;
            
            OnReloadCompleted?.Invoke();
            OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
        }
        
        /// <summary>
        /// Equips the weapon
        /// </summary>
        public virtual void Equip()
        {
            isEquipped = true;
            gameObject.SetActive(true);
            OnWeaponEquipped?.Invoke();
        }
        
        /// <summary>
        /// Unequips the weapon
        /// </summary>
        public virtual void Unequip()
        {
            isEquipped = false;
            gameObject.SetActive(false);
            OnWeaponUnequipped?.Invoke();
        }
        
        /// <summary>
        /// Adds ammo to reserve
        /// </summary>
        public void AddAmmo(int amount)
        {
            reserveAmmo = Mathf.Min(reserveAmmo + amount, maxTotalAmmo - currentAmmo);
            OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
        }
        
        /// <summary>
        /// Gets weapon stats for display
        /// </summary>
        public WeaponStats GetStats()
        {
            return new WeaponStats
            {
                Damage = damage,
                FireRate = fireRate,
                Range = range,
                MagazineSize = magazineSize,
                ReloadTime = reloadTime
            };
        }
    }
    
    /// <summary>
    /// Weapon type enumeration
    /// </summary>
    public enum WeaponType
    {
        Primary,
        Secondary
    }
    
    /// <summary>
    /// Fire mode enumeration
    /// </summary>
    public enum FireMode
    {
        Automatic,  // Hold to fire continuously
        SemiAuto,   // One shot per click
        Burst       // Multiple shots per click
    }
    
    /// <summary>
    /// Struct to hold weapon statistics
    /// </summary>
    [Serializable]
    public struct WeaponStats
    {
        public float Damage;
        public float FireRate;
        public float Range;
        public int MagazineSize;
        public float ReloadTime;
    }
}