using UnityEngine;

public class ClueTrigger : MonoBehaviour
{
    public GameObject clueText;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            clueText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            clueText.SetActive(false);
        }
    }
}