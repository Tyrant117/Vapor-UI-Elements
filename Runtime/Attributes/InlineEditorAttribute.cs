using System;
using System.Diagnostics;
using UnityEngine;

namespace VaporUIElements
{
    [Conditional("VAPOR_INSPECTOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class InlineEditorAttribute : PropertyAttribute
    {

    }
}
