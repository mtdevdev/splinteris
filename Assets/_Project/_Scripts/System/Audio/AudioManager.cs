using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _ambienceSource;
    [SerializeField] private AudioSource _mainNormalSource;
    [SerializeField] private AudioSource _mainSlowSource;

    [Header("Settings")]
    [SerializeField] private float _transitionSpeed = 5f;

    private float _minTimeScaleForFullSlow = 0.2f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        _ambienceSource?.Play();
        _mainNormalSource?.Play();

        if (_mainSlowSource != null)
        {
            _mainSlowSource.volume = 0f;
            _mainSlowSource.Play();
        }
    }

    void Update()
    {
        if (_mainNormalSource == null || _mainSlowSource == null)
            return;

        float t = Mathf.InverseLerp(_minTimeScaleForFullSlow, 1f, Time.timeScale);

        t = Mathf.Clamp01(t);

        float targetNormal = t;    
        float targetSlow = 1f - t;  

        _mainNormalSource.volume = Mathf.MoveTowards(
            _mainNormalSource.volume,
            targetNormal,
            _transitionSpeed * Time.unscaledDeltaTime
        );

        _mainSlowSource.volume = Mathf.MoveTowards(
            _mainSlowSource.volume,
            targetSlow,
            _transitionSpeed * Time.unscaledDeltaTime
        );
    }

    public void StopAllMusic()
    {
        if (_mainNormalSource != null) _mainNormalSource.Stop();
        if (_mainSlowSource != null) _mainSlowSource.Stop();
        if (_ambienceSource != null) _ambienceSource.Stop();
    }

}
