using UnityEngine;

/// <summary>
/// Central audio singleton. Drag AudioClips onto the SerializeField slots in the Inspector.
/// Each logical channel (music, chase, ambient, footstep, SFX) gets its own AudioSource
/// created at runtime so loops can be started and stopped independently.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }

    // ── Runtime AudioSources (created in Awake, no prefab setup needed) ──────
    private AudioSource musicSource;    // looping statue dance song
    private AudioSource chaseSource;    // looping demon chase tension
    private AudioSource ambientSource;  // looping rabbit whispers
    private AudioSource footstepSource; // footstep one-shots
    private AudioSource sfxSource;      // all other one-shots

    // ── Inspector clips & volumes ─────────────────────────────────────────────

    [Header("Realm Swap")]
    [Tooltip("Sound when switching from magician → rabbit form.")]
    [SerializeField] private AudioClip swapToRabbitSFX;
    [Tooltip("Sound when switching from rabbit → magician form.")]
    [SerializeField] private AudioClip swapToMagicianSFX;
    [SerializeField] [Range(0f, 1f)] private float swapVolume = 1f;

    [Header("Dancing Statue")]
    [Tooltip("Whimsical looping song played while the statue is dancing.")]
    [SerializeField] private AudioClip statueDanceSong;
    [SerializeField] [Range(0f, 1f)] private float statueDanceVolume = 0.6f;

    [Header("Demon Chase")]
    [Tooltip("Tense looping sound played while one or more demons are chasing the player.")]
    [SerializeField] private AudioClip demonChaseLoop;
    [SerializeField] [Range(0f, 1f)] private float demonChaseVolume = 0.7f;

    [Header("Rabbit Form Ambience")]
    [Tooltip("Soft whisper loop played while the player is in rabbit form.")]
    [SerializeField] private AudioClip rabbitWhisperLoop;
    [SerializeField] [Range(0f, 1f)] private float rabbitWhisperVolume = 0.4f;

    [Header("Ring Collection")]
    [Tooltip("One-shot played when the player picks up a ring piece.")]
    [SerializeField] private AudioClip ringCollectSFX;
    [SerializeField] [Range(0f, 1f)] private float ringCollectVolume = 1f;

    [Header("Magician Sounds")]
    [Tooltip("Footstep sound for the magician form.")]
    [SerializeField] private AudioClip magicianFootstep;
    [Tooltip("Jump sound for the magician form.")]
    [SerializeField] private AudioClip magicianJump;
    [SerializeField] [Range(0f, 1f)] private float magicianSoundVolume = 0.8f;

    [Header("Rabbit Sounds")]
    [Tooltip("Footstep sound for the rabbit form.")]
    [SerializeField] private AudioClip rabbitFootstep;
    [Tooltip("Jump sound for the rabbit form.")]
    [SerializeField] private AudioClip rabbitJump;
    [SerializeField] [Range(0f, 1f)] private float rabbitSoundVolume = 0.6f;

    [Header("Slide Puzzle")]
    [Tooltip("Short click/slide played each time a puzzle tile moves.")]
    [SerializeField] private AudioClip tileSlideSFX;
    [SerializeField] [Range(0f, 1f)] private float tileSlideVolume = 0.9f;

    // Tracks how many demons are currently chasing so the loop only stops
    // when ALL demons have disengaged (supports multiple statue enemies).
    private int activeChaseCount;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource    = CreateSource("MusicSource",    loop: true);
        chaseSource    = CreateSource("ChaseSource",    loop: true);
        ambientSource  = CreateSource("AmbientSource",  loop: true);
        footstepSource = CreateSource("FootstepSource", loop: false);
        sfxSource      = CreateSource("SFXSource",      loop: false);
    }

    private AudioSource CreateSource(string childName, bool loop)
    {
        var go  = new GameObject(childName);
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.loop        = loop;
        src.playOnAwake = false;
        return src;
    }

    // ── Realm Swap ────────────────────────────────────────────────────────────

    public void PlaySwapToRabbit()
    {
        if (swapToRabbitSFX != null)
            sfxSource.PlayOneShot(swapToRabbitSFX, swapVolume);
    }

    public void PlaySwapToMagician()
    {
        if (swapToMagicianSFX != null)
            sfxSource.PlayOneShot(swapToMagicianSFX, swapVolume);
    }

    // ── Dancing Statue ────────────────────────────────────────────────────────

    public void PlayStatueDance()
    {
        if (statueDanceSong == null) return;
        musicSource.clip   = statueDanceSong;
        musicSource.volume = statueDanceVolume;
        musicSource.Play();
    }

    public void StopStatueDance() => musicSource.Stop();

    // ── Demon Chase ───────────────────────────────────────────────────────────

    /// <summary>Call when a demon starts chasing the player.</summary>
    public void StartDemonChase()
    {
        activeChaseCount++;
        if (activeChaseCount == 1 && demonChaseLoop != null)
        {
            chaseSource.clip   = demonChaseLoop;
            chaseSource.volume = demonChaseVolume;
            chaseSource.Play();
        }
    }

    /// <summary>Call when a demon stops chasing. Loop stops only when all demons disengage.</summary>
    public void StopDemonChase()
    {
        activeChaseCount = Mathf.Max(0, activeChaseCount - 1);
        if (activeChaseCount == 0) chaseSource.Stop();
    }

    // ── Rabbit Form Ambience ──────────────────────────────────────────────────

    public void PlayRabbitWhispers()
    {
        if (rabbitWhisperLoop == null) return;
        ambientSource.clip   = rabbitWhisperLoop;
        ambientSource.volume = rabbitWhisperVolume;
        ambientSource.Play();
    }

    public void StopRabbitWhispers() => ambientSource.Stop();

    // ── Ring Collection ───────────────────────────────────────────────────────

    public void PlayRingCollect()
    {
        if (ringCollectSFX != null)
            sfxSource.PlayOneShot(ringCollectSFX, ringCollectVolume);
    }

    // ── Footsteps ─────────────────────────────────────────────────────────────

    /// <param name="isRabbit">True = rabbit form, False = magician form.</param>
    public void PlayFootstep(bool isRabbit)
    {
        AudioClip clip = isRabbit ? rabbitFootstep : magicianFootstep;
        float     vol  = isRabbit ? rabbitSoundVolume : magicianSoundVolume;
        if (clip != null) footstepSource.PlayOneShot(clip, vol);
    }

    // ── Jump ──────────────────────────────────────────────────────────────────

    /// <param name="isRabbit">True = rabbit form, False = magician form.</param>
    public void PlayJump(bool isRabbit)
    {
        AudioClip clip = isRabbit ? rabbitJump : magicianJump;
        float     vol  = isRabbit ? rabbitSoundVolume : magicianSoundVolume;
        if (clip != null) sfxSource.PlayOneShot(clip, vol);
    }

    // ── Slide Puzzle ──────────────────────────────────────────────────────────

    public void PlayTileSlide()
    {
        if (tileSlideSFX != null)
            sfxSource.PlayOneShot(tileSlideSFX, tileSlideVolume);
    }
}
