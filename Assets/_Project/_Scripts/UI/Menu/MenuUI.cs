using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{

    [Header ("UI References")]
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private GameObject quitButton;
    [SerializeField] private GameObject startButton;

    [Header ("Audio References")]
    [SerializeField] private AudioSource menuMusic;

    public void startGame()
    {
        SceneManager.LoadScene("Main Game");
    }

    public void quitGame()
    {
        
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif

    }

}
