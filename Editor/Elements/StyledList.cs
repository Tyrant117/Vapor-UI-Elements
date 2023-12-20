using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VaporUIElements;
using VisualElementExtensions = VaporUIElements.VisualElementExtensions;

namespace VaporUIElementsEditor
{
    public class StyledList : Box
    {
        public VaporDrawerInfo Drawer { get; }
        public SerializedProperty ArrayProperty { get; }
        public TypeInfo ElementType { get; }
        public Foldout Foldout { get; private set; }
        public Label Label { get; private set; }
        public VisualElement Content { get; private set; }
        public ListView ListView { get; private set; }

        public override VisualElement contentContainer => ListView;

        public StyledList(VaporDrawerInfo drawer)
        {
            Drawer = drawer;
            ArrayProperty = drawer.Property.FindPropertyRelative("Array.size");
            StyleBox();
            StyleFoldout();
            hierarchy.Add(ListView);
            var ti = Drawer.FieldInfo.FieldType.GetTypeInfo();
            ElementType = ti.IsGenericType
                ? ti.GenericTypeArguments[0].GetTypeInfo()
                : ti.GetElementType().GetTypeInfo();
        }

        protected void StyleBox()
        {
            name = "styled-list";
            style.borderBottomColor = ContainerStyles.BorderColor;
            style.borderTopColor = ContainerStyles.BorderColor;
            style.borderRightColor = ContainerStyles.BorderColor;
            style.borderLeftColor = ContainerStyles.BorderColor;
            style.borderBottomLeftRadius = 3;
            style.borderBottomRightRadius = 3;
            style.borderTopLeftRadius = 3;
            style.borderTopRightRadius = 3;
            style.marginTop = 3;
            style.marginBottom = 3;
            style.marginLeft = 0;
            style.marginRight = 0;
            // style.backgroundColor = ContainerStyles.BackgroundColor;
        }

