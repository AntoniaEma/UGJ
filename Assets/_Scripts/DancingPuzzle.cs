using System.Collections.Generic;
using UnityEngine;

public class DancingPuzzle : MonoBehaviour
{
    public static DancingPuzzle instance;
    public List<DancingStep> correctSteps = new List<DancingStep>();
    private List<DancingStep> playerInput = new List<DancingStep>();

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterStep(DancingStep playerStep)
    {
        playerInput.Add(playerStep);

        int i = playerInput.Count - 1;

        if(playerInput[i] != correctSteps[i])
        {
            playerInput.Clear();
            return;
        }

        if(playerInput.Count == correctSteps.Count)
        {
            //Level 1 complete logic
            Debug.Log("Level 1 complete");
            gameObject.SetActive(false);
        }
    }

    public void HideSteps()
    {
        foreach(DancingStep step in correctSteps)
        {
            step.gameObject.SetActive(false);
        }
    }
    public void RevealSteps()
    {
        foreach(DancingStep step in correctSteps)
        {
            step.gameObject.SetActive(true);
        }
    }
}
