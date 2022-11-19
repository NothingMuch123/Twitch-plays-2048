using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Game2048 : MonoBehaviour
{
    public enum Direction
    {
        Left,
        Right,
        Up,
        Down,
    }

    private static string tag = "Game2048";

    #region Singleton Instance
    public static Game2048 Instance { get; private set; } = null;
    #endregion

    #region Settings
    [Header("Settings")]
    [SerializeField]
    private int columns = 4;
    [SerializeField]
    private int rows = 4;
    [SerializeField]
    private float tileSpacing = 0.2f;
    [SerializeField]
    private float tileZPos = -0.1f;
    #endregion

    public int TileCounter = 0;

    // Commonly used variables
    private Vector3 tileScale;

    // Game information
    private List<List<Tile>> tiles;
    public int Score { get; private set; } = 0;
    public int HighScore { get; private set; } = 0;

    private void Awake()
    {
        if (Instance)
            Logger.Log(tag, "Attempting to overwrite a singleton instance", Logger.LogType.Warning);
        Instance = this;
    }

    private void Start()
    {
        tileScale = GameManager.Instance.RefTileObjectPool.RefBlueprint.transform.localScale;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            Move(Direction.Left);
        if (Input.GetKeyDown(KeyCode.Space))
            StartGame(4, 4);
    }

    public void StartGame(int? newColumn = null, int? newRow = null)
    {
        // Assign width and height
        columns = newColumn.GetValueOrDefault(columns);
        rows = newRow.GetValueOrDefault(rows);

        // Clear tiles
        GameManager.Instance.RefTileObjectPool.ResetAll();
        TileCounter = 0;

        // Generate tiles
        tiles = new List<List<Tile>>(rows);
        for (int r = 0; r < rows; ++r)
        {
            tiles.Add(new List<Tile>());
            for (int c = 0; c < columns; ++c)
            {
                tiles[r].Add(null);
            }
        }

        // Generate 2 tiles
        generateTile(2);
    }

    public void Move(Direction dir)
    {
        bool change = false;
        switch (dir)
        {
            case Direction.Left:
                {
                    // Loop through all rows
                    for (int r = 0; r < rows; ++r)
                    {
                        List<Tile> result = new List<Tile>();
                        List<Tile> tilesInRow = new List<Tile>();//(from c in tiles[r] where c select c).ToList();

                        // Fetch list of movable tiles
                        bool isSpace = false;
                        for (int c = 0; c < columns; ++c)
                        {
                            var tile = tiles[r][c];
                            if (tile)
                            {
                                tilesInRow.Add(tile);
                                if (isSpace)
                                    change = true;
                            }
                            else
                            {
                                isSpace = true;
                            }
                        }

                        // Loop through all tiles in the row
                        for (int placement = 1; placement < tilesInRow.Count; ++placement)
                        {
                            Tile leftTile = tilesInRow[placement - 1];
                            Tile current = tilesInRow[placement];

                            // Can merge with left
                            if (leftTile.IsActive && leftTile.Value == current.Value)
                            {
                                leftTile.Merge(current);
                                addScore(leftTile.Value);
                            }
                            
                            // Add active left tile to result
                            if (leftTile.IsActive)
                                result.Add(leftTile);

                            // Add active last tile to result
                            if (placement >= tilesInRow.Count - 1 && current.IsActive)
                                result.Add(current);
                        }

                        // Result empty, use original
                        if (result.Count <= 0)
                            result = tilesInRow;

                        if (change || (!change && hasChange(tilesInRow, result)))
                        {
                            // Place tiles
                            for (int c = 0; c < columns; ++c)
                            {
                                if (c < result.Count)
                                {
                                    var tile = result[c];
                                    tile.Move(r, c);
                                    tiles[r][c] = tile;
                                }
                                else
                                {
                                    tiles[r][c] = null;
                                }
                            }
                            change = true;
                        }
                    }
                }
                break;
            case Direction.Right:
                {

                }
                break;
        }

        // Generate new tile
        if (change)
            generateTile();
    }

    #region Helper Functions
    public Vector3 GetTilePosition(int rowIndex, int colIndex)
    {
        colIndex = Mathf.Clamp(colIndex, 0, columns - 1);
        rowIndex = Mathf.Clamp(rowIndex, 0, rows - 1);

        // Calculate starting position
        var starting = new Vector3(
            -(((columns * tileScale.x + (tileSpacing * (columns - 1))) * 0.5f) + (tileScale.x * 0.5f)),
            ((rows * tileScale.y + (tileSpacing * (rows - 1))) * 0.5f) - (tileScale.y * 0.5f),
            tileZPos
            );

        // Add offset based off index
        return starting + new Vector3(
            colIndex * (tileScale.x + tileSpacing),
            -rowIndex * (tileScale.y + tileSpacing)
            );
    }

    private int generateNewTileValue()
    {
        return 2 * Random.Range(1, 3);
    }

    private void generateTile(int count = 1)
    {
        if (TileCounter >= rows * columns)
            return;

        for (int i = 0; i < count; ++i)
        {
            // Randomly generate new unoccupied position
            int rowIndex = -1, colIndex = -1;
            while (true)
            {
                // Generate random tile index
                rowIndex = Random.Range(0, rows);
                colIndex = Random.Range(0, columns);

                // Check occupied
                if (!tiles[rowIndex][colIndex])
                    break;
            }

            // Get new tile
            var tileObj = GameManager.Instance.RefTileObjectPool.Get();
            tileObj.transform.SetParent(transform, true);

            // Place tile
            var tile = tileObj.GetComponent<Tile>();
            tile.Place(rowIndex, colIndex, generateNewTileValue());

            // Save tile
            tiles[rowIndex][colIndex] = tile;
        }
    }

    private void addScore(int score)
    {
        Score += score;
        if (score >= HighScore)
        {
            HighScore = Score;

            // TODO: Update high score file
        }
    }

    private bool hasChange(List<Tile> original, List<Tile> result)
    {
        if (original.Count != result.Count)
            return true;

        for (int i = 0; i < original.Count; ++i)
        {
            if (original[i].Value != result[i].Value)
                return true;
        }
        return false;
    }
    #endregion
}
