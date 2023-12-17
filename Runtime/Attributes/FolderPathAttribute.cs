using System;
using UnityEngine;

namespace VaporUIElements
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class FolderPathAttribute : PropertyAttribute
    {
        public bool AbsolutePath { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="absolutePath">Should the absolute path be returned</param>
        public FolderPathAttribute(bool absolutePath = false)
        {
            AbsolutePath = absolutePath;
        }
    }
}
