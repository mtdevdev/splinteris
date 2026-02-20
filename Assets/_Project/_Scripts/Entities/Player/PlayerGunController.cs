using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGunController : MonoBehaviour
{

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootForce = 500f;
    [SerializeField] private float projectileLifeTime = 2f;
    [SerializeField] private float projectileDamage = 10f;

    [Header("Fire Rate")]
    [SerializeField] private float timeBetweenShots = 0.3f;

    [Header("VFX / SFX")]
    [SerializeField] private ParticleSystem shootParticleEffect;
    [SerializeField] private Light muzzleFlashLight;
    [SerializeField] private AudioSource shotSound;

    [Header("References")]
    public Player player;

    private float shotTimer;
    private bool canShoot = true;

    void Start()
    {
        player = GetComponent<Player>();

        shotTimer = 0f;
    }

    void Update()
    {
        if (!player.isAlive) return;

        shotTimer += Time.deltaTime;

        if (shotTimer >= timeBetweenShots && canShoot)
        {
            if ((Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame))
            {
                Shoot();
            }
        }
    }

    void Shoot()
    {
        CreateProjectile();
        PlayShootSFX();
        shootParticleEffect.Play();
        StartCoroutine(MuzzleFlashCoroutine());

        CameraShaker.Instance?.ShakeHeavy(0.05f);
        
        shotTimer = 0f;
    }

    private IEnumerator MuzzleFlashCoroutine()
    {
        if (muzzleFlashLight != null)
        {
            muzzleFlashLight.enabled = true;
            yield return new WaitForSeconds(0.05f);
            muzzleFlashLight.enabled = false;
        }
    }

    private void CreateProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);

        Projectile projectileScript = projectile.GetComponent<Projectile>();
        projectileScript = projectile.AddComponent<Projectile>();
        projectileScript.SetDamage(projectileDamage);

        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        projectileRb.AddForce(shootPoint.forward * shootForce);

        Destroy(projectile, projectileLifeTime);
    }

    private void PlayShootSFX()
    {
        GameObject tempAudio = new GameObject("ShotSound");
        tempAudio.transform.position = shootPoint.position;

        AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
        tempSource.clip = shotSound.clip;
        tempSource.outputAudioMixerGroup = shotSound.outputAudioMixerGroup;
        tempSource.spatialBlend = shotSound.spatialBlend;

        float randomPitch = Random.Range(0.8f, 1.2f) * Time.timeScale;
        tempSource.pitch = randomPitch;

        tempSource.Play();
        Destroy(tempAudio, tempSource.clip.length / Mathf.Abs(randomPitch));
    }

}

