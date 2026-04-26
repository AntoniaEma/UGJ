using UnityEngine;
using UnityEngine.AI;

public class StatueMovement : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if(target != null) 
        {
            agent.SetDestination(target.position);
        }
        else
        {
            agent.velocity = Vector3.zero;
            // agent.ResetPath();
        }
    }
    public void SetStatueTarget(Transform t)
    {
        target = t;
    }
}
