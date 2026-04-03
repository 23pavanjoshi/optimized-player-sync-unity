using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ShooterGame.WeaponSystem.UI
{
    /// <summary>
    /// HUD controller that displays weapon information
    /// Listens to Player and Weapon events to update UI
    /// </summary>
    public class WeaponHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Player player;
        
        [Header("Current Weapon Display")]
        [SerializeField] private TextMeshProUGUI weaponNameText;
        [SerializeField] private Image weaponIconImage;
        [SerializeField] private TextMeshProUGUI currentAmmoText;
        [SerializeField] private TextMeshProUGUI reserveAmmoText;
        [SerializeField] private GameObject reloadingIndicator;
        
        [Header("Weapon Slots Display")]
        [SerializeField] private WeaponSlotUI[] weaponSlots = new WeaponSlotUI[3];
        
        [Header("Fire Mode Display")]
        [SerializeField] private TextMeshProUGUI fireModeText;
        
        private Weapon currentWeapon;
        
        private void OnEnable()
        {
            if (player != null)
            {
                // Subscribe to player events
                player.OnWeaponSwitched += HandleWeaponSwitched;
                player.OnWeaponSlotChanged += HandleWeaponSlotChanged;
            }
        }
        
        private void OnDisable()
        {
            if (player != null)
            {
                // Unsubscribe from player events
                player.OnWeaponSwitched -= HandleWeaponSwitched;
                player.OnWeaponSlotChanged -= HandleWeaponSlotChanged;
            }
            
            // Unsubscribe from current weapon events
            UnsubscribeFromWeapon(currentWeapon);
        }
        
        private void Start()
        {
            // Initialize all weapon slots
            InitializeWeaponSlots();
            
            // Update current weapon display
            if (player != null && player.CurrentWeapon != null)
            {
                HandleWeaponSwitched(player.CurrentWeapon);
            }
        }
        
        /// <summary>
        /// Initializes the weapon slot displays
        /// </summary>
        private void InitializeWeaponSlots()
        {
            if (player == null)
                return;
            
            UpdateWeaponSlot(0, player.PrimaryWeapon1);
            UpdateWeaponSlot(1, player.PrimaryWeapon2);
            UpdateWeaponSlot(2, player.SecondaryWeapon);
        }
        
        /// <summary>
        /// Handles weapon switch event
        /// </summary>
        private void HandleWeaponSwitched(Weapon newWeapon)
        {
            // Unsubscribe from old weapon
            UnsubscribeFromWeapon(currentWeapon);
            
            // Update current weapon reference
            currentWeapon = newWeapon;
            
            // Subscribe to new weapon events
            if (currentWeapon != null)
            {
                currentWeapon.OnAmmoChanged += UpdateAmmoDisplay;
                currentWeapon.OnReloadStarted += ShowReloadIndicator;
                currentWeapon.OnReloadCompleted += HideReloadIndicator;
                currentWeapon.OnWeaponFired += OnWeaponFired;
            }
            
            // Update UI
            UpdateCurrentWeaponDisplay();
            UpdateWeaponSlotHighlight();
        }
        
        /// <summary>
        /// Handles weapon slot change event
        /// </summary>
        private void HandleWeaponSlotChanged(Weapon weapon, int slotIndex)
        {
            UpdateWeaponSlot(slotIndex, weapon);
        }
        
        /// <summary>
        /// Updates the current weapon display
        /// </summary>
        private void UpdateCurrentWeaponDisplay()
        {
            if (currentWeapon == null)
            {
                // No weapon equipped
                if (weaponNameText != null)
                    weaponNameText.text = "No Weapon";
                if (weaponIconImage != null)
                    weaponIconImage.enabled = false;
                if (currentAmmoText != null)
                    currentAmmoText.text = "--";
                if (reserveAmmoText != null)
                    reserveAmmoText.text = "--";
                if (fireModeText != null)
                    fireModeText.text = "";
                return;
            }
            
            // Update weapon name
            if (weaponNameText != null)
                weaponNameText.text = currentWeapon.WeaponName;
            
            // Update weapon icon
            if (weaponIconImage != null && currentWeapon.Icon != null)
            {
                weaponIconImage.sprite = currentWeapon.Icon;
                weaponIconImage.enabled = true;
            }
            
            // Update fire mode
            if (fireModeText != null)
                fireModeText.text = currentWeapon.Mode.ToString();
            
            // Update ammo
            UpdateAmmoDisplay(currentWeapon.CurrentAmmo, currentWeapon.ReserveAmmo);
            
            // Hide reload indicator initially
            if (reloadingIndicator != null)
                reloadingIndicator.SetActive(false);
        }
        
        /// <summary>
        /// Updates the ammo display
        /// </summary>
        private void UpdateAmmoDisplay(int current, int reserve)
        {
            if (currentAmmoText != null)
                currentAmmoText.text = current.ToString();
            
            if (reserveAmmoText != null)
                reserveAmmoText.text = reserve.ToString();
            
            // Change color if low on ammo
            if (currentAmmoText != null)
            {
                if (currentWeapon != null)
                {
                    float ammoPercentage = (float)current / currentWeapon.MagazineSize;
                    if (ammoPercentage <= 0.25f)
                        currentAmmoText.color = Color.red;
                    else if (ammoPercentage <= 0.5f)
                        currentAmmoText.color = Color.yellow;
                    else
                        currentAmmoText.color = Color.white;
                }
            }
        }
        
        /// <summary>
        /// Shows the reloading indicator
        /// </summary>
        private void ShowReloadIndicator()
        {
            if (reloadingIndicator != null)
                reloadingIndicator.SetActive(true);
        }
        
        /// <summary>
        /// Hides the reloading indicator
        /// </summary>
        private void HideReloadIndicator()
        {
            if (reloadingIndicator != null)
                reloadingIndicator.SetActive(false);
        }
        
        /// <summary>
        /// Called when weapon is fired (for visual feedback)
        /// </summary>
        private void OnWeaponFired()
        {
            // Could add muzzle flash animation or screen shake here
        }
        
        /// <summary>
        /// Updates a specific weapon slot display
        /// </summary>
        private void UpdateWeaponSlot(int slotIndex, Weapon weapon)
        {
            if (slotIndex < 0 || slotIndex >= weaponSlots.Length)
                return;
            
            if (weaponSlots[slotIndex] != null)
            {
                weaponSlots[slotIndex].UpdateSlot(weapon);
            }
        }
        
        /// <summary>
        /// Updates which weapon slot is highlighted as active
        /// </summary>
        private void UpdateWeaponSlotHighlight()
        {
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (weaponSlots[i] != null)
                {
                    Weapon slotWeapon = null;
                    if (player != null)
                    {
                        switch (i)
                        {
                            case 0: slotWeapon = player.PrimaryWeapon1; break;
                            case 1: slotWeapon = player.PrimaryWeapon2; break;
                            case 2: slotWeapon = player.SecondaryWeapon; break;
                        }
                    }
                    
                    bool isActive = slotWeapon == currentWeapon;
                    weaponSlots[i].SetHighlight(isActive);
                }
            }
        }
        
        /// <summary>
        /// Unsubscribes from weapon events
        /// </summary>
        private void UnsubscribeFromWeapon(Weapon weapon)
        {
            if (weapon != null)
            {
                weapon.OnAmmoChanged -= UpdateAmmoDisplay;
                weapon.OnReloadStarted -= ShowReloadIndicator;
                weapon.OnReloadCompleted -= HideReloadIndicator;
                weapon.OnWeaponFired -= OnWeaponFired;
            }
        }
        
        /// <summary>
        /// Sets the player reference (useful for runtime setup)
        /// </summary>
        public void SetPlayer(Player newPlayer)
        {
            // Unsubscribe from old player
            if (player != null)
            {
                player.OnWeaponSwitched -= HandleWeaponSwitched;
                player.OnWeaponSlotChanged -= HandleWeaponSlotChanged;
            }
            
            // Unsubscribe from current weapon
            UnsubscribeFromWeapon(currentWeapon);
            
            // Set new player
            player = newPlayer;
            
            // Subscribe to new player
            if (player != null)
            {
                player.OnWeaponSwitched += HandleWeaponSwitched;
                player.OnWeaponSlotChanged += HandleWeaponSlotChanged;
                
                // Initialize display
                InitializeWeaponSlots();
                if (player.CurrentWeapon != null)
                {
                    HandleWeaponSwitched(player.CurrentWeapon);
                }
            }
        }
    }
}