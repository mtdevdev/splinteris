using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsVictory { get; private set; } 

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void HandleVictory()
    {
        IsVictory = true;
    }

    /// <summary>
    /// Initiates the Game Over sequence, updating UI, audio, and transitioning scenes.
    /// </summary>
    public void TriggerGameOver()
    {
        // Stop music
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopAllMusic();

        StartCoroutine(TransitionToGameOverSceneRoutine());
    }

    private IEnumerator TransitionToGameOverSceneRoutine()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("GameOver");
    }
}