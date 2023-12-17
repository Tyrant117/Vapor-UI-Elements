using System;
using UnityEngine.Assertions;

namespace VaporUIElements
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class HorizontalGroupAttribute : VaporGroupAttribute
    {
        public override UIGroupType Type => UIGroupType.Horizontal;

        public HorizontalGroupAttribute(string groupName, int order = 0)
        {
            GroupName = groupName;
            Order = order;
            Assert.IsFalse(Order == int.MaxValue, "Int.MaxValue is reserved");

            int last = GroupName.LastIndexOf('/');
            ParentName = last != -1 ? GroupName[..last] : "";
        }
    }
}
