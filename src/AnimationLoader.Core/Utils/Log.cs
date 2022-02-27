//
// class to control the display of logs
// Missing rotating logs :( have to do something about that
//
using BepInEx.Logging;


/// <summary>
/// Show logs when enabled
/// </summary>
static internal class Log
{
    static private ManualLogSource _logSource;
    static private bool _enabled = false;
    static private bool _debugToConsole = false;

    static public bool Enabled {
        get 
        {
            return _enabled;
        }

        set 
        {
            _enabled = value;
        }
    }

    static public bool DebugToConsole {
        get 
        {
            return _debugToConsole;
        }
        set 
        {
            _debugToConsole = value;
        }
    }

    static public ManualLogSource LogSource {
        get 
        { 
            return _logSource; 
        }
        set 
        { 
            _logSource = value; 
        }
    }

    static public void SetLogSource(ManualLogSource logSource)
    {
        _logSource = logSource;
    }

    static public void Info(object data)
    {
        if (_enabled)
        {
            _logSource.LogInfo(data);
        }
    }

    static public void Debug(object data)
    {
        if (_enabled)
        {
            if (_debugToConsole)
            {
                _logSource.LogInfo(data);
            }
            else
            {
                _logSource.LogDebug(data);
            }
        }
    }

    static public void Error(object data)
    {
        if (_enabled)
        {
            _logSource.LogError(data);
        }
    }

    static public void Fatal(object data)
    {
        if (_enabled)
        {
            _logSource.LogFatal(data);
        }
    }

    static public void Message(object data)
    {
        if (_enabled)
        {
            _logSource.LogMessage(data);
        }
    }

    static public void Warning(object data)
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
    static public void Level(LogLevel level, object data)
    {
        _logSource.Log(level, data);
    }
}
