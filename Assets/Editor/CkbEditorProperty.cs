using NetickEditor;
using System;
using UnityEditor;
using UnityEngine;

#nullable disable
namespace CkbEditor
{
    public class CkbEditorProperty
    {
        internal static class CkbEditorToggleState
        {
            internal static string SelectedPropertyName = "";
            internal static bool ShowAdvancedNetworkTransform = false;
            internal static bool ShowAdvancedErrorSmoothingNetworkTransform = false;
            internal static bool ShowAdvancedNetworkObject = false;
            internal static bool ShowAdvanced = false;
            internal static bool ShowNetowrkSettings = true;
            internal static bool ShowMiscSettings = true;
            internal static bool ShowGeneralSettings = true;
            internal static bool ShowAdvancedSettings = true;
            internal static bool ShowLagCompSettings = true;
            internal static bool ShowSimulationSettings = true;
            internal static bool ShowSettings = true;
            internal static bool ShowIM = false;
            internal static bool ShowObjectIMSettings = true;
            internal static bool VolumeDebug = false;
        }
        // 绘制属性的方法，带标题和帮助文本
        public static void DrawProperty(
            UnityEngine.Object behaviour,
            string title,
            SerializedProperty property,
            string helpText = "")
        {
            CkbEditorProperty.PropertyDrawWithName method = new CkbEditorProperty.PropertyDrawWithName()
            {
                Property = property
            };
            CkbEditorProperty.DrawInternal(behaviour, title, (CkbEditorProperty.DrawMethod)method, helpText, property);
        }

        // 绘制属性的方法，不带标题
        public static void DrawProperty(UnityEngine.Object behaviour, SerializedProperty property, string helpText = "")
        {
            CkbEditorProperty.PropertyDraw method = new CkbEditorProperty.PropertyDraw()
            {
                Property = property
            };
            if (behaviour == null || property == null)
                    return;
            CkbEditorProperty.DrawInternal(behaviour, property.name, (CkbEditorProperty.DrawMethod)method, helpText, property);
        }

        // 绘制字段的方法，带标题、文本和帮助按钮
        public static void DrawField(
            UnityEngine.Object behaviour,
            string title,
            string text,
            bool hasHelpButton,
            string helpText = "")
        {
            CkbEditorProperty.LabelDraw method = new CkbEditorProperty.LabelDraw()
            {
                Text = text,
                ToolTipText = helpText,
                HasHelpButton = hasHelpButton
            };
            CkbEditorProperty.DrawInternal(behaviour, title, (CkbEditorProperty.DrawMethod)method, helpText, helpButton: hasHelpButton);
        }

        // 绘制标签字段
        public static void DrawLabel(UnityEngine.Object behaviour, string title, string text, string helpText = "")
        {
            CkbEditorProperty.LabelDraw method = new CkbEditorProperty.LabelDraw()
            {
                Text = text,
                ToolTipText = helpText
            };
            CkbEditorProperty.DrawInternal(behaviour, title, (CkbEditorProperty.DrawMethod)method, helpText);
        }

        // 绘制整数滑动条
        public static void DrawIntSlider(
            UnityEngine.Object behaviour,
            SerializedProperty property,
            string title,
            int max,
            int min,
            string helpText = "")
        {
            CkbEditorProperty.SliderDraw method = new CkbEditorProperty.SliderDraw()
            {
                Property = property,
                UseInt = true,
                Max = (float)max,
                Min = (float)min
            };
            CkbEditorProperty.DrawInternal(behaviour, title, (CkbEditorProperty.DrawMethod)method, helpText);
        }

