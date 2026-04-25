using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SlidePuzzleTile : MonoBehaviour
{
    [Tooltip("The Renderer on the TOP FACE child object (the quad/plane that shows the puzzle image).")]
    [SerializeField] private Renderer faceRenderer;
    [SerializeField] private float slideSpeed = 10f;

    // The index this tile represents in the solved layout (0 = top-left, reading order).
    public int TileIndex { get; private set; }

    private Vector3 targetPos;
    private bool isSliding;
    private Action onSlideComplete;

    // Called by SlidePuzzle immediately after Instantiate.
    public void Init(int index, Texture2D image, int gridSize)
    {
        TileIndex = index;

        int row = index / gridSize;
        int col = index % gridSize;
        float tiling = 1f / gridSize;

        // UV (0,0) is bottom-left in Unity, but row 0 should map to the TOP of the
        // image (reading order), so the V offset is flipped.
        float uOffset = col * tiling;
        float vOffset = (gridSize - 1 - row) * tiling;

        // Clone the shared material so each tile has independent UV settings.
        Material mat = new Material(faceRenderer.sharedMaterial);
        mat.mainTexture = image;
        mat.mainTextureScale  = new Vector2(tiling, tiling);
        mat.mainTextureOffset = new Vector2(uOffset, vOffset);
        faceRenderer.material = mat;
    }

    // Instant repositioning used during shuffle setup.
    public void Teleport(Vector3 pos)
    {
        transform.position = pos;
        targetPos = pos;
        isSliding = false;
    }

    // Smooth slide to target; fires onComplete when the tile arrives.
    public void SlideTo(Vector3 pos, Action onComplete = null)
    {
        targetPos = pos;
        isSliding = true;
        onSlideComplete = onComplete;
    }

    void Update()
    {
        if (!isSliding) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, slideSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.001f)
        {
            transform.position = targetPos;
            isSliding = false;
            onSlideComplete?.Invoke();
            onSlideComplete = null;
        }
    }
}
