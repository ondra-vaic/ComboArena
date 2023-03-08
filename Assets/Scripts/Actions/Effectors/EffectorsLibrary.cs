using System.Collections.Generic;
using UnityEngine;

namespace Actions.Effectors
{
    [CreateAssetMenu(fileName = "NewEffectorsLibrary", menuName = "ScriptableObjects/EffectorsLibrary", order = 1)]
    public class EffectorsLibrary : ScriptableObject
    {
        [SerializeField] private List<ActionEffectorPrototype> effectors;

        private readonly Dictionary<int, ActionEffectorPrototype> effectorsDictionary = new (); 
        
        public void Initialize()
        {
            foreach (var effector in effectors)
            {
                if (effectorsDictionary.ContainsKey(effector.ID))
                {
                    Debug.LogError("Duplicate effector id: " + effector.ID);
                }
                
                effectorsDictionary.Add(effector.ID, effector);
            }
        }
        
        public ActionEffectorPrototype GetEffectorPrototype(int id) => effectorsDictionary[id];
    }
}
