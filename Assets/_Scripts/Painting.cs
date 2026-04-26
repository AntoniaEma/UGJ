using UnityEngine;

public class Painting : MonoBehaviour
{

    public Animator animator;
    void Start()
    {
        animator.StopPlayback();
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PaintingManager.instance.SubmitPainting(this);
        }
    }
    public void TogglePaintingLight()
    {
        Light light = transform.GetChild(0).gameObject.GetComponent<Light>();
        light.enabled = !light.isActiveAndEnabled;
    }
    public void SubmitPainting()
    {
        //Logic for Flying painting
        animator.SetTrigger("Fly");
        gameObject.SetActive(false);

    }
}
