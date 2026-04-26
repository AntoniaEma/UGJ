using UnityEngine;

/// <summary>
/// Attach to any world object (e.g. a piano) to play a looping sound
/// that smoothly fades in when the player comes within range and fades
/// out when they leave. The AudioSource is kept fully 3D so it also
/// pans naturally as the player moves around the object.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ProximityAudio : MonoBehaviour
{
    [Tooltip("The looping audio clip to play near this object.")]
    [SerializeField] private AudioClip clip;

    [Tooltip("Distance (world units) at which the sound reaches full volume.")]
    [SerializeField] private float hearRadius = 6f;

    [Tooltip("Peak volume reached when the player is inside the hear radius.")]
    [SerializeField] [Range(0f, 1f)] private float maxVolume = 0.8f;

    [Tooltip("How quickly the volume fades in and out (units per second).")]
    [SerializeField] private float fadeSpeed = 2f;

    private AudioSource audioSource;
    private Transform   player;

    void Start()
    {
        audioSource              = GetComponent<AudioSource>();
        audioSource.clip         = clip;
        audioSource.loop         = true;
        audioSource.spatialBlend = 1f;   // fully 3D
        audioSource.volume       = 0f;
        audioSource.playOnAwake  = false;

        if (clip != null) audioSource.Play();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning($"ProximityAudio on '{name}': no GameObject with tag 'Player' found.", this);
    }

    void Update()
    {
        if (player == null || clip == null) return;

        float dist          = Vector3.Distance(transform.position, player.position);
        float targetVolume  = dist <= hearRadius ? maxVolume : 0f;
        audioSource.volume  = Mathf.MoveTowards(audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);
    }

#if UNITY_EDITOR
    // Visualise the hear radius as a wire sphere in the Scene view.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, hearRadius);
    }
#endif
}
