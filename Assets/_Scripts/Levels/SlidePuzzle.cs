using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlidePuzzle : Level
{
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

    [Header("Level Integration")]
    public GameObject ringPiece;
    public Animator levelWall;

    // grid[slot] = tile index occupying that slot, -1 = empty
    private int[] grid;
    private SlidePuzzleTile[] tiles;
    private int emptySlot;

    private bool playerInRange = false;
    private bool isSolved = false;
    private bool tileIsSliding = false;

    void Start()
    {
        GameManager.instance.gameLevels.Add(this);
        transform.SetParent(GameManager.instance.transform);
        ringPiece.SetActive(false);

        InitGrid();
        SpawnTiles();
        Shuffle();
    }

    void Update()
    {
        if (!playerInRange || isSolved || tileIsSliding) return;

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
            tile.Init(i, puzzleImage, gridSize);
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
    }

    // ── Slot Helpers ────────────────────────────────────────────────────────

    // Converts a grid slot index to a world position aligned to puzzleOrigin's
    // local right/forward axes so the board can be rotated in the scene.
    private Vector3 SlotToWorldPos(int slot)
    {
        float step = tileSize + tileSpacing;
        float halfExtent = (gridSize - 1) * step * 0.5f;
        int row = slot / gridSize;
        int col = slot % gridSize;
        return puzzleOrigin.position
             + puzzleOrigin.right   * (col * step - halfExtent)
             + puzzleOrigin.forward * (row * step - halfExtent);
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

    // ── Win Condition ────────────────────────────────────────────────────────

    private void CheckWin()
    {
        int total = gridSize * gridSize;
        for (int i = 0; i < total - 1; i++)
            if (grid[i] != i) return;
        if (emptySlot != total - 1) return;

        isSolved = true;
        StartCoroutine(WinDelay());
    }

    private IEnumerator WinDelay()
    {
        yield return new WaitForSeconds(0.5f);
        UnlockLevel();
    }

    // ── Proximity Detection ──────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    // ── Level Overrides ──────────────────────────────────────────────────────

    public override void UnlockLevel() => ringPiece.SetActive(true);

    public override void CompleteLevel() => levelWall.Play("GateOpen");
}
