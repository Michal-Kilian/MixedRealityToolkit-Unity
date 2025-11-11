using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum VisualizationModes
{
    Flash,
    Heatmap,
}

public interface IVisualizationMode
{
    void Initialize(Dictionary<string, GameObject> methodFloors, Dictionary<GameObject, Color> baseColors);
    void OnExecutionSample(ExecutionSample sample);
    void Update();
    void ResetAll();
}
