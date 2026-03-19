using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using UnityEngine;

[Flags]
public enum InteractionType
{
    None                        = 0,

    Reconnect                   = 1 << 0,

    CityFloorSelect             = 1 << 1,
    CityFloorHover              = 1 << 2,
    CityDistrictSelect          = 1 << 3,
    CityDistrictHover           = 1 << 4,

    CityLockUnlock              = 1 << 5,
    CityMethodOrderingOpen      = 1 << 6,
    CityMethodOrderingChange    = 1 << 7,
    CityRebuild                 = 1 << 8,
    CityDestroy                 = 1 << 9,

    CityFloorOpenInIDE          = 1 << 10,
    CityDistrictHide            = 1 << 11,

    FlameGraphMethodSelect      = 1 << 12,
    FlameGraphMethodHover       = 1 << 13,

    ActivityMapMethodSelect     = 1 << 14,
    ActivityMapMethodHover      = 1 << 15,

    All =
        CityFloorSelect |
        CityFloorHover |
        CityDistrictSelect |
        CityDistrictHover |
        CityLockUnlock |
        CityMethodOrderingOpen |
        CityMethodOrderingChange |
        CityRebuild |
        CityDestroy |
        CityFloorOpenInIDE |
        CityDistrictHide |
        FlameGraphMethodSelect |
        FlameGraphMethodHover |
        ActivityMapMethodSelect |
        ActivityMapMethodHover
}

[Serializable]
public class InteractionEvent
{
    public InteractionType type;
    public string packageName;
    public string className;
    public string methodName;
    public string methodKey;
    public float timestamp;
    public long unixTimestamp;
}

public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager Instance { get; private set; }

    [SerializeField] private bool defaultLoggingEnabled = true;
    [SerializeField] private bool enableLogging = true;
    [SerializeField] private InteractionType loggedInteractionTypes = InteractionType.All;
    [SerializeField] private float dumpIntervalSeconds = 10f;

    private readonly List<InteractionEvent> eventBuffer = new();
    private string sessionId;
    public string SessionID => sessionId;

    private string filePath;

    private float sessionStartTime;

    public bool EnableLogging => enableLogging;
    public bool SetEnableLogging(bool enable) => enableLogging = enable;

    public bool DefaultLoggingEnabled => defaultLoggingEnabled;

    private const string CSV_HEADER = "sessionId,type,package,class,method,key,timestamp,unix\n";

    public string LogDirectory => Path.Combine(Application.persistentDataPath, "ExperimentLogs");

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeSession();
    }

    private void InitializeSession()
    {
        sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        sessionStartTime = Time.time;

        if (!Directory.Exists(LogDirectory))
            Directory.CreateDirectory(LogDirectory);

        string safeSessionId = MakeSafeFileName(sessionId);
        filePath = Path.Combine(LogDirectory, $"session_{safeSessionId}.csv");

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, CSV_HEADER);
        }

        Debug.Log($"[Experiment Manager] File created: {filePath}");

        StartCoroutine(PeriodicDump());
    }

    public void LogInteraction(
        InteractionType type,
        string packageName = null,
        string className = null,
        string methodName = null
    )
    {
        if (!enableLogging || (loggedInteractionTypes & type) == 0) return;

        string methodKey = $"{packageName}.{className}.{methodName}";

        InteractionEvent interactionEvent = new()
        {
            type = type,
            packageName = packageName,
            className = className,
            methodName = methodName,
            methodKey = methodKey,
            timestamp = Time.time - sessionStartTime,
            unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };

        eventBuffer.Add(interactionEvent);

        if (eventBuffer.Count > 100)
            DumpToFile();
    }

    private IEnumerator PeriodicDump()
    {
        while (true)
        {
            yield return new WaitForSeconds(dumpIntervalSeconds);
            DumpToFile();
        }
    }

    public void DumpToFile()
    {
        if (eventBuffer.Count == 0) return;

        try
        {
            using (StreamWriter writer = new(filePath, true))
            {
                foreach (var evt in eventBuffer)
                {
                    writer.WriteLine(
                        $"{sessionId}," +
                        $"{evt.type}," +
                        $"{Escape(evt.packageName)}," +
                        $"{Escape(evt.className)}," +
                        $"{Escape(evt.methodName)}," +
                        $"{Escape(evt.methodKey)}," +
                        $"{evt.timestamp.ToString("F4", CultureInfo.InvariantCulture)}," +
                        $"{evt.unixTimestamp}"
                    );
                }
            }

            Debug.Log($"[Experiment Manager] Logged {eventBuffer.Count} interactions");

            eventBuffer.Clear();
        }
        catch (Exception e)
        {
            Debug.LogError($"Experiment dump failed: {e.Message}");
        }
    }

    private string Escape(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            value = value.Replace("\"", "\"\"");
            return $"\"{value}\"";
        }

        return value;
    }

    private string MakeSafeFileName(string value)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(c, '_');
        }

        return value;
    }

    private void OnApplicationQuit()
    {
        DumpToFile();
    }

    [Serializable]
    private class InteractionWrapper
    {
        public List<InteractionEvent> events;
    }
}
