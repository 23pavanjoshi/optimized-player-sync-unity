using UnityEngine;
using System.Collections.Generic;

namespace ShooterGame.WeaponSystem
{
    /// <summary>
    /// Central manager for weapon system
    /// Handles weapon spawning, pooling, and configuration
    /// Implements factory pattern for weapon creation
    /// </summary>
    public class WeaponManager : MonoBehaviour
    {
        [Header("Weapon Prefabs")]
        [SerializeField] private List<WeaponConfig> weaponConfigs = new List<WeaponConfig>();
        
        private static WeaponManager instance;
        public static WeaponManager Instance => instance;
        
        private Dictionary<string, WeaponConfig> weaponConfigDict;
        
        private void Awake()
        {
            // Singleton pattern
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize weapon config dictionary
            InitializeWeaponConfigs();
        }
        
        /// <summary>
        /// Initializes the weapon configuration dictionary
        /// </summary>
        private void InitializeWeaponConfigs()
        {
            weaponConfigDict = new Dictionary<string, WeaponConfig>();
            
            foreach (var config in weaponConfigs)
            {
                if (config != null && !string.IsNullOrEmpty(config.weaponID))
                {
                    weaponConfigDict[config.weaponID] = config;
                }
            }
        }
        
        /// <summary>
        /// Creates a weapon instance by ID
        /// </summary>
        public Weapon CreateWeapon(string weaponID, Transform parent = null)
        {
            if (!weaponConfigDict.ContainsKey(weaponID))
            {
                Debug.LogError($"Weapon with ID '{weaponID}' not found!");
                return null;
            }
            
            WeaponConfig config = weaponConfigDict[weaponID];
            
            if (config.weaponPrefab == null)
            {
                Debug.LogError($"Weapon prefab for '{weaponID}' is null!");
                return null;
            }
            
            // Instantiate weapon
            GameObject weaponObj = Instantiate(config.weaponPrefab, parent);
            Weapon weapon = weaponObj.GetComponent<Weapon>();
            
            if (weapon == null)
            {
                Debug.LogError($"Weapon prefab '{weaponID}' does not have a Weapon component!");
                Destroy(weaponObj);
                return null;
            }
            
            return weapon;
        }
        
        /// <summary>
        /// Gets weapon configuration by ID
        /// </summary>
        public WeaponConfig GetWeaponConfig(string weaponID)
        {
            weaponConfigDict.TryGetValue(weaponID, out WeaponConfig config);
            return config;
        }
        
        /// <summary>
        /// Gets all available weapon IDs
        /// </summary>
        public List<string> GetAllWeaponIDs()
        {
            return new List<string>(weaponConfigDict.Keys);
        }
        
        /// <summary>
        /// Equips a weapon on a player by ID
        /// </summary>
        public bool EquipWeaponOnPlayer(Player player, string weaponID, int slotIndex)
        {
            if (player == null)
            {
                Debug.LogError("Player is null!");
                return false;
            }
            
            Weapon weapon = CreateWeapon(weaponID, player.transform);
            
            if (weapon == null)
                return false;
            
            return player.EquipWeapon(weapon, slotIndex);
        }
    }
    
    /// <summary>
    /// Configuration data for a weapon
    /// </summary>
    [System.Serializable]
    public class WeaponConfig
    {
        public string weaponID;
        public string weaponName;
        public GameObject weaponPrefab;
        public Sprite weaponIcon;
        public WeaponType weaponType;
        [TextArea(3, 5)]
        public string description;
    }
}