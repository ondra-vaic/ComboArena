using System;
using System.Collections.Generic;
using Actions.Effectors;
using Actions.Movement;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
namespace Actions
{
    [Serializable]
    [CreateAssetMenu(fileName = "NewCastActionPrototype", menuName = "ScriptableObjects/CastActionPrototype", order = 1)]
    public class CastActionPrototype : ScriptableObject
    {
        [SerializeField] private float toEndTime;
        [SerializeField] private float toCancelTime;
        [SerializeField] private bool customEnqueuing;
        [SerializeField] private float toEnqueueTime;
        [SerializeField] private float enqueueTimeToEnd;
        [SerializeField] private MovementData movementData;
        [SerializeField] private List<ActionEffectorPrototype> effectorsPrototypes;
        [SerializeField] private int priority = 1;
        [SerializeField] public string actionUIName;
        [SerializeField] private string actionUIDescription;
        [SerializeField] private Color actionUIColor;
        [SerializeField] private bool showInUI = true;
        [SerializeField] private bool canEndWithLocation = true;
        [SerializeField] private bool hasEndTime = true;
        [SerializeField] private bool hasCancelTime = true;
        
        [SerializeField] private string actionName = "";

        public string ActionUIName => actionUIName;
        public string ActionUIDescription => actionUIDescription;
        public Color ActionUIColor => actionUIColor;
        public int ID => castId.GetHashCode();
        public bool ShowInUI => showInUI;

        [ReadOnly] [SerializeField] private string castId;

        public CastAction Create(ICastable parent, float castMultiplier) => new(
            actionName + " clone",
            calculateToEndTime() / CastAction.CalculateCastMultiplier(castMultiplier),
            calculateToCancelTime() / CastAction.CalculateCastMultiplier(castMultiplier),
            calculateToEnqueueTime(castMultiplier),
            canEndWithLocation,
            hasEndTime,
            movementData.Clone(hasEndTime, toEndTime, CastAction.CalculateCastMultiplier(castMultiplier)),
            createEffectorsQueue(),
            actionName == "" ? name : actionName,
            priority,
            parent,
            castMultiplier);

        private float calculateToEndTime() => hasEndTime ? toEndTime : float.MaxValue;

        private float calculateToEnqueueTime(float castMultiplier)
        {
            if (!hasEndTime)
            {
                return toEnqueueTime / CastAction.CalculateCastMultiplier(castMultiplier);
            }
            
            return toEndTime / CastAction.CalculateCastMultiplier(castMultiplier) - enqueueTimeToEnd;
        }

        private float calculateToCancelTime() => hasCancelTime ? toCancelTime : calculateToEndTime();


        private Queue<ActionEffectorPrototype> createEffectorsQueue()
        {
            return new Queue<ActionEffectorPrototype>(effectorsPrototypes);
        }
        
        
        private void OnValidate()
        {
#if UNITY_EDITOR
            if (castId == "")
            {
                castId = GUID.Generate().ToString();
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }
    }
}
