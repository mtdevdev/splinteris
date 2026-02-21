using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private float _health = 100f;

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 15f;
    [SerializeField] private float _runningRotationOffset = 20f;
    [SerializeField] private float _walkingRotationOffset = 31f;

    [Header("References")]
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _playerModel;
    [SerializeField] private GunController _gunController; 

    [Header("Death Effects")]
    [SerializeField] private GameObject _deathEffectPrefab;
    [SerializeField] private AudioSource _deathAudioSource;

    [Header("State (Read Only)")]
    public bool IsRunning = false;
    public bool IsAlive = true;

    private Camera _mainCamera;
    private float _currentRotationOffset;

    private void Start()
    {
        if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody>();
        if (_gunController == null) _gunController = GetComponent<GunController>();
        
        _mainCamera = Camera.main;

        if (_animator != null)
        {
            _animator.SetBool("isRunning", false);
            _animator.SetBool("isAttacking", false);
        }
    }

    private void Update()
    {
        if (!IsAlive) return;
        
        HandleShootingInput();
    }

    private void FixedUpdate()
    {
        if (!IsAlive) return;

        HandleMovement();
        RotateTowardsMouse();
    }

    private void HandleShootingInput()
    {
        // Continuous fire while holding left click
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            if (_gunController != null)
            {
                _gunController.TryShoot();
            }
        }
    }

    private void HandleMovement()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.wKey.isPressed) vertical += 1;
        if (Keyboard.current.sKey.isPressed) vertical -= 1;
        if (Keyboard.current.aKey.isPressed) horizontal -= 1;
        if (Keyboard.current.dKey.isPressed) horizontal += 1;

        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        IsRunning = moveDirection.sqrMagnitude > 0.001f;
        _currentRotationOffset = IsRunning ? _runningRotationOffset : _walkingRotationOffset;

        Vector3 velocity = new Vector3(
            moveDirection.x * _moveSpeed,
            _rigidbody.linearVelocity.y,
            moveDirection.z * _moveSpeed
        );

        _rigidbody.linearVelocity = velocity;

        if (_animator != null && _rigidbody.linearVelocity.magnitude > 0f)
        {
            _animator.SetBool("isRunning", IsRunning);
        }

        if (_rigidbody.linearVelocity.magnitude < 0.2f)
        {
            _animator.SetBool("isRunning", false);
        }
    }

    private void RotateTowardsMouse()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = _mainCamera.ScreenPointToRay(mousePos);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 targetPoint = ray.GetPoint(distance);
            Vector3 direction = targetPoint - transform.position;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                Quaternion offsetRotation = Quaternion.Euler(0, _currentRotationOffset, 0);
                targetRotation *= offsetRotation;

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    _rotationSpeed * Time.fixedDeltaTime
                );
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;

        _health -= damage;
        if (_health <= 0)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        IsAlive = false;
        IsRunning = false;

        _playerModel.SetActive(false);
        _rigidbody.linearVelocity = Vector3.zero;

        if (_animator != null) 
            _animator.SetBool("isRunning", false);

        if (_deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(_deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 10f);
        }

        PlayDeathSFX();
        
        GameManager.Instance.TriggerGameOver();
    }

    private void PlayDeathSFX()
    {
        if (_deathAudioSource == null) return;

        float basePitch = Random.Range(0.8f, 1.2f);
        GameObject tempAudioObj = new GameObject("PlayerDeathSound");
        tempAudioObj.transform.position = transform.position;
        
        AudioSource newSource = tempAudioObj.AddComponent<AudioSource>();
        newSource.clip = _deathAudioSource.clip;
        newSource.outputAudioMixerGroup = _deathAudioSource.outputAudioMixerGroup;
        newSource.spatialBlend = _deathAudioSource.spatialBlend;
        newSource.playOnAwake = false;

        DeathSFXController sfxController = tempAudioObj.AddComponent<DeathSFXController>();
        sfxController.Init(newSource, basePitch);
    }

}

// Kept in the same file as requested by the original structure, but cleaned up
public class DeathSFXController : MonoBehaviour
{
    private AudioSource _audioSource;
    private const float DestroyDelay = 0.1f;

    public void Init(AudioSource source, float pitch)
    {
        _audioSource = source ?? GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = _audioSource.transform.position;
        _audioSource.pitch = pitch;
        _audioSource.Play();

        StartCoroutine(DestroyWhenFinishedRoutine());
    }

    private IEnumerator DestroyWhenFinishedRoutine()
    {
        if (_audioSource == null || _audioSource.clip == null)
        {
            Destroy(gameObject);
            yield break;
        }

        float clipLength = _audioSource.clip.length / Mathf.Max(0.0001f, Mathf.Abs(_audioSource.pitch));
        yield return new WaitForSecondsRealtime(clipLength + DestroyDelay);

        Destroy(gameObject);
    }

    private void OnDisable()
    {
        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }
}