using System.Collections.Generic;

public class FlameNodeNew
{
    public string Name;
    public int Count;
    public readonly Dictionary<string, FlameNodeNew> Children = new();

    public FlameNodeNew(string name)
    {
        Name = name;
    }

    public FlameNodeNew GetOrAddChild(string key)
    {
        if (!Children.TryGetValue(key, out var child))
        {
            child = new FlameNodeNew(key);
            Children[key] = child;
        }
        return child;
    }
}
