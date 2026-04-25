using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject overlay;      // panel-ul de pauza
    public GameObject pauseButton;  // butonul ||
    public GameObject playButton;   // butonul ▶

    private bool isPaused = false;

    void Start()
    {
        overlay.SetActive(false);

        pauseButton.SetActive(true);
        playButton.SetActive(false);

        Time.timeScale = 1f;

        // cursor mereu vizibil
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 🔘 apasare pe butonul Pause
    public void Pause()
    {
        if (isPaused) return;

        overlay.SetActive(true);
        pauseButton.SetActive(false);
        playButton.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    // ▶ apasare pe butonul Play
    public void Resume()
    {
        if (!isPaused) return;

        overlay.SetActive(false);
        pauseButton.SetActive(true);
        playButton.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    // 🔄 restart nivel
    public void Replay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ❌ iesire joc
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}