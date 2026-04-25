using UnityEngine;

//Level baseclass (for each level to implement their own logic)
public abstract class Level : MonoBehaviour
{
    // The ring piece is picked up and the gates are unlocked
    public abstract void CompleteLevel();

    // The level is complete and the ring piece is shown
    public abstract void UnlockLevel();
}
