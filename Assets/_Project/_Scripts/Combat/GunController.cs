using System.Collections;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireForce = 20f;
    [SerializeField] private float _projectileLifeTime = 2f;
    [SerializeField] private float _damage = 10f;

    [Header("Fire Rate Settings")]
    [SerializeField] private float _fireRate = 0.3f;
    private float _nextFireTime;

    [Header("VFX / SFX")]
    [SerializeField] private ParticleSystem _muzzleFlashParticle;
    [SerializeField] private Light _muzzleLight;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _shootSound;

    private void Start()
    {
        if (_audioSource == null) 
            _audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Attempts to fire a projectile if the fire rate cooldown has passed.
    /// Called by Player and Enemy AI.
    /// </summary>
    public void TryShoot()
    {
        if (Time.time >= _nextFireTime)
        {
            Fire();
            _nextFireTime = Time.time + _fireRate;
        }
    }

    private void Fire()
    {
        if (_projectilePrefab == null || _firePoint == null) return;

        // Create Projectile
        GameObject projectile = Instantiate(_projectilePrefab, _firePoint.position, _firePoint.rotation);

        // Setup Projectile Component
        if (!projectile.TryGetComponent(out Projectile projScript))
        {
            projScript = projectile.AddComponent<Projectile>();
        }
        projScript.SetDamage(_damage);

        // Apply Physics
        if (projectile.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = _firePoint.forward * _fireForce;
        }

        // Play Visual Effects
        if (_muzzleFlashParticle != null) 
            _muzzleFlashParticle.Play();

        if (_muzzleLight != null) 
            StartCoroutine(MuzzleLightFlashRoutine());
        
        // Play Audio
        if (_audioSource != null && _shootSound != null)
        {
            _audioSource.pitch = Random.Range(0.9f, 1.1f) * Time.timeScale;
            _audioSource.PlayOneShot(_shootSound);
        }

        // Camera Shake
        if (CameraShaker.Instance != null)
        {
            CameraShaker.Instance.ShakeHeavy(0.05f);
        }

        Destroy(projectile, _projectileLifeTime);
    }

    private IEnumerator MuzzleLightFlashRoutine()
    {
        _muzzleLight.enabled = true;
        yield return new WaitForSeconds(0.05f);
        _muzzleLight.enabled = false;
    }
}