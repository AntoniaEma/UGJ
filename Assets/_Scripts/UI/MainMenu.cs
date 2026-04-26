using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject instructionsPanel;
    public GameObject settingsPanel;

    [Tooltip("Name of the story/cutscene scene that plays before the game.")]
    [SerializeField] private string storySceneName = "StoryScene";

    public void PlayGame()
    {
        SceneManager.LoadScene(storySceneName);
    }

    public void OpenInstructions()
    {
        mainPanel.SetActive(false);
        instructionsPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void BackToMenu()
    {
        mainPanel.SetActive(true);
        instructionsPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Closed");
    }
}