using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace VaporUIElements
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class PropertyOrderAttribute : PropertyAttribute
    {
        public int Order { get; }
        public PropertyOrderAttribute(int order)
        {
            Order = order;
            Assert.IsFalse(Order == int.MaxValue, "Int.MaxValue is reserved for internal use. Please use another order value.");
        }
    }
}
