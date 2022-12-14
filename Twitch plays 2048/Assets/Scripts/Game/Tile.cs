using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool IsActive { get { return gameObject.activeSelf; } }

    public int Value;
    public int RowIndex;
    public int ColIndex;

    private SpriteRenderer refSR;
    private TextMeshPro refValueText;

    #region MonoBehaviour
    private void Awake()
    {
        refValueText = GetComponentInChildren<TextMeshPro>();
        refSR = GetComponentInChildren<SpriteRenderer>();
    }
    #endregion

    public void Place(int rowIndex, int colIndex, int? value = null)
    {
        updateValue(value.GetValueOrDefault(Value));
        Move(rowIndex, colIndex);

        // Update tile counter
        ++Game2048.Instance.TileCounter;
    }

    public void Move(int rowIndex, int colIndex)
    {
        RowIndex = rowIndex;
        ColIndex = colIndex;
        transform.localPosition = Game2048.Instance.GetTilePosition(rowIndex, colIndex);
    }

    public void Merge(Tile t)
    {
        updateValue(Value + t.Value);
        t.Deactivate();
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);

        // Update tile counter
        --Game2048.Instance.TileCounter;
    }

    #region Helper Functions
    private void updateValue(int newValue)
    {
        Value = newValue;

        // Update value text
        if (refValueText)
            refValueText.text = Value.ToString();

        // Update tile color
        if (refSR)
            refSR.color = Game2048.Instance.GetTileColor(Value);
    }
    #endregion
}
