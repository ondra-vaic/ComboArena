using System;
using UnityEngine;

namespace Actions.Effectors
{
    [Serializable]
    [CreateAssetMenu(fileName = "NewActionEffectorPrototype", menuName = "ScriptableObjects/ActionEffectorPrototype", order = 1)]
    public class ActionEffectorPrototype : ScriptableObject
    {
        [SerializeField] private GameObject actionEffector;
        [SerializeField] private float relativeTimeToSpawn;
        [SerializeField] private int id;

        public int ID => id;
        
        public bool CanSpawn(float time, float castMultiplier) => time > relativeTimeToSpawn / castMultiplier;
    
        public GameObject GetPrefab() => actionEffector;
    }
}
