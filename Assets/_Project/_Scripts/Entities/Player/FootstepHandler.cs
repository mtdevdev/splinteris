using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class FootstepHandler : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _footstepClips;
    [SerializeField] private float _stepInterval = 0.5f;
    [SerializeField, Range(0f, 0.5f)] private float _pitchVariance = 0.1f; 

    [Header("References")]
    [SerializeField] private Player _player;

    private float _stepTimer;
    private Rigidbody _playerRigidbody;

    private void Start()
    {
        _playerRigidbody = GetComponent<Rigidbody>();
    }

    private void Awake()
    {
        if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
        if (_player == null) _player = GetComponent<Player>();
        
        // Starts at interval so the first step plays immediately when moving
        _stepTimer = _stepInterval;
    }

    private void Update()
    {
        if (_player != null && !_player.IsAlive) return;

        if (IsMoving())
        {
            _stepTimer += Time.deltaTime;

            if (_stepTimer >= _stepInterval)
            {
                PlayFootstep();
                _stepTimer = 0f;
            }
        }
        else
        {
            // Reset timer when stopped
            _stepTimer = _stepInterval;
        }
    }

    private void PlayFootstep()
    {
        _audioSource.pitch = 1f + Random.Range(-_pitchVariance, _pitchVariance);

        if (_footstepClips != null && _footstepClips.Length > 0)
        {
            AudioClip randomClip = _footstepClips[Random.Range(0, _footstepClips.Length)];
            _audioSource.PlayOneShot(randomClip);
        }
        else
        {
            _audioSource.Play();
        }
    }

    private bool IsMoving()
    {
        return _playerRigidbody.linearVelocity.magnitude > 0.1f; 
    }

}
