using UnityEngine;
using System;
using System.Collections.Generic;

namespace ShooterGame.WeaponSystem
{
    /// <summary>
    /// Player class that manages weapon inventory and equipped weapons
    /// Handles weapon switching, firing, and reloading
    /// </summary>
    public class Player : MonoBehaviour
    {
        [Header("Weapon Slots")]
        [SerializeField] private Transform weaponHolder; // Parent transform for weapon GameObjects
        
        // Weapon inventory - 2 primary, 1 secondary
        private Weapon primaryWeapon1;
        private Weapon primaryWeapon2;
        private Weapon secondaryWeapon;
        
        // Current state
        private Weapon currentlyEquippedWeapon;
        private int currentWeaponIndex = 0; // 0 = primary1, 1 = primary2, 2 = secondary
        
        // Input state
        private bool isFiring;
        
        // Events for UI and other systems
        public event Action<Weapon> OnWeaponSwitched;
        public event Action<Weapon, int> OnWeaponSlotChanged; // weapon, slotIndex
        
        // Properties
        public Weapon CurrentWeapon => currentlyEquippedWeapon;
        public Weapon PrimaryWeapon1 => primaryWeapon1;
        public Weapon PrimaryWeapon2 => primaryWeapon2;
        public Weapon SecondaryWeapon => secondaryWeapon;
        
        private void Start()
        {
            // Equip first available weapon
            if (primaryWeapon1 != null)
            {
                EquipWeaponAtIndex(0);
            }
            else if (primaryWeapon2 != null)
            {
                EquipWeaponAtIndex(1);
            }
            else if (secondaryWeapon != null)
            {
                EquipWeaponAtIndex(2);
            }
        }
        
        private void Update()
        {
            HandleWeaponInput();
        }
        
        /// <summary>
        /// Handles player input for weapons
        /// </summary>
        private void HandleWeaponInput()
        {
            // Fire input
            if (currentlyEquippedWeapon != null)
            {
                if (currentlyEquippedWeapon.Mode == FireMode.Automatic)
                {
                    if (Input.GetMouseButton(0)) // Hold for automatic
                    {
                        Fire();
                    }
                }
                else // Semi-auto or burst
                {
                    if (Input.GetMouseButtonDown(0)) // Click for semi-auto
                    {
                        Fire();
                    }
                }
            }
            
            // Reload input
            if (Input.GetKeyDown(KeyCode.R))
            {
                Reload();
            }
            
            // Weapon switching with number keys
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SwitchToWeapon(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SwitchToWeapon(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SwitchToWeapon(2);
            }
            
            // Scroll wheel weapon switching
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f)
            {
                SwitchToNextWeapon();
            }
            else if (scroll < 0f)
            {
                SwitchToPreviousWeapon();
            }
        }
        
        /// <summary>
        /// Fires the currently equipped weapon
        /// </summary>
        public void Fire()
        {
            if (currentlyEquippedWeapon != null)
            {
                currentlyEquippedWeapon.TryFire();
            }
        }
        
        /// <summary>
        /// Reloads the currently equipped weapon
        /// </summary>
        public void Reload()
        {
            if (currentlyEquippedWeapon != null)
            {
                currentlyEquippedWeapon.StartReload();
            }
        }
        
        /// <summary>
        /// Equips a weapon in the specified slot (0 = primary1, 1 = primary2, 2 = secondary)
        /// </summary>
        public bool EquipWeapon(Weapon weapon, int slotIndex)
        {
            if (weapon == null)
                return false;
            
            // Unequip current weapon in slot if exists
            Weapon existingWeapon = GetWeaponAtIndex(slotIndex);
            if (existingWeapon != null)
            {
                existingWeapon.Unequip();
                Destroy(existingWeapon.gameObject);
            }
            
            // Set weapon to appropriate slot
            switch (slotIndex)
            {
                case 0:
                    if (weapon.Type != WeaponType.Primary)
                        return false;
                    primaryWeapon1 = weapon;
                    break;
                case 1:
                    if (weapon.Type != WeaponType.Primary)
                        return false;
                    primaryWeapon2 = weapon;
                    break;
                case 2:
                    if (weapon.Type != WeaponType.Secondary)
                        return false;
                    secondaryWeapon = weapon;
                    break;
                default:
                    return false;
            }
            
            // Parent weapon to weapon holder
            weapon.transform.SetParent(weaponHolder);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
            
            // Unequip initially
            weapon.Unequip();
            
            // Notify listeners
            OnWeaponSlotChanged?.Invoke(weapon, slotIndex);
            
            return true;
        }
        
