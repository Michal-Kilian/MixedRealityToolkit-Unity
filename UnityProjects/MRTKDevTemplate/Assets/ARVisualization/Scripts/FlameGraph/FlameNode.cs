using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlameNode
{
    public string name;
    public int sampleCount = 0;
    public Dictionary<string, FlameNode> children = new();
    public FlameBar bar;
    public bool userDefined;

    public FlameNode(string name, bool userDefined = true)
    {
        this.name = name;
        this.userDefined = userDefined;
    }

    public FlameNode GetOrAdd(string childName, bool user)
    {
        if (!children.TryGetValue(childName, out var node))
        {
            node = new FlameNode(childName, user);
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
