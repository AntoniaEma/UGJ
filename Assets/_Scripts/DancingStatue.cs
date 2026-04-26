using UnityEngine;

public class DancingStatue : MonoBehaviour
{
    public static DancingStatue instance;
    Animator animator;
    void Awake() => instance = this;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        StopDancing();
    }

    public void Dance()
    {
        gameObject.SetActive(true);
        animator.Play("StatueDance", 0, 0);
        SoundManager.instance?.PlayStatueDance();
    }

    public void StopDancing()
    {
        animator.StopPlayback();
        gameObject.SetActive(false);
        SoundManager.instance?.StopStatueDance();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
