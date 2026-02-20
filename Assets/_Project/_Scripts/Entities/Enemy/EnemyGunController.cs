using System.Collections;
using UnityEngine;

public class EnemyGunController : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1.5f; 
    [SerializeField] private float projectileForce = 20f; 
    [SerializeField] private float damage = 10f;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Light muzzleLight;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSFX;

    private float _nextFireTime;

    private void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public void TryShoot()
    {
        if (Time.time >= _nextFireTime)
        {
            Fire();
            _nextFireTime = Time.time + fireRate;
        }
    }

    private IEnumerator MuzzleLightFlash()
    {
        muzzleLight.enabled = true;
        yield return new WaitForSeconds(0.05f);
        muzzleLight.enabled = false;
    }

    private void Fire()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        if (projectile.TryGetComponent(out Projectile projScript))
        {
            projScript.SetDamage(damage);
        }
        else
        {
            var p = projectile.AddComponent<Projectile>();
            p.SetDamage(damage);
        }

        if (projectile.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = firePoint.forward * projectileForce; 
        }

        if (muzzleFlash) muzzleFlash.Play();

        if (muzzleLight)
        {
            StartCoroutine(MuzzleLightFlash());
        }
        
        if (audioSource && shootSFX)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f) * Time.timeScale;
            audioSource.PlayOneShot(shootSFX);
        }

        CameraShaker.Instance?.ShakeHeavy(0.05f);

        Destroy(projectile, 5f);
    }
}