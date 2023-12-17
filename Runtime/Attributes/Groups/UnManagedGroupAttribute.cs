using System;
using UnityEngine;

namespace VaporUIElements
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class UnManagedGroupAttribute : Attribute
    {
        public UIGroupType UnmanagedGroupType { get; }
        public int UnmanagedGroupOrder { get; }
        public string UnmanagedGroupHeader { get; }

        public UnManagedGroupAttribute(UIGroupType unmanagedGroupType = UIGroupType.Vertical, int unmanagedGroupOrder = int.MaxValue)
        {
            UnmanagedGroupType = unmanagedGroupType;
            UnmanagedGroupOrder = unmanagedGroupOrder;
            UnmanagedGroupHeader = "Un-Grouped";
        }
    }
}
