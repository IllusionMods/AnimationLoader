//
// class to control the display of logs
//
using BepInEx.Logging;


/// <summary>
/// Show logs when enabled
/// </summary>
internal static class Log
{
    private static ManualLogSource _logSource;
    private static bool _enabled = false;

    public static bool Enabled {
        get 
        {
            return _enabled;
        }

        set 
        {
            _enabled = value;
        }
    }

    public static void SetLogSource(ManualLogSource logSource)
    {
        _logSource = logSource;
    }

    public static void Info(object data)
    {
        if (_enabled)
        {
            _logSource.LogInfo(data);
        }
    }

    public static void Debug(object data)
    {
        if (_enabled)
        {
            _logSource.LogDebug(data);
        }
    }

    public static void Error(object data)
    {
        if (_enabled)
        {
            _logSource.LogError(data);
        }
    }

    public static void Fatal(object data)
    {
        if (_enabled)
        {
            _logSource.LogFatal(data);
        }
    }

    public static void Message(object data)
    {
        if (_enabled)
        {
            _logSource.LogMessage(data);
        }
    }

    public static void Warning(object data)
    {
        if (_enabled)
        {
            _logSource.LogWarning(data);
        }
    }

    /// <summary>
    /// For logs that should not depend on the logs been enabled
    /// </summary>
    /// <param name="level"></param>
    /// <param name="data"></param>
    public static void Level(LogLevel level, object data)
    {
        _logSource.Log(level, data);
    }
}
