using System;
using UnityEngine;

namespace VaporUIElements
{
    /// <summary>
    /// Place this attribute on a <see cref="MonoBehaviour"/> or <see cref="ScriptableObject"/> that should be drawn with unity default drawers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DrawWithUnityAttribute : Attribute
    {

    }
}
