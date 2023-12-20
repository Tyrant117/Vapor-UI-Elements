using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VaporUIElements;

namespace VaporUIElementsEditor
{
    public abstract class BaseVaporInspector : Editor
    {
        public static readonly Func<FieldInfo, bool> FieldSearchPredicate = f => !f.IsDefined(typeof(HideInInspector))
                                                                                 && !f.IsDefined(typeof(NonSerializedAttribute))
                                                                                 && (f.IsPublic || f.IsDefined(typeof(SerializeField)));
        public static readonly Func<MethodInfo, bool> MethodSearchPredicate = f => f.IsDefined(typeof(ButtonAttribute));
        public static readonly Func<PropertyInfo, bool> PropertySearchPredicate = f => f.IsDefined(typeof(ShowInInspectorAttribute));

        protected static readonly Dictionary<string, VaporGroupNode> NodeBag = new();

        protected readonly List<VaporDrawerInfo> SerializedDrawerInfo = new();
        // protected readonly Dictionary<string, VaporDrawerInfo> PropertyPathToDrawerMap = new();
        protected VaporGroupNode RootNode;
        protected string UnmanagedGroupName;

        public override VisualElement CreateInspectorGUI()
        {
            var drawWithVapor = target.GetType().IsDefined(typeof(DrawWithVaporAttribute));
            var inspector = new VisualElement();
            inspector.Add(DrawScript());
            if (drawWithVapor)
            {
                GetSerializedDrawerInfo();
                DrawVaporInspector(inspector);
            }
            else
            {
                DrawUIElementsDefaultInspector(inspector);
            }

            return inspector;
        }

        protected VisualElement DrawScript()
        {
            var script = new PropertyField(serializedObject.FindProperty("m_Script"));
            var hide = target.GetType().IsDefined(typeof(HideMonoScriptAttribute));
            script.style.display = hide ? DisplayStyle.None : DisplayStyle.Flex;
            script.SetEnabled(false);
            return script;
        }

        #region - Default Inspector -
        protected virtual void DrawUIElementsDefaultInspector(VisualElement container)
        {
            foreach (var element in DrawUIElementsPropertiesExcluding())
            {
                container.Add(element);
            }
        }

        protected List<VisualElement> DrawUIElementsPropertiesExcluding(params string[] propertyToExclude)
        {
            List<VisualElement> result = new();
            var iterator = serializedObject.GetIterator();
            var enterChildren = true;
            var clearedScript = false;
            var excludeProperties = propertyToExclude.Length > 0;
            
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (!clearedScript && iterator.name.Contains("m_Script"))
                {
                    clearedScript = true;
                    continue;
                }

                if (excludeProperties && propertyToExclude.Contains(iterator.name))
                {
                    continue;
                }

                result.Add(new PropertyField(iterator));
            }
            return result;
        }
        #endregion

        #region - Vapor Inspector -
        protected void DrawVaporInspector(VisualElement container)
        {
            NodeBag.Clear();
            UnmanagedGroupName = $"{target.GetType().Name}-Unmanaged";
            RootNode = new VaporGroupNode(null, null, container);

            BuildGroups();
            BuildNodeGraph();
        }

