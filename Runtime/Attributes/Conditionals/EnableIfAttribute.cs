using System;
using UnityEngine;

namespace VaporUIElements
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class EnableIfAttribute : PropertyAttribute
    {
        public ResolverType ResolverType { get; }
        public string Resolver { get; } = "";

        public EnableIfAttribute(string resolver)
        {
            if (ResolverUtility.HasResolver(resolver, out var type))
            {
                ResolverType = type;
                Resolver = resolver;
            }
        }
    }
}
