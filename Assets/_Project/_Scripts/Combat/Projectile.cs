using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private ParticleSystem _trailParticle;
    [SerializeField] private TrailRenderer _trailRenderer;

    private float _damage;

    public void SetDamage(float damageAmount)
    {
        _damage = damageAmount;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject hitObject = collision.gameObject;

        // Check tags and apply damage accordingly
        if (hitObject.CompareTag("Enemy"))
        {
            Enemy enemy = hitObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(_damage);
            }
        }
        else if (hitObject.CompareTag("Player"))
        {
            Player player = hitObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(_damage);
            }
        }

        // Destroy the bullet on any collision
        detachParticles();
        Destroy(gameObject);
    }

    private void detachParticles()
    {
        if (_trailParticle != null)
        {
            _trailParticle.transform.parent = null;
            _trailParticle.Stop();
            Destroy(_trailParticle.gameObject, 2f);
        }

        if (_trailRenderer != null)
        {
            _trailRenderer.transform.parent = null;
            _trailRenderer.emitting = false;
            Destroy(_trailRenderer.gameObject, 2f);
        }
    }
}