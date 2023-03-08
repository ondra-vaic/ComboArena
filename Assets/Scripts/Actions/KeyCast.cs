using System;
using UnityEngine;

namespace Actions
{
    [Serializable]
    public enum KeyType
    {
        Q,
        W,
        E,
        R,
        SPACE,
        MOUSE_R
    }

    [Serializable]
    public class KeyCast
    {
        [SerializeField] private float coolDown;
        private CastActionGroup castActionGroupPrototype;
        
        public ICastable CastPrototype => castActionGroupPrototype;
        public float CoolDown => coolDown;
        
        private float timeTriggered = float.NegativeInfinity;

        public CastActionGroup Cast(ICastable parent)
        {
            timeTriggered = Time.time;
            return (CastActionGroup) castActionGroupPrototype.Clone(parent);
        }
    
        public bool CanCast()
        {
#if UNITY_EDITOR
            return true;
#endif
            return Time.time - timeTriggered > coolDown;
        }

        public bool CanEnqueueNext(ICastable castable)
        {
            return castActionGroupPrototype.CanEnqueueNext(castable.GetLeafPriority());
        }

        public void SetCastActionGroup(CastActionGroup castActionGroup)
        {
            this.castActionGroupPrototype = castActionGroup;
        }
    }
}