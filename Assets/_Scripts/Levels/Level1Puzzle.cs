using UnityEngine;

public class Level1Puzzle : Level
{
    public GameObject ringPiece;
    public Animator levelWall;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.instance.gameLevels.Add(this);
        transform.SetParent(GameManager.instance.transform);
        UnlockLevel();
    }


    // The ring piece is picked up and the gates are unlocked
    public override void CompleteLevel()
    {
        levelWall.Play("GateOpen", 0, 0);
    }

    // The level is complete and the ring piece is shown
    public override void UnlockLevel()
    {
        ringPiece.SetActive(true);

    }
}
