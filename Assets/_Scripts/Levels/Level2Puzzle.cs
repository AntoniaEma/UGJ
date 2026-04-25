using UnityEngine;

public class Level2Puzzle : Level
{
    public GameObject ringPiece;
    public Animator levelWall;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.instance.gameLevels.Add(this);
        transform.SetParent(GameManager.instance.transform);
        ringPiece.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void UnlockLevel()
    {
        ringPiece.SetActive(true);

    }
    public override void CompleteLevel()
    {
        levelWall.Play("GateOpen");
    }
}
