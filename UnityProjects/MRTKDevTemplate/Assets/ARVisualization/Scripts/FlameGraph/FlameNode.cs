using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlameNode
{
    public string name;
    public int sampleCount = 0;
    public Dictionary<string, FlameNode> children = new();

    public FlameBar bar;

    public FlameNode(string name)
    {
        this.name = name;
    }

    public FlameNode GetOrAdd(string childName)
    {
        if (!children.TryGetValue(childName, out var node))
        {
            node = new FlameNode(childName);
            children[childName] = node;
        }
        return node;
    }

    public int TotalSampleCount()
    {
        int childrenTotal = children.Values.Sum(c => c.TotalSampleCount());
        return sampleCount + childrenTotal;
    }
}
