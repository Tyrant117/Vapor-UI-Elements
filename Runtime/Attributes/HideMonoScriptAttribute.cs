using System;
using System.Diagnostics;

namespace VaporUIElements
{
    [Conditional("VAPOR_INSPECTOR")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class HideMonoScriptAttribute : Attribute
    {

    }
}
