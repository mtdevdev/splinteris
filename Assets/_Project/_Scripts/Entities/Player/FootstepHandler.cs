using UnityEngine;
using UnityEngine.InputSystem;

public class FootstepHandler : MonoBehaviour
{

    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private float footstepInterval = 0.5f;

    private float footstepTimer = 0f;

    private Player player;

    void Start()
    {
        player = GetComponent<Player>();
    }

    void Update()
    {
        if (!player.isAlive) return;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.aKey.isPressed || Keyboard.current.sKey.isPressed || Keyboard.current.dKey.isPressed)  
        {
            if (!footstepAudioSource.isPlaying)
            {
                footstepAudioSource.Play();
                footstepTimer = 0f;
            }
        }

        footstepTimer += Time.deltaTime;
        if (footstepTimer >= footstepInterval && footstepAudioSource.isPlaying && (Keyboard.current.wKey.isPressed || Keyboard.current.aKey.isPressed || Keyboard.current.sKey.isPressed || Keyboard.current.dKey.isPressed))
        {
            footstepAudioSource.Play();
            footstepTimer = 0f;
        }
    }

}