        protected void BuildGroups()
        {
            var unmanagedNode = _DrawUnmanagedGroupNode(RootNode);

            foreach (var drawer in SerializedDrawerInfo)
            {
                if (!drawer.IsUnmanagedGroup)
                {
                    var node = RootNode;
                    foreach (var attribute in drawer.Groups)
                    {
                        switch (attribute.Type)
                        {
                            case UIGroupType.Horizontal:
                                if (attribute is HorizontalGroupAttribute horizontalAttribute)
                                {
                                    node = _DrawHorizontalGroupNode(horizontalAttribute, node);
                                }
                                break;
                            case UIGroupType.Vertical:
                                if (attribute is VerticalGroupAttribute verticalAttribute)
                                {
                                    node = _DrawVerticalGroupNode(verticalAttribute, node);
                                }
                                break;
                            case UIGroupType.Foldout:
                                if (attribute is FoldoutGroupAttribute foldoutAttribute)
                                {
                                    node = _DrawFoldoutGroupNode(foldoutAttribute, node);
                                }
                                break;
                            case UIGroupType.Box:
                                if (attribute is BoxGroupAttribute boxAttribute)
                                {
                                    node = _DrawBoxGroupNode(boxAttribute, node);
                                }
                                break;
                            case UIGroupType.Tab:
                                if (attribute is TabGroupAttribute tabAttribute)
                                {
                                    node = _DrawTabGroupNode(tabAttribute, node);
                                }
                                break;
                            case UIGroupType.Title:
                                if (attribute is TitleGroupAttribute titleAttribute)
                                {
                                    node = _DrawTitleGroupNode(titleAttribute, node);
                                }
                                break;
                        }
                    }
                    if (drawer.IsDrawnWithVapor)
                    {
                        drawer.BuildGroups(/*this,*/ "", node, NodeBag);
                    }
                    else
                    {
                        node.AddContent(/*this,*/ drawer);
                    }
                }
                else
                {
                    if (drawer.IsDrawnWithVapor)
                    {
                        drawer.BuildGroups(/*this,*/ UnmanagedGroupName, unmanagedNode, NodeBag);
                    }
                    else
                    {
                        unmanagedNode.AddContent(/*this,*/ drawer);
                    }
                }
            }

            VaporGroupNode _DrawUnmanagedGroupNode(VaporGroupNode parentNode)
            {
                if (!NodeBag.TryGetValue(UnmanagedGroupName, out var node))
                {
                    var atr = target.GetType().GetCustomAttribute<UnManagedGroupAttribute>() ?? new UnManagedGroupAttribute();
                    VisualElement ve = atr.UnmanagedGroupType switch
                    {
                        UIGroupType.Horizontal => new StyledHorizontalGroup()
                        {
                            name = UnmanagedGroupName
                        },
                        UIGroupType.Vertical => new StyledVerticalGroup()
                        {
                            name = UnmanagedGroupName
                        },
                        UIGroupType.Foldout => new StyledFoldout(atr.UnmanagedGroupHeader)
                        {
                            name = UnmanagedGroupName
                        },
                        UIGroupType.Box => new StyledHeaderBox(atr.UnmanagedGroupHeader)
                        {
                            name = UnmanagedGroupName
                        },
                        UIGroupType.Tab => new StyledTabGroup()
                        {
                            name = UnmanagedGroupName
                        },
                        UIGroupType.Title => new StyledTitleGroup(new TitleGroupAttribute(UnmanagedGroupName, atr.UnmanagedGroupHeader))
                        {
                            name = UnmanagedGroupName
                        },
                        _ => new StyledVerticalGroup()
                        {
                            name = UnmanagedGroupName
                        },
                    };
                    parentNode.AddChild(atr.UnmanagedGroupType, UnmanagedGroupName, atr.UnmanagedGroupOrder, ve);
                    var added = parentNode.Children[^1];
                    NodeBag.Add(UnmanagedGroupName, added);
                    return added;
                }
                else
                {
                    return node;
                }
            }

            VaporGroupNode _DrawHorizontalGroupNode(HorizontalGroupAttribute attribute, VaporGroupNode parentNode)
            {
                if (!NodeBag.TryGetValue(attribute.GroupName, out var node))
                {
                    var horizontal = new StyledHorizontalGroup
                    {
                        name = attribute.GroupName
                    };
                    parentNode.AddChild(attribute, horizontal);
                    var added = parentNode.Children[^1];
                    NodeBag.Add(attribute.GroupName, added);
                    return added;
                }
                else
                {
                    return node;
                }
            }

            VaporGroupNode _DrawVerticalGroupNode(VerticalGroupAttribute attribute, VaporGroupNode parentNode)
            {
                if (!NodeBag.TryGetValue(attribute.GroupName, out var node))
                {
                    var vertical = new StyledVerticalGroup
                    {
                        name = attribute.GroupName
                    };
                    parentNode.AddChild(attribute, vertical);
                    var added = parentNode.Children[^1];
                    NodeBag.Add(attribute.GroupName, added);
                    return added;
                }
                else
                {
                    return node;
                }
            }

            VaporGroupNode _DrawFoldoutGroupNode(FoldoutGroupAttribute attribute, VaporGroupNode parentNode)
            {
                if (!NodeBag.TryGetValue(attribute.GroupName, out var node))
                {
                    var foldout = new StyledFoldout(attribute.Header)
                    {
                        name = attribute.GroupName
                    };
                    parentNode.AddChild(attribute, foldout);
                    var added = parentNode.Children[^1];
                    NodeBag.Add(attribute.GroupName, added);
                    return added;
                }
                else
                {
                    return node;
                }
            }

            VaporGroupNode _DrawBoxGroupNode(BoxGroupAttribute attribute, VaporGroupNode parentNode)
            {
                if (!NodeBag.TryGetValue(attribute.GroupName, out var node))
                {
                    var box = new StyledHeaderBox(attribute.Header)
                    {
                        name = attribute.GroupName
                    };
                    parentNode.AddChild(attribute, box);
                    var added = parentNode.Children[^1];
                    NodeBag.Add(attribute.GroupName, added);
                    return added;
                }
                else
                {
                    return node;
                }
            }

            VaporGroupNode _DrawTabGroupNode(TabGroupAttribute attribute, VaporGroupNode parentNode)
            {
                if (!NodeBag.TryGetValue(attribute.GroupName, out var node))
                {
                    var tabs = new StyledTabGroup()
                    {
                        name = attribute.GroupName
                    };
                    parentNode.AddChild(attribute, tabs);
                    var added = parentNode.Children[^1];
                    NodeBag.Add(attribute.GroupName, added);
                    return added;
                }
                else
                {
                    return node;
                }
            }

            VaporGroupNode _DrawTitleGroupNode(TitleGroupAttribute attribute, VaporGroupNode parentNode)
            {
                if (!NodeBag.TryGetValue(attribute.GroupName, out var node))
                {
                    var title = new StyledTitleGroup(attribute)
                    {
                        name = attribute.GroupName
                    };
                    parentNode.AddChild(attribute, title);
                    var added = parentNode.Children[^1];
                    NodeBag.Add(attribute.GroupName, added);
                    return added;
                }
                else
                {
                    return node;
                }
            }
        }
        protected void BuildNodeGraph()
        {
            foreach (var childNode in RootNode.Children.OrderBy(n => n.GroupOrder))
            {
                if (childNode.ShouldDraw)
                {
                    childNode.BuildContent();
                    RootNode.Container.Add(childNode.Container);
                    _TraverseNodes(childNode);
                }
            }

            static void _TraverseNodes(VaporGroupNode parentNode)
            {
                foreach (var childNode in parentNode.Children.OrderBy(n => n.GroupOrder))
                {
                    if (!childNode.ShouldDraw) continue;
                    if (parentNode.GroupType == UIGroupType.Horizontal)
                    {
                        childNode.Container.style.flexGrow = 1;
                    }
                    childNode.BuildContent();
                    parentNode.Container.Add(childNode.Container);
                    _TraverseNodes(childNode);
                }
            }
        }

