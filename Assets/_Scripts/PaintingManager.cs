using System.Collections.Generic;
using UnityEngine;

public class PaintingManager : MonoBehaviour
{
    public static PaintingManager instance;
    public List<Painting> paintingOrder;
    public Level level;

    void Awake() => instance = this;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SubmitPainting(Painting painting)
    {
        if(paintingOrder[0] == painting)
        {
            painting.SubmitPainting();
            paintingOrder.Remove(painting);
            if(paintingOrder.Count == 0)
            {
                level.UnlockLevel();    
            }
        }
        
    }

    public void SwitchRealms()
    {
        foreach(Painting currentPainting in paintingOrder)
        {
            currentPainting.TogglePaintingLight();
        }
        

    }
}
