using System;
using UnityEngine;

namespace VaporUIElements
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class OnValueChangedAttribute : PropertyAttribute
    {
        public string MethodName { get; } = "";

        public OnValueChangedAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}
