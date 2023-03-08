using System.Collections.Generic;
using Actions.Effectors;
using Actions.Movement;
using Networking;
using UnityEngine;

namespace Actions
{
    public interface ICastable
    {
        public MovementData GetLeafMovementData();
        public int GetLeafAnimationNameHash();
        public int GetLeafPriority();
        public void Cast(PlayerCastState startPlayerCastState);
        public bool IsDone();
        Queue<ActionEffectorPrototype> DequeueLeafEffectors();
        bool CanEnqueueNext(int nextCastPriority);
        bool CanBeCanceled(int nextCastPriority);
        public float GetLeafCastMultiplier();
        ICastable GetCurrentLeaf();
        ICastable GetLowestNonEmptyParent();
        ICastable GetLeftMostChild();
        PlayerCastState GetLeafPlayerCastState();
    }

    public class PlayerCastState
    {
        public Vector3 StartPlayerPosition { get; }
        public Vector3 StartDirectionForward { get; }
        public Vector3 StartMousePosition { get; }
        public float StartTime { get; }
        public Player.Player Player { get; }
    
        private static readonly float THRESHOLD = 0.5f;

        public PlayerCastState(Player.Player player, Vector3 startMousePosition)
        {
            StartPlayerPosition = player != null ? player.transform.position : Vector3.zero;
            StartMousePosition = startMousePosition;
            Player = player;
        
            Vector3 toTarget = startMousePosition - StartPlayerPosition;
            if(player != null)
                StartDirectionForward = toTarget.magnitude > THRESHOLD ? toTarget.normalized : player.transform.forward;
            else
                StartDirectionForward = Vector3.forward;
        }
        
        public PlayerCastState(Vector3 startPlayerPosition, Vector3 startDirectionForward, Vector3 startMousePosition, float startTime, Player.Player player)
        {
            StartPlayerPosition = startPlayerPosition;
            StartDirectionForward = startDirectionForward;
            StartMousePosition = startMousePosition;
            StartTime = startTime;
            Player = player;
        }

        public PlayerCastStateNetworkData ToNetworkData(ulong playerId) => 
            new (StartPlayerPosition, StartDirectionForward, StartMousePosition, playerId);
    }
}