        protected void StyleFoldout()
        {
            ListView = new ListView
            {
                name = "styled-list-view",
                style =
                {
                    maxHeight = 251,
                    flexGrow = 1
                },
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showFoldoutHeader = true,
                showAddRemoveFooter = false,
                showBorder = false,
                showBoundCollectionSize = false,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                makeItem = OnCustomMake,
                bindItem = OnCustomBind,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated
            };
            ListView.BindProperty(Drawer.Property);
            // ListView.RegisterCallback<WheelEvent>(evt => { ListView.RefreshItems(); });

            Foldout = ListView.Q<Foldout>();
            Foldout.text = Drawer.Property.displayName;
            Foldout.name = "styled-list-foldout";
            Foldout.viewDataKey = $"styled-list-foldout__vdk_{Drawer.Property.displayName}";

            var tog = Foldout.Q<Toggle>();
            tog.RegisterCallback<NavigationSubmitEvent>(evt =>
            {
                evt.StopPropagation();
            }, TrickleDown.TrickleDown);
            tog.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Clear", ca =>
                {
                    Drawer.Property.ClearArray();
                    Drawer.Property.serializedObject.ApplyModifiedProperties();
                    ListView.schedule.Execute(() => ListView.RefreshItems()).ExecuteLater(100);
                });
            }));
            var togStyle = tog.style;
            togStyle.marginTop = 0;
            togStyle.marginLeft = 0;
            togStyle.marginRight = 0;
            togStyle.marginBottom = 0;
            togStyle.backgroundColor = ContainerStyles.BackgroundColor;

            var togContainerStyle = tog.hierarchy[0].style;
            togContainerStyle.marginLeft = 3;
            togContainerStyle.marginTop = 3;
            togContainerStyle.marginBottom = 3;
            togContainerStyle.flexShrink = 1;

            // Label
            Label = Foldout.Q<Toggle>().Q<Label>();
            Label.style.textOverflow = TextOverflow.Ellipsis;

            // tog.Add(new ToolbarSearchField());
            var sizeVal = new IntegerField()
            {
                style =
                {
                    minWidth = 51,
                    marginRight = 2,
                    marginTop = 2,
                }
            };
            sizeVal.BindProperty(ArrayProperty);
            var valText = sizeVal[0][0];
            valText.style.marginLeft = 0;
            valText.style.paddingLeft = 2;
            // sizeVal.SetValueWithoutNotify(Drawer.Property.arraySize);
            tog.Add(sizeVal);
            var minus = new Button(OnRemoveFromList)
            {
                text = "-",
                style =
                {
                    paddingLeft = 5,
                    paddingRight = 5,
                    fontSize = 14,
                    borderBottomWidth = 0,
                    borderLeftWidth = 1,
                    borderRightWidth = 0,
                    borderTopWidth = 0,
                    borderBottomLeftRadius = 0,
                    borderBottomRightRadius = 0,
                    borderTopLeftRadius = 0,
                    borderTopRightRadius = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    minWidth = 21,
                }
            };
            tog.Add(minus);
            var plus = new Button(OnAddToList)
            {
                text = "+",
                style =
                {
                    paddingLeft = 5,
                    paddingRight = 5,
                    fontSize = 14,
                    borderBottomWidth = 0,
                    borderLeftWidth = 1,
                    borderRightWidth = 0,
                    borderTopWidth = 0,
                    borderBottomLeftRadius = 0,
                    borderBottomRightRadius = 0,
                    borderTopLeftRadius = 0,
                    borderTopRightRadius = 0,
                    marginLeft = 0,
                    marginRight = 0,
                }
            };
            tog.Add(plus);

            // Content
            Content = Foldout.Q<VisualElement>("unity-content");

            Foldout.value = false;
        }

        private void OnCustomBind(VisualElement toBind, int index)
        {
            var prop = GetPropertyAtIndex(index);
            if (prop == null)
            {
                return;
            }

            if (ElementType.IsDefined(typeof(DrawWithVaporAttribute)))
            {
                var rootElement = toBind[0];
                var drawers = rootElement.userData as List<VaporDrawerInfo>;
                if (rootElement is ILabeledGroup labeledGroup)
                {
                    labeledGroup.Label.text = $"Element {index}";
                }

                // ReSharper disable once PossibleNullReferenceException
                foreach (var drawer in drawers)
                {
                    // Debug.Log($"Rebinding {drawer.Path} to {prop.propertyPath}");
                    drawer.Rebind(prop);
                }

                foreach (var bindable in rootElement.Query().Where(x => x is IBindable).ToList())
                {
                    if (bindable is not PropertyField field)
                    {
                        continue;
                    }

                    if (!bindable.name.Contains(Drawer.Property.name))
                    {
                        continue;
                    }

                    var lastIndex = bindable.name.LastIndexOf('.') + 1;
                    var lastElement = bindable.name[lastIndex..];
                    var propToBind = prop.FindPropertyRelative(lastElement);
                    field.BindProperty(propToBind);
                    DrawerUtility.OnPropertyBuilt(field);
                }

                foreach (var b in rootElement.Query<StyledButton>().ToList())
                {
                    DrawerUtility.OnMethodBuilt(b);
                }
            }
            else
            {
                var field = toBind.Q<PropertyField>();
                field.BindProperty(prop);
            }

            var button = toBind.Q<Button>("styled-list-element-button__delete");
            button.clickable = new Clickable(() => RemoveIndexFromList(index));
        }

        protected VisualElement OnCustomMake()
        {
            var be = new BindableElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };
            var nextProp = GetPropertyAtIndex(ArrayProperty.intValue - 1);
            var root = new VaporListElementRoot(ElementType, nextProp);
            if (root.IsDrawnWithVapor)
            {
                var rootContainer = root.GetVisualElement();
                be.Add(rootContainer);
            }
            else
            {
                var pe = new PropertyField()
                {
                    style =
                    {
                        flexGrow = 1,
                        marginRight = 3,
                    }
                };
                be.Add(pe);
            }
            
            var del = new Button()
            {
                name = "styled-list-element-button__delete",
                text = "x",
                style =
                {
                    paddingLeft = 4,
                    paddingRight = 4,
                    fontSize = 11,
                    marginLeft = 1,
                    marginRight = -2,
                }
            };
            be.Add(del);

            return be;
        }

        private void OnAddToList()
        {
            Drawer.Property.arraySize++;
            Drawer.Property.serializedObject.ApplyModifiedProperties();
            ListView.schedule.Execute(() => { ListView.RefreshItems(); }).ExecuteLater(100L);
        }

        private void OnRemoveFromList()
        {
            if (Drawer.Property.arraySize <= 0)
            {
                return;
            }

            Drawer.Property.arraySize--;
            Drawer.Property.serializedObject.ApplyModifiedProperties();
            ListView.schedule.Execute(() => { ListView.RefreshItems(); }).ExecuteLater(100L);
        }

        private void RemoveIndexFromList(int index)
        {
            Drawer.Property.DeleteArrayElementAtIndex(index);
            Drawer.Property.serializedObject.ApplyModifiedProperties();
            ListView.schedule.Execute(() => { ListView.RefreshItems(); }).ExecuteLater(100L);
        }

        private SerializedProperty GetPropertyAtIndex(int index)
        {
            return index < Drawer.Property.arraySize && index >= 0
                ? Drawer.Property.FindPropertyRelative($"Array.data[{index}]")
                : null;
        }
    }
}
