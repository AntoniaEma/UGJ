using System.Collections.Generic;
using UnityEngine;

public class RabbitPathwaysManager : MonoBehaviour
{
    public static RabbitPathwaysManager instance;
    public List<GameObject> walls;

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
    public void EnableWalls()
    {
        foreach(GameObject wall in walls)
        {
            wall.SetActive(true);
        }
    }
    public void DisableWalls()
    {
        foreach(GameObject wall in walls)
        {
            wall.SetActive(false);
        }
    }
}
