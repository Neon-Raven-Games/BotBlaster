using System;
using NRTools.AtlasHelper;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class ShowIf : PropertyAttribute
{
    public string ConditionalFieldName { get; }
    public object[] CompareValues { get; }

    public ShowIf(string conditionalFieldName, params object[] compareValues)
    {
        ConditionalFieldName = conditionalFieldName;
        CompareValues = compareValues;
    }
}