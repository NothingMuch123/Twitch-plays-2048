using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool IsActive { get { return !merging; } }

    public int Value;
    public int RowIndex;
    public int ColIndex;

    // References
    private SpriteRenderer refSR;
    private TextMeshPro refValueText;
    private Vector3 originalScale;

    // Animation flags
    private bool appearing = false;
    private bool moving = false;
    private bool merging = false;

    // Animation speed
    private float appearingSpeed = 1.0f;
    private float disappearingSpeed = 1.0f;
    private float? movingSpeed = null;
    private Tile toFollow = null;
    private Vector3 newPos = Vector3.zero;

    #region MonoBehaviour
    private void Awake()
    {
        refValueText = GetComponentInChildren<TextMeshPro>();
        refSR = GetComponentInChildren<SpriteRenderer>();

        // Appearing animation
        originalScale = transform.localScale;
        appearingSpeed = originalScale.x / Settings.Instance.AnimationTime;
        disappearingSpeed = 1.0f / Settings.Instance.AnimationTime;
    }

    private void Update()
    {
        // Animation
        if (appearing)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, originalScale, appearingSpeed * Time.deltaTime);

            // Stop once done
            if (transform.localScale == originalScale)
                appearing = false;
        }
        else if (moving)
        {
            // Calculate moving speed
            if (!movingSpeed.HasValue)
            {
                calculateMovingSpeed();
            }

            // Move
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, newPos, movingSpeed.Value * Time.deltaTime);

            // Stop once done
            if (transform.localPosition == newPos)
            {
                moving = false;
                toFollow = null;

                if (merging)
                    Deactivate();
            }
        }

        // Disppearing
        if (merging)
        {
            if (refSR)
            {
                var c = refSR.color;
                refSR.color = CommonUtility.SetAlpha(c, Mathf.Clamp01(c.a - (disappearingSpeed * Time.deltaTime)));
            }
            if (refValueText)
            {
                var c = refValueText.color;
                refValueText.color = CommonUtility.SetAlpha(c, Mathf.Clamp01(c.a - (disappearingSpeed * Time.deltaTime)));
            }
        }
    }
    #endregion

    public void Place(int rowIndex, int colIndex, int? value = null)
    {
        // Reset values
        merging = moving = appearing = false;
        toFollow = null;

        updateValue(value.GetValueOrDefault(Value));
        Move(rowIndex, colIndex, true);

        // Update tile counter
        ++Game2048.Instance.TileCounter;

        // Set appearing animation flag
        Appearing();
    }

    public void Move(int rowIndex, int colIndex, bool skipAnimation = false)
    {
        RowIndex = rowIndex;
        ColIndex = colIndex;

        if (skipAnimation)
        {
            transform.localPosition = Game2048.Instance.GetTilePosition(rowIndex, colIndex);
            return;
        }

        // Set moving animation flag
        moving = true;
        toFollow = this;
        calculateMovingSpeed();

    }

    public void Merge(Tile t)
    {
        updateValue(Value + t.Value);
        t.Merging(this);
    }

    public void Deactivate()
    {
        if (refSR)
            refSR.color = CommonUtility.SetAlpha(refSR.color, 1.0f);
        if (refValueText)
            refValueText.color = CommonUtility.SetAlpha(refValueText.color, 1.0f);
        gameObject.SetActive(false);

        // Update tile counter
        --Game2048.Instance.TileCounter;
    }

    #region Animation
    public void Appearing()
    {
        // Set scale to 0
        transform.localScale = Vector3.zero;

        appearing = true;
    }

    public void Merging(Tile toFollow)
    {
        merging = true;
        moving = true;
        this.toFollow = toFollow;

        // Do not update speed yet because the tile to follow has not update its position
    }
    #endregion

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

    private void calculateMovingSpeed()
    {
        newPos = Game2048.Instance.GetTilePosition(toFollow);
        // Calculate speed from distance
        movingSpeed = Vector3.Distance(transform.localPosition, newPos) / Settings.Instance.AnimationTime;
    }
    #endregion
}
