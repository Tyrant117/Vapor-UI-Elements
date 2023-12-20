using System;
using UnityEngine;
using System.Diagnostics;

namespace VaporUIElements
{
    [Conditional("VAPOR_INSPECTOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HideInPlayModeAttribute : PropertyAttribute
    {
        public HideInPlayModeAttribute()
        {
        }
    }
}
