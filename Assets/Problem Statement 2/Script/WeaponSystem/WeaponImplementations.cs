using UnityEngine;

namespace ShooterGame.WeaponSystem
{
    /// <summary>
    /// Example implementation: Assault Rifle
    /// Primary weapon with automatic fire
    /// </summary>
    public class AssaultRifle : Weapon
    {
        [Header("Rifle Specific")]
        [SerializeField] private GameObject muzzleFlash;
        [SerializeField] private Transform firePoint;
        [SerializeField] private LayerMask hitLayers;
        
        protected override void ExecuteFireBehavior()
        {
            // Raycast for hitscan weapon
            if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, GetStats().Range, hitLayers))
            {
                // Process hit
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(GetStats().Damage);
                }
                
                // Spawn impact effect at hit point
                Debug.DrawLine(firePoint.position, hit.point, Color.red, 0.1f);
            }
            
            // Visual effects
            if (muzzleFlash != null)
            {
                muzzleFlash.SetActive(false);
                muzzleFlash.SetActive(true);
            }
        }
    }
    
    /// <summary>
    /// Example implementation: Pistol
    /// Secondary weapon with semi-auto fire
    /// </summary>
    public class Pistol : Weapon
    {
        [Header("Pistol Specific")]
        [SerializeField] private GameObject muzzleFlash;
        [SerializeField] private Transform firePoint;
        [SerializeField] private LayerMask hitLayers;
        
        protected override void ExecuteFireBehavior()
        {
            // Similar to rifle but with different stats
            if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, GetStats().Range, hitLayers))
            {
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(GetStats().Damage);
                }
                
                Debug.DrawLine(firePoint.position, hit.point, Color.yellow, 0.1f);
            }
            
            if (muzzleFlash != null)
            {
                muzzleFlash.SetActive(false);
                muzzleFlash.SetActive(true);
            }
        }
    }
    
    /// <summary>
    /// Example implementation: Shotgun
    /// Fires multiple projectiles in a spread pattern
    /// </summary>
    public class Shotgun : Weapon
    {
        [Header("Shotgun Specific")]
        [SerializeField] private int pelletsPerShot = 8;
        [SerializeField] private float spreadAngle = 5f;
        [SerializeField] private Transform firePoint;
        [SerializeField] private LayerMask hitLayers;
        
        protected override void ExecuteFireBehavior()
        {
            // Fire multiple pellets
            for (int i = 0; i < pelletsPerShot; i++)
            {
                // Calculate spread
                float randomSpreadX = Random.Range(-spreadAngle, spreadAngle);
                float randomSpreadY = Random.Range(-spreadAngle, spreadAngle);
                
                Vector3 spreadDirection = Quaternion.Euler(randomSpreadX, randomSpreadY, 0) * firePoint.forward;
                
                if (Physics.Raycast(firePoint.position, spreadDirection, out RaycastHit hit, GetStats().Range, hitLayers))
                {
                    IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        // Each pellet does damage divided by total pellets
                        damageable.TakeDamage(GetStats().Damage / pelletsPerShot);
                    }
                    
                    Debug.DrawLine(firePoint.position, hit.point, Color.blue, 0.1f);
                }
            }
        }
    }
    
    /// <summary>
    /// Interface for entities that can take damage
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float damage);
    }
}