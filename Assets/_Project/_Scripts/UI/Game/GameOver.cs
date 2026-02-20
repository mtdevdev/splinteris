using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{

    [Header("UI Elements")]
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private GameObject BGImage;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource mainNormalMusic;
    [SerializeField] private AudioSource mainSlowMusic;
    [SerializeField] private AudioSource ambienceMusic;

    void Start()
    {
        BGImage.SetActive(false);
    }

    IEnumerator timeToShowScreen()
    {
        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene("Game Over");

        BGImage.SetActive(true);

    }
    
    public void ShowGameOverScreen()
    {

        gameOverCanvas.SetActive(true);

        mainNormalMusic.Stop();
        mainSlowMusic.Stop();
        ambienceMusic.Stop();

        StartCoroutine(timeToShowScreen());

    }
}
