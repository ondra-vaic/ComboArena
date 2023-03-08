using System;
using System.Collections.Generic;
using Actions.Effectors;
using Actions.Movement;
using UnityEngine;

namespace Actions
{
    [Serializable]
    public class CastAction : ICastable
    {
        private string name;
        private float toEndTime;
        private float toCancelTime;
        private float toEnqueueTime;
        private bool canEndWithLocation;
        private bool canEndWithTime;
        private MovementData movementData;
        private Queue<ActionEffectorPrototype> effectorsPrefabs;
        private int priority;
        
        private ICastable parent;
        private int animationNameHash;
        private float castMultiplier;
        
        private PlayerCastState startPlayerCastState = new(null, Vector3.zero);

        public CastAction(
            string name,
            float toEndTime,
            float toCancelTime,
            float toEnqueueTime,
            bool canEndWithLocation,
            bool canEndWithTime,
            MovementData movementData,
            Queue<ActionEffectorPrototype> effectorsPrefabs,
            string animationName,
            int priority,
            ICastable parent,
            float castMultiplier)
        {
            this.name = name;
            this.toEndTime = toEndTime;
            this.toCancelTime = toCancelTime;
            this.toEnqueueTime = toEnqueueTime;
            this.canEndWithLocation = canEndWithLocation;
            this.canEndWithTime = canEndWithTime;
            this.movementData = movementData;
            this.effectorsPrefabs = effectorsPrefabs;
            this.priority = priority;
            this.parent = parent;
            this.animationNameHash = Animator.StringToHash(animationName);
            this.castMultiplier = castMultiplier;
        }

        public void Cast(PlayerCastState startPlayerCastState)
        {
            this.startPlayerCastState = startPlayerCastState;
            movementData.Cast(startPlayerCastState);
        }

        public Queue<ActionEffectorPrototype> DequeueLeafEffectors()
        {
            Queue<ActionEffectorPrototype> currentEffectors = new Queue<ActionEffectorPrototype>();
            float runTime = timeRunning();
            
            foreach (ActionEffectorPrototype effector in effectorsPrefabs)
            {
                if (effector.CanSpawn(runTime, CalculateCastMultiplier(castMultiplier)))
                {
                    currentEffectors.Enqueue(effector);
                }
            }

            for (int i = 0; i < currentEffectors.Count; i++)
            {
                effectorsPrefabs.Dequeue();
            }
            
            return currentEffectors;
        }

        public MovementData GetLeafMovementData() => movementData;
        
        public int GetLeafAnimationNameHash() => animationNameHash;
        
        public float GetLeafCastMultiplier() => CalculateCastMultiplier(castMultiplier);
        
        public int GetLeafPriority() => priority;
        
        public ICastable GetCurrentLeaf() => this;
        
        public ICastable GetLeftMostChild() => this;
        
        public PlayerCastState GetLeafPlayerCastState() => startPlayerCastState;

        public ICastable GetLowestNonEmptyParent() => parent.GetLowestNonEmptyParent();
        
        public bool IsDone() => canEndWithTime && timeConditionMet(toEndTime) ||
                                canEndWithLocation && movementData.IsDone();
        
        public bool CanEnqueueNext(int nextCastPriority) => CanBeCanceled(nextCastPriority) || timeConditionMet(toEnqueueTime);

        public bool CanBeCanceled(int nextCastPriority) => IsDone() || (timeConditionMet(toCancelTime) && nextCastPriority >= priority);
        
        private bool timeConditionMet(float time) => time < timeRunning();

        private float timeRunning() => Time.time - startPlayerCastState.StartTime;

        public override string ToString()
        {
            return " name: " + name + ", startTime = " + startPlayerCastState.StartTime;
        }

        public static float CalculateCastMultiplier(float baseMultiplier) => 1 + 1.3f * Mathf.Log10(baseMultiplier);
    }
}