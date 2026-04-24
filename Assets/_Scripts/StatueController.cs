using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StatueController : MonoBehaviour
{
    public static StatueController instance;
    public List<GameObject> statues;
    public Transform resetPosition;
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
    void Start()
    {
        DisableAllStatues();
    }

    public void EnableAllStatues()
    {
        foreach(GameObject statue in statues)
        {
            foreach (Transform child in statue.transform)
            {
                child.gameObject.SetActive(true);
            }
        }
    }
    public void DisableAllStatues()
    {
        foreach(GameObject statue in statues)
        {
            statue.GetComponent<StatueMovement>().SetStatueTarget(null);
            foreach (Transform child in statue.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}
