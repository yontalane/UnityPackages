using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace YontalaneEditor.UIElements
{
    public abstract class MenuPropertyDrawerUIE : PropertyDrawer
    {
        #region Constants
        internal const string STYLE_SHEET = "Menu";
        private const string EXPANDED_CLASS = "expanded";
        #endregion

        #region Private Variables
        private StyleSheet m_styleSheet;
        private VisualElement m_root;
        private Button m_header;
        private VisualElement m_container;
        private SerializedProperty m_expanded;
        #endregion

        #region Accessors
        private bool Expanded
        {
            get
            {
                return m_expanded.boolValue;
            }
            set
            {
                if (value)
                {
                    m_header.text = $"+ {HeaderText}";
                    m_root.AddToClassList(EXPANDED_CLASS);
                }
                else
                {
                    m_header.text = $"- {HeaderText}";
                    m_root.RemoveFromClassList(EXPANDED_CLASS);
                }
                m_expanded.boolValue = value;
                m_expanded.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        protected abstract string HeaderText { get; }
        #endregion

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            m_expanded = property.FindPropertyRelative("editorExpanded");

            m_root = new() { name = HeaderText.Replace(" ", string.Empty) };
            m_header = new() { name = "Header" };
            m_container = new() { name = "Container" };

            m_root.AddToClassList("root");

            if (m_styleSheet == null)
            {
                m_styleSheet = Resources.Load<StyleSheet>(STYLE_SHEET);
            }
            m_root.styleSheets.Add(m_styleSheet);

            m_header.clicked += () =>
            {
                Expanded = !Expanded;
            };
            Expanded = Expanded;

            MenuGUI(property, m_container);

            m_root.Add(m_header);
            m_root.Add(m_container);

            return m_root;
        }

        protected abstract void MenuGUI(SerializedProperty property, VisualElement container);
    }
}
