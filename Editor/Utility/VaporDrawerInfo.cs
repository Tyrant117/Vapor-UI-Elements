using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VaporUIElements;
using Object = UnityEngine.Object;

namespace VaporUIElementsEditor
{
    public enum DrawerInfoType
    {
        Field,
        Property,
        Method
    }

    public class VaporDrawerInfo
    {
        private static Func<VaporGroupAttribute, int> ShortestToLongestName => group => group.GroupName.Length;

        public string Path { get; }
        public DrawerInfoType InfoType { get; }
        public FieldInfo FieldInfo { get; }
        public MethodInfo MethodInfo { get; }
        public PropertyInfo PropertyInfo { get; }
        public SerializedProperty Property { get; }
        public object Target { get; }

        public VaporDrawerInfo Parent { get; }
        public bool HasParent { get; }
        public List<VaporDrawerInfo> Children { get; }
        public bool HasChildren { get; }
        public bool HasUnmanagedChildren { get; }
        public bool IsDrawnWithVapor { get; }

        public List<VaporGroupAttribute> Groups { get; } = new();
        public VaporGroupAttribute ContainingGroup { get; }
        public bool IsUnmanagedGroup { get; }

        public string UpdatedGroupName { get; set; }
        public int UpdatedOrder { get; }
        public bool HasUpdatedOrder { get; }

        private readonly DrawWithVaporAttribute _drawWithVaporAttribute;
        private readonly UnManagedGroupAttribute _unmanagedGroupAttribute;

