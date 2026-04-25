using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SlidePuzzleTile : MonoBehaviour
{
    [Tooltip("MeshRenderer on the top-face Quad/Plane child that displays the puzzle image. " +
             "Leave empty to auto-find a child named 'TopFace'.")]
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

        // Auto-find if not manually assigned in the prefab inspector.
        if (faceRenderer == null)
        {
            Transform face = transform.Find("TopFace");
            if (face != null)
                faceRenderer = face.GetComponent<Renderer>();

            // Broader fallback: first child renderer that is NOT on the root.
            if (faceRenderer == null)
            {
                foreach (Renderer r in GetComponentsInChildren<Renderer>())
                {
                    if (r.gameObject != gameObject) { faceRenderer = r; break; }
                }
            }

            if (faceRenderer == null)
            {
                Debug.LogError($"SlidePuzzleTile '{name}': could not find a child Renderer. " +
                               "Add a Quad child named 'TopFace' with a Renderer and a URP material.", this);
                return;
            }
        }

        if (image == null)
        {
            Debug.LogWarning($"SlidePuzzleTile '{name}': puzzleImage is not assigned on SlidePuzzle.", this);
            return;
        }

        int row = index / gridSize;
        int col = index % gridSize;
        float tiling = 1f / gridSize;

        // UV (0,0) is bottom-left in Unity, but row 0 = TOP of the image (reading order).
        float uOffset = col * tiling;
        float vOffset = (gridSize - 1 - row) * tiling;

        Vector2 scale  = new Vector2(tiling, tiling);
        Vector2 offset = new Vector2(uOffset, vOffset);

        // Clone shared material so each tile gets independent UV settings.
        Material mat = new Material(faceRenderer.sharedMaterial);
        bool appliedTexture = false;

        // URP shaders expose _BaseMap; Built-in shaders expose _MainTex.
        if (mat.HasProperty("_BaseMap"))
        {
            mat.SetTexture("_BaseMap", image);
            mat.SetTextureScale("_BaseMap", scale);
            mat.SetTextureOffset("_BaseMap", offset);
            appliedTexture = true;
        }

        if (mat.HasProperty("_MainTex"))
        {
            mat.SetTexture("_MainTex", image);
            mat.SetTextureScale("_MainTex", scale);
            mat.SetTextureOffset("_MainTex", offset);
            appliedTexture = true;
        }

        if (!appliedTexture)
            Debug.LogWarning($"SlidePuzzleTile '{name}': the material '{mat.name}' uses shader " +
                             $"'{mat.shader.name}' which has no _BaseMap or _MainTex property. " +
                             "Change the shader to URP/Lit or Universal Render Pipeline/Unlit.", this);

        Debug.Log($"[SlidePuzzle] Tile {index} → renderer: '{faceRenderer.gameObject.name}' " +
                  $"| shader: '{mat.shader.name}' | textureApplied: {appliedTexture}", this);

        faceRenderer.material = mat;
    }

    // Instant repositioning used during shuffle setup (no animation).
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
