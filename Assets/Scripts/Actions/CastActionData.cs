using System;
using System.Collections.Generic;
using Actions.Effectors;
using Actions.Movement;
using UnityEngine;

namespace Actions
{
    // [Serializable]
    // public class CastActionData
    // {
    //     [SerializeField] public float toEndTime;
    //     [SerializeField] private float toCancelTime;
    //     [SerializeField] private float toEnqueueTime = -1;
    //     [SerializeField] private EndStateType endStateType;
    //     [SerializeField] private MovementData movementData;
    //     [SerializeField] private List<ActionEffectorPrototype> effectorsPrototypes;
    //     [SerializeField] private int priority = 1;
    //     [SerializeField] private string actionUIName;
    //     [SerializeField] private string actionUIDescription;
    //     [SerializeField] private Color actionUIColor;
    //     [SerializeField] private int id;
    //     [SerializeField] private bool showInUI = true;
    //     [SerializeField] private string name = "";
    //
    //     public string ActionUIName => actionUIName;
    //     public string ActionUIDescription => actionUIDescription;
    //     public Color ActionUIColor => actionUIColor;
    //     public int ID => id;
    //     public bool ShowInUI => showInUI;
    //
    //     public CastAction Create(ICastable parent, float castMultiplier) => new(
    //         name + " clone",
    //         toEndTime / castMultiplier,
    //         toCancelTime / castMultiplier,
    //         toEnqueueTime,
    //         true,
    //         movementData.Clone(toEndTime, castMultiplier),
    //         createEffectorsQueue(),
    //         name,
    //         priority,
    //         parent,
    //         castMultiplier);
    //
    //     private Queue<ActionEffectorPrototype> createEffectorsQueue()
    //     {
    //         return new Queue<ActionEffectorPrototype>(effectorsPrototypes);
    //     }
    // }
}
