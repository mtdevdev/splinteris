using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI remainingEnemiesText;

    [SerializeField] private GameObject enemiesObjectGroup;

    [SerializeField] private GameObject fadeScreen;

    [SerializeField] private GameObject player;

    private int remainingEnemies = 0;
    private bool gameWon = false;

    void Start()
    {
        remainingEnemies = enemiesObjectGroup.transform.childCount;
        UpdateRemainingEnemiesDisplay();
    }

    public void UpdateRemainingEnemies(int change = -1)
    {
        remainingEnemies += change;
        UpdateRemainingEnemiesDisplay();
    }

    private void UpdateRemainingEnemiesDisplay()
    {
        remainingEnemiesText.text = "there's " + remainingEnemies.ToString() + " remaining enemies";
    }

    void Update()
    {
        if (remainingEnemies <= 0)
        {
            remainingEnemiesText.text = "All enemies defeated!";
            WinGame();
        }
    }

    void WinGame()
    {
        if (!gameWon)
        {
            gameWon = true;

            player.GetComponent<Player>().GameWon();

            StartCoroutine(waitAndLoadVictoryScene());
        }
    }

    private IEnumerator waitAndLoadVictoryScene()
    {
        fadeScreen.GetComponent<Animator>().Play("FadeIn");
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Victory");
    }

}
