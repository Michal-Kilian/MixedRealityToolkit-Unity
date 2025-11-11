using System.Linq;
using UnityEngine;

public class CityHelpers
{
    public static CityHelpers Instance { get; set; } = new CityHelpers();

    public float FindMaxRawBuildingHeight(ProjectStructure project)
    {
        float maxRawHeight = 0f;

        if (project?.Packages == null) return 0f;

        foreach (var package in project.Packages)
        {
            maxRawHeight = Mathf.Max(maxRawHeight, FindMaxHeightInPackageRecursive(package));
        }

        return maxRawHeight;
    }

    public float FindMaxHeightInPackageRecursive(PackageNode pkg)
    {
        float maxRawHeight = 0f;

        if (pkg?.Files != null)
        {
            foreach (var file in pkg.Files)
            {
                foreach (var cls in file.Classes)
                {
                    float currentClassHeight = cls.Methods?.Sum(m => Mathf.Max(0.01f, m.LineCount * 0.001f)) ?? 0f;
                    maxRawHeight = Mathf.Max(maxRawHeight, currentClassHeight);
                }
            }
        }

        if (pkg?.SubPackages != null)
        {
            foreach (var subPackage in pkg.SubPackages)
            {
                maxRawHeight = Mathf.Max(maxRawHeight, FindMaxHeightInPackageRecursive(subPackage));
            }
        }

        return maxRawHeight;
    }

    public bool HasAnyClassesRecursive(PackageNode pkg)
    {
        if (pkg.Files != null && pkg.Files.Any(f => f.Classes.Any()))
            return true;

        if (pkg.SubPackages == null) return false;
        foreach (var sub in pkg.SubPackages)
        {
            if (HasAnyClassesRecursive(sub))
                return true;
        }

        return false;
    }

    /*public static Color BaseClassColor(ClassNode c)
    {
        if (c.IsInterface)
            return new Color(0.25f, 0.65f, 1.0f);
        if (c.IsAbstract)
            return new Color(0.65f, 0.4f, 1.0f);

    }*/
}
