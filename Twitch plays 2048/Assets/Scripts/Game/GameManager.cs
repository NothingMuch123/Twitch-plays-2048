using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static string tag = "GameManager";

    #region Singleton Instance
    public static GameManager Instance { get; private set; } = null;
    #endregion

    #region References
    [Header("References")]
    [SerializeField]
    private TwitchIRC refTwitchIRC;
    [SerializeField]
    public ObjectPool RefTileObjectPool;
    #endregion

    #region Debugging
    [Header("Debugging")]
    [SerializeField]
    private bool connectOnStart = true;
    [SerializeField]
    private TextMeshProUGUI refDebugChatMessage;
    #endregion

    private void Awake()
    {
        if (Instance != null)
        {
            Logger.Log(tag, "Attempting to overwrite a singleton instance", Logger.LogType.Warning);
        }
        Instance = this;
    }

    private void Start()
    {
        if (connectOnStart)
        {
            ConnectToTwitchChat();
        }
    }

    #region Event Listener
    public void NewMessage(Chatter c)
    {
        if (refDebugChatMessage)
        {
            switch (c.message)
            {
                case "!start":
                    Game2048.Instance.StartGame(4, 4);
                    break;
            }
            refDebugChatMessage.text = $"{c.tags.displayName}: {c.message}";
        }
    }
    #endregion

    #region Helper Functions
    public void ConnectToTwitchChat()
    {
        if (!refTwitchIRC)
            return;

        // Read oauth token from file
        if (string.IsNullOrWhiteSpace(refTwitchIRC.twitchDetails.oauth))
            refTwitchIRC.twitchDetails.oauth = ((TextAsset)Resources.Load("OAuthKey")).text;

        // Connect
        refTwitchIRC.IRC_Connect();

        // Attach message listener
        refTwitchIRC.newChatMessageEvent.AddListener(NewMessage);
    }
    #endregion
}
