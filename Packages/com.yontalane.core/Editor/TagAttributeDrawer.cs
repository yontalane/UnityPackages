using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using Yontalane;

namespace YontalaneEditor
{
    [CustomPropertyDrawer(typeof(TagAttribute))]
    internal class TagAttributeDrawer : PropertyDrawer
    {
        #region Styles
        private static GUIStyle m_warningStyle = null;
        private static GUIStyle WarningStyle
        {
            get
            {
                if (m_warningStyle == null)
                {
                    m_warningStyle = new GUIStyle();
                    m_warningStyle.normal.textColor = Color.red;
                    m_warningStyle.alignment = TextAnchor.MiddleRight;

                }
                return m_warningStyle;
            }
        }
        #endregion

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, property);

            // if incorrectly placed
            if (property.propertyType != SerializedPropertyType.Integer && property.propertyType != SerializedPropertyType.String)
            {
                Rect leftRect = new Rect(position.x, position.y, position.width * 0.5f, position.height);
                Rect rightRect = new Rect(position.x + position.width * 0.5f, position.y, position.width * 0.5f, position.height);

                EditorGUI.PrefixLabel(leftRect, label);
                EditorGUI.LabelField(rightRect, new GUIContent($"Incorrect Attribute", "Only use LayerAttribute on an integer or a string."), WarningStyle);
                return;
            }

            // positions
            Rect labelRect = new Rect(position.x, position.y, position.width * 0.5f, position.height);
            Rect menuRect = new Rect(position.x + position.width * 0.5f, position.y, position.width * 0.5f, position.height);

            EditorGUI.PrefixLabel(labelRect, label);
            EditorGUI.BeginChangeCheck();

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                int value = EditorGUI.Popup(menuRect, property.intValue, InternalEditorUtility.tags);
                if (EditorGUI.EndChangeCheck())
                {
                    property.intValue = value;
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
            }
            else if (property.propertyType == SerializedPropertyType.String)
            {
                string value = property.stringValue;
                int index = -1;
                for (int i = 0; i < InternalEditorUtility.tags.Length; i++)
                {
                    if (InternalEditorUtility.tags[i] == value)
                    {
                        index = i;
                        break;
                    }
                }
                index = EditorGUI.Popup(menuRect, index, InternalEditorUtility.tags);
                if (EditorGUI.EndChangeCheck())
                {
                    if (index == -1)
                    {
                        property.stringValue = InternalEditorUtility.tags[0];
                    }
                    else
                    {
                        property.stringValue = InternalEditorUtility.tags[index];
                    }
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
    }
}
