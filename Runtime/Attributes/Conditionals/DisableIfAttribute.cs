using System;
using System.Diagnostics;
using UnityEngine;

namespace VaporUIElements
{
    [Conditional("VAPOR_INSPECTOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class DisableIfAttribute : PropertyAttribute
    {
        public ResolverType ResolverType { get; }
        public string Resolver { get; } = "";

        public DisableIfAttribute(string resolver)
        {
            if (ResolverUtility.HasResolver(resolver, out var type))
            {
                ResolverType = type;
                Resolver = resolver;
            }
        }
    }
}