using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VaporUIElements;

namespace VaporUIElementsEditor
{
    public static class DrawerUtility
    {
        public static void DrawLabel(PropertyField field, VaporDrawerInfo drawer, List<Action> resolvers)
        {
            if (drawer.TryGetAttribute<LabelAttribute>(out var atr))
            {
                var label = field.Q<Label>();
                switch (atr.LabelResolverType)
                {
                    case ResolverType.None:
                        label.text = atr.Label;
                        break;
                    case ResolverType.Property:
                        PropertyInfo pi = ReflectionUtility.GetProperty(drawer.Target, atr.LabelResolver[1..]);
                        label.text = (string)pi.GetValue(drawer.Target);
                        resolvers.Add(() => label.text = (string)pi.GetValue(drawer.Target));
                        break;
                    case ResolverType.Method:
                        MethodInfo mi = ReflectionUtility.GetMethod(drawer.Target, atr.LabelResolver[1..]);
                        label.text = (string)mi.Invoke(drawer.Target, null);
                        resolvers.Add(() => label.text = (string)mi.Invoke(drawer.Target, null));
                        break;
                }

                switch (atr.LabelColorResolverType)
                {
                    case ResolverType.None:
                        label.style.color = atr.LabelColor;
                        break;
                    case ResolverType.Property:
                        PropertyInfo pi = ReflectionUtility.GetProperty(drawer.Target, atr.LabelColorResolver[1..]);
                        label.style.color = (Color)pi.GetValue(drawer.Target);
                        resolvers.Add(() => label.style.color = (Color)pi.GetValue(drawer.Target));
                        break;
                    case ResolverType.Method:
                        MethodInfo mi = ReflectionUtility.GetMethod(drawer.Target, atr.LabelColorResolver[1..]);
                        label.style.color = (Color)mi.Invoke(drawer.Target, null);
                        resolvers.Add(() => label.style.color = (Color)mi.Invoke(drawer.Target, null));
                        break;
                }

                if (atr.HasIcon)
                {
                    var image = new Image
                    {
                        image = EditorGUIUtility.IconContent(atr.Icon).image,
                        scaleMode = ScaleMode.ScaleToFit,
                        pickingMode = PickingMode.Ignore
                    };
                    image.style.alignSelf = Align.FlexEnd;
                    switch (atr.IconColorResolverType)
                    {
                        case ResolverType.None:
                            image.tintColor = atr.IconColor.value;
                            break;
                        case ResolverType.Property:
                            PropertyInfo pi = ReflectionUtility.GetProperty(drawer.Target, atr.IconColorResolver[1..]);
                            image.tintColor = (Color)pi.GetValue(drawer.Target);
                            resolvers.Add(() => image.tintColor = (Color)pi.GetValue(drawer.Target));
                            break;
                        case ResolverType.Method:
                            MethodInfo mi = ReflectionUtility.GetMethod(drawer.Target, atr.IconColorResolver[1..]);
                            image.tintColor = (Color)mi.Invoke(drawer.Target, null);
                            resolvers.Add(() => image.tintColor = (Color)mi.Invoke(drawer.Target, null));
                            break;
                    }
                    label.Add(image);
                }
            }
        }

        public static void DrawLabelWidth(PropertyField field, VaporDrawerInfo drawer)
        {
            if (drawer.TryGetAttribute<LabelWidthAttribute>(out var atr))
            {
                field[0].RemoveFromClassList("unity-base-field__aligned");
                var label = field.Q<Label>();
                if (atr.UseAutoWidth)
                {
                    label.style.minWidth = new StyleLength(StyleKeyword.Auto);
                    label.style.width = new StyleLength(StyleKeyword.Auto);
                }
                else
                {
                    float minWidth = Mathf.Min(label.resolvedStyle.minWidth.value, atr.Width);
                    label.style.minWidth = minWidth;
                    label.style.width = atr.Width;
                }
            }
        }

        public static void DrawHideLabel(PropertyField field, VaporDrawerInfo drawer)
        {
            if (drawer.HasAttribute<HideLabelAttribute>())
            {
                var label = field.Q<Label>();
                label.style.display = DisplayStyle.None;
            }
        }

