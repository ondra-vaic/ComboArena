using System;
using Actions;
using Actions.Movement;
using Cinemachine.Editor;
using UnityEditor;
using UnityEngine;

namespace CustomEditors
{
    public class CastActionsLibraryEditorWindow : EditorWindow
    {
        protected SerializedObject libraryObject;
        protected SerializedProperty libraryActions;
        protected SerializedObject selectedObject;
        
        private GUIStyle largeBoldStyle;

        private GUIStyle smallBoldStyle;
        
        private GUIStyle smallStyle;
        
        private void OnEnable()
         {
             largeBoldStyle = new GUIStyle
             {
                 fontStyle = FontStyle.Bold,
                 fontSize = 16,
                 normal = { textColor = Color.white }
             };
             
             smallBoldStyle = new GUIStyle
             {
                 fontStyle = FontStyle.Bold,
                 fontSize = 14,
                 normal = { textColor = Color.white }
             };
             
             smallStyle = new GUIStyle
             {
                 fontStyle = FontStyle.Bold,
                 fontSize = 11,
                 normal = { textColor = Color.white }
             };
             
             Undo.undoRedoPerformed += OnUndoRedo; // subscribe to the event
         }

        public static void Open(CastActionsLibrary actionsLibrary)
        {
            CastActionsLibraryEditorWindow window = GetWindow<CastActionsLibraryEditorWindow>("Cast Actions Editor");
            window.libraryObject = new SerializedObject(actionsLibrary);
            window.libraryActions = window.libraryObject.FindProperty("actions");
            
            window.selectedObject = window.libraryActions.arraySize > 0 ? 
                new SerializedObject(window.libraryActions.GetArrayElementAtIndex(0).objectReferenceValue) :
                null;
        }
        
        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();

            if(libraryObject == null)
            {
                findOrCreateLibrary();
                return;
            }

            CastActionsLibrary castActionsLibrary = (CastActionsLibrary)libraryObject.targetObject;

            selectedObject ??= findFirstCastAction();
            
            EditorGUILayout.BeginHorizontal("box", GUILayout.ExpandHeight(true));

                EditorGUILayout.BeginHorizontal();
                
                    drawSidebar();

                    if(selectedObject != null)
                    {
                        EditorGUILayout.BeginVertical();
                            drawCastActionsTopBar();
                            EditorGUILayout.Separator();
                            drawCastActionEditor();
                        EditorGUILayout.EndVertical();
                    }

                EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndHorizontal();
            
            if (EditorGUI.EndChangeCheck())
            {
                selectedObject?.ApplyModifiedProperties();
                libraryObject?.ApplyModifiedProperties();
            }
        }

        private void drawCastActionsTopBar()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
            
                EditorGUILayout.LabelField("Library settings", largeBoldStyle, GUILayout.MaxWidth(250));
                EditorGUILayout.Separator();
                
                EditorGUI.indentLevel++;
                
                EditorGUI.BeginChangeCheck();
                drawField(libraryObject, "defaultEnqueueTime", "Default action enqueue time to end", 200);
                // createFloatField(libraryObject, "Default action enqueue time to end", "defaultEnqueueTime", 350, 50);

                if (EditorGUI.EndChangeCheck())
                {
                    foreach (SerializedProperty serializedProperty in libraryActions)
                    {
                        if(serializedProperty == null || serializedProperty.objectReferenceValue == null) continue;

                        SerializedObject currentObject = new SerializedObject(serializedProperty.objectReferenceValue);
                        if(currentObject.FindProperty("customEnqueuing").boolValue) continue;
                            
                        float defaultEnqueueTime = currentObject.FindProperty("toEndTime").floatValue - libraryObject.FindProperty("defaultEnqueueTime").floatValue;

                        currentObject.FindProperty("enqueueTimeToEnd").floatValue = defaultEnqueueTime;
                        currentObject.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
                EditorGUI.indentLevel--;
                
            EditorGUILayout.EndVertical();
        }

        private void drawSideTopBar()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.MaxHeight(30));
            
