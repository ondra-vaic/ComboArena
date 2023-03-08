using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Actions.Movement
{
    public enum MoveType
    {
        None,
        Location,
        Direction,
        StandingCast,
        FlippedDirection
    }
    
    [Serializable]
    public class MovementData
    {
        [SerializeField] private MoveType moveType;
        [SerializeField] private float maxDistanceToTravel;
        [SerializeField] private AnimationCurve animationCurve;
        [SerializeField] private float speed = 0;
        [SerializeField] private bool affectsRotation;
        [SerializeField] private float rotationSpeed;
        
        private bool hasEndTime;
        private float timeToEnd;
        private PlayerCastState playerCastState;
        private float blockDistance = Mathf.Infinity;

        private float castMultiplier = 1;
        
        private static readonly float MIN_OBSTACLE_DISTANCE = 0.75f;
        private static readonly float DELTA = 0.01f;
        
        public MovementData Clone(bool hasEndTime, float timeToEnd, float castMultiplier) => new(){
                castMultiplier = castMultiplier,
                maxDistanceToTravel = maxDistanceToTravel,
                animationCurve = animationCurve,
                speed = speed,
                hasEndTime = hasEndTime,
                playerCastState = playerCastState,
                moveType = moveType,
                timeToEnd = timeToEnd,
                affectsRotation = affectsRotation,
                rotationSpeed = rotationSpeed,
        };

        public void Cast(PlayerCastState playerCastState)
        {
            this.playerCastState = playerCastState;
            
            if (moveType is MoveType.None or MoveType.StandingCast)
            {
                maxDistanceToTravel = 0;
                return;
            }
            
            if (moveType == MoveType.Location)
            {
                float distanceToTarget = Vector3.Distance(playerCastState.StartPlayerPosition, playerCastState.StartMousePosition);
                if (distanceToTarget < GetMaxDistanceToTravel())
                {
                    maxDistanceToTravel = distanceToTarget * castMultiplier;
                }
                return;
            }
            
            if (Physics.Raycast(playerCastState.Player.transform.position + Vector3.up, GetDirection(), out RaycastHit raycastHit, GetMaxDistanceToTravel(), LayerMask.GetMask("Default")))
            {
                if(raycastHit.distance - DELTA < GetMaxDistanceToTravel())
                {
                    blockDistance = (raycastHit.distance - DELTA);
                }
            }
        }

        public bool IsDone() => moveType is MoveType.None or MoveType.StandingCast ||
                                HasTraveledDistance(GetMaxDistanceToTravel()) ||
                                IsTimeReached() ||
                                IsBlocked();
        
        public bool HasTraveledDistance(float distance) => 
            Vector3.Distance(playerCastState.StartPlayerPosition, playerCastState.Player.transform.position) >=
            distance;

        public bool IsTimeReached() => GetDuration() < getTimeRunning();

        public bool IsBlocked() => 
            Physics.Raycast(
            playerCastState.Player.transform.position + Vector3.up,
            GetDirection(), out _, MIN_OBSTACLE_DISTANCE, LayerMask.GetMask("Default"));

        public float GetCurrentSpeedModifier()
        {
            float progress = GetProgress();
            return (animationCurve.Evaluate(progress + DELTA) - animationCurve.Evaluate(progress)) / DELTA;
        }
            
        public float GetProgress() => getTimeRunning() / GetDuration();
        
        public float GetDuration() => GetMaxDistanceToTravel() / GetSpeed();
        
        public Vector3 GetDirection() => GetClickDirection() * getDirectionSign();
        
        public Vector3 GetClickDirection() => playerCastState.StartDirectionForward;

        public float GetSpeed()
        {
            if (moveType is MoveType.StandingCast or MoveType.None)
            {
                return 0;
            }

            if (hasEndTime)
            {
                return GetMaxDistanceToTravel() / getTimeToEnd();
            }
            
            return speed;
        }

        public float GetRotationSpeed() => rotationSpeed / castMultiplier;

        public bool GetAffectsRotation() => affectsRotation;

        public bool GetAffectsPosition() => moveType is not (MoveType.None or MoveType.StandingCast);
        
        private float GetMaxDistanceToTravel() => maxDistanceToTravel / castMultiplier;
        
        public float GetMaxDistanceBlockingDistance() => Mathf.Min(GetMaxDistanceToTravel(), blockDistance / castMultiplier);

        private int getDirectionSign() => moveType == MoveType.FlippedDirection ? -1 : 1;
        
        private float getTimeRunning() => Time.time - playerCastState.StartTime;
        
        private float getTimeToEnd() => timeToEnd / castMultiplier;
    }
}