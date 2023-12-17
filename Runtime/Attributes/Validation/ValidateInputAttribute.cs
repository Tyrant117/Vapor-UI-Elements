using System;
using UnityEngine;

namespace VaporUIElements
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class ValidateInputAttribute : PropertyAttribute
    {
        public string MethodName { get; } = "";

        public ValidateInputAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}