                GUILayout.BeginVertical();
                    GUILayout.FlexibleSpace();
                    
                    EditorGUILayout.LabelField("Cast Actions", largeBoldStyle, GUILayout.MaxWidth(100));
                    
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                    GUILayout.FlexibleSpace();
                        createCastAction();
                    GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        
        Vector2 scrollPos;


        private void createFloatField(SerializedObject serializedObject, string label, string propertyName, float width = 125, float textAreWidth = 55)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(0));
                EditorGUILayout.LabelField(label, GUILayout.Width(width - textAreWidth));
                GUILayout.FlexibleSpace();
                serializedObject.FindProperty(propertyName).floatValue = EditorGUILayout.FloatField(serializedObject.FindProperty(propertyName).floatValue, GUILayout.MinWidth(textAreWidth), GUILayout.MaxWidth(textAreWidth));
            EditorGUILayout.EndHorizontal();
        }
        
        private void createIntField(SerializedObject serializedObject, string label, string propertyName, float width = 125, float textAreWidth = 55)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(0));
                EditorGUILayout.LabelField(label, GUILayout.Width(width - textAreWidth));
                GUILayout.FlexibleSpace();
                serializedObject.FindProperty(propertyName).intValue = EditorGUILayout.IntField(serializedObject.FindProperty(propertyName).intValue, GUILayout.MinWidth(textAreWidth), GUILayout.MaxWidth(textAreWidth));
            EditorGUILayout.EndHorizontal();
        }
        
        private bool createBoolField(SerializedObject serializedObject, string label, string propertyName, GUIStyle style, float width = 125, float areaWidth = 30)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(0));
                if (style == null)
                {
                    EditorGUILayout.LabelField(label, GUILayout.Width(width - areaWidth));
                }
                else
                {
                    EditorGUILayout.LabelField(label, style,  GUILayout.Width(width - areaWidth));   
                }
                GUILayout.FlexibleSpace();
                serializedObject.FindProperty(propertyName).boolValue = EditorGUILayout.Toggle(serializedObject.FindProperty(propertyName).boolValue, GUILayout.MinWidth(areaWidth), GUILayout.MaxWidth(areaWidth));
            EditorGUILayout.EndHorizontal();
            
            return serializedObject.FindProperty(propertyName).boolValue;
        }

        private void drawEndConditions()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(355), GUILayout.MinWidth(355));
            
                EditorGUILayout.LabelField("End Conditions", smallBoldStyle);
                EditorGUILayout.Separator();

                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(GUILayout.Width(350));
                
                    createIntField(selectedObject, "Priority", "priority", 250, 80);
                    createBoolField(selectedObject, "Can finish reaching location", "canEndWithLocation", null, 250, 45);
                    EditorGUILayout.Separator();
                    
                    bool hasEndTime = createBoolField(selectedObject, "Can finish by reaching time", "hasEndTime", smallStyle, 250, 45);
                    if (hasEndTime)
                    {
                        EditorGUI.indentLevel++;
                        createFloatField(selectedObject, "Time to End", "toEndTime", 250, 100);
                        EditorGUI.indentLevel--;  
                    }
                    
                    EditorGUILayout.Separator();
                    
                    bool hasCancelTime = createBoolField(selectedObject, "Can be canceled", "hasCancelTime", smallStyle, 250, 45);
                    if (hasCancelTime)
                    {
                        EditorGUI.indentLevel++;
                        createFloatField(selectedObject, "Time to Cancel", "toCancelTime", 250, 100);
                        EditorGUI.indentLevel--;  
                    }
                    
                    float defaultEnqueueTime = selectedObject.FindProperty("toEndTime").floatValue - libraryObject.FindProperty("defaultEnqueueTime").floatValue;
                    bool customEnqueue = createBoolField(selectedObject, "Use custom enqueuing time", "customEnqueuing", smallStyle, 250, 45);
                        
                    EditorGUILayout.Separator();

                    if (!hasEndTime)
                    {
                        EditorGUI.indentLevel++;
                        createFloatField(selectedObject, "Time to Enqueue", "toEnqueueTime", 250, 100);
                        EditorGUI.indentLevel--;
                        selectedObject.FindProperty("customEnqueuing").boolValue = true;
                    }
                    else if (customEnqueue)
                    {
                        EditorGUI.indentLevel++;
                        createFloatField(selectedObject, "Enqueue time from end", "enqueueTimeToEnd", 350, 100);
                        EditorGUI.indentLevel--;
                        selectedObject.FindProperty("customEnqueuing").boolValue = true;
                    }
                    else
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(0));        
                            EditorGUILayout.LabelField("Enqueue time from end", GUILayout.Width(250));

                            GUILayout.FlexibleSpace();
                            
                            var prev = GUI.contentColor;
                            
                            GUI.contentColor = Color.gray;
                            EditorGUILayout.LabelField($"{defaultEnqueueTime:0.00}", EditorStyles.textField, GUILayout.Width(100));
                            GUI.contentColor = prev;
                            
                        EditorGUILayout.EndHorizontal();
                        selectedObject.FindProperty("enqueueTimeToEnd").floatValue = defaultEnqueueTime;
                        EditorGUI.indentLevel--;
                    }

                EditorGUILayout.Separator();    
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void createMovementDataMenu()
        {
            bool hasEndTime = selectedObject.FindProperty("hasEndTime").boolValue;
            bool canEndWithLocation = selectedObject.FindProperty("canEndWithLocation").boolValue;
           
            GUILayout.BeginVertical("box", GUILayout.MaxWidth(505), GUILayout.MinWidth(505));
                
                EditorGUILayout.LabelField("Movement", smallBoldStyle);
                EditorGUILayout.Separator();

                EditorGUI.indentLevel++;
                GUILayout.BeginVertical(GUILayout.Width(500));
                
                    var movementDataProperty = selectedObject.FindProperty("movementData");

                    MoveType movementType;
                    EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(0));
                            
                                EditorGUILayout.LabelField("Movement Type", smallStyle, GUILayout.MinWidth(150));
                                GUILayout.FlexibleSpace();
                                var movementTypeProperty = movementDataProperty.FindPropertyRelative("moveType");
                                movementType = (MoveType) EditorGUILayout.EnumPopup((MoveType) movementTypeProperty.intValue,
                                    GUILayout.MinWidth(100), GUILayout.MaxWidth(100));
                                movementTypeProperty.intValue = (int)movementType;
                                
                        EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    
                    
                    EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(0));
                        EditorGUILayout.LabelField("Affects rotation", smallStyle, GUILayout.MinWidth(150));
                        GUILayout.FlexibleSpace();
                        var affectsRotationProperty = movementDataProperty.FindPropertyRelative("affectsRotation");
                        bool affectsRotationSet = EditorGUILayout.Toggle(affectsRotationProperty.boolValue, GUILayout.MinWidth(45), GUILayout.MaxWidth(45));
                        affectsRotationProperty.boolValue = affectsRotationSet;
                    EditorGUILayout.EndHorizontal();
                    
                    if (affectsRotationSet)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(0));
                            EditorGUILayout.LabelField("Rotation Speed",GUILayout.MinWidth(150));
                            GUILayout.FlexibleSpace();
                            var rotationSpeedProperty = movementDataProperty.FindPropertyRelative("rotationSpeed");
                            rotationSpeedProperty.floatValue = EditorGUILayout.FloatField(rotationSpeedProperty.floatValue, GUILayout.MinWidth(100), GUILayout.MaxWidth(100));
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.indentLevel--;
                    }
                    
                    EditorGUILayout.Separator();

                    if (movementType != MoveType.StandingCast && movementType != MoveType.None)
                    {
                        float maxDistance = movementDataProperty.FindPropertyRelative("maxDistanceToTravel").floatValue;
                        float toEndTime = selectedObject.FindProperty("toEndTime").floatValue;
                        float speed = maxDistance / toEndTime;
                        var speedProperty = movementDataProperty.FindPropertyRelative("speed");

                        if (canEndWithLocation || hasEndTime)
                        {
                            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(0));
                                EditorGUILayout.LabelField("Max Distance", GUILayout.MinWidth(150));
                                GUILayout.FlexibleSpace();
                                    
                                movementDataProperty.FindPropertyRelative("maxDistanceToTravel").floatValue =
                                    EditorGUILayout.FloatField(movementDataProperty.FindPropertyRelative("maxDistanceToTravel").floatValue, 
                                        GUILayout.Width(100), GUILayout.MaxWidth(100));
                            EditorGUILayout.EndHorizontal();   
                        }

                        if (hasEndTime)
                        {
                            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(0));
                                 EditorGUILayout.LabelField("Speed", GUILayout.MinWidth(150));
                                 GUILayout.FlexibleSpace();
                                         
                                 var prev = GUI.contentColor;
                                 GUI.contentColor = Color.gray;
                                 EditorGUILayout.LabelField($"{speed:0.00}", EditorStyles.textField, GUILayout.Width(100));
                                 GUI.contentColor = prev;
                            EditorGUILayout.EndHorizontal();
                            speedProperty.floatValue = speed;
                        }
                        else
                        {
                            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(0));
                                EditorGUILayout.LabelField("Speed",GUILayout.MinWidth(150));
                                
                                GUILayout.FlexibleSpace();
                                speedProperty.floatValue = EditorGUILayout.FloatField(speedProperty.floatValue, GUILayout.MinWidth(100), GUILayout.MaxWidth(100));
                            EditorGUILayout.EndHorizontal();
                        }
                        
                        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(0));
                            EditorGUILayout.LabelField("Distance curve", GUILayout.MinWidth(150));
                            GUILayout.FlexibleSpace();
                            var movementCurveProperty = movementDataProperty.FindPropertyRelative("animationCurve");
                            movementCurveProperty.animationCurveValue = EditorGUILayout.CurveField(movementCurveProperty.animationCurveValue,  GUILayout.MinWidth(200), GUILayout.MaxWidth(200));
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                    
                GUILayout.EndVertical();
            GUILayout.EndVertical();
        }

        private void createInGameUIMenu()
        {
            GUILayout.BeginVertical("box", GUILayout.MaxWidth(505), GUILayout.MinWidth(505));   

                bool showInUI = createBoolField(selectedObject, "Show in UI", "showInUI", smallBoldStyle, 500, 40);
                EditorGUILayout.Separator();

                if (showInUI)
                {
                    GUILayout.BeginVertical(GUILayout.MaxWidth(500));
                        EditorGUI.indentLevel++;
                        GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Label", GUILayout.MinWidth(100), GUILayout.MaxWidth(100));
                            GUILayout.FlexibleSpace();
                            var uiName = selectedObject.FindProperty("actionUIName");
                            uiName.stringValue = EditorGUILayout.TextField(uiName.stringValue, GUILayout.MinWidth(150), GUILayout.MaxWidth(150));
                        GUILayout.EndHorizontal();
                        
                        EditorGUILayout.Separator();
                                        
                        GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Description", GUILayout.MinWidth(130), GUILayout.MaxWidth(130));
                            GUILayout.FlexibleSpace();
                            
                            var descriptionProperty = selectedObject.FindProperty("actionUIDescription");
                            descriptionProperty.stringValue = EditorGUILayout.TextArea(descriptionProperty.stringValue, GUILayout.MinWidth(300), GUILayout.MaxWidth(300), GUILayout.MinHeight(60));
                        GUILayout.EndHorizontal();
                        
                        EditorGUILayout.Separator();
                        
                        GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Color", GUILayout.MinWidth(130), GUILayout.MaxWidth(130));
                            GUILayout.FlexibleSpace();
                                
                            var colorProperty = selectedObject.FindProperty("actionUIColor");
                            colorProperty.colorValue = EditorGUILayout.ColorField(colorProperty.colorValue, GUILayout.MinWidth(300), GUILayout.MaxWidth(300));
                        GUILayout.EndHorizontal();
                        
                        EditorGUILayout.Separator();
                        EditorGUI.indentLevel--;
                    GUILayout.EndVertical();
                }
            EditorGUILayout.EndVertical();  
        }
        
        private void createEffectorsMenu()
        {
            GUILayout.BeginVertical("box", GUILayout.MaxWidth(505), GUILayout.MinWidth(505));   
                GUILayout.BeginVertical("box", GUILayout.MaxWidth(500), GUILayout.MinWidth(500));   

                    EditorGUILayout.LabelField("Effectors", smallBoldStyle);
                    EditorGUILayout.Separator();

                    EditorGUI.indentLevel++;
                    drawField(selectedObject, "effectorsPrototypes");
                    EditorGUI.indentLevel--;
                GUILayout.EndVertical();
            EditorGUILayout.EndVertical();  
        }

        private void drawCastActionEditor()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(400));
                EditorGUILayout.Separator();
            
                EditorGUILayout.LabelField(selectedObject.targetObject.name, largeBoldStyle, GUILayout.MaxWidth(175));
                EditorGUILayout.Space();
                EditorGUILayout.Separator();
                
                if (selectedObject != null)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical();
                        drawEndConditions();
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Separator(); 
                    
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical();
                        createMovementDataMenu();
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Separator();
                    
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical();
                        createInGameUIMenu();
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                    
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical();
                        EditorGUILayout.Separator();
                        createEffectorsMenu();
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                    
                    EditorGUILayout.Separator();
                }

                GUILayout.FlexibleSpace();

                drawDefaultArray();
                    
            EditorGUILayout.EndVertical();
        }

        private void drawDefaultArray()
        {
            EditorGUILayout.BeginVertical(GUILayout.MaxHeight(180));
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(520));

                    EditorGUILayout.BeginHorizontal();
                                    
                        EditorGUILayout.LabelField("Cast action array", largeBoldStyle);
                        GUILayout.FlexibleSpace();
                        drawFindAllCastActionsButton();
                                    
                    EditorGUILayout.EndHorizontal();
                                
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(500));
                    EditorGUILayout.PropertyField(libraryObject.FindProperty("actions"), true, GUILayout.MinWidth(420));
                    EditorGUILayout.EndScrollView();
                                
                    EditorGUILayout.Separator();
                EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private void drawFindAllCastActionsButton()
        {
            if (GUILayout.Button("Find all cast actions", GUILayout.Width(150)))
            {
                libraryActions.ClearArray();
                
                string[] castActionsIds = AssetDatabase.FindAssets("t:CastActionPrototype");
                foreach (var castActionsId in castActionsIds)
                {
                    string path = AssetDatabase.GUIDToAssetPath(castActionsId);
                    CastActionPrototype castActionPrototype = AssetDatabase.LoadAssetAtPath<CastActionPrototype>(path);
                    
                    libraryActions.InsertArrayElementAtIndex(libraryActions.arraySize);
                    libraryActions.GetArrayElementAtIndex(libraryActions.arraySize - 1).objectReferenceValue = castActionPrototype;
                }
                
                selectedObject = libraryActions.arraySize > 1 ? new SerializedObject(libraryActions.GetArrayElementAtIndex(0).objectReferenceValue) : null;
            }
        }
        
        private void drawSideElements()
        {
            EditorGUILayout.BeginVertical();
            
                GUI.color = Color.white;
                GUI.skin.button.alignment = TextAnchor.MiddleLeft;

                bool foundSelected = false;
                bool clicked = false;
                
                foreach (SerializedProperty castActionProperty in libraryActions)
                {
                    if(castActionProperty == null || castActionProperty.objectReferenceValue == null) continue;
                    
                    bool isSelected = castActionProperty.objectReferenceValue == selectedObject.targetObject;
                    GUI.color = isSelected ? Color.white : Color.gray;
                    foundSelected |= isSelected;

                    if (GUILayout.Button(castActionProperty.objectReferenceValue.name))
                    {
                        selectedObject = new SerializedObject(castActionProperty.objectReferenceValue);
                        GUI.FocusControl(null);
                        Undo.ClearAll();
                        
                        clicked = true;
                        break;
                    }
                }

                if (!foundSelected && !clicked)
                {
                    selectedObject = findLastCastAction();
                }
            
                GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                GUI.color = Color.white;
            EditorGUILayout.EndVertical();
        }

        protected void drawSidebar()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));

                drawSideTopBar();
                drawSideElements();
            
            EditorGUILayout.EndVertical();
        }

        protected void drawField(SerializedObject actionSerializedObject, string propertyName, string label = "" , float maxWidth = Single.MaxValue)
        {
            if(actionSerializedObject == null) return;
            
            EditorGUILayout.PropertyField(actionSerializedObject.FindProperty(propertyName), true,  GUILayout.MaxWidth(maxWidth));
        }

        void OnUndoRedo()
        {
            if(selectedObject == null) return;
            
            selectedObject = new SerializedObject(selectedObject.targetObject);
            GUI.FocusControl(null);
            Repaint();
        }
        
        private void findOrCreateLibrary()
        {
            string[] librariesIds = AssetDatabase.FindAssets("t:CastActionsLibrary");
            if(librariesIds.Length > 0 && Event.current.type != EventType.Layout)
            {
                string libraryPath = AssetDatabase.GUIDToAssetPath(librariesIds[0]);
                CastActionsLibrary library = AssetDatabase.LoadAssetAtPath<CastActionsLibrary>(libraryPath);
                Open(library);
                return;
            }
            
            if(librariesIds.Length == 0)
            {
                EditorGUILayout.BeginVertical();
                
                    EditorGUILayout.LabelField("No Actions Library found, create one in" + AssetDatabase.GenerateUniqueAssetPath("Assets/ScriptableObjects/Libraries/NewCastActionsLibrary.asset"));
                    if (GUILayout.Button("Create Library"))
                    {
                        CastActionsLibrary library = CreateInstance<CastActionsLibrary>();
                        AssetDatabase.CreateAsset(library, AssetDatabase.GenerateUniqueAssetPath("Assets/ScriptableObjects/Libraries/NewCastActionsLibrary.asset"));
                        AssetDatabase.SaveAssets();
                        Open(library);
                    }
                    
                EditorGUILayout.EndVertical();
            }
        }
        
        private void createCastAction()
        {
            if (GUILayout.Button("+"))
            {
                CastActionPrototype castActionPrototype = CreateInstance<CastActionPrototype>();
                AssetDatabase.CreateAsset(castActionPrototype, AssetDatabase.GenerateUniqueAssetPath("Assets/ScriptableObjects/Actions/NewCastActionPrototype.asset"));
                AssetDatabase.SaveAssets();
                
                libraryActions.InsertArrayElementAtIndex(libraryActions.arraySize);
                libraryActions.GetArrayElementAtIndex(libraryActions.arraySize - 1).objectReferenceValue = castActionPrototype;
                selectedObject = new SerializedObject(libraryActions.GetArrayElementAtIndex(libraryActions.arraySize - 1).objectReferenceValue);
            }
        }

        private SerializedObject findFirstCastAction()
        {
            if (libraryActions == null) return null;
            
            return libraryActions.arraySize > 0 ? new SerializedObject(libraryActions.GetArrayElementAtIndex(0).objectReferenceValue) : null;
        }
        
        private SerializedObject findLastCastAction()
        {
            if (libraryActions == null) return null;
            
            return libraryActions.arraySize > 0 ? new SerializedObject(libraryActions.GetArrayElementAtIndex(libraryActions.arraySize - 1).objectReferenceValue) : null;
        }
    }
}


