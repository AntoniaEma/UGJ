using System;
using UnityEngine;

public class SlidePuzzleTile : MonoBehaviour
{
    [SerializeField] private float slideSpeed = 10f;

    public int TileIndex { get; private set; }

    private Vector3 targetPos;
    private bool isSliding;
    private Action onSlideComplete;
    private GameObject imageFace; // reference kept so we can show/hide it

    // Called by SlidePuzzle immediately after Instantiate.
    public void Init(int index, Texture2D image, int gridSize,
                     bool flipHorizontal = false, bool flipVertical = false,
                     float faceYOffset = -0.55f)
    {
        TileIndex = index;

        if (image == null)
        {
            Debug.LogWarning($"SlidePuzzleTile '{name}': Puzzle Image is not assigned on SlidePuzzle.", this);
            return;
        }

        int row = index / gridSize;
        int col = index % gridSize;
        float tiling = 1f / gridSize;

        // Flip the column/row index when requested so the user can correct orientation
        // from the SlidePuzzle Inspector without touching code.
        int displayCol = flipHorizontal ? (gridSize - 1 - col) : col;
        int displayRow = flipVertical   ? (gridSize - 1 - row) : row;

        float uOffset = displayCol * tiling;
        float vOffset = (gridSize - 1 - displayRow) * tiling; // UV origin is bottom-left in Unity

        Vector2 scale  = new Vector2(tiling, tiling);
        Vector2 offset = new Vector2(uOffset, vOffset);

        // Find the cube child's renderer to borrow its material (guaranteed to render in this project).
        Renderer cubeRenderer = null;
        foreach (Renderer cr in GetComponentsInChildren<Renderer>())
        {
            if (cr.gameObject == gameObject) continue;
            cubeRenderer = cr;
            break;
        }

        Material mat = cubeRenderer != null
            ? new Material(cubeRenderer.sharedMaterial)
            : new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture"));

        mat.color = Color.white;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
        if (mat.HasProperty("_Color"))     mat.SetColor("_Color",     Color.white);

        mat.mainTexture       = image;
        mat.mainTextureScale  = scale;
        mat.mainTextureOffset = offset;

        if (mat.HasProperty("_BaseMap"))
        {
            mat.SetTexture("_BaseMap",      image);
            mat.SetTextureScale("_BaseMap",  scale);
            mat.SetTextureOffset("_BaseMap", offset);
        }
        if (mat.HasProperty("_MainTex"))
        {
            mat.SetTexture("_MainTex",      image);
            mat.SetTextureScale("_MainTex",  scale);
            mat.SetTextureOffset("_MainTex", offset);
        }

        // Place the image face at the requested local Y offset.
        // Negative values put it behind/below the cube body; positive puts it above.
        GameObject face = GameObject.CreatePrimitive(PrimitiveType.Quad);
        face.name = "ImageFace";
        face.transform.SetParent(transform, false);
        face.transform.localPosition = new Vector3(0f, faceYOffset, 0f);
        face.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f); // always faces +Y (up)
        face.transform.localScale    = new Vector3(0.95f, 0.95f, 1f);
        Destroy(face.GetComponent<Collider>());

        face.GetComponent<Renderer>().material = mat;
        imageFace = face;
        imageFace.SetActive(false); // hidden by default; shown only in rabbit form
    }

    public void ShowImageFace() { if (imageFace != null) imageFace.SetActive(true); }
    public void HideImageFace() { if (imageFace != null) imageFace.SetActive(false); }

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
