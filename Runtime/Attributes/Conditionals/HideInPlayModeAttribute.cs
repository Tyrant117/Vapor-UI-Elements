using System;
using UnityEngine;

namespace VaporUIElements
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HideInPlayModeAttribute : PropertyAttribute
    {
        public HideInPlayModeAttribute()
        {
        }
    }
}
