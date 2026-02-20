using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryUI : MonoBehaviour
{

    [Header ("UI References")]
    [SerializeField] private Canvas victoryCanvas;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private GameObject menuButton;

    [Header ("Audio References")]
    [SerializeField] private AudioSource victoryMusic;

    public void restartGame()
    {
        SceneManager.LoadScene("Main Game");
    }

    public void returnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

}
