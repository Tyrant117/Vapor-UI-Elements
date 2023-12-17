using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VaporUIElements
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class LabelWidthAttribute : PropertyAttribute
    {
        public float Width { get; }
        public bool UseAutoWidth { get; }

        public LabelWidthAttribute(float width = -1)
        {
            Width = width;
            UseAutoWidth = Width < 0;
        }
    }
}
