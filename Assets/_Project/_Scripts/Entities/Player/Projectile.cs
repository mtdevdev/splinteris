using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public ParticleSystem trailParticle;
    public TrailRenderer trailRenderer;

    private float damage;

    public void SetDamage(float damageAmount)
    {
        damage = damageAmount;
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
                enemy.TakeDamage(damage);
            }
        }
        else if (hitObject.CompareTag("Player"))
        {
            Player player = hitObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }

        // Destroy the bullet on any collision
        detachParticles();
        Destroy(gameObject);
    }

    private void detachParticles()
    {
        if (trailParticle != null)
        {
            trailParticle.transform.parent = null;
            trailParticle.Stop();
            Destroy(trailParticle.gameObject, 2f);
        }

        if (trailRenderer != null)
        {
            trailRenderer.transform.parent = null;
            trailRenderer.emitting = false;
            Destroy(trailRenderer.gameObject, 2f);
        }
    }
}