        // 绘制浮动滑动条
        public static void DrawSlider(
            UnityEngine.Object behaviour,
            SerializedProperty property,
            string title,
            float max,
            float min,
            string helpText = "")
        {
            CkbEditorProperty.SliderDraw method = new CkbEditorProperty.SliderDraw()
            {
                Property = property,
                UseInt = false,
                Max = max,
                Min = min
            };
            CkbEditorProperty.DrawInternal(behaviour, title, (CkbEditorProperty.DrawMethod)method, helpText);
        }

   
        // 内部绘制方法，处理所有的绘制逻辑
        private static object DrawInternal(
            UnityEngine.Object behaviour,
            string title,
            CkbEditorProperty.DrawMethod method,
            string helpText,
            SerializedProperty property = null,
            bool helpButton = true)
        {
            if (helpText == "")
                helpText = method.ToolTip(); // 如果没有传入帮助文本，使用默认的 tooltip
            string str1 = behaviour.GetInstanceID().ToString();
            string str2 = title + str1;
            bool flag = CkbEditorToggleState.SelectedPropertyName == str2;
            Color backgroundColor = GUI.backgroundColor;
            Vector3 vector3 = new Vector3(103f, 122f, 138f);
            Color color = new Color(vector3.x / (float)byte.MaxValue, vector3.y / (float)byte.MaxValue, vector3.z / (float)byte.MaxValue);

            // 绘制横向布局
            EditorGUILayout.BeginHorizontal();
            if (helpButton)
            {
                EditorGUI.BeginDisabledGroup(helpText == "" || helpText == null);
                GUI.backgroundColor = !EditorGUIUtility.isProSkin ? (flag ? Color.grey : color) : (flag ? Color.grey : color);
                if (GUILayout.Button(flag ? "▼" : "?", GUILayout.Width(17f), GUILayout.Height(17f)))
                    CkbEditorToggleState.SelectedPropertyName = !flag ? str2 : "";
                GUI.backgroundColor = backgroundColor;
                EditorGUI.EndDisabledGroup();
            }

            // 绘制方法
            object obj = method.Draw(title);
            EditorGUILayout.EndHorizontal();

            // 绘制帮助文本
            if (!(CkbEditorToggleState.SelectedPropertyName == str2))
                return obj;

            // 设置样式并绘制帮助文本
            GUIStyle style = GUI.skin.GetStyle("HelpBox");
            style.richText = true;
            style.margin.bottom = 5;
            style.border.top = 15;
            style.border.bottom = 15;
            style.border.left = 15;
            style.border.right = 15;
            style.fontSize = 12;
            style.padding = new RectOffset(10, 10, 10, 10);
            EditorGUILayout.LabelField(helpText, style);
            return obj;
        }

        // 绘制方法的接口
        private interface DrawMethod
        {
            object Draw(string title);
            string ToolTip();
        }

        // 绘制标签的方法
        private struct LabelDraw : CkbEditorProperty.DrawMethod
        {
            public string Text;
            public string ToolTipText;
            public bool HasHelpButton;

            public object Draw(string title)
            {
                EditorGUILayout.LabelField(title, this.Text);
                return null;
            }

            public string ToolTip() => this.ToolTipText;
        }

        // 绘制属性的方法
        private struct PropertyDraw : CkbEditorProperty.DrawMethod
        {
            public SerializedProperty Property;

            public object Draw(string title)
            {
                EditorGUILayout.PropertyField(this.Property);
                return null;
            }

            public string ToolTip() => this.Property.tooltip;
        }

        // 绘制带标题的属性
        private struct PropertyDrawWithName : CkbEditorProperty.DrawMethod
        {
            public SerializedProperty Property;

            public object Draw(string title)
            {
                EditorGUILayout.PropertyField(this.Property, new GUIContent(title));
                return null;
            }

            public string ToolTip() => this.Property.tooltip;
        }

        // 绘制滑动条的方法
        private struct SliderDraw : CkbEditorProperty.DrawMethod
        {
            public bool UseInt;
            public float Max;
            public float Min;
            public SerializedProperty Property;

            public object Draw(string title)
            {
                if (!this.UseInt)
                    this.Property.floatValue = EditorGUILayout.Slider(title, this.Property.floatValue, this.Min, this.Max);
                else
                    this.Property.intValue = EditorGUILayout.IntSlider(title, this.Property.intValue, (int)this.Min, (int)this.Max);
                return null;
            }

            public string ToolTip() => this.Property.tooltip;
        }
    }
}
