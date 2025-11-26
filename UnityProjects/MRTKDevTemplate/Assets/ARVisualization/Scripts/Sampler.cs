using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Sampler : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private string folderRelativePath = "Samples/sample-dump";
    [SerializeField] private float intervalSeconds = 1f;

    private readonly List<ExecutionSample> samples = new();
    private int currentIndex = 0;
    private float timer = 0f;

    private void Start()
    {
        LoadSamples();
    }

    private void Update()
    {
        if (samples.Count == 0 || FlameGraph.Instance == null || FlameGraph.Instance.Paused) return;

        timer += Time.deltaTime;
        if (timer > intervalSeconds)
        {
            timer = 0f;
            SendNextSample();
        }
    }

    private void LoadSamples()
    {
        samples.Clear();
        string absolutePath = Path.Combine(Application.dataPath, folderRelativePath);
        if (!Directory.Exists(absolutePath))
        {
            Debug.LogError($"Sampler: Folder not found: {absolutePath}");
            return;
        }

        string[] files = Directory.GetFiles(absolutePath, "*.txt", SearchOption.TopDirectoryOnly);
        System.Array.Sort(files);

        foreach (var file in files)
        {
            try
            {
                string json = File.ReadAllText(file);

                ExecutionSample sample = JsonConvert.DeserializeObject<ExecutionSample>(json);
                if (sample != null && sample.Frames != null)
                {
                    samples.Add(sample);
                }
                else
                {
                    Debug.LogError($"Sampler: Invalid or empty sample in: {file}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Sampler: Failed to read {file}: {e.Message}");
            }
        }

        Debug.Log($"Sampler: Loaded {samples.Count} samples from {absolutePath}");
    }

    private void SendNextSample()
    {
        if (samples.Count == 0) return;

        if (FlameGraph.Instance != null)
            FlameGraph.Instance.OnExecutionSample(samples[currentIndex]);
        if (ActivityMap.Instance != null)
            ActivityMap.Instance.OnExecutionSample(samples[currentIndex]);
        currentIndex = (currentIndex + 1) % samples.Count;
    }
}
