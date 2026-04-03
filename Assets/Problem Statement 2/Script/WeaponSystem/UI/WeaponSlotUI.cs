using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ShooterGame.WeaponSystem.UI
{
    /// <summary>
    /// UI component for displaying individual weapon slot information
    /// Shows weapon icon, ammo count, and highlight state
    /// </summary>
    public class WeaponSlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image weaponIconImage;
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private GameObject highlightPanel;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI slotNumberText;
        
        [Header("Colors")]
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] private Color emptySlotColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        
        private Weapon currentWeapon;
        
        private void OnEnable()
        {
            // Subscribe to weapon events if weapon exists
            if (currentWeapon != null)
            {
                currentWeapon.OnAmmoChanged += UpdateAmmoDisplay;
            }
        }
        
        private void OnDisable()
        {
            // Unsubscribe from weapon events
            if (currentWeapon != null)
            {
                currentWeapon.OnAmmoChanged -= UpdateAmmoDisplay;
            }
        }
        
        /// <summary>
        /// Updates the slot with weapon information
        /// </summary>
        public void UpdateSlot(Weapon weapon)
        {
            // Unsubscribe from old weapon
            if (currentWeapon != null)
            {
                currentWeapon.OnAmmoChanged -= UpdateAmmoDisplay;
            }
            
            currentWeapon = weapon;
            
            // Subscribe to new weapon
            if (currentWeapon != null)
            {
                currentWeapon.OnAmmoChanged += UpdateAmmoDisplay;
            }
            
            // Update display
            UpdateDisplay();
        }
        
        /// <summary>
        /// Updates the visual display of the slot
        /// </summary>
        private void UpdateDisplay()
        {
            if (currentWeapon == null)
            {
                // Empty slot
                if (weaponIconImage != null)
                    weaponIconImage.enabled = false;
                
                if (ammoText != null)
                    ammoText.text = "";
                
                if (backgroundImage != null)
                    backgroundImage.color = emptySlotColor;
            }
            else
            {
                // Weapon equipped in slot
                if (weaponIconImage != null)
                {
                    weaponIconImage.sprite = currentWeapon.Icon;
                    weaponIconImage.enabled = currentWeapon.Icon != null;
                }
                
                UpdateAmmoDisplay(currentWeapon.CurrentAmmo, currentWeapon.ReserveAmmo);
                
                if (backgroundImage != null)
                    backgroundImage.color = inactiveColor;
            }
        }
        
        /// <summary>
        /// Updates the ammo display
        /// </summary>
        private void UpdateAmmoDisplay(int current, int reserve)
        {
            if (ammoText != null)
            {
                ammoText.text = $"{current}/{reserve}";
            }
        }
        
        /// <summary>
        /// Sets whether this slot is highlighted (active)
        /// </summary>
        public void SetHighlight(bool isActive)
        {
            if (highlightPanel != null)
                highlightPanel.SetActive(isActive);
            
            if (backgroundImage != null && currentWeapon != null)
            {
                backgroundImage.color = isActive ? activeColor : inactiveColor;
            }
        }
        
        /// <summary>
        /// Sets the slot number display
        /// </summary>
        public void SetSlotNumber(int number)
        {
            if (slotNumberText != null)
                slotNumberText.text = number.ToString();
        }
    }
}