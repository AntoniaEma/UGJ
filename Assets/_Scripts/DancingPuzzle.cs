using System.Collections.Generic;
using UnityEngine;

public class DancingPuzzle : MonoBehaviour
{
    public static DancingPuzzle instance;
    public Level level;
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

        if((playerInput.Count != 0 && playerStep == playerInput[playerInput.Count - 1]) || playerInput.Count == correctSteps.Count)
        {
            return;
        }
        playerInput.Add(playerStep);

        int i = playerInput.Count - 1;

        if(playerInput[i] != correctSteps[i])
        {
            playerInput.Clear();
            foreach(DancingStep step in correctSteps)
            {
                step.ResetColor();
            }
            return;
        }
        playerStep.MarkStep();

        if(playerInput.Count == correctSteps.Count)
        {
            //Level 1 complete logic
            // gameObject.SetActive(false);
            level.UnlockLevel();
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
