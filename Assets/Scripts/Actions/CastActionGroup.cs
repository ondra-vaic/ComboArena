using System;
using System.Collections.Generic;
using System.Linq;
using Actions.Effectors;
using Actions.Movement;
using UnityEngine;

namespace Actions
{
    [Serializable]
    public class CastActionGroup : ICastable
    {
        [SerializeField] private List<CastActionGroup> castGroupPrototypes = new();
        [SerializeField] private CastActionPrototype leafCastablePrototype;
        
        private ICastable parent;
        private Queue<ICastable> castables;
        private ICastable currentCastable;
        private PlayerCastState startPlayerCastState;
        private int castMultiplier = 1;

        public void Cast(PlayerCastState startPlayerCastState)
        {
            this.startPlayerCastState = startPlayerCastState;
            currentCastable = castables.Dequeue();
            currentCastable.Cast(startPlayerCastState);
        }
        
        public void Enqueue(CastActionGroup castable) => castables.Enqueue(castable);
        
        public bool TryChangeCast(PlayerCastState playerCastState)
        {
            ICastable currentLeaf = GetCurrentLeaf();
            ICastable sharedParent = currentLeaf?.GetLowestNonEmptyParent() ?? this;
            ICastable nextLeaf = sharedParent.GetLeftMostChild();
            
            if (nextLeaf == null) return false;
            if (!CanBeCanceled(nextLeaf.GetLeafPriority())) return false;
            
            sharedParent.Cast(playerCastState);            
            return true;
        }
        
        public ICastable Clone(ICastable parent) =>
            new CastActionGroup(
                castGroupPrototypes,
                leafCastablePrototype,
                parent,
                castMultiplier);

        public CastActionGroup(){}
        
        public CastActionGroup(
            List<CastActionGroup> castGroupPrototypes,
            CastActionPrototype leafCastablePrototype,
            ICastable parent,
            int castMultiplier)
        {
            Initialize(castGroupPrototypes, leafCastablePrototype, parent, castMultiplier);
        }

        public ICastable GetCurrentLeaf() => currentCastable?.GetCurrentLeaf();
        
        public Queue<ActionEffectorPrototype> DequeueLeafEffectors() => currentCastable?.DequeueLeafEffectors();
        
        public MovementData GetLeafMovementData() => currentCastable?.GetLeafMovementData();
        
        public int GetLeafAnimationNameHash() => currentCastable?.GetLeafAnimationNameHash() ?? -1;
        
        public int GetLeafPriority() => currentCastable?.GetLeafPriority() ?? -1;
        
        public float GetLeafCastMultiplier() => currentCastable?.GetLeafCastMultiplier() ?? 1;
        
        public bool CanBeCanceled(int nextCastPriority) => currentCastable?.CanBeCanceled(nextCastPriority) ?? true;
        
        public ICastable GetLowestNonEmptyParent() => castables.Any() ? this : parent?.GetLowestNonEmptyParent();
        
        public ICastable GetLeftMostChild() => castables.FirstOrDefault()?.GetLeftMostChild();
        
        public PlayerCastState GetLeafPlayerCastState() => currentCastable?.GetLeafPlayerCastState();

        public bool IsDone() => !castables.Any() && (currentCastable?.IsDone() ?? true);
        
        public bool CanEnqueueNext(int nextCastPriority) => !castables.Any() && (currentCastable?.CanEnqueueNext(nextCastPriority) ?? true);
        
        public override string ToString()
        {
            return "startTime = " + startPlayerCastState.StartTime;
        }

        private bool isLeaf() => !castGroupPrototypes.Any() && !isRoot();
        
        private bool isRoot() => parent == null;

        public void Initialize(List<CastActionGroup> castGroupPrototypes, CastActionPrototype leafCastablePrototype, ICastable parent, int castMultiplier)
        {
            this.castGroupPrototypes = castGroupPrototypes;
            this.leafCastablePrototype = leafCastablePrototype;
            this.parent = parent;
            this.castables = new Queue<ICastable>();
            this.castMultiplier = castMultiplier;
            
            if(isLeaf())
            {
                castables.Enqueue(leafCastablePrototype.Create(this, castMultiplier));
            }
            
            foreach (var castable in castGroupPrototypes)
            {
                castables.Enqueue(castable.Clone(this));
            }
        }
    }
}