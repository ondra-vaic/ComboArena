using Actions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace CustomEditors
{
    [CustomEditor(typeof(CastActionsLibrary))]
    public class CastActionsLibraryCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor"))
            {
                CastActionsLibraryEditorWindow.Open(target as CastActionsLibrary);
            }
            base.OnInspectorGUI();
        }
        
        [OnOpenAsset]
        public static bool OpenEditor(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is CastActionsLibrary prototype)
            {
                CastActionsLibraryEditorWindow.Open(prototype);
                return true;
            }
            return false;
        }  
    }
}
