using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] float health = 100f;

    [Header("Movement Settings")]
    [SerializeField] Rigidbody rigidBody;
    [SerializeField] Animator animator;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 15f;

    [Header("Time Scale")]
    [SerializeField] float slowTimeScale = 0.2f;
    [SerializeField] float timeScaleSmoothSpeed = 3f;
    private float defaultFixedDeltaTime;

    [Header("UI")]
    [SerializeField] GameOver gameOver;

    [Header("Death Effects")]
    [SerializeField] GameObject deathEffect;
    [SerializeField] AudioSource deathSound;

    [Header("Player States")]
    public bool isRunning = false;
    public bool isAlive = true;
    public bool gameWon = false;

    [Header("References")]
    [SerializeField] GameObject playerModel;
    [SerializeField] DeathSFXController deathSFXController;

    private Camera mainCamera;

    private float mouseRotationOffset;

    private List<Transform> enemies = new List<Transform>();

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        defaultFixedDeltaTime = Time.fixedDeltaTime;

        // Initialize animator states
        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isAttacking", false);
        }
    }

    void Update()
    {
       //if (!isAlive) return;
       UpdateTimeScale();
    }

    void FixedUpdate()
    {
        if (!isAlive) return;

        // Physics-related updates
        Move();
        RotateTowardsMouse();
    }

    public void GameWon()
    {
        gameWon = true;
        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * slowTimeScale;
    }

    void HandlePlayerDeath()
    {

        playerModel.SetActive(false);
        
        rigidBody.linearVelocity = Vector3.zero;

        isAlive = false;
        isRunning = false;

        animator.SetBool("isRunning", false);

        if (deathEffect != null)
        {
            var effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(effect, 10f);
        }

        PlayDeathSFX();
        
        gameOver.ShowGameOverScreen();

    }

    private void PlayDeathSFX()
    {
        if (deathSound == null) return;

        float basePitch = Random.Range(0.8f, 1.2f);

        var tempAudio = new GameObject("DeathSound") { transform = { position = transform.position } };
        var aSource = tempAudio.AddComponent<AudioSource>();

        aSource.clip = deathSound.clip;
        aSource.outputAudioMixerGroup = deathSound.outputAudioMixerGroup;
        aSource.spatialBlend = deathSound.spatialBlend;
        aSource.playOnAwake = false;

        tempAudio.AddComponent<DeathSFXController>().Init(aSource, basePitch);
    }

    void Move()
    {
        float h = 0f;
        float v = 0f;

        if (isRunning) mouseRotationOffset = 20f;
        else mouseRotationOffset = 31f;

        if (Keyboard.current.wKey.isPressed) v += 1;
        if (Keyboard.current.sKey.isPressed) v -= 1;
        if (Keyboard.current.aKey.isPressed) h -= 1;
        if (Keyboard.current.dKey.isPressed) h += 1;

        Vector3 moveDirection = new Vector3(h, 0f, v).normalized;
        isRunning = moveDirection.sqrMagnitude > 0.001f;

        if (animator != null) animator.SetBool("isRunning", isRunning);

        Vector3 velocity = new Vector3(
            moveDirection.x * moveSpeed,
            rigidBody.linearVelocity.y,
            moveDirection.z * moveSpeed
        );

        rigidBody.linearVelocity = velocity;
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.7f)
            {
                // Prevent running animation from getting stuck
                if (animator != null)
                {
                    animator.SetBool("isRunning", isRunning);
                }

                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Stop running animation when leaving the ground
        if (animator != null)
        {
            animator.SetBool("isRunning", false);
        }
    }
    
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            HandlePlayerDeath();

            Debug.Log("Player has died.");
        }
    }

    void RotateTowardsMouse()
    {
        // Convert mouse position to a world point
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 targetPoint = ray.GetPoint(distance);
            Vector3 direction = targetPoint - transform.position;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Apply rotation offset
                Quaternion offset = Quaternion.Euler(0, mouseRotationOffset, 0);
                targetRotation *= offset;

                // Smooth rotation towards mouse direction
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }

    void UpdateTimeScale()
    {
        if (gameWon) return;
        
        // Smoothly interpolate Time.timeScale based on whether the player is running
        float target = isRunning ? 1f : slowTimeScale;
        Time.timeScale = Mathf.Lerp(Time.timeScale, target, timeScaleSmoothSpeed * Time.unscaledDeltaTime);

        // Snap to target if very close to avoid tiny lingering differences
        if (Mathf.Abs(Time.timeScale - target) < 0.001f)
            Time.timeScale = target;

        // Keep fixedDeltaTime in sync with timeScale so physics behaves correctly
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
    }

    void OnDisable()
    {
        // Restore time scale when the player controller is disabled
        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
    }

    void OnDestroy()
    {
        // Extra safety to restore values when the scene closes / object is destroyed
        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
    }
}

public class DeathSFXController : MonoBehaviour
{
    AudioSource audioSource;
    float destroyDelay = 0.1f;

    public void Init(AudioSource source, float pitch)
    {
        audioSource = source ?? GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Destroy(gameObject);
            return;
        }

        // Ensure this object stays at the audio source position
        transform.position = audioSource.transform.position;

        audioSource.pitch = pitch;
        audioSource.Play();

        // Start lifecycle coroutine using real time to avoid being affected by Time.timeScale
        StartCoroutine(DestroyWhenFinished());
    }

    IEnumerator DestroyWhenFinished()
    {
        if (audioSource == null)
        {
            Destroy(gameObject);
            yield break;
        }

        if (audioSource.clip == null || audioSource.clip.length <= 0f)
        {
            Destroy(gameObject);
            yield break;
        }

        float length = audioSource.clip.length / Mathf.Max(0.0001f, Mathf.Abs(audioSource.pitch));
        // Use realtime wait so this isn't slowed by Time.timeScale
        yield return new WaitForSecondsRealtime(length + destroyDelay);

        Destroy(gameObject);
    }

    void OnDisable()
    {
        // Cleanup if component is disabled for any reason
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}