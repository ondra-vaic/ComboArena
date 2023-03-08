using System;
using System.Collections.Generic;
using UnityEngine;

namespace Actions
{
    [Serializable]
    [CreateAssetMenu(fileName = "NewCastActionsLibrary", menuName = "ScriptableObjects/CastActionsLibrary", order = 1)]
    public class CastActionsLibrary : ScriptableObject
    {
        [SerializeField] private List<CastActionPrototype> actions;
        
        [SerializeField] private float defaultEnqueueTime;

        public List<CastActionPrototype> ActionsList => actions;
        
        private readonly Dictionary<int, CastActionPrototype> effectorsDictionary = new();

        public void Initialize()
        {
            foreach (var action in actions)
            {
                if (effectorsDictionary.ContainsKey(action.ID))
                {
                    Debug.LogError("Duplicate action id: " + action.ID + " " + action.actionUIName);
                    Debug.LogError(effectorsDictionary[action.ID].actionUIName);
                }

                effectorsDictionary.Add(action.ID, action);
            }
        }
        
        public CastActionPrototype GetActionPrototype(int id) => effectorsDictionary[id];

        public CastActionPrototype GetActionPrototype(string name)
        {
            foreach (var action in actions)
            {
                if (action.name == name)
                    return action;
            }

            return null;
        }
    }
}