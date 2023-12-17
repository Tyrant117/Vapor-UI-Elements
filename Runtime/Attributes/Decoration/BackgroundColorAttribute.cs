using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace VaporUIElements
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class BackgroundColorAttribute : PropertyAttribute
    {
        public StyleColor BackgroundColor { get; }

        public BackgroundColorAttribute(float r, float g, float b)
        {
            BackgroundColor = new (new Color(r, g, b));
        }

        public BackgroundColorAttribute(string html)
        {
            BackgroundColor = ColorUtility.TryParseHtmlString(html, out var htmlColor) ? (new(htmlColor)) : new(new Color(0, 0, 0, 0));
        }
    }
}
