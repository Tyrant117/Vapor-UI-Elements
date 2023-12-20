using System;
using System.Diagnostics;
using UnityEngine;

namespace VaporUIElements
{
    [Conditional("VAPOR_INSPECTOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class ValueDropdownAttribute : PropertyAttribute
    {
        public ResolverType ResolverType { get; }
        public string Resolver { get; } = "";

        public ValueDropdownAttribute(string resolver)
        {
            if (!ResolverUtility.HasResolver(resolver, out var type)) return;
            
            ResolverType = type;
            Resolver = resolver;
        }
    }
}