        public VaporDrawerInfo(string path, FieldInfo fieldInfo, SerializedProperty property, object target, VaporDrawerInfo parentDrawer, Dictionary<string, VaporDrawerInfo> pathToDrawerMap)
        {
            Path = path;
            FieldInfo = fieldInfo;
            InfoType = DrawerInfoType.Field;
            Property = property;
            Parent = parentDrawer;
            HasParent = parentDrawer != null;
            IsDrawnWithVapor = FieldInfo.FieldType.IsDefined(typeof(DrawWithVaporAttribute)) && !FieldInfo.FieldType.IsSubclassOf(typeof(Object));
            Target = target; 

            if (TryGetAttributes<VaporGroupAttribute>(out var attributes))
            {
                if (attributes.Length > 1)
                {
                    Groups = attributes.OrderBy(ShortestToLongestName).ToList();
                }
                else
                {
                    Groups.Add(attributes[0]);
                }
                ContainingGroup = Groups[^1];
            }
            IsUnmanagedGroup = ContainingGroup == null || string.IsNullOrEmpty(ContainingGroup.GroupName);
            if (TryGetAttribute<PropertyOrderAttribute>(out var propOrder))
            {
                UpdatedOrder = propOrder.Order;
                HasUpdatedOrder = true;
            }

            if (IsDrawnWithVapor)
            {
                Children = new List<VaporDrawerInfo>();
                HasChildren = true;
                _drawWithVaporAttribute = FieldInfo.FieldType.GetCustomAttribute<DrawWithVaporAttribute>();
                _unmanagedGroupAttribute = FieldInfo.FieldType.GetCustomAttribute<UnManagedGroupAttribute>() ?? new UnManagedGroupAttribute();
                var targetType = FieldInfo.FieldType;
                var subTarget = FieldInfo.GetValue(Target);
                //Debug.Log($"{Path} - MyTarget: {subTarget}");
                Stack<Type> typeStack = new();
                while (targetType != null)
                {
                    typeStack.Push(targetType);
                    targetType = targetType.BaseType;
                }
                List<FieldInfo> fieldInfoList = new();
                List<PropertyInfo> propertyInfoList = new();
                List<MethodInfo> methodInfoList = new();
                while (typeStack.TryPop(out var type))
                {
                    fieldInfoList.AddRange(type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
                    propertyInfoList.AddRange(type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
                    methodInfoList.AddRange(type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
                }

                foreach (var field in fieldInfoList.Where(BaseVaporInspector.FieldSearchPredicate))
                {
                    SerializedProperty relativeProperty = Property.FindPropertyRelative(field.Name);
                    var relTarget = Property.boxedValue;// SerializedPropertyUtility.GetTargetObjectWithProperty(relativeProperty);
                    var info = new VaporDrawerInfo(relativeProperty.propertyPath, field, relativeProperty, relTarget, this, pathToDrawerMap);
                    if (info.IsUnmanagedGroup)
                    {
                        HasUnmanagedChildren = true;
                    }
                    Children.Add(info);
                    pathToDrawerMap.Add(relativeProperty.propertyPath, info);
                }
                
                foreach (var childProperty in propertyInfoList.Where(BaseVaporInspector.PropertySearchPredicate))
                {
                    var info = new VaporDrawerInfo($"{Property.propertyPath}_p_{childProperty.Name}", childProperty, subTarget, this, pathToDrawerMap);
                    if (info.IsUnmanagedGroup)
                    {
                        HasUnmanagedChildren = true;
                    }
                    Children.Add(info);
                }
                foreach (var method in methodInfoList.Where(BaseVaporInspector.MethodSearchPredicate))
                {                    
                    var info = new VaporDrawerInfo($"{Property.propertyPath}_m_{method.Name}", method, subTarget, this, pathToDrawerMap);
                    if (info.IsUnmanagedGroup)
                    {
                        HasUnmanagedChildren = true;
                    }
                    Children.Add(info);
                }
            }
        }

        public VaporDrawerInfo(string path, PropertyInfo propertyInfo, object target, VaporDrawerInfo parentDrawer, Dictionary<string, VaporDrawerInfo> pathToDrawerMap)
        {
            Path = path;
            PropertyInfo = propertyInfo;
            Target = target;

            //if (Target.GetType().IsSubclassOf(typeof(Component)))
            //{
            //    var type = Target.GetType();
            //    while (FieldInfo == null)
            //    {
            //        FieldInfo = type.GetField($"<{PropertyInfo.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            //        type = type.BaseType;
            //    }
            //}

            InfoType = DrawerInfoType.Property;
            Parent = parentDrawer;
            HasParent = parentDrawer != null;

            if (TryGetAttributes<VaporGroupAttribute>(out var attributes))
            {
                if (attributes.Length > 1)
                {
                    Groups = attributes.OrderBy(ShortestToLongestName).ToList();
                }
                else
                {
                    Groups.Add(attributes[0]);
                }
                ContainingGroup = Groups[^1];
            }
            IsUnmanagedGroup = ContainingGroup == null || string.IsNullOrEmpty(ContainingGroup.GroupName);
            if (!TryGetAttribute<PropertyOrderAttribute>(out var propOrder)) return;
            
            UpdatedOrder = propOrder.Order;
            HasUpdatedOrder = true;
        }

        public VaporDrawerInfo(string path, MethodInfo methodInfo, object target, VaporDrawerInfo parentDrawer, Dictionary<string, VaporDrawerInfo> pathToDrawerMap)
        {
            Path = path;
            MethodInfo = methodInfo;
            InfoType = DrawerInfoType.Method;
            Target = target;
            Parent = parentDrawer;
            HasParent = parentDrawer != null;

            if (TryGetAttributes<VaporGroupAttribute>(out var attributes))
            {
                if (attributes.Length > 1)
                {
                    Groups = attributes.OrderBy(ShortestToLongestName).ToList();
                }
                else
                {
                    Groups.Add(attributes[0]);
                }
                ContainingGroup = Groups[^1];
            }
            IsUnmanagedGroup = ContainingGroup == null || string.IsNullOrEmpty(ContainingGroup.GroupName);
            if (!TryGetAttribute<PropertyOrderAttribute>(out var propOrder)) return;
            
            UpdatedOrder = propOrder.Order;
            HasUpdatedOrder = true;
        }



        #region - Drawing -
        public void BuildGroups(BaseVaporInspector inspector, string rootName, VaporGroupNode node, Dictionary<string, VaporGroupNode> nodeBag)
        {
            //Debug.Log($"Draw Groups of {Path} with RootName: {rootName}");
            if (IsUnmanagedGroup)
            {
                UpdatedGroupName = string.IsNullOrEmpty(rootName) ? $"_{FieldInfo.Name}" : $"{rootName}/_{FieldInfo.Name}";
                //node = _DrawVerticalGroup(node, UpdatedGroupName, HasUpdatedOrder ? UpdatedOrder : int.MaxValue);
                node = _DrawInlineGroup(node, ObjectNames.NicifyVariableName(FieldInfo.Name), UpdatedGroupName, HasUpdatedOrder ? UpdatedOrder : int.MaxValue);
            }
            else
            {
                string parentName = string.IsNullOrEmpty(rootName) ? $"{ContainingGroup.GroupName}" : $"{rootName}/{ContainingGroup.GroupName}";
                UpdatedGroupName = string.IsNullOrEmpty(rootName) ? $"{ContainingGroup.GroupName}/_{FieldInfo.Name}" : $"{rootName}/{ContainingGroup.GroupName}/_{FieldInfo.Name}";
                //node = _DrawVerticalGroup(node, UpdatedGroupName, HasUpdatedOrder ? UpdatedOrder : ContainingGroup.Order);
                node = _DrawInlineGroup(node, ObjectNames.NicifyVariableName(FieldInfo.Name), UpdatedGroupName, HasUpdatedOrder ? UpdatedOrder : ContainingGroup.Order);
            }
            VaporGroupNode rootNode = node;
            VaporGroupNode unmanagedNode = null;
            if (HasUnmanagedChildren)
            {
                unmanagedNode = _DrawUnmanagedGroup(rootNode, $"{UpdatedGroupName}-Unmanaged");
            }
            foreach (var child in Children)
            {
                if (!child.IsUnmanagedGroup)
                {
                    child.UpdatedGroupName = $"{UpdatedGroupName}/{child.ContainingGroup.GroupName}";
                    foreach (var attribute in child.Groups)
                    {
                        string parentName = attribute.ParentName != string.Empty ? $"{UpdatedGroupName}/{attribute.ParentName}" : UpdatedGroupName;
                        string groupName = $"{UpdatedGroupName}/{attribute.GroupName}";
                        switch (attribute.Type)
                        {
                            case UIGroupType.Horizontal:
                                if (attribute is HorizontalGroupAttribute horizontalAttribute)
                                {
                                    node = _DrawHorizontalGroup(node, groupName, child.HasUpdatedOrder ? child.UpdatedOrder : attribute.Order);
                                }
                                break;
                            case UIGroupType.Vertical:
                                if (attribute is VerticalGroupAttribute verticalAttribute)
                                {
                                    node = _DrawVerticalGroup(node, groupName, child.HasUpdatedOrder ? child.UpdatedOrder : attribute.Order);
                                }
                                break;
                            case UIGroupType.Foldout:
                                if (attribute is FoldoutGroupAttribute foldoutAttribute)
                                {
                                    node = _DrawFoldoutGroup(node, groupName, foldoutAttribute.Header, child.HasUpdatedOrder ? child.UpdatedOrder : attribute.Order);
                                }
                                break;
                            case UIGroupType.Box:
                                if (attribute is BoxGroupAttribute boxAttribute)
                                {
                                    node = _DrawBoxGroup(node, groupName, boxAttribute.Header, child.HasUpdatedOrder ? child.UpdatedOrder : attribute.Order);
                                }
                                break;
                            case UIGroupType.Tab:
                                if (attribute is TabGroupAttribute tabAttribute)
                                {
                                    node = _DrawTabGroup(node, groupName, child.HasUpdatedOrder ? child.UpdatedOrder : attribute.Order);
                                }
                                break;
                            case UIGroupType.Title:
                                if (attribute is TitleGroupAttribute titleAttribute)
                                {
                                    node = _DrawTitleGroup(node, groupName, titleAttribute, child.HasUpdatedOrder ? child.UpdatedOrder : attribute.Order);
                                }
                                break;
                        }
                    }
                    if (child.IsDrawnWithVapor)
                    {
                        child.BuildGroups(inspector, UpdatedGroupName, node, nodeBag);
                    }
                    else
                    {
                        node.AddContent(inspector, child);
                    }
                }
                else
                {
                    child.UpdatedGroupName = UpdatedGroupName;
                    if (child.IsDrawnWithVapor)
                    {
                        child.BuildGroups(inspector, UpdatedGroupName, rootNode, nodeBag);
                    }
                    else
                    {
                        unmanagedNode.AddContent(inspector, child);
                    }
                }
            }

            VaporGroupNode _DrawUnmanagedGroup(VaporGroupNode parentNode, string groupName)
            {
                if (!nodeBag.TryGetValue(groupName, out var node))
                {
                    var order = _unmanagedGroupAttribute.UnmanagedGroupOrder;
                    VisualElement ve = _unmanagedGroupAttribute.UnmanagedGroupType switch
                    {
                        UIGroupType.Horizontal => new StyledHorizontalGroup()
                        {
                            name = groupName
                        },
                        UIGroupType.Vertical => new StyledVerticalGroup()
                        {
                            name = groupName
                        },
                        UIGroupType.Foldout => new StyledFoldout(_unmanagedGroupAttribute.UnmanagedGroupHeader)
                        {
                            name = groupName
                        },
                        UIGroupType.Box => new StyledHeaderBox(_unmanagedGroupAttribute.UnmanagedGroupHeader)
                        {
                            name = groupName
                        },
                        UIGroupType.Tab => new StyledTabGroup()
                        {
                            name = groupName
                        },
                        UIGroupType.Title => new StyledTitleGroup(new TitleGroupAttribute(groupName, _unmanagedGroupAttribute.UnmanagedGroupHeader))
                        {
                            name = groupName
                        },
                        _ => new StyledVerticalGroup()
                        {
                            name = groupName
                        },
                    };

                    //var vertical = new StyledVerticalGroup
                    //{
                    //    name = groupName
                    //};
                    parentNode.AddChild(_unmanagedGroupAttribute.UnmanagedGroupType, groupName, order, ve);
                    var added = parentNode.Children[^1];
                    nodeBag.Add(groupName, added);
                    return added;
                }
                else
                {
                    return node;
                }
            }

            VaporGroupNode _DrawHorizontalGroup(VaporGroupNode parentNode, string groupName, int order)
            {
                if (!nodeBag.TryGetValue(groupName, out var node))
                {
                    var horizontal = new StyledHorizontalGroup
                    {
                        name = groupName
                    };
                    parentNode.AddChild(UIGroupType.Horizontal, groupName, order, horizontal);
                    var added = parentNode.Children[^1];
                    nodeBag.Add(groupName, added);
                    return added;
                }
                else
                {
                    return node;
                }
            }

            VaporGroupNode _DrawVerticalGroup(VaporGroupNode parentNode, string groupName, int order)
            {
                if (!nodeBag.TryGetValue(groupName, out var node))
                {
                    var vertical = new StyledVerticalGroup
                    {
                        name = groupName
                    };
                    parentNode.AddChild(UIGroupType.Vertical, groupName, order, vertical);
                    var added = parentNode.Children[^1];
                    nodeBag.Add(groupName, added);
                    return added;
                }
                else
                {
                    return node;
                }
            }

            VaporGroupNode _DrawFoldoutGroup(VaporGroupNode parentNode, string groupName, string header, int order)
            {
                if (!nodeBag.TryGetValue(groupName, out var node))
                {
                    var foldout = new StyledFoldout(header)
                    {
                        name = groupName
                    };
                    parentNode.AddChild(UIGroupType.Foldout, groupName, order, foldout);
                    var added = parentNode.Children[^1];
                    nodeBag.Add(groupName, added);
                    return added;
                }
                else
                {
                    return node;
                }
            }

            VaporGroupNode _DrawBoxGroup(VaporGroupNode parentNode, string groupName, string header, int order)
            {
                if (!nodeBag.TryGetValue(groupName, out var node))
                {
                    var box = new StyledHeaderBox(header)
                    {
                        name = groupName
                    };
                    parentNode.AddChild(UIGroupType.Box, groupName, order, box);
                    var added = parentNode.Children[^1];
                    nodeBag.Add(groupName, added);
                    return added;
                }
                else
                {
                    return node;
                }
            }

            VaporGroupNode _DrawTabGroup(VaporGroupNode parentNode, string groupName, int order)
            {
                if (!nodeBag.TryGetValue(groupName, out var node))
                {
                    var tabs = new StyledTabGroup()
                    {
                        name = groupName
                    };
                    parentNode.AddChild(UIGroupType.Tab, groupName, order, tabs);
                    var added = parentNode.Children[^1];
                    nodeBag.Add(groupName, added);
                    return added;
                }
                else
                {
                    return node;
                }
            }

            VaporGroupNode _DrawTitleGroup(VaporGroupNode parentNode, string groupName, TitleGroupAttribute attribute, int order)
            {
                if (!nodeBag.TryGetValue(groupName, out var node))
                {
                    var title = new StyledTitleGroup(attribute)
                    {
                        name = groupName
                    };
                    parentNode.AddChild(UIGroupType.Title, groupName, order, title);
                    var added = parentNode.Children[^1];
                    nodeBag.Add(groupName, added);
                    return added;
                }
                else
                {
                    return node;
                }
            }

            VaporGroupNode _DrawInlineGroup(VaporGroupNode parentNode, string header, string groupName, int order)
            {
                return _drawWithVaporAttribute.InlinedGroupType switch
                {
                    UIGroupType.Horizontal => _DrawHorizontalGroup(parentNode, groupName, order),
                    UIGroupType.Vertical => _DrawVerticalGroup(parentNode, groupName, order),
                    UIGroupType.Foldout => _DrawFoldoutGroup(parentNode, groupName, header, order),
                    UIGroupType.Box => _DrawBoxGroup(parentNode, groupName, header, order),
                    UIGroupType.Tab => _DrawVerticalGroup(parentNode, groupName, order),
                    UIGroupType.Title => _DrawTitleGroup(parentNode, groupName, new TitleGroupAttribute(groupName, header), order),
                    _ => _DrawVerticalGroup(parentNode, groupName, order),
                };
            }
        }

        public void PopulateNodeGraph(BaseVaporInspector inspector, Dictionary<string, VaporGroupNode> nodeBag)
        {
            foreach (var child in Children)
            {
                if (!child.IsDrawnWithVapor)
                {
                    if (!child.IsUnmanagedGroup)
                    {
                        //Debug.Log($"{child.Property.propertyPath} - {child.UpdatedGroupName}");
                        if (nodeBag.TryGetValue(child.UpdatedGroupName, out var node))
                        {
                            switch (child.ContainingGroup.Type)
                            {
                                case UIGroupType.Horizontal:
                                    _PopulateHorizontalGroup(node, child);
                                    break;
                                case UIGroupType.Vertical:
                                    _PopulateVerticalGroup(node, child);
                                    break;
                                case UIGroupType.Foldout:
                                    _PopulateFoldout(node, child);
                                    break;
                                case UIGroupType.Box:
                                    _PopulateBox(node, child);
                                    break;
                                case UIGroupType.Tab:
                                    break;
                                case UIGroupType.Title:
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (nodeBag.TryGetValue($"{child.UpdatedGroupName}-Unmanaged", out var group))
                        {
                            //Debug.Log($"Populating Unamanged: {group.Parent.GroupName} - {group.GroupName}");
                            _PopulateVerticalGroup(group, child);
                        }
                    }
                }
                if (child.HasChildren)
                {
                    child.PopulateNodeGraph(inspector, nodeBag);
                }
            }

            void _PopulateHorizontalGroup(VaporGroupNode node, VaporDrawerInfo drawer)
            {
                bool isFirst = node.Container.childCount == 0;
                var drawn = inspector.DrawVaporPropertyWithVerticalLayout(drawer, "vapor-horizontal-prop");
                drawn.style.flexGrow = 1;
                drawn.style.marginLeft = isFirst ? 0 : 2;
                node.Container.Add(drawn);
            }

            void _PopulateVerticalGroup(VaporGroupNode node, VaporDrawerInfo drawer)
            {
                bool formatWithVerticalLayout = false;
                var parentNode = node.Parent;
                while (parentNode != null)
                {
                    if (parentNode.IsRootNode) { break; }

                    var parentGroup = nodeBag[parentNode.GroupName];
                    if (parentGroup.GroupType == UIGroupType.Horizontal)
                    {
                        formatWithVerticalLayout = true;
                    }
                    parentNode = parentNode.Parent;
                }

                bool isFirst = node.Container.childCount == 0;
                if (formatWithVerticalLayout)
                {
                    var drawn = inspector.DrawVaporPropertyWithVerticalLayout(drawer, "vapor-vertical-prop");
                    drawn.style.marginTop = 1;
                    node.Container.Add(drawn);
                }
                else
                {
                    var drawn = inspector.DrawVaporProperty(drawer, "vapor-vertical-prop");
                    drawn.style.marginTop = 1;
                    node.Container.Add(drawn);
                }
            }

            void _PopulateFoldout(VaporGroupNode node, VaporDrawerInfo drawer)
            {
                bool isFirst = node.Container.childCount == 0;
                var drawn = inspector.DrawVaporProperty(drawer, "vapor-foldout-prop");
                node.Container.Add(drawn);
            }

            void _PopulateBox(VaporGroupNode node, VaporDrawerInfo drawer)
            {
                bool isFirst = node.Container.childCount == 0;
                var drawn = inspector.DrawVaporProperty(drawer, "vapor-box-prop");
                node.Container.Add(drawn);
            }
        }

        public void DrawGroups(string rootName, Dictionary<string, GroupedVisualElement> groupBag, Dictionary<int, List<GroupedVisualElement>> orderBag)
        {
            Debug.Log($"Draw Groups of {Path} with RootName: {rootName}");
            if (IsDrawnWithVapor)
            {
                if (IsUnmanagedGroup)
                {
                    UpdatedGroupName = string.IsNullOrEmpty(rootName) ? $"_{FieldInfo.Name}" : $"{rootName}/_{FieldInfo.Name}";
                    _DrawVerticalGroup(rootName, UpdatedGroupName, HasUpdatedOrder ? UpdatedOrder : int.MaxValue);
                }
                else
                {
                    string parentName = string.IsNullOrEmpty(rootName) ? $"{ContainingGroup.GroupName}" : $"{rootName}/{ContainingGroup.GroupName}";
                    UpdatedGroupName = string.IsNullOrEmpty(rootName) ? $"{ContainingGroup.GroupName}/_{FieldInfo.Name}" : $"{rootName}/{ContainingGroup.GroupName}/_{FieldInfo.Name}";
                    _DrawVerticalGroup(parentName, UpdatedGroupName, HasUpdatedOrder ? UpdatedOrder : ContainingGroup.Order);
                }
                bool anyUnmanaged = false;
                foreach (var child in Children)
                {
                    if (!child.IsUnmanagedGroup)
                    {
                        child.UpdatedGroupName = $"{UpdatedGroupName}/{child.ContainingGroup.GroupName}";
                        foreach (var attribute in child.Groups)
                        {
                            string parentName = attribute.ParentName != string.Empty ? $"{UpdatedGroupName}/{attribute.ParentName}" : UpdatedGroupName;
                            string groupName = $"{UpdatedGroupName}/{attribute.GroupName}";
                            switch (attribute.Type)
                            {
                                case UIGroupType.Horizontal:
                                    if (attribute is HorizontalGroupAttribute horizontalAttribute)
                                    {
                                        _DrawHorizontalGroup(parentName, groupName, child.HasUpdatedOrder ? child.UpdatedOrder : attribute.Order);
                                    }
                                    break;
                                case UIGroupType.Vertical:
                                    if (attribute is VerticalGroupAttribute verticalAttribute)
                                    {
                                        _DrawVerticalGroup(parentName, groupName, child.HasUpdatedOrder ? child.UpdatedOrder : attribute.Order);
                                    }
                                    break;
                                case UIGroupType.Foldout:
                                    if (attribute is FoldoutGroupAttribute foldoutAttribute)
                                    {
                                        _DrawFoldoutGroup(parentName, groupName, foldoutAttribute.Header, child.HasUpdatedOrder ? child.UpdatedOrder : attribute.Order);
                                    }
                                    break;
                                case UIGroupType.Box:
                                    if (attribute is BoxGroupAttribute boxAttribute)
                                    {
                                        _DrawBoxGroup(parentName, groupName, boxAttribute.Header, child.HasUpdatedOrder ? child.UpdatedOrder : attribute.Order);
                                    }
                                    break;
                                case UIGroupType.Tab:
                                    break;
                                case UIGroupType.Title:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        child.UpdatedGroupName = UpdatedGroupName;
                        anyUnmanaged = true;
                    }
                    if (anyUnmanaged)
                    {
                        _DrawUnmanagedGroup(UpdatedGroupName, $"{UpdatedGroupName}-Unmanaged", child.HasUpdatedOrder ? child.UpdatedOrder : int.MaxValue);
                    }
                    child.DrawGroups(UpdatedGroupName, groupBag, orderBag);
                }
            }

            void _DrawUnmanagedGroup(string parentName, string groupName, int order)
            {
                if (!groupBag.ContainsKey(groupName))
                {
                    if (!orderBag.TryGetValue(order, out var propList))
                    {
                        propList = new();
                        orderBag[order] = propList;
                    }
                    var vertical = new StyledVerticalGroup();
                    GroupedVisualElement element = new()
                    {
                        parent = parentName,
                        group = groupName,
                        first = true,
                        type = UIGroupType.Vertical,
                        container = vertical
                    };
                    Debug.Log($"Adding Unmanaged Group: {groupName} to {Path}");
                    propList.Add(element);
                    groupBag.Add(groupName, element);
                }
            }

            void _DrawHorizontalGroup(string parentName, string groupName, int order)
            {
                if (!groupBag.ContainsKey(groupName))
                {
                    if (!orderBag.TryGetValue(order, out var propList))
                    {
                        propList = new();
                        orderBag[order] = propList;
                    }
                    var horizontal = new StyledHorizontalGroup();
                    GroupedVisualElement element = new()
                    {
                        parent = parentName,
                        group = groupName,
                        first = true,
                        type = UIGroupType.Horizontal,
                        container = horizontal
                    };
                    Debug.Log($"Adding Horizontal Group: {groupName} to {Path}");
                    propList.Add(element);
                    groupBag.Add(groupName, element);
                }
            }

            void _DrawVerticalGroup(string parentName, string groupName, int order)
            {
                if (!groupBag.ContainsKey(groupName))
                {
                    if (!orderBag.TryGetValue(order, out var propList))
                    {
                        propList = new();
                        orderBag[order] = propList;
                    }
                    var vertical = new StyledVerticalGroup();
                    vertical.name = groupName;
                    GroupedVisualElement element = new()
                    {
                        parent = parentName,
                        group = groupName,
                        first = true,
                        type = UIGroupType.Vertical,
                        container = vertical
                    };
                    Debug.Log($"Adding Vertical Group: {groupName} to {Path}");
                    propList.Add(element);
                    groupBag.Add(groupName, element);
                }
            }

            void _DrawFoldoutGroup(string parentName, string groupName, string header, int order)
            {
                if (!groupBag.ContainsKey(groupName))
                {
                    if (!orderBag.TryGetValue(order, out var propList))
                    {
                        propList = new();
                        orderBag[order] = propList;
                    }
                    var foldout = new StyledFoldout(header);
                    GroupedVisualElement element = new()
                    {
                        parent = parentName,
                        group = groupName,
                        first = true,
                        type = UIGroupType.Foldout,
                        container = foldout
                    };
                    Debug.Log($"Adding Foldout Group: {groupName} to {Path}");
                    propList.Add(element);
                    groupBag.Add(groupName, element);
                }
            }

            void _DrawBoxGroup(string parentName, string groupName, string header, int order)
            {
                if (!groupBag.ContainsKey(groupName))
                {
                    if (!orderBag.TryGetValue(order, out var propList))
                    {
                        propList = new();
                        orderBag[order] = propList;
                    }
                    var box = new StyledHeaderBox(header);
                    GroupedVisualElement element = new()
                    {
                        parent = parentName,
                        group = groupName,
                        first = true,
                        type = UIGroupType.Box,
                        container = box
                    };
                    Debug.Log($"Adding Box Group: {groupName} to {Path}");
                    propList.Add(element);
                    groupBag.Add(groupName, element);
                }
            }
        }

        public void PopulateGroups(BaseVaporInspector inspector, Dictionary<string, GroupedVisualElement> groupBag)
        {
            foreach (var child in Children)
            {
                if (!child.IsDrawnWithVapor)
                {
                    if (!child.IsUnmanagedGroup)
                    {
                        Debug.Log($"{child.Property.propertyPath} - {child.UpdatedGroupName}");
                        if (groupBag.TryGetValue(child.UpdatedGroupName, out var group))
                        {
                            switch (child.ContainingGroup.Type)
                            {
                                case UIGroupType.Horizontal:
                                    _PopulateHorizontalGroup(group, child);
                                    break;
                                case UIGroupType.Vertical:
                                    _PopulateVerticalGroup(group, child);
                                    break;
                                case UIGroupType.Foldout:
                                    _PopulateFoldout(group, child);
                                    break;
                                case UIGroupType.Box:
                                    _PopulateBox(group, child);
                                    break;
                                case UIGroupType.Tab:
                                    break;
                                case UIGroupType.Title:
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (groupBag.TryGetValue($"{child.UpdatedGroupName}-Unmanaged", out var group))
                        {
                            Debug.Log($"Populating Unamanged: {group.parent} - {group.group}");
                            _PopulateVerticalGroup(group, child);
                        }
                    }
                }
                if (child.HasChildren)
                {
                    child.PopulateGroups(inspector, groupBag);
                }
            }

            void _PopulateHorizontalGroup(GroupedVisualElement group, VaporDrawerInfo drawer)
            {
                bool isFirst = group.first;
                group.first = false;
                var drawn = inspector.DrawVaporPropertyWithVerticalLayout(drawer, "vapor-horizontal-prop");
                drawn.style.flexGrow = 1;
                drawn.style.marginLeft = isFirst ? 0 : 2;
                group.container.Add(drawn);
            }

            void _PopulateVerticalGroup(GroupedVisualElement group, VaporDrawerInfo drawer)
            {
                bool formatWithVerticalLayout = false;
                string parent = group.parent;
                while (parent != string.Empty)
                {
                    var parentGroup = groupBag[parent];
                    if (parentGroup.type == UIGroupType.Horizontal)
                    {
                        formatWithVerticalLayout = true;
                    }
                    parent = parentGroup.parent;
                }

                bool isFirst = group.first;
                group.first = false;
                if (formatWithVerticalLayout)
                {
                    var drawn = inspector.DrawVaporPropertyWithVerticalLayout(drawer, "vapor-vertical-prop");
                    drawn.style.marginTop = 1;
                    group.container.Add(drawn);
                }
                else
                {
                    var drawn = inspector.DrawVaporProperty(drawer, "vapor-vertical-prop");
                    drawn.style.marginTop = 1;
                    group.container.Add(drawn);
                }
            }

            void _PopulateFoldout(GroupedVisualElement group, VaporDrawerInfo drawer)
            {
                bool isFirst = group.first;
                group.first = false;
                var drawn = inspector.DrawVaporProperty(drawer, "vapor-foldout-prop");
                group.container.Add(drawn);
            }

            void _PopulateBox(GroupedVisualElement group, VaporDrawerInfo drawer)
            {
                bool isFirst = group.first;
                group.first = false;
                var drawn = inspector.DrawVaporProperty(drawer, "vapor-box-prop");
                group.container.Add(drawn);
            }
        }
        #endregion

        #region - Attributes -
        public bool HasAttribute<T>() where T : Attribute
        {
            return InfoType switch
            {
                DrawerInfoType.Field => FieldInfo.IsDefined(typeof(T), true),
                DrawerInfoType.Property => PropertyInfo.IsDefined(typeof(T), true),
                DrawerInfoType.Method => MethodInfo.IsDefined(typeof(T), true),
                _ => false,
            };
        }

        public bool TryGetAttribute<T>(out T attribute) where T : Attribute
        {
            bool result = false;
            switch (InfoType)
            {
                case DrawerInfoType.Field:
                    result = FieldInfo.IsDefined(typeof(T), true);
                    attribute = result ? FieldInfo.GetCustomAttribute<T>(true) : null;
                    return result;
                case DrawerInfoType.Property:
                    result = PropertyInfo.IsDefined(typeof(T), true);
                    attribute = result ? PropertyInfo.GetCustomAttribute<T>(true) : null;
                    return result;
                case DrawerInfoType.Method:
                    result = MethodInfo.IsDefined(typeof(T), true);
                    attribute = result ? MethodInfo.GetCustomAttribute<T>(true) : null;
                    return result;
                default:
                    attribute = null;
                    return result;
            }
        }

        public bool TryGetAttributes<T>(out T[] attribute) where T : Attribute
        {
            bool result = false;
            switch (InfoType)
            {
                case DrawerInfoType.Field:
                    result = FieldInfo.IsDefined(typeof(T), true);
                    attribute = (T[])(result ? FieldInfo.GetCustomAttributes<T>(true) : null);
                    return result;
                case DrawerInfoType.Property:
                    result = PropertyInfo.IsDefined(typeof(T), true);
                    attribute = (T[])(result ? PropertyInfo.GetCustomAttributes<T>(true) : null);
                    return result;
                case DrawerInfoType.Method:
                    result = MethodInfo.IsDefined(typeof(T), true);
                    attribute = (T[])(result ? MethodInfo.GetCustomAttributes<T>(true) : null);
                    return result;
                default:
                    attribute = null;
                    return result;
            }
        }
        #endregion
    }
}