        /// <summary>
        /// Switches to weapon at specified index
        /// </summary>
        public void SwitchToWeapon(int index)
        {
            Weapon targetWeapon = GetWeaponAtIndex(index);
            
            if (targetWeapon == null || targetWeapon == currentlyEquippedWeapon)
                return;
            
            EquipWeaponAtIndex(index);
        }
        
        /// <summary>
        /// Switches to the next available weapon
        /// </summary>
        public void SwitchToNextWeapon()
        {
            int startIndex = currentWeaponIndex;
            int nextIndex = (currentWeaponIndex + 1) % 3;
            
            // Find next available weapon
            while (nextIndex != startIndex)
            {
                if (GetWeaponAtIndex(nextIndex) != null)
                {
                    EquipWeaponAtIndex(nextIndex);
                    return;
                }
                nextIndex = (nextIndex + 1) % 3;
            }
        }
        
        /// <summary>
        /// Switches to the previous available weapon
        /// </summary>
        public void SwitchToPreviousWeapon()
        {
            int startIndex = currentWeaponIndex;
            int prevIndex = currentWeaponIndex - 1;
            if (prevIndex < 0) prevIndex = 2;
            
            // Find previous available weapon
            while (prevIndex != startIndex)
            {
                if (GetWeaponAtIndex(prevIndex) != null)
                {
                    EquipWeaponAtIndex(prevIndex);
                    return;
                }
                prevIndex--;
                if (prevIndex < 0) prevIndex = 2;
            }
        }
        
        /// <summary>
        /// Equips weapon at the specified index
        /// </summary>
        private void EquipWeaponAtIndex(int index)
        {
            Weapon targetWeapon = GetWeaponAtIndex(index);
            
            if (targetWeapon == null)
                return;
            
            // Unequip current weapon
            if (currentlyEquippedWeapon != null)
            {
                currentlyEquippedWeapon.Unequip();
            }
            
            // Equip new weapon
            currentlyEquippedWeapon = targetWeapon;
            currentWeaponIndex = index;
            currentlyEquippedWeapon.Equip();
            
            // Notify listeners
            OnWeaponSwitched?.Invoke(currentlyEquippedWeapon);
        }
        
        /// <summary>
        /// Gets weapon at specified index
        /// </summary>
        private Weapon GetWeaponAtIndex(int index)
        {
            switch (index)
            {
                case 0: return primaryWeapon1;
                case 1: return primaryWeapon2;
                case 2: return secondaryWeapon;
                default: return null;
            }
        }
        
        /// <summary>
        /// Gets all equipped weapons
        /// </summary>
        public List<Weapon> GetAllWeapons()
        {
            List<Weapon> weapons = new List<Weapon>();
            
            if (primaryWeapon1 != null)
                weapons.Add(primaryWeapon1);
            if (primaryWeapon2 != null)
                weapons.Add(primaryWeapon2);
            if (secondaryWeapon != null)
                weapons.Add(secondaryWeapon);
            
            return weapons;
        }
        
        /// <summary>
        /// Removes weapon from specified slot
        /// </summary>
        public void RemoveWeapon(int slotIndex)
        {
            Weapon weaponToRemove = GetWeaponAtIndex(slotIndex);
            
            if (weaponToRemove == null)
                return;
            
            // If removing currently equipped weapon, switch to another
            if (weaponToRemove == currentlyEquippedWeapon)
            {
                SwitchToNextWeapon();
            }
            
            weaponToRemove.Unequip();
            Destroy(weaponToRemove.gameObject);
            
            switch (slotIndex)
            {
                case 0: primaryWeapon1 = null; break;
                case 1: primaryWeapon2 = null; break;
                case 2: secondaryWeapon = null; break;
            }
            
            OnWeaponSlotChanged?.Invoke(null, slotIndex);
        }
    }
}