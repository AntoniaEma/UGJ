using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public Slider volumeSlider;

    private void Start()
    {
        volumeSlider.value = 1f;
        AudioListener.volume = 1f;
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        Debug.Log("Volume: " + volume);
    }
}