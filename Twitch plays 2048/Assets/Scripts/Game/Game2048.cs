using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[Serializable]
public struct ColorPair
{
    public int Value;
    public Color TileColor;
}

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

    #region Public Properties
    public Dictionary<int, Color> Colors { get; private set; } = new Dictionary<int, Color>();
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
    [SerializeField]
    private List<ColorPair> colors = new List<ColorPair>();
    #endregion

    #region References
    [Header("References")]
    [SerializeField]
    private TextMeshProUGUI refScoreText; 
    #endregion

    public int TileCounter = 0;

    // Commonly used variables
    private Vector3 tileScale;
    private Timer timer = new Timer();

    // Game information
    private List<List<Tile>> tiles;

    // Score
    public int Score { get; private set; } = 0;
    public int HighScore { get; private set; } = 0;

    private void Awake()
    {
        if (Instance)
        {
            Logger.Log(tag, "Attempting to overwrite a singleton instance", Logger.LogType.Warning);
            return;
        }
        Instance = this;

        // Assign colors dictionary
        foreach (var c in colors)
        {
            Colors.Add(c.Value, c.TileColor);
        }
    }

    private void Start()
    {
        tileScale = GameManager.Instance.RefTileObjectPool.RefBlueprint.transform.localScale;

        timer.Setup(Settings.Instance.AnimationTime, true);
    }

    private void Update()
    {
        // Update timer
        timer.Update(Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            Move(Direction.Left);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            Move(Direction.Right);
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            Move(Direction.Up);
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            Move(Direction.Down);
        if (Input.GetKeyDown(KeyCode.Space))
            StartGame(4, 4);
    }

    public void StartGame(int? newColumn = null, int? newRow = null)
    {
        // Assign width and height
        columns = newColumn.GetValueOrDefault(columns);
        rows = newRow.GetValueOrDefault(rows);

        // Reset score
        Score = 0;
        if (refScoreText)
            refScoreText.text = Score.ToString();

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
        timer.Reset(false);
    }

    public void Move(Direction dir)
    {
        // Do not move if animation is not done
        if (!timer.Pause && !timer.IsTime)
            return;

        bool change = false;
        switch (dir)
        {
            case Direction.Left:
                {
                    // Loop through all rows
                    for (int r = 0; r < rows; ++r)
                    {
                        // Fetch list of tiles
                        List<Tile> tilesWithoutSpace = new List<Tile>();
                        bool isSpace = false;
                        for (int c = 0; c < columns; ++c)
                        {
                            var tile = tiles[r][c];
                            if (tile)
                            {
                                tilesWithoutSpace.Add(tile);
                                if (isSpace)
                                    change = true;
                            }
                            else
                                isSpace = true;
                        }

                        // Try moving
                        var result = move(tilesWithoutSpace);

                        // Move tiles if there is a change
                        if (change || (!change && hasChange(tilesWithoutSpace, result)))
                        {
                            // Move tiles
                            for (int c = 0; c < columns; ++c)
                            {
                                if (c < result.Count)
                                {
                                    var tile = result[c];
                                    tile.Move(r, c);
                                    tiles[r][c] = tile;
                                }
                                else
                                    tiles[r][c] = null;
                            }
                            change = true;
                        }
                    }
                }
                break;
            case Direction.Right:
                {
                    // Loop through all rows
                    for (int r = 0; r < rows; ++r)
                    {
                        // Fetch list of tiles
                        List<Tile> tilesWithoutSpace = new List<Tile>();
                        bool isSpace = false;
                        for (int c = columns - 1; c >= 0; --c)
                        {
                            var tile = tiles[r][c];
                            if (tile)
                            {
                                tilesWithoutSpace.Add(tile);
                                if (isSpace)
                                    change = true;
                            }
                            else
                                isSpace = true;
                        }

                        // Try moving
                        var result = move(tilesWithoutSpace);

                        // Move tiles if there is a change
                        if (change || (!change && hasChange(tilesWithoutSpace, result)))
                        {
                            // Move tiles
                            for (int c = columns - 1; c >= 0; --c)
                            {
                                var resultIndex = columns - c - 1;
                                if (resultIndex < result.Count)
                                {
                                    var tile = result[resultIndex];
                                    tile.Move(r, c);
                                    tiles[r][c] = tile;
                                }
                                else
                                    tiles[r][c] = null;
                            }
                            change = true;
                        }
                    }
                }
                break;
            case Direction.Up:
                {
                    // Loop through all columns
                    for (int c = 0; c < columns; ++c)
                    {
                        // Fetch list of tiles
                        List<Tile> tilesWithoutSpace = new List<Tile>();
                        bool isSpace = false;
                        for (int r = 0; r < rows; ++r)
                        {
                            var tile = tiles[r][c];
                            if (tile)
                            {
                                tilesWithoutSpace.Add(tile);
                                if (isSpace)
                                    change = true;
                            }
                            else
                                isSpace = true;
                        }

                        // Try moving
                        var result = move(tilesWithoutSpace);

                        // Move tiles if there is a change
                        if (change || (!change && hasChange(tilesWithoutSpace, result)))
                        {
                            for (int r = 0; r < rows; ++r)
                            {
                                if (r < result.Count)
                                {
                                    var tile = result[r];
                                    tile.Move(r, c);
                                    tiles[r][c] = tile;
                                }
                                else
                                    tiles[r][c] = null;
                            }
                        }
                    }
                }
                break;
            case Direction.Down:
                {
                    // Loop through all columns
                    for (int c = 0; c < columns; ++c)
                    {
                        // Fetch list of tiles
                        List<Tile> tilesWithoutSpace = new List<Tile>();
                        bool isSpace = false;
                        for (int r = rows - 1; r >= 0; --r)
                        {
                            var tile = tiles[r][c];
                            if (tile)
                            {
                                tilesWithoutSpace.Add(tile);
                                if (isSpace)
                                    change = true;
                            }
                            else
                                isSpace = true;
                        }

                        // Try moving
                        var result = move(tilesWithoutSpace);

                        // Move tiles if there is a change
                        if (change || (!change && hasChange(tilesWithoutSpace, result)))
                        {
                            for (int r = rows - 1; r >= 0; --r)
                            {
                                var resultIndex = rows - r - 1;
                                if (resultIndex < result.Count)
                                {
                                    var tile = result[resultIndex];
                                    tile.Move(r, c);
                                    tiles[r][c] = tile;
                                }
                                else
                                    tiles[r][c] = null;
                            }
                        }
                    }
                }
                break;
        }

        // Generate new tile
        if (change)
        {
            generateTile();
            timer.Reset(false);
        }

        if (isGameOver())
        {
            if (refScoreText)
                refScoreText.text = "Game Over";
        }
    }

    #region Helper Functions
    public Vector3 GetTilePosition(Tile t)
    {
        return GetTilePosition(t.RowIndex, t.ColIndex);
    }

    public Vector3 GetTilePosition(int rowIndex, int colIndex)
    {
        colIndex = Mathf.Clamp(colIndex, 0, columns - 1);
        rowIndex = Mathf.Clamp(rowIndex, 0, rows - 1);

        // Calculate starting position
        var totalX = (columns * tileScale.x) + (tileSpacing * (columns - 1));
        var totalY = (rows * tileScale.y) + (tileSpacing * (rows - 1));
        var starting = new Vector3(
            -(totalX * 0.5f) + (tileScale.x * 0.5f),
            (totalY * 0.5f) - (tileScale.y * 0.5f),
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
        return 2 * UnityEngine.Random.Range(1, 3);
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
                rowIndex = UnityEngine.Random.Range(0, rows);
                colIndex = UnityEngine.Random.Range(0, columns);

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
        if (refScoreText)
            refScoreText.text = Score.ToString();
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

    private List<Tile> move(List<Tile> tilesWithoutSpace)
    {
        // Loop through all tiles in the row
        List<Tile> result = new List<Tile>();
        for (int placement = 1; placement < tilesWithoutSpace.Count; ++placement)
        {
            Tile leftTile = tilesWithoutSpace[placement - 1];
            Tile current = tilesWithoutSpace[placement];

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
            if (placement >= tilesWithoutSpace.Count - 1 && current.IsActive)
                result.Add(current);
        }

        // Result empty, use original
        if (result.Count <= 0)
            result = tilesWithoutSpace;

        return result;
    }

    public Color GetTileColor(int value)
    {
        Color c;
        if (Colors.TryGetValue(value, out c))
        {
            return c;
        }
        return colors.Last().TileColor;
    }

    private bool isGameOver()
    {
        if (TileCounter >= rows * columns)
        {
            for (int r = 0; r < rows; ++r)
            {
                for (int c = 0; c < columns; ++c)
                {
                    var current = tiles[r][c];
                    
                    // Check able to merge with right tile
                    Tile right = c + 1 < columns ? tiles[r][c + 1] : null;
                    if (right && current.Value == right.Value)
                        return false;

                    // Check able to merge with bototm tile
                    Tile bottom = r + 1 < rows ? tiles[r + 1][c] : null;
                    if (bottom && current.Value == bottom.Value)
                        return false;
                }
            }
            return true;
        }
        return false;
    }
    #endregion
}