        public static void DrawDecorators(PropertyField field, VaporDrawerInfo drawer)
        {
            if (drawer.TryGetAttribute<BackgroundColorAttribute>(out var bcatr))
            {
                field.style.backgroundColor = bcatr.BackgroundColor;
            }

            if (drawer.TryGetAttribute<MarginsAttribute>(out var matr))
            {
                if (matr.Bottom != float.MinValue)
                {
                    field.style.marginBottom = matr.Bottom;
                }
                if (matr.Top != float.MinValue)
                {
                    field.style.marginTop = matr.Top;
                }
                if (matr.Left != float.MinValue)
                {
                    field.style.marginLeft = matr.Left;
                }
                if (matr.Right != float.MinValue)
                {
                    field.style.marginRight = matr.Right;
                }
            }

            if (drawer.TryGetAttribute<PaddingAttribute>(out var patr))
            {
                if (patr.Bottom != float.MinValue)
                {
                    field.style.paddingBottom = patr.Bottom;
                }
                if (patr.Top != float.MinValue)
                {
                    field.style.paddingTop = patr.Top;
                }
                if (patr.Left != float.MinValue)
                {
                    field.style.paddingLeft = patr.Left;
                }
                if (patr.Right != float.MinValue)
                {
                    field.style.paddingRight = patr.Right;
                }
            }

            if (drawer.TryGetAttribute<BordersAttribute>(out var batr))
            {
                if (batr.Bottom != float.MinValue)
                {
                    field.style.borderBottomWidth = batr.Bottom;
                    field.style.borderBottomColor = batr.Color;
                }
                if (batr.Top != float.MinValue)
                {
                    field.style.borderTopWidth = batr.Top;
                    field.style.borderTopColor = batr.Color;
                }
                if (batr.Left != float.MinValue)
                {
                    field.style.borderLeftWidth = batr.Left;
                    field.style.borderLeftColor = batr.Color;
                }
                if (batr.Right != float.MinValue)
                {
                    field.style.borderRightWidth = batr.Right;
                    field.style.borderRightColor = batr.Color;
                }
                if (batr.Rounded)
                {
                    field.style.borderBottomLeftRadius = 3;
                    field.style.borderBottomRightRadius = 3;
                    field.style.borderTopLeftRadius = 3;
                    field.style.borderTopRightRadius = 3;
                }
            }
        }

