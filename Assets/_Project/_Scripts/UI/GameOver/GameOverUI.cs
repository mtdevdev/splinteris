using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{

    [Header ("UI References")]
    [SerializeField] private Canvas gameOverCanvas;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private GameObject menuButton;

    [Header ("Audio References")]
    [SerializeField] private AudioSource gameOverMusic;

    public void restartGame()
    {
        SceneManager.LoadScene("Main Game");
    }

    public void returnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

}
