using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    private static string tag = "Settings";

    #region Singleton
    public static Settings Instance { get; private set; } = null;
    #endregion

    public float AnimationTime { get { return animationTime; } }
    [SerializeField]
    private float animationTime = 0.5f;

    #region MonoBehaviour
    private void Awake()
    {
        if (Instance != null)
        {
            Logger.Log(tag, "Attempting to overwrite a singleton instance", Logger.LogType.Warning);
        }
        Instance = this;
    }
    #endregion
}
