using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using VaporUIElements;

namespace VaporUIElementsEditor
{
    public class VaporGroupNode
    {
        public VaporGroupNode Parent { get; }
        public List<VaporGroupNode> Children { get; }
        public List<(int, VisualElement)> ContainerContent { get; }
        public UIGroupType GroupType { get; }
        public string GroupName { get; }
        public int GroupOrder { get; }
        public VisualElement Container { get; }
        public bool IsRootNode { get; }
        public bool ShouldDraw => Children.Count > 0 || ContainerContent.Count > 0;

        public VaporGroupNode(VaporGroupNode parent, VaporGroupAttribute groupAttribute, VisualElement container)
        {
            Parent = parent;
            Children = new();
            ContainerContent = new();
            if (groupAttribute != null)
            {
                GroupType = groupAttribute.Type;
                GroupName = groupAttribute.GroupName;
                GroupOrder = groupAttribute.Order;
            }
            Container = container;
            IsRootNode = Parent == null;
        }

        public VaporGroupNode(VaporGroupNode parent, UIGroupType type, string groupName, int order, VisualElement container)
        {
            Parent = parent;
            Children = new();
            ContainerContent = new();
            GroupType = type;
            GroupName = groupName;
            GroupOrder = order;
            Container = container;
            IsRootNode = Parent == null;
        }

        public void AddChild(VaporGroupAttribute groupAttribute, VisualElement container)
        {
            var childNode = new VaporGroupNode(this, groupAttribute, container);
            Children.Add(childNode);
        }

        public void AddChild(UIGroupType type, string groupName, int order, VisualElement container)
        {
            var childNode = new VaporGroupNode(this, type, groupName, order, container);
            Children.Add(childNode);
        }

        public void AddContent(BaseVaporInspector inspector, VaporDrawerInfo info)
        {
            VisualElement ve = null;
            switch (GroupType)
            {
                case UIGroupType.Horizontal:
                    ve = _PopulateHorizontalGroup(info);
                    break;
                case UIGroupType.Vertical:
                    ve = _PopulateVerticalGroup(info);
                    break;
                case UIGroupType.Foldout:
                    ve = _PopulateFoldout(info);
                    break;
                case UIGroupType.Box:
                    ve = _PopulateBox(info);
                    break;
                case UIGroupType.Tab:
                    ve = _PopulateTabGroup(info);
                    break;
                case UIGroupType.Title:
                    ve = _PopulateTitleGroup(info);
                    break;
                default:
                    break;
            }
            ContainerContent.Add(new(info.UpdatedOrder, ve));
            //Content.Add(info);

            VisualElement _PopulateHorizontalGroup(VaporDrawerInfo drawer)
            {
                bool isFirst = ContainerContent.Count == 0;
                var drawn = inspector.DrawVaporPropertyWithVerticalLayout(drawer, drawer.Path);
                drawn.style.flexGrow = 1;
                drawn.style.marginLeft = isFirst ? 0 : 2;
                return drawn;
                //Container.Add(drawn);
            }

            VisualElement _PopulateVerticalGroup(VaporDrawerInfo drawer)
            {
                bool formatWithVerticalLayout = false;
                var parentNode = Parent;
                while (parentNode != null)
                {
                    if (parentNode.IsRootNode) { break; }

                    if (Parent.GroupType == UIGroupType.Horizontal)
                    {
                        formatWithVerticalLayout = true;
                    }
                    parentNode = parentNode.Parent;
                }

                bool isFirst = ContainerContent.Count == 0;
                if (formatWithVerticalLayout)
                {
                    var drawn = inspector.DrawVaporPropertyWithVerticalLayout(drawer, drawer.Path);
                    drawn.style.marginTop = 1;
                    return drawn;
                    //Container.Add(drawn);
                }
                else
                {
                    var drawn = inspector.DrawVaporProperty(drawer, drawer.Path);
                    drawn.style.marginTop = 1;
                    return drawn;
                    //Container.Add(drawn);
                }
            }

            VisualElement _PopulateFoldout(VaporDrawerInfo drawer)
            {
                bool isFirst = ContainerContent.Count == 0;
                var drawn = inspector.DrawVaporProperty(drawer, drawer.Path);
                return drawn;
                //Container.Add(drawn);
            }

            VisualElement _PopulateBox(VaporDrawerInfo drawer)
            {
                bool isFirst = ContainerContent.Count == 0;
                var drawn = inspector.DrawVaporProperty(drawer, drawer.Path);
                return drawn;
                //Container.Add(drawn);
            }

            VisualElement _PopulateTabGroup(VaporDrawerInfo drawer)
            {
                bool isFirst = ContainerContent.Count == 0;
                var drawn = inspector.DrawVaporProperty(drawer, drawer.Path);
                return drawn;
                //Container.Add(drawn);
            }

            VisualElement _PopulateTitleGroup(VaporDrawerInfo drawer)
            {
                bool isFirst = ContainerContent.Count == 0;
                var drawn = inspector.DrawVaporProperty(drawer, drawer.Path);
                return drawn;
                //Container.Add(drawn);
            }
        }

        public void BuildContent()
        {
            foreach (var next in ContainerContent.OrderBy(x => x.Item1))
            {
                Container.Add(next.Item2);
            }
        }
    }
}
