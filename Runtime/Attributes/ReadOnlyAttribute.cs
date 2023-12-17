using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VaporUIElements
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class ReadOnlyAttribute : PropertyAttribute
    {

    }
}
