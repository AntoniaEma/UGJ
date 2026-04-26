using GLTF.Schema.KHR_lights_punctual;
using UnityEngine;

public class DancingStep : MonoBehaviour
{
    public Color unsteppedColor;
    public Color correctColor;
    Light stepLight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stepLight = transform.GetChild(0).GetComponent<Light>();
        ResetColor();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ResetColor()
    {
        stepLight.color = unsteppedColor;
    }
    public void MarkStep()
    {
        stepLight.color = correctColor;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            DancingPuzzle.instance.RegisterStep(this);
        }

    }
}
