using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioSource mainNormalSource;
    [SerializeField] private AudioSource mainSlowSource;

    [Header("Settings")]
    [SerializeField] private float transitionSpeed = 5f;

    private float minTimeScaleForFullSlow = 0.2f;

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
        ambienceSource?.Play();
        mainNormalSource?.Play();

        if (mainSlowSource != null)
        {
            mainSlowSource.volume = 0f;
            mainSlowSource.Play();
        }
    }

    void Update()
    {
        if (mainNormalSource == null || mainSlowSource == null)
            return;

        float t = Mathf.InverseLerp(minTimeScaleForFullSlow, 1f, Time.timeScale);

        t = Mathf.Clamp01(t);

        float targetNormal = t;    
        float targetSlow = 1f - t;  

        mainNormalSource.volume = Mathf.MoveTowards(
            mainNormalSource.volume,
            targetNormal,
            transitionSpeed * Time.unscaledDeltaTime
        );

        mainSlowSource.volume = Mathf.MoveTowards(
            mainSlowSource.volume,
            targetSlow,
            transitionSpeed * Time.unscaledDeltaTime
        );
    }
}
