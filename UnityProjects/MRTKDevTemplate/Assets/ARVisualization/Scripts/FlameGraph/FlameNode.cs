using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FlameNode
{
    public string Key;
    public string Method;
    public int Depth;
    public int SampleCount;
    public float NormalizedWidth;
    public Dictionary<string, FlameNode> Children = new();

    [NonSerialized] public GameObject Cube;
    [NonSerialized] public float LastUpdatedTime;
}
