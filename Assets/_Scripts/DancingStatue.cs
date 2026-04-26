using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DancingStatue : MonoBehaviour
{
    public static DancingStatue instance;

    [Header("Dance Music")]
    [Tooltip("Whimsical song that loops while the statue dances. Plays in 3D — " +
             "only audible in rabbit form (statue is hidden otherwise).")]
    [SerializeField] private AudioClip danceSong;
    [SerializeField] [Range(0f, 1f)] private float danceMusicVolume = 0.7f;

    private Animator animator;
    private AudioSource audioSource;

    void Awake()
    {
        instance = this;

        audioSource              = GetComponent<AudioSource>();
        audioSource.loop         = true;
        audioSource.spatialBlend = 1f;   // 3D — heard only when near the statue
        audioSource.playOnAwake  = false;
        audioSource.volume       = danceMusicVolume;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        StopDancing();
    }

    public void Dance()
    {
        gameObject.SetActive(true);
        animator.Play("StatueDance", 0, 0);

        if (danceSong != null)
        {
            audioSource.clip = danceSong;
            audioSource.Play();
        }
    }

    public void StopDancing()
    {
        animator.StopPlayback();
        audioSource.Stop();
        gameObject.SetActive(false);
    }

    void Update() { }
}
