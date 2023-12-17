using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VaporUIElements
{
    public class AutoReferenceAttribute : PropertyAttribute
    {
        public bool SearchChildren { get; }
        public bool SearchParents { get; }
        public bool AddIfNotFound { get; }

        public AutoReferenceAttribute(bool searchChildren = false, bool searchParents = false, bool addIfNotFound = false)
        {
            SearchChildren = searchChildren;
            SearchParents = searchParents;
            AddIfNotFound = addIfNotFound;
        }
    }
}
