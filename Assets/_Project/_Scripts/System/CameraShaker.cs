using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CinemachineCamera virtualCamera;
    
    private CinemachineBasicMultiChannelPerlin noise;
    private Coroutine shakeCoroutine;

    void Start()
    {
        StopShake();
    }

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Pega o componente de noise da Cinemachine
        if (virtualCamera != null)
        {
            noise = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            
            if (noise == null)
            {
                Debug.LogError("CinemachineBasicMultiChannelPerlin não encontrado na Virtual Camera!");
            }
        }
        else
        {
            Debug.LogError("Virtual Camera não atribuída no CameraShake!");
        }
    }

    /// <summary>
    /// Faz a câmera tremer com parâmetros personalizados
    /// </summary>
    /// <param name="intensity">Intensidade do tremor (amplitude)</param>
    /// <param name="frequency">Frequência do tremor</param>
    /// <param name="duration">Duração em segundos</param>
    public void Shake(float intensity, float frequency, float duration)
    {
        if (noise == null) return;

        // Se já está tremendo, para o anterior
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(ShakeCoroutine(intensity, frequency, duration));
    }

    /// <summary>
    /// Tremor rápido e intenso (exemplo: explosão)
    /// </summary>
    public void ShakeExplosion()
    {
        Shake(3f, 2f, 0.5f);
    }

    /// <summary>
    /// Tremor leve e contínuo (exemplo: motor, terremoto leve)
    /// </summary>
    public void ShakeLight(float duration = 1f)
    {
        Shake(0.5f, 1f, duration);
    }

    /// <summary>
    /// Tremor médio (exemplo: impacto, queda)
    /// </summary>
    public void ShakeMedium(float duration = 0.3f)
    {
        Shake(1.5f, 1.5f, duration);
    }

    /// <summary>
    /// Tremor forte (exemplo: terremoto, grande explosão)
    /// </summary>
    public void ShakeHeavy(float duration = 0.8f)
    {
        Shake(4f, 2.5f, duration);
    }

    /// <summary>
    /// Para o tremor imediatamente
    /// </summary>
    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        if (noise != null)
        {
            noise.AmplitudeGain = 0f;
            noise.FrequencyGain = 0f;
        }
    }

    private IEnumerator ShakeCoroutine(float intensity, float frequency, float duration)
    {
        // Define os valores do noise
        noise.AmplitudeGain = intensity;
        noise.FrequencyGain = frequency;

        // Aguarda a duração
        yield return new WaitForSeconds(duration);

        // Fade out suave (opcional)
        float fadeTime = 0.2f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            
            noise.AmplitudeGain = Mathf.Lerp(intensity, 0f, t);
            noise.FrequencyGain = Mathf.Lerp(frequency, 0f, t);
            
            yield return null;
        }

        // Garante que voltou ao zero
        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 0f;

        shakeCoroutine = null;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}