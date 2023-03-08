using UnityEditor;

namespace CustomEditors
{
    public class ExtendedEditorWindow : EditorWindow
    {

        protected void drawProperties(SerializedProperty property, bool drawChildren)
        {
            string lastPropertyPath = string.Empty;
            foreach (SerializedProperty p in property)
            {
                if (p.isArray && p.propertyType == SerializedPropertyType.Generic)
                {
                    EditorGUILayout.BeginHorizontal();
                    p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.displayName);
                    EditorGUILayout.EndHorizontal();

                    if (p.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        drawProperties(p, drawChildren);
                        EditorGUI.indentLevel--;
                    }
                }
                else
                {
                    if(!string.IsNullOrEmpty(lastPropertyPath) && p.propertyPath.Contains(lastPropertyPath)) continue;
                    
                    lastPropertyPath = p.propertyPath;
                    EditorGUILayout.PropertyField(p, drawChildren);
                }
            }
        }
    }
}
