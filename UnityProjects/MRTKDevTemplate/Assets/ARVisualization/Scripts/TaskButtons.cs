using MixedReality.Toolkit.UX;
using System;
using System.IO;
using UnityEngine;

public class TaskButtons : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private PressableButton task1Button;
    [SerializeField] private PressableButton task2Button;
    [SerializeField] private PressableButton task3Button;

    [Header("Visual Targets")]
    [SerializeField] private Renderer task1Renderer;
    [SerializeField] private Renderer task2Renderer;
    [SerializeField] private Renderer task3Renderer;

    [Header("Colors")]
    [SerializeField] private Material availableMaterial;
    [SerializeField] private Material disabledMaterial;
    [SerializeField] private Material runningMaterial;

    private PressableButton currentRunningButton;
    private int currentRunningTask = 0;

    private string csvFilePath;
    private string sessionId;

    private void Start()
    {
        task1Button.selectEntered.AddListener(arg0 => OnButtonPressed(1, task1Button));
        task2Button.selectEntered.AddListener(arg0 => OnButtonPressed(2, task2Button));
        task3Button.selectEntered.AddListener(arg0 => OnButtonPressed(3, task3Button));

        if (ExperimentManager.Instance == null)
        {
            Debug.LogError("[TaskButtons] ExperimentManager.Instance is null");
            enabled = false;
            return;
        }

        sessionId = ExperimentManager.Instance.SessionID;

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            Debug.LogError("[TaskButtons] SessionID is null or empty");
            enabled = false;
            return;
        }

        string logDirectory = ExperimentManager.Instance.LogDirectory;
        string safeSessionId = MakeSafeFileName(sessionId);
        csvFilePath = Path.Combine(logDirectory, $"task-events-{safeSessionId}.csv");

        EnsureCsvFileExists();
        UpdateButtonsState();
    }

    private void OnDestroy()
    {
        task1Button.selectEntered.RemoveAllListeners();
        task2Button.selectEntered.RemoveAllListeners();
        task3Button.selectEntered.RemoveAllListeners();
    }

    private void OnButtonPressed(int task, PressableButton pressedButton)
    {
        if (currentRunningTask == 0)
        {
            StartTask(task, pressedButton);
            return;
        }

        if (pressedButton == currentRunningButton)
        {
            StopCurrentTask();
        }
    }

    private void StartTask(int task, PressableButton button)
    {
        currentRunningTask = task;
        currentRunningButton = button;

        WriteCsvEvent("start", task);
        UpdateButtonsState();
    }

    private void StopCurrentTask()
    {
        int taskToStop = currentRunningTask;

        currentRunningTask = 0;
        currentRunningButton = null;

        WriteCsvEvent("stop", taskToStop);
        UpdateButtonsState();
    }

    private void UpdateButtonsState()
    {
        bool noTaskRunning = currentRunningTask == 0;

        SetButtonState(
            task1Button,
            task1Renderer,
            noTaskRunning || currentRunningButton == task1Button,
            currentRunningButton == task1Button
        );

        SetButtonState(
            task2Button,
            task2Renderer,
            noTaskRunning || currentRunningButton == task2Button,
            currentRunningButton == task2Button
        );

        SetButtonState(
            task3Button,
            task3Renderer,
            noTaskRunning || currentRunningButton == task3Button,
            currentRunningButton == task3Button
        );
    }

    private void SetButtonState(
        PressableButton button,
        Renderer targetRenderer,
        bool canInteract,
        bool isRunning
    )
    {
        button.enabled = canInteract;

        if (targetRenderer != null)
        {
            if (isRunning)
            {
                targetRenderer.material = runningMaterial;
            }
            else if (canInteract)
            {
                targetRenderer.material = availableMaterial;
            }
            else
            {
                targetRenderer.material = disabledMaterial;
            }
        }
    }

    private void EnsureCsvFileExists()
    {
        if (!File.Exists(csvFilePath))
        {
            File.WriteAllText(csvFilePath, "sessionId,timestamp,type,task\n");
        }
    }

    private void WriteCsvEvent(string eventType, int task)
    {
        try
        {
            string timestamp = DateTimeOffset.Now.ToString("o");
            string line = $"{EscapeCsv(sessionId)},{EscapeCsv(timestamp)},{EscapeCsv(eventType)},{task}{Environment.NewLine}";

            File.AppendAllText(csvFilePath, line);

            if (eventType == "start")
            {
                Debug.Log($"[TaskButtons] Started Task {task} at {timestamp}");
            }
            else
            {
                Debug.Log($"[TaskButtons] Stopped Task {task} at {timestamp}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[TaskButtons] Failed to write CSV event: {e.Message}");
        }
    }

    private string MakeSafeFileName(string value)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(c, '_');
        }

        return value;
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        if (
            value.Contains(",")
            || value.Contains("\"")
            || value.Contains("\n")
            || value.Contains("\r")
        )
        {
            value = value.Replace("\"", "\"\"");
            return $"\"{value}\"";
        }

        return value;
    }
}
