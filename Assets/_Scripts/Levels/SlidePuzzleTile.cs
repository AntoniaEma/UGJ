using System;
using UnityEngine;

public class SlidePuzzleTile : MonoBehaviour
{
    [SerializeField] private float slideSpeed = 10f;

    public int TileIndex { get; private set; }

    private Vector3 targetPos;
    private bool isSliding;
    private Action onSlideComplete;

    // Called by SlidePuzzle immediately after Instantiate.
    public void Init(int index, Texture2D image, int gridSize)
    {
        TileIndex = index;

        if (image == null)
        {
            Debug.LogWarning($"SlidePuzzleTile '{name}': Puzzle Image is not assigned on SlidePuzzle.", this);
            return;
        }

        int row = index / gridSize;
        int col = index % gridSize;
        float tiling  = 1f / gridSize;
        float uOffset = col * tiling;
        float vOffset = (gridSize - 1 - row) * tiling; // flip Y: UV origin is bottom-left

        // Create a flat quad on top of the tile body to display the image.
        // This is done in code so no manual TopFace setup is needed in the prefab.
        GameObject face = GameObject.CreatePrimitive(PrimitiveType.Quad);
        face.name = "ImageFace";
        face.transform.SetParent(transform, false);
        face.transform.localPosition = new Vector3(0f, 0.11f, 0f); // just above the cube top
        face.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f); // rotate to face upward
        face.transform.localScale    = new Vector3(0.95f, 0.95f, 1f);

        // The Quad primitive adds a MeshCollider — remove it so only the cube's
        // BoxCollider is used for raycasting.
        Destroy(face.GetComponent<Collider>());

        Renderer r = face.GetComponent<Renderer>();
        r.material = BuildMaterial(image, tiling, uOffset, vOffset);
    }

    private static Material BuildMaterial(Texture2D image, float tiling, float uOffset, float vOffset)
    {
        // Use Unlit so the image appears at full brightness regardless of scene lighting.
        // Try URP Unlit first, fall back to Built-in Unlit/Texture.
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                     ?? Shader.Find("Unlit/Texture");

        if (shader == null)
        {
            Debug.LogError("SlidePuzzleTile: Could not find a usable Unlit shader. " +
                           "Make sure URP is set up correctly.");
            return new Material(Shader.Find("Standard"));
        }

        Material mat    = new Material(shader);
        Vector2  scale  = new Vector2(tiling, tiling);
        Vector2  offset = new Vector2(uOffset, vOffset);

        // _BaseMap  = URP Unlit texture slot
        // _MainTex  = Built-in Unlit/Texture slot
        if (mat.HasProperty("_BaseMap"))
        {
            mat.SetTexture("_BaseMap", image);
            mat.SetTextureScale("_BaseMap",  scale);
            mat.SetTextureOffset("_BaseMap", offset);
        }
        if (mat.HasProperty("_MainTex"))
        {
            mat.SetTexture("_MainTex", image);
            mat.SetTextureScale("_MainTex",  scale);
            mat.SetTextureOffset("_MainTex", offset);
        }

        // Ensure base color is white so it never tints the texture.
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
        if (mat.HasProperty("_Color"))     mat.SetColor("_Color",     Color.white);

        return mat;
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
