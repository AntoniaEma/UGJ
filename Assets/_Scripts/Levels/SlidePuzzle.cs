using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlidePuzzle : Level
{
    public static SlidePuzzle instance { get; private set; }

    void Awake()
    {
        instance = this;
    }
    [Header("Puzzle Settings")]
    [SerializeField] private int gridSize = 3;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float tileSpacing = 0.05f;
    [SerializeField] private int shuffleMoves = 100;

    [Header("References")]
    [SerializeField] private SlidePuzzleTile tilePrefab;
    [SerializeField] private Texture2D puzzleImage;
    [Tooltip("Empty GameObject that marks the center position and orientation of the puzzle board.")]
    [SerializeField] private Transform puzzleOrigin;

    [Header("Image Orientation")]
    [Tooltip("Mirror the image left ↔ right (flip across the vertical axis).")]
    [SerializeField] private bool flipImageHorizontal = false;
    [Tooltip("Mirror the image top ↔ bottom (flip across the horizontal axis).")]
    [SerializeField] private bool flipImageVertical = false;
    [Tooltip("Local Y offset of the image face relative to the tile root. " +
             "Negative = behind/below the cube body. Positive = above it.")]
    [SerializeField] private float faceYOffset = -0.55f;

    [Header("Level Integration")]
    public GameObject ringPiece;
    public Animator levelWall;

    // grid[slot] = tile index occupying that slot, -1 = empty
    private int[] grid;
    private SlidePuzzleTile[] tiles;
    private int emptySlot;

    [Header("Interaction")]
    [SerializeField] private float interactionRadius = 4f;

    private bool isSolved = false;
    private bool tileIsSliding = false;
    private Transform player;

    void Start()
    {
        GameManager.instance.gameLevels.Add(this);
        transform.SetParent(GameManager.instance.transform);
        ringPiece.SetActive(false);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("SlidePuzzle: No GameObject with tag 'Player' found.", this);

        InitGrid();
        SpawnTiles();
        Shuffle();
        HideImages();      // images hidden in magician form; revealed only in rabbit form
        RefreshHighlights(); // yellow highlight on moveable tiles from the start
    }

    void Update()
    {
        if (isSolved || tileIsSliding) return;
        if (player == null) return;
        if (Vector3.Distance(puzzleOrigin.position, player.position) > interactionRadius) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            HandleClick();
    }

    // ── Grid Initialisation ─────────────────────────────────────────────────

    private void InitGrid()
    {
        int total = gridSize * gridSize;
        grid = new int[total];
        for (int i = 0; i < total - 1; i++) grid[i] = i;
        grid[total - 1] = -1;
        emptySlot = total - 1;
    }

    private void SpawnTiles()
    {
        int tileCount = gridSize * gridSize - 1;
        tiles = new SlidePuzzleTile[tileCount];

        for (int i = 0; i < tileCount; i++)
        {
            Vector3 pos = SlotToWorldPos(i);
            SlidePuzzleTile tile = Instantiate(tilePrefab, pos, puzzleOrigin.rotation, puzzleOrigin);
            tile.Init(i, puzzleImage, gridSize, flipImageHorizontal, flipImageVertical, faceYOffset);
            tiles[i] = tile;
        }
    }

    // Shuffle by executing random valid moves from the solved state so the
    // result is always solvable. Avoids immediately undoing the previous move.
    private void Shuffle()
    {
        int lastEmptySlot = -1;
        for (int i = 0; i < shuffleMoves; i++)
        {
            List<int> neighbors = GetAdjacentFilledSlots(emptySlot);
            if (lastEmptySlot >= 0) neighbors.Remove(lastEmptySlot);
            if (neighbors.Count == 0) continue;

            int pick = neighbors[Random.Range(0, neighbors.Count)];
            lastEmptySlot = emptySlot;
            QuickSwap(pick);
        }

        for (int slot = 0; slot < grid.Length; slot++)
        {
            int tileIdx = grid[slot];
            if (tileIdx >= 0) tiles[tileIdx].Teleport(SlotToWorldPos(slot));
        }

        // Count displaced tiles so we can confirm shuffleMoves is being respected.
        int displaced = 0;
        for (int i = 0; i < grid.Length - 1; i++)
            if (grid[i] != i) displaced++;
        Debug.Log($"[SlidePuzzle] Shuffled {shuffleMoves} move(s) → {displaced} tile(s) out of place. " +
                  $"Empty slot at index {emptySlot}.");
    }

    // ── Slot Helpers ────────────────────────────────────────────────────────

    // Converts a grid slot index to a world position.
    // Uses TransformPoint so that scaling puzzleOrigin uniformly scales the whole board.
    private Vector3 SlotToWorldPos(int slot)
    {
        float step = tileSize + tileSpacing;
        float halfExtent = (gridSize - 1) * step * 0.5f;
        int row = slot / gridSize;
        int col = slot % gridSize;
        Vector3 localPos = new Vector3(col * step - halfExtent, 0f, row * step - halfExtent);
        return puzzleOrigin.TransformPoint(localPos);
    }

    private List<int> GetAdjacentFilledSlots(int slot)
    {
        var result = new List<int>();
        int row = slot / gridSize;
        int col = slot % gridSize;
        if (row > 0)            result.Add((row - 1) * gridSize + col);
        if (row < gridSize - 1) result.Add((row + 1) * gridSize + col);
        if (col > 0)            result.Add(row * gridSize + (col - 1));
        if (col < gridSize - 1) result.Add(row * gridSize + (col + 1));
        return result;
    }

    // ── Tile Movement ───────────────────────────────────────────────────────

    // Instant swap used during shuffle (no animation).
    private void QuickSwap(int filledSlot)
    {
        int tileIdx = grid[filledSlot];
        grid[emptySlot] = tileIdx;
        grid[filledSlot] = -1;
        emptySlot = filledSlot;
    }

    // Animated slide used during gameplay.
    private void SlideIntoEmpty(int filledSlot)
    {
        int tileIdx = grid[filledSlot];
        Vector3 destination = SlotToWorldPos(emptySlot);
        grid[emptySlot] = tileIdx;
        grid[filledSlot] = -1;
        emptySlot = filledSlot;

        tileIsSliding = true;
        tiles[tileIdx].SlideTo(destination, OnTileSlideFinished);
    }

    private void OnTileSlideFinished()
    {
        tileIsSliding = false;
        RefreshHighlights();
        CheckWin();
    }

    // ── Click Handling ───────────────────────────────────────────────────────

    private void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        SlidePuzzleTile tile = hit.collider.GetComponentInParent<SlidePuzzleTile>();
        if (tile == null) return;

        int tileSlot = System.Array.IndexOf(grid, tile.TileIndex);
        if (tileSlot < 0) return;

        if (!GetAdjacentFilledSlots(emptySlot).Contains(tileSlot)) return;

        SlideIntoEmpty(tileSlot);
    }

    // ── Highlights ────────────────────────────────────────────────────────────

    // Yellow-tint the tiles that are adjacent to the empty slot (i.e. can be clicked).
    private void RefreshHighlights()
    {
        foreach (SlidePuzzleTile t in tiles) t.HideHighlight();

        foreach (int slot in GetAdjacentFilledSlots(emptySlot))
        {
            int idx = grid[slot];
            if (idx >= 0) tiles[idx].ShowHighlight();
        }
    }

    // ── Win Condition ────────────────────────────────────────────────────────

    private void CheckWin()
    {
        int total = gridSize * gridSize;
        int displaced = 0;
        for (int i = 0; i < total - 1; i++)
            if (grid[i] != i) displaced++;

        Debug.Log($"[SlidePuzzle] CheckWin: {displaced} tile(s) still displaced, emptySlot={emptySlot} (need {total - 1}).");

        if (displaced > 0 || emptySlot != total - 1) return;

        isSolved = true;
        foreach (SlidePuzzleTile t in tiles) t.HideHighlight();
        StartCoroutine(WinDelay());
    }

    private IEnumerator WinDelay()
    {
        yield return new WaitForSeconds(0.5f);
        UnlockLevel();
    }

    // ── Realm Visibility ────────────────────────────────────────────────────

    public void ShowImages()
    {
        foreach (SlidePuzzleTile t in tiles) t.ShowImageFace();
    }

    public void HideImages()
    {
        foreach (SlidePuzzleTile t in tiles) t.HideImageFace();
    }

    // ── Level Overrides ──────────────────────────────────────────────────────

    public override void UnlockLevel() => ringPiece.SetActive(true);

    public override void CompleteLevel() => levelWall.Play("GateOpen");
}