        public static void DrawConditionals(PropertyField field, VaporDrawerInfo drawer, List<Action> resolvers)
        {
            if (drawer.TryGetAttribute<ShowIfAttribute>(out var siatr))
            {
                switch (siatr.ResolverType)
                {
                    case ResolverType.None:
                        break;
                    case ResolverType.Property:
                        PropertyInfo pi = ReflectionUtility.GetProperty(drawer.Target, siatr.Resolver[1..]);
                        field.style.display = (bool)pi.GetValue(drawer.Target) ? DisplayStyle.Flex : DisplayStyle.None;
                        resolvers.Add(() => field.style.display = (bool)pi.GetValue(drawer.Target) ? DisplayStyle.Flex : DisplayStyle.None);
                        break;
                    case ResolverType.Method:
                        MethodInfo mi = ReflectionUtility.GetMethod(drawer.Target, siatr.Resolver[1..]);
                        field.style.display = (bool)mi.Invoke(drawer.Target, null) ? DisplayStyle.Flex : DisplayStyle.None;
                        resolvers.Add(() => field.style.display = (bool)mi.Invoke(drawer.Target, null) ? DisplayStyle.Flex : DisplayStyle.None);
                        break;
                }
            }

            if (drawer.TryGetAttribute<ShowIfAttribute>(out var hiatr))
            {
                switch (hiatr.ResolverType)
                {
                    case ResolverType.None:
                        break;
                    case ResolverType.Property:
                        PropertyInfo pi = ReflectionUtility.GetProperty(drawer.Target, hiatr.Resolver[1..]);
                        field.style.display = (bool)pi.GetValue(drawer.Target) ? DisplayStyle.None : DisplayStyle.Flex;
                        resolvers.Add(() => field.style.display = (bool)pi.GetValue(drawer.Target) ? DisplayStyle.None : DisplayStyle.Flex);
                        break;
                    case ResolverType.Method:
                        MethodInfo mi = ReflectionUtility.GetMethod(drawer.Target, hiatr.Resolver[1..]);
                        field.style.display = (bool)mi.Invoke(drawer.Target, null) ? DisplayStyle.None : DisplayStyle.Flex;
                        resolvers.Add(() => field.style.display = (bool)mi.Invoke(drawer.Target, null) ? DisplayStyle.None : DisplayStyle.Flex);
                        break;
                }
            }

            if (drawer.TryGetAttribute<ShowIfAttribute>(out var diatr))
            {
                switch (diatr.ResolverType)
                {
                    case ResolverType.None:
                        break;
                    case ResolverType.Property:
                        PropertyInfo pi = ReflectionUtility.GetProperty(drawer.Target, diatr.Resolver[1..]);
                        field.SetEnabled(!(bool)pi.GetValue(drawer.Target));
                        resolvers.Add(() => field.SetEnabled(!(bool)pi.GetValue(drawer.Target)));
                        break;
                    case ResolverType.Method:
                        MethodInfo mi = ReflectionUtility.GetMethod(drawer.Target, diatr.Resolver[1..]);
                        field.SetEnabled(!(bool)mi.Invoke(drawer.Target, null));
                        resolvers.Add(() => field.SetEnabled(!(bool)mi.Invoke(drawer.Target, null)));
                        break;
                }
            }

            if (drawer.TryGetAttribute<ShowIfAttribute>(out var eiatr))
            {
                switch (eiatr.ResolverType)
                {
                    case ResolverType.None:
                        break;
                    case ResolverType.Property:
                        PropertyInfo pi = ReflectionUtility.GetProperty(drawer.Target, eiatr.Resolver[1..]);
                        field.SetEnabled((bool)pi.GetValue(drawer.Target));
                        resolvers.Add(() => field.SetEnabled((bool)pi.GetValue(drawer.Target)));
                        break;
                    case ResolverType.Method:
                        MethodInfo mi = ReflectionUtility.GetMethod(drawer.Target, eiatr.Resolver[1..]);
                        field.SetEnabled((bool)mi.Invoke(drawer.Target, null));
                        resolvers.Add(() => field.SetEnabled((bool)mi.Invoke(drawer.Target, null)));
                        break;
                }
            }

            if (drawer.HasAttribute<HideInEditorModeAttribute>())
            {
                field.style.display = EditorApplication.isPlaying ? DisplayStyle.Flex : DisplayStyle.None;
                resolvers.Add(() => field.style.display = EditorApplication.isPlaying ? DisplayStyle.Flex : DisplayStyle.None);
            }

            if (drawer.HasAttribute<HideInPlayModeAttribute>())
            {
                field.style.display = EditorApplication.isPlaying ? DisplayStyle.None : DisplayStyle.Flex;
                resolvers.Add(() => field.style.display = EditorApplication.isPlaying ? DisplayStyle.None : DisplayStyle.Flex);
            }

            if (drawer.HasAttribute<DisableInEditorModeAttribute>())
            {
                field.SetEnabled(EditorApplication.isPlaying);
                resolvers.Add(() => field.SetEnabled(EditorApplication.isPlaying));
            }

            if (drawer.HasAttribute<DisableInPlayModeAttribute>())
            {
                field.SetEnabled(!EditorApplication.isPlaying);
                resolvers.Add(() => field.SetEnabled(!EditorApplication.isPlaying));
            }
        }

