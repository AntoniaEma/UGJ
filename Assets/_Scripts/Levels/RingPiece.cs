using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RingPiece : MonoBehaviour
{
    public Level levelToUnlock;

    [Header("Radial Ambient Sound")]
    [Tooltip("Looping ambient sound that radiates from the ring in 3D space.")]
    [SerializeField] private AudioClip ringAmbientLoop;
    [SerializeField] [Range(0f, 1f)] private float ambientVolume = 0.7f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop        = true;
        audioSource.spatialBlend = 1f;   // fully 3D — volume falls off with distance
        audioSource.playOnAwake  = false;
        audioSource.volume       = ambientVolume;
    }

    void OnEnable()
    {
        // Plays whenever the ring becomes visible (e.g. after UnlockLevel activates it).
        if (ringAmbientLoop != null)
        {
            audioSource.clip = ringAmbientLoop;
            audioSource.Play();
        }
    }

    void OnDisable() => audioSource.Stop();

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SoundManager.instance?.PlayRingCollect();
            levelToUnlock.CompleteLevel();
            gameObject.SetActive(false);
        }
    }
}
