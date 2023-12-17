using System;
using UnityEngine;

namespace VaporUIElements
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class DrawWithVaporAttribute : Attribute
    {
        public UIGroupType InlinedGroupType { get; }

        public DrawWithVaporAttribute(UIGroupType inlinedGroupType = UIGroupType.Foldout)
        {
            InlinedGroupType = inlinedGroupType;
        }
    }
}
