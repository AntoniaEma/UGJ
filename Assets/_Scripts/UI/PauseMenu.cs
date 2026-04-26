using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject overlay;     
    public GameObject pauseButton;  
    public GameObject playButton;  

    private bool isPaused = false;

    void Start()
    {
        overlay.SetActive(false);

        pauseButton.SetActive(true);
        playButton.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Pause()
    {
        if (isPaused) return;

        overlay.SetActive(true);
        pauseButton.SetActive(false);
        playButton.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        if (!isPaused) return;

        overlay.SetActive(false);
        pauseButton.SetActive(true);
        playButton.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Replay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}