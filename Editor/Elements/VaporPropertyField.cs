using System;
using UnityEngine.UIElements;

namespace VaporUIElementsEditor
{
    public class VaporPropertyField : VisualElement, IBindable
    {
        public IBinding binding { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string bindingPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
