using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CinemachineCamera _virtualCamera;
    
    private CinemachineBasicMultiChannelPerlin _noiseProfile;
    private Coroutine _shakeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (_virtualCamera != null)
        {
            _noiseProfile = _virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (_noiseProfile == null)
            {
                Debug.LogError("CinemachineBasicMultiChannelPerlin not found on Virtual Camera!");
            }
        }
        else
        {
            Debug.LogError("Virtual Camera not assigned in CameraShaker!");
        }
    }

    private void Start()
    {
        StopShake();
    }

    /// <summary>
    /// Shakes the camera with custom parameters.
    /// </summary>
    public void Shake(float intensity, float frequency, float duration)
    {
        if (_noiseProfile == null) return;

        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
        }

        _shakeCoroutine = StartCoroutine(ShakeRoutine(intensity, frequency, duration));
    }

    /// <summary>
    /// Fast and intense shake (e.g., explosions).
    /// </summary>
    public void ShakeExplosion() => Shake(3f, 2f, 0.5f);

    /// <summary>
    /// Light and continuous shake (e.g., engines, light tremors).
    /// </summary>
    public void ShakeLight(float duration = 1f) => Shake(0.5f, 1f, duration);

    /// <summary>
    /// Medium shake (e.g., impacts, falling).
    /// </summary>
    public void ShakeMedium(float duration = 0.3f) => Shake(1.5f, 1.5f, duration);

    /// <summary>
    /// Heavy shake (e.g., earthquakes, huge explosions).
    /// </summary>
    public void ShakeHeavy(float duration = 0.8f) => Shake(4f, 2.5f, duration);

    /// <summary>
    /// Stops the camera shake immediately.
    /// </summary>
    public void StopShake()
    {
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = null;
        }

        if (_noiseProfile != null)
        {
            _noiseProfile.AmplitudeGain = 0f;
            _noiseProfile.FrequencyGain = 0f;
        }
    }

    private IEnumerator ShakeRoutine(float intensity, float frequency, float duration)
    {
        _noiseProfile.AmplitudeGain = intensity;
        _noiseProfile.FrequencyGain = frequency;

        yield return new WaitForSeconds(duration);

        // Smooth fade out
        float fadeTime = 0.2f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            
            _noiseProfile.AmplitudeGain = Mathf.Lerp(intensity, 0f, t);
            _noiseProfile.FrequencyGain = Mathf.Lerp(frequency, 0f, t);
            
            yield return null;
        }

        _noiseProfile.AmplitudeGain = 0f;
        _noiseProfile.FrequencyGain = 0f;
        _shakeCoroutine = null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}