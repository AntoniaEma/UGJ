using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Displays a full-screen story image between the main menu and the game.
/// Drag your story picture onto the Story Image field in the Inspector.
/// Click anywhere, press Space, or press Enter to continue.
/// </summary>
public class StoryScreen : MonoBehaviour
{
    [Tooltip("TextMeshPro text that shows the hint. E.g. 'Click anywhere to continue'.")]
    [SerializeField] private TMP_Text hintText;

    [Tooltip("How fast the hint text pulses in and out.")]
    [SerializeField] private float pulseSpeed = 1.5f;

    [Tooltip("Name of the gameplay scene to load when the player continues.")]
    [SerializeField] private string gameSceneName = "SampleScene 1";

    void Start()
    {
        if (hintText != null)
            hintText.text = "Click anywhere or press Space to continue";
    }

    void Update()
    {
        // Pulse the hint text alpha so it draws attention.
        if (hintText != null)
        {
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed));
            Color c = hintText.color;
            c.a = alpha;
            hintText.color = c;
        }

        // Any click anywhere on the screen, or Space / Enter, continues.
        bool clicked = Mouse.current    != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool spaced  = Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame ||
                                                    Keyboard.current.enterKey.wasPressedThisFrame);
        if (clicked || spaced)
            Continue();
    }

    public void Continue()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