        public static void DrawPathSelection(PropertyField field, VaporDrawerInfo drawer)
        {
            if (drawer.TryGetAttribute<VaporUIElements.FilePathAttribute>(out var fileAtr) && field[0] is TextField filePathTextField)
            {
                var inlineButton = new Button(() => filePathTextField.value = _FormatFilePath(fileAtr.AbsolutePath, fileAtr.FileExtension))
                {
                    text = "",
                };
                var image = new Image
                {
                    image = EditorGUIUtility.IconContent("d_FolderOpened Icon").image,
                    scaleMode = ScaleMode.ScaleToFit
                };
                filePathTextField.style.width = 0;
                inlineButton.style.paddingLeft = 3;
                inlineButton.style.paddingRight = 3;
                inlineButton.style.backgroundColor = new Color(0, 0, 0, 0);
                image.style.width = 16;
                image.style.height = 16;
                inlineButton.Add(image);
                field.Add(inlineButton);
                field.style.flexDirection = FlexDirection.Row;
                field[0].style.flexGrow = 1f;
            }

            if (drawer.TryGetAttribute<FolderPathAttribute>(out var folderAtr) && field[0] is TextField folderPathTextField)
            {
                var inlineButton = new Button(() => folderPathTextField.value = _FormatFolderPath(folderAtr.AbsolutePath))
                {
                    text = "",
                };
                var image = new Image
                {
                    image = EditorGUIUtility.IconContent("d_FolderOpened Icon").image,
                    scaleMode = ScaleMode.ScaleToFit
                };
                folderPathTextField.style.width = 0;
                inlineButton.style.paddingLeft = 3;
                inlineButton.style.paddingRight = 3;
                inlineButton.style.backgroundColor = new Color(0, 0, 0, 0);
                image.style.width = 16;
                image.style.height = 16;
                inlineButton.Add(image);
                field.Add(inlineButton);
                field.style.flexDirection = FlexDirection.Row;
                field[0].style.flexGrow = 1f;
            }

            string _FormatFilePath(bool absolutePath, string fileExtension)
            {
                if (!absolutePath)
                {
                    string path = EditorUtility.OpenFilePanel("File Path", "Assets", fileExtension);
                    int start = path.IndexOf("Assets");
                    return path[start..];
                }
                else
                {
                    return EditorUtility.OpenFilePanel("File Path", "Assets", fileExtension);
                }
            }

            string _FormatFolderPath(bool absolutePath)
            {
                if (!absolutePath)
                {
                    string path = EditorUtility.OpenFolderPanel("Folder Path", "Assets", "");
                    int start = path.IndexOf("Assets");
                    return path[start..];
                }
                else
                {
                    return EditorUtility.OpenFolderPanel("Folder Path", "Assets", "");
                }
            }
        }

        public static void DrawReadOnly(PropertyField field, VaporDrawerInfo drawer)
        {
            if (drawer.HasAttribute<ReadOnlyAttribute>())
            {
                field.SetEnabled(false);
            }
        }

        public static void DrawTitle(PropertyField field, VaporDrawerInfo drawer)
        {
            if (drawer.TryGetAttribute<TitleAttribute>(out var atr))
            {
                string labelText = $"<b>{atr.Title}</b>";
                if (atr.Subtitle != string.Empty)
                {
                    labelText = $"<b>{atr.Title}</b>\n<color=#9E9E9E><i><size=10>{atr.Subtitle}</size></i></color>";
                }
                var title = new Label(labelText);
                title.style.borderBottomWidth = atr.Underline ? 1 : 0;
                title.style.paddingBottom = 2;
                title.style.borderBottomColor = ContainerStyles.TextDefault;
                title.style.marginBottom = 1f;
                int index = field.parent.IndexOf(field);
                field.parent.Insert(index, title);
            }
        }

