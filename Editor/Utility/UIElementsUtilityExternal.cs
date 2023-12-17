using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace VaporUIElementsEditor
{
    public class UIElementsUtilityExternal
    {
        private const string SceneHiearchyWindowName = "unity-panel-container4";

        private static bool _typesRequested;
        private static Type _utilityType;
        private static Type _panelType;

        private static Type _panelIteratorKeyValueType;
        private static PropertyInfo _panelIteratorValuePropertyInfo;

        private static bool _getPanelsIteratorMethodInfoRequested;
        private static MethodInfo _getPanelsInteratorMethodInfo;

        private static List<IPanel> _panels = new();

        private static Type GetUtilityType()
        {
            if (_utilityType == null && _typesRequested)
                return null;

            if (_utilityType == null)
            {
                _typesRequested = true;

                var iPanelType = typeof(IPanel); // We assume UIElementsUtility is in the same assembly as IPanel.
                _utilityType = iPanelType.Assembly.GetType("UnityEngine.UIElements.UIElementsUtility");
                _panelType = iPanelType.Assembly.GetType("UnityEngine.UIElements.Panel");
            }

            return _utilityType;
        }        

        public static List<IPanel> GetAllPanels(ContextType? contextType = null)
        {
            _panels.Clear();
            var iterator = GetPanelsIterator();
            if (iterator != null)
            {
                // Sadly the tpye of the iterator is KeyValue<int, Panel> and Panel is NOT a public type.
                // Therefore we have to jump through some hoops to get the actual panel value as an IPanel.
                while (iterator.MoveNext())
                {
                    if (_panelIteratorKeyValueType == null)
                    {
                        _panelIteratorKeyValueType = iterator.Current.GetType();
                        _panelIteratorValuePropertyInfo = _panelIteratorKeyValueType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
                    }

                    if (_panelIteratorValuePropertyInfo != null)
                    {
                        var panel = (IPanel)_panelIteratorValuePropertyInfo.GetValue(iterator.Current);
                        if (panel != null && (!contextType.HasValue || panel.contextType == contextType.Value))
                        {
                            _panels.Add(panel);
                        }
                    }
                }
            }
            return _panels;
        }

        public static IPanel GetSceneHierarchyWindow()
        {
            return GetAllPanels().Where(p => p.visualTree.Q(SceneHiearchyWindowName) != null).FirstOrDefault();
        }

        private static IEnumerator GetPanelsIterator()
        {
            if (_getPanelsInteratorMethodInfo == null && _getPanelsIteratorMethodInfoRequested)
            {
                return null;
            }

            if (_getPanelsInteratorMethodInfo == null && !_getPanelsIteratorMethodInfoRequested)
            {
                _getPanelsIteratorMethodInfoRequested = true;

                var type = GetUtilityType();
                if (type == null)
                    return null;

                _getPanelsInteratorMethodInfo = type.GetMethod("GetPanelsIterator", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            }

            if (_getPanelsInteratorMethodInfo != null && _panelType != null)
            {
                var enumerator = (IEnumerator)_getPanelsInteratorMethodInfo.Invoke(null, null);
                return enumerator;
            }

            return null;
        }

        public static bool IsUIBuilderPanel(IPanel panel)
        {
            // We assume it's the builder if the builder-viewport class is present.
            var builderViewport = panel.visualTree.Q(className: "unity-builder-viewport");
            return builderViewport != null && panel.contextType == ContextType.Editor;
        }

        public static bool IsGameViewPanel(IPanel panel)
        {
            return panel.contextType == ContextType.Player;
        }
    }
}
