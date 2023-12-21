using System;
using System.Diagnostics;
using UnityEngine;

namespace VaporUIElements
{
    /// <summary>
    /// Place this attribute on a <see cref="MonoBehaviour"/> or <see cref="ScriptableObject"/> that should be drawn with unity default drawers.
    /// </summary>
    [Conditional("VAPOR_INSPECTOR")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
    public class DrawWithUnityAttribute : Attribute
    {

    }
}