        public VisualElement DrawVaporProperty(VaporDrawerInfo drawer, string drawerName)
        {
            switch (drawer.InfoType)
            {
                case DrawerInfoType.Field:
                    if (drawer.TryGetAttribute<ValueDropdownAttribute>(out var dropdownAtr))
                    {
                        var container = new VisualElement()
                        {
                            name = drawerName,
                            userData = drawer
                        };
                        List<string> keys = new();
                        List<object> values = new();
                        switch (dropdownAtr.ResolverType)
                        {
                            case ResolverType.None:
                                break;
                            case ResolverType.Property:
                                PropertyInfo pi = ReflectionUtility.GetProperty(drawer.Target, dropdownAtr.Resolver[1..]);
                                _ConvertToTupleList(keys, values, (IList)pi.GetValue(drawer.Target));
                                break;
                            case ResolverType.Method:
                                MethodInfo mi = ReflectionUtility.GetMethod(drawer.Target, dropdownAtr.Resolver[1..]);
                                _ConvertToTupleList(keys, values, (IList)mi.Invoke(drawer.Target, null));
                                break;
                        }
                        int index = 0;
                        foreach (var value in values)
                        {
                            if (value.Equals(drawer.Property.boxedValue))
                            {
                                break;
                            }
                            index++;
                        }

                        var field = new DropdownField(drawer.Property.displayName, keys, index)
                        {
                            userData = values
                        };
                        field.AddToClassList("unity-base-field__aligned");
                        field.RegisterValueChangedCallback(OnDropdownChanged);
                        container.Add(field);
                        return container;
                    }
                    else
                    {
                        if (drawer.Property.isArray && !drawer.HasAttribute<DrawWithUnityAttribute>())
                        {
                            var list = new StyledList(drawer)
                            {
                                name = drawerName,
                                userData = drawer
                            };
                            return list;
                        }
                        else
                        {
                            var field = new PropertyField(drawer.Property)
                            {
                                name = drawerName,
                                userData = drawer,
                            };
                            field.RegisterCallback<GeometryChangedEvent>(OnPropertyBuilt);
                            return field;
                        }
                    }
                case DrawerInfoType.Property:
                    object clonedTarget = drawer.Target;
                    bool cleanupImmediate = false;
                    if (drawer.Target.GetType().IsSubclassOf(typeof(Component)))
                    {
                        clonedTarget = Component.Instantiate((Component)drawer.Target);
                        cleanupImmediate = true;
                    }
                    else
                    {
                        clonedTarget = Activator.CreateInstance(clonedTarget.GetType());
                    }
                    var val = drawer.PropertyInfo.GetValue(clonedTarget).ToString();
                    if (drawer.FieldInfo != null)
                    {
                        val = drawer.FieldInfo.GetValue(clonedTarget).ToString();
                    }
                    var prop = new TextField(drawer.Path[(drawer.Path.IndexOf("p_", StringComparison.Ordinal) + 2)..])
                    {
                        name = drawerName,
                    };
                    prop.SetValueWithoutNotify(val);
                    prop.SetEnabled(false);
                    if (cleanupImmediate)
                    {
                        var obj = (Component)clonedTarget;
                        Component.DestroyImmediate(obj.gameObject);
                    }
                    if (drawer.TryGetAttribute<ShowInInspectorAttribute>(out var showAtr))
                    {
                        if (showAtr.Dynamic)
                        {
                            prop.schedule.Execute(() => OnDynamicPropertyShow(drawer, prop)).Every(showAtr.DynamicInterval);
                        }
                    }
                    return prop;
                case DrawerInfoType.Method:
                    var atr = drawer.MethodInfo.GetCustomAttribute<ButtonAttribute>();
                    var label = atr.Label;
                    if (string.IsNullOrEmpty(label))
                    {
                        label = drawer.MethodInfo.Name;
                    }
                    var button = new StyledButton(() =>
                    {
                        drawer.MethodInfo.Invoke(drawer.Target, null);
                    }
                    , atr.Size)
                    {
                        text = label
                    };
                    return button;
                default:
                    return null;
            }

            static void _ConvertToTupleList(List<string> keys, List<object> values, IList convert)
            {
                foreach (var obj in convert)
                {
                    var item1 = (string)obj.GetType().GetField("Item1", BindingFlags.Instance | BindingFlags.Public)
                        ?.GetValue(obj);
                    var item2 = obj.GetType().GetField("Item2", BindingFlags.Instance | BindingFlags.Public)
                        ?.GetValue(obj);
                    if (item1 == null || item2 == null)
                    {
                        continue;
                    }

                    keys.Add(item1);
                    values.Add(item2);
                }
            }
        }

