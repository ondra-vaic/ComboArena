using System.Collections.Generic;
using UnityEngine;

namespace Actions
{
    [CreateAssetMenu(fileName = "NewActionsLibrary", menuName = "ScriptableObjects/ActionsLibrary", order = 1)]
    public class ActionsLibrary : ScriptableObject
    {
        [SerializeField] private List<CastActionPrototype> actions;

        public List<CastActionPrototype> ActionsList => actions;
        
        private readonly Dictionary<int, CastActionPrototype> effectorsDictionary = new();

        public void Initialize()
        {
            foreach (var action in actions)
            {
                if (effectorsDictionary.ContainsKey(action.ID))
                {
                    Debug.LogError("Duplicate effector id: " + action.ID);
                }

                effectorsDictionary.Add(action.ID, action);
            }
        }

        public CastActionPrototype GetActionPrototype(int id) => effectorsDictionary[id];
    }
}
