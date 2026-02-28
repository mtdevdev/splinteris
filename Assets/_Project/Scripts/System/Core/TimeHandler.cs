using System.Collections;
using UnityEngine;

public class TimeHandler : MonoBehaviour
{
    [Header("Time Scale Settings")]
    [SerializeField] private float _slowTimeScale = 0.2f;
    [SerializeField] private float _timeScaleSmoothSpeed = 3f;

    private Player _player;
    private AudioSource _audioSource;
    private float _basePitch = 1f;
    private float _defaultFixedDeltaTime;

    private void Start()
    {
        // Get components
        _player = GetComponent<Player>();

        // Store default value to scale physics correctly
        _defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void Update()
    {
        if (_player == null) return;

        UpdateTimeScale();
    }

    private void UpdateTimeScale()
    {
        float targetTimeScale = 1f;

        bool isVictory = GameManager.Instance != null && GameManager.Instance.IsVictory;

        // Centralized Time State Logic
        if (isVictory)
        {
            targetTimeScale = _slowTimeScale;
        }
        else
        {
            targetTimeScale = _player.IsRunning ? 1f : _slowTimeScale;
        }

        // Smoothly transition TimeScale
        Time.timeScale = Mathf.Lerp(Time.timeScale, targetTimeScale, _timeScaleSmoothSpeed * Time.unscaledDeltaTime);

        // Snap if close enough to target
        if (Mathf.Abs(Time.timeScale - targetTimeScale) < 0.001f)
        {
            Time.timeScale = targetTimeScale;
        }

        // Keep Physics in sync with TimeScale
        Time.fixedDeltaTime = _defaultFixedDeltaTime * Time.timeScale;
    }

    public void Init(AudioSource source)
    {
        _audioSource = source;
        _basePitch = Random.Range(0.9f, 1.1f);
        _audioSource.playOnAwake = false;
        _audioSource.Play();
        
        StartCoroutine(ManageAudioPitchRoutine());
    }

    private IEnumerator ManageAudioPitchRoutine()
    {
        while (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.pitch = _basePitch * Mathf.Max(0.01f, Time.timeScale);
            yield return null;
        }
    }

    // Safety resets
    private void OnDisable()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = _defaultFixedDeltaTime > 0 ? _defaultFixedDeltaTime : 0.02f;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = _defaultFixedDeltaTime > 0 ? _defaultFixedDeltaTime : 0.02f;
    }
}