        private void OnDropdownChanged(ChangeEvent<string> evt)
        {
            if (evt.target is DropdownField dropdown && dropdown.parent.userData is VaporDrawerInfo drawer && dropdown.userData is List<object> values)
            {
                var newVal = values[dropdown.index];
                drawer.Property.boxedValue = newVal;
                drawer.Property.serializedObject.ApplyModifiedProperties();
            }
        }

        public VisualElement DrawVaporPropertyWithVerticalLayout(VaporDrawerInfo drawer, string drawerName)
        {
            var vertical = new StyledVerticalGroup(0, 0, true);
            var field = DrawVaporProperty(drawer, drawerName);
            vertical.Add(field);
            return vertical;
        }

        private void OnPropertyBuilt(GeometryChangedEvent evt)
        {
            var field = (PropertyField)evt.target;
            if (field != null && field.childCount > 0)
            {
                field.UnregisterCallback<GeometryChangedEvent>(OnPropertyBuilt);
                OnPropertyBuilt(field);
            }
        }

        private void OnPropertyBuilt(PropertyField field)
        {
            var list = field.Q<ListView>();
            bool isList = list != null;
            bool isGrouped = !field.name.Contains("non-grouped");
            if (isList && isGrouped)
            {
                list.Q<Toggle>().style.marginLeft = 3;
            }

            List<Action> resolvers = new();

            if (field.userData is not VaporDrawerInfo drawer)
            {
                return;
            }

            var prop = drawer.Property; // serializedObject.FindProperty(field.bindingPath);
            // var drawer = PropertyPathToDrawerMap[field.bindingPath];
            if (prop.propertyType == SerializedPropertyType.Generic && !drawer.IsDrawnWithVapor)
            {
                if (drawer.HasAttribute<InlineEditorAttribute>())
                {
                    field.Q<Toggle>().RemoveFromHierarchy();
                    var inlineContent = field.Q<VisualElement>("unity-content");
                    inlineContent.style.display = DisplayStyle.Flex;
                    inlineContent.style.marginLeft = 0;
                }
                else
                {
                    field.Q<Toggle>().style.marginLeft = 0;
                }
            }

            DrawerUtility.DrawDecorators(field, drawer);
            DrawerUtility.DrawLabel(field, drawer, resolvers);
            DrawerUtility.DrawLabelWidth(field, drawer);
            DrawerUtility.DrawHideLabel(field, drawer);
            DrawerUtility.DrawConditionals(field, drawer, resolvers);
            DrawerUtility.DrawReadOnly(field, drawer);
            DrawerUtility.DrawAutoReference(field, drawer);
            DrawerUtility.DrawTitle(field, drawer);
            DrawerUtility.DrawPathSelection(field, drawer);
            DrawerUtility.DrawInlineButtons(field, drawer, resolvers);
            DrawerUtility.DrawSuffix(field, drawer);

            // Validation
            DrawerUtility.DrawValidation(field, drawer, resolvers);

            if (resolvers.Count > 0)
            {
                field.schedule.Execute(() => Resolve(resolvers)).Every(1000);
            }
        }