        public static void DrawInlineButtons(PropertyField field, VaporDrawerInfo drawer, List<Action> resolvers)
        {
            if (drawer.TryGetAttributes<InlineButtonAttribute>(out var atrs))
            {
                foreach (var atr in atrs)
                {
                    var methodInfo = ReflectionUtility.GetMethod(drawer.Target, atr.MethodName);
                    if (methodInfo != null)
                    {
                        var inlineButton = new Button(() => methodInfo.Invoke(drawer.Target, null))
                        {
                            text = atr.Label,
                        };
                        inlineButton.style.paddingLeft = 3;
                        inlineButton.style.paddingRight = 3;
                        if (atr.Icon != string.Empty)
                        {
                            var image = new Image
                            {
                                image = EditorGUIUtility.IconContent(atr.Icon).image,
                                scaleMode = ScaleMode.ScaleToFit,
                                tintColor = atr.Tint
                            };
                            if (atr.TintResolverType != ResolverType.None)
                            {
                                switch (atr.TintResolverType)
                                {
                                    case ResolverType.None:
                                        break;
                                    case ResolverType.Property:
                                        PropertyInfo pi = ReflectionUtility.GetProperty(drawer.Target, atr.TintResolver[1..]);
                                        image.tintColor = (Color)pi.GetValue(drawer.Target);
                                        resolvers.Add(() => image.tintColor = (Color)pi.GetValue(drawer.Target));
                                        break;
                                    case ResolverType.Method:
                                        MethodInfo mi = ReflectionUtility.GetMethod(drawer.Target, atr.TintResolver[1..]);
                                        image.tintColor = (Color)mi.Invoke(drawer.Target, null);
                                        resolvers.Add(() => image.tintColor = (Color)mi.Invoke(drawer.Target, null));
                                        break;
                                }
                            }
                            inlineButton.Add(image);
                        }

                        field.Add(inlineButton);
                        field.style.flexDirection = FlexDirection.Row;
                        field[0].style.flexGrow = 1f;
                    }
                }
            }
        }

        public static void DrawSuffix(PropertyField field, VaporDrawerInfo drawer)
        {
            if (drawer.TryGetAttribute<SuffixAttribute>(out var atr))
            {
                var suffix = new Label(atr.Suffix);
                suffix.style.color = new Color(0.5f, 0.5f, 0.5f, 1);
                suffix.style.alignSelf = Align.Center;
                suffix.style.marginLeft = 3;
                suffix.style.paddingLeft = 3;
                field.Add(suffix);
                field.style.flexDirection = FlexDirection.Row;
                field[0].style.flexGrow = 1f;
            }
        }

        public static void DrawAutoReference(PropertyField field, VaporDrawerInfo drawer)
        {
            if (drawer.TryGetAttribute<AutoReferenceAttribute>(out var atr)
                && drawer.Property.propertyType == SerializedPropertyType.ObjectReference
                && !drawer.Property.objectReferenceValue
                && drawer.Property.serializedObject.targetObject is Component component)
            {
                var comp = component.GetComponent(drawer.FieldInfo.FieldType);
                if (!comp && atr.SearchChildren)
                {
                    comp = component.GetComponentInChildren(drawer.FieldInfo.FieldType, true);
                }
                if (!comp && atr.SearchParents)
                {
                    comp = component.GetComponentInParent(drawer.FieldInfo.FieldType, true);
                }
                drawer.Property.objectReferenceValue = comp;
                drawer.Property.serializedObject.ApplyModifiedProperties();
            }
        }

