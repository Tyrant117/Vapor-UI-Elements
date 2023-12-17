using System;
using UnityEngine;

namespace VaporUIElements
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public ResolverType ResolverType { get; }
        public string Resolver { get; } = "";

        public ShowIfAttribute(string resolver)
        {
            if (ResolverUtility.HasResolver(resolver, out var type))
            {
                ResolverType = type;
                Resolver = resolver;
            }
        }
    }
}