        private static void Resolve(List<Action> resolvers)
        {
            foreach (var item in resolvers)
            {
                item.Invoke();
            }
        }

        private void OnDynamicPropertyShow(VaporDrawerInfo drawer, TextField field)
        {
            object clonedTarget = drawer.Target;
            bool cleanupImmediate = false;
            if (drawer.Target.GetType().IsSubclassOf(typeof(Component)))
            {
                clonedTarget = Component.Instantiate((Component)drawer.Target);
                cleanupImmediate = true;
            }
            else
            {
                clonedTarget = Activator.CreateInstance(clonedTarget.GetType());
            }
            var val = drawer.PropertyInfo.GetValue(clonedTarget).ToString();
            if (drawer.FieldInfo != null)
            {
                val = drawer.FieldInfo.GetValue(clonedTarget).ToString();
            }
            field.SetValueWithoutNotify(val);
            if (cleanupImmediate)
            {
                var obj = (Component)clonedTarget;
                Component.DestroyImmediate(obj.gameObject);
            }
        }
        #endregion        

        #region - Helpers -
        protected void GetSerializedDrawerInfo()
        {
            SerializedDrawerInfo.Clear();
            // PropertyPathToDrawerMap.Clear();
            List<FieldInfo> fieldInfo = new();
            List<PropertyInfo> propertyInfo = new();
            List<MethodInfo> methodInfo = new();
            var targetType = target.GetType();
            Stack<Type> typeStack = new();
            while (targetType != null)
            {
                typeStack.Push(targetType);
                targetType = targetType.BaseType;
            }

            while (typeStack.TryPop(out var type))
            {
                fieldInfo.AddRange(type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
                propertyInfo.AddRange(type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
                methodInfo.AddRange(type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
            }

            foreach (var field in fieldInfo.Where(FieldSearchPredicate))
            {
                var property = serializedObject.FindProperty(field.Name);
                if (property == null)
                {
                    continue;
                }

                var info = new VaporDrawerInfo(property.propertyPath, field, property, target, null);
                // var info = new VaporDrawerInfo(property.propertyPath, field, property, target, null, PropertyPathToDrawerMap);
                SerializedDrawerInfo.Add(info);
                // PropertyPathToDrawerMap.Add(property.propertyPath, info);
            }

            foreach (var property in propertyInfo.Where(PropertySearchPredicate))
            {
                var info = new VaporDrawerInfo($"{target.name}_p_{property.Name}", property, target, null);
                SerializedDrawerInfo.Add(info);
            }
            foreach (var method in methodInfo.Where(MethodSearchPredicate))
            {
                var info = new VaporDrawerInfo(method.Name, method, target, null);
                SerializedDrawerInfo.Add(info);
            }

            // foreach (var sf in _serializedDrawerInfo)
            // {
            //     //Debug.Log($"{sf.FieldInfo.Name} - {sf.FieldInfo.FieldType} - {sf.Path}");
            // }
        }
        #endregion
    }
}
