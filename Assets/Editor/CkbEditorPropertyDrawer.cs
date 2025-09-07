using UnityEditor;
using UnityEngine;
using CkbEditor;  // 引入CkbEditor命名空间

[CustomPropertyDrawer(typeof(FieldHelpAttribute))]
public class CkbEditorPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 获取自定义的 HelpText
        FieldHelpAttribute helpAttribute = (FieldHelpAttribute)attribute;

        CkbEditorProperty.DrawProperty(property.serializedObject.targetObject, property, helpAttribute.HelpText);
    /*    if (property.propertyType == SerializedPropertyType.Integer)
        {
            CkbEditorProperty.DrawIntSlider(property.serializedObject.targetObject, property, property.displayName, 100, 0, helpAttribute.HelpText);
        }
        else if (property.propertyType == SerializedPropertyType.Float)
        {
            CkbEditorProperty.DrawSlider(property.serializedObject.targetObject, property, property.displayName, 100f, 0f, helpAttribute.HelpText);
        }
        else
        {
            CkbEditorProperty.DrawProperty(property.serializedObject.targetObject, property, helpAttribute.HelpText);
        }*/
    }

}
