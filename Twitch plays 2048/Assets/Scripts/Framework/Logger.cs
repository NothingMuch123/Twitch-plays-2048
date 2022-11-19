using UnityEngine;

public static class Logger
{
    public enum LogType
    {
        Info,
        Warning,
        Error,
    }
    public static void Log(string tag, string message, LogType type)
    {
        // Generate log message
        var log = $"[{tag}] {message}";

        // TODO: Export our own logs
        var ownLog = $"{type.ToString().ToUpper()}: {log}";

        // Log with unity console
        switch (type)
        {
            case LogType.Info:
                Debug.Log(log);
                break;
            case LogType.Warning:
                Debug.LogWarning(log);
                break;
            case LogType.Error:
                Debug.LogError(log);
                break;
        }
    }
}