        public static void DrawValidation(PropertyField field, VaporDrawerInfo drawer, List<Action> resolvers)
        {
            if (drawer.TryGetAttribute<OnValueChangedAttribute>(out var ovcatr))
            {
                var methodInfo = ReflectionUtility.GetMethod(drawer.Target, ovcatr.MethodName);
                if (methodInfo != null)
                {
                    field.RegisterValueChangeCallback(x => methodInfo.Invoke(drawer.Target, null));
                }
            }

            if (drawer.TryGetAttribute<ValidateInputAttribute>(out var viatr))
            {
                var label = field.Q<Label>();
                var image = new Image
                {
                    name = "image-error",
                    image = EditorGUIUtility.IconContent("Error").image,
                    scaleMode = ScaleMode.ScaleToFit,
                    pickingMode = PickingMode.Ignore,
                    //tintColor = ContainerStyles.ErrorText.value,
                };
                image.style.alignSelf = Align.FlexEnd;
                label.Add(image);

                var methodInfo = ReflectionUtility.GetMethod(drawer.Target, viatr.MethodName);
                if (methodInfo != null)
                {
                    bool validated = _OnValidateInput(drawer.Property, methodInfo, drawer.Target);
                    image.style.display = validated ? (StyleEnum<DisplayStyle>)DisplayStyle.None : (StyleEnum<DisplayStyle>)DisplayStyle.Flex;
                    field.RegisterValueChangeCallback(x => _ValidateInput(x, methodInfo, drawer.Target));
                }
            }

            static void _ValidateInput(SerializedPropertyChangeEvent evt, MethodInfo mi, object target)
            {
                bool validated = _OnValidateInput(evt.changedProperty, mi, target);
                var field = evt.target as PropertyField;
                var image = field.Q<Image>("image-error");
                image.style.display = validated ? (StyleEnum<DisplayStyle>)DisplayStyle.None : (StyleEnum<DisplayStyle>)DisplayStyle.Flex;
            }

            static bool _OnValidateInput(SerializedProperty sp, MethodInfo mi, object target)
            {
                return sp.propertyType switch
                {
                    SerializedPropertyType.Generic => (bool)mi.Invoke(target, new object[] { sp.boxedValue }),
                    SerializedPropertyType.Integer => (bool)mi.Invoke(target, new object[] { sp.intValue }),
                    SerializedPropertyType.Boolean => (bool)mi.Invoke(target, new object[] { sp.boolValue }),
                    SerializedPropertyType.Float => (bool)mi.Invoke(target, new object[] { sp.floatValue }),
                    SerializedPropertyType.String => (bool)mi.Invoke(target, new object[] { sp.stringValue }),
                    SerializedPropertyType.Color => (bool)mi.Invoke(target, new object[] { sp.colorValue }),
                    SerializedPropertyType.ObjectReference => (bool)mi.Invoke(target, new object[] { sp.objectReferenceValue }),
                    SerializedPropertyType.LayerMask => (bool)mi.Invoke(target, new object[] { sp.intValue }),
                    SerializedPropertyType.Enum => (bool)mi.Invoke(target, new object[] { sp.enumValueIndex }),
                    SerializedPropertyType.Vector2 => (bool)mi.Invoke(target, new object[] { sp.vector2Value }),
                    SerializedPropertyType.Vector3 => (bool)mi.Invoke(target, new object[] { sp.vector3Value }),
                    SerializedPropertyType.Vector4 => (bool)mi.Invoke(target, new object[] { sp.vector4Value }),
                    SerializedPropertyType.Rect => (bool)mi.Invoke(target, new object[] { sp.rectValue }),
                    SerializedPropertyType.ArraySize => (bool)mi.Invoke(target, new object[] { sp.arraySize }),
                    SerializedPropertyType.Character => (bool)mi.Invoke(target, new object[] { sp.stringValue }),
                    SerializedPropertyType.AnimationCurve => (bool)mi.Invoke(target, new object[] { sp.animationCurveValue }),
                    SerializedPropertyType.Bounds => (bool)mi.Invoke(target, new object[] { sp.boundsValue }),
                    SerializedPropertyType.Gradient => (bool)mi.Invoke(target, new object[] { sp.gradientValue }),
                    SerializedPropertyType.Quaternion => (bool)mi.Invoke(target, new object[] { sp.quaternionValue }),
                    SerializedPropertyType.ExposedReference => (bool)mi.Invoke(target, new object[] { sp.exposedReferenceValue }),
                    SerializedPropertyType.FixedBufferSize => (bool)mi.Invoke(target, new object[] { sp.fixedBufferSize }),
                    SerializedPropertyType.Vector2Int => (bool)mi.Invoke(target, new object[] { sp.vector2IntValue }),
                    SerializedPropertyType.Vector3Int => (bool)mi.Invoke(target, new object[] { sp.vector3IntValue }),
                    SerializedPropertyType.RectInt => (bool)mi.Invoke(target, new object[] { sp.rectIntValue }),
                    SerializedPropertyType.BoundsInt => (bool)mi.Invoke(target, new object[] { sp.boundsIntValue }),
                    SerializedPropertyType.ManagedReference => (bool)mi.Invoke(target, new object[] { sp.managedReferenceValue }),
                    SerializedPropertyType.Hash128 => (bool)mi.Invoke(target, new object[] { sp.hash128Value }),
                    _ => false,
                };
            }
        }
    }
}
