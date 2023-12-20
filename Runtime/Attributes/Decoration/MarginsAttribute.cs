using System;
using System.Diagnostics;
using UnityEngine;

namespace VaporUIElements
{
    [Conditional("VAPOR_INSPECTOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class MarginsAttribute : PropertyAttribute
    {
        public float Left { get; }
        public float Right { get; }
        public float Top { get; }
        public float Bottom { get; }

        /// <summary>
        /// Use this attribute to change the margins of a property.
        /// Float.MinValue are the default margins.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        public MarginsAttribute(float left = float.MinValue, float right = float.MinValue, float top = float.MinValue, float bottom = float.MinValue)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }        
    }
}
