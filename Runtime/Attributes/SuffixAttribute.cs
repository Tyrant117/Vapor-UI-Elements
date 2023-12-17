using System;
using UnityEngine;

namespace VaporUIElements
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class SuffixAttribute : PropertyAttribute
    {
        public string Suffix { get; }

        public SuffixAttribute(string suffix)
        {
            Suffix = suffix;
        }
    }
}
