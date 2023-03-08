using Actions;
using Unity.Netcode;
using UnityEngine;

namespace Networking
{
    public struct PlayerCastStateNetworkData : INetworkSerializable
    {
        public Vector3 StartPlayerPosition;
        public Vector3 StartDirectionForward;
        public Vector3 StartMousePosition;
        public ulong PlayerId;

        public PlayerCastStateNetworkData(Vector3 startPlayerPosition, Vector3 startDirectionForward, Vector3 startMousePosition, ulong playerId)
        {
            StartPlayerPosition = startPlayerPosition;
            StartDirectionForward = startDirectionForward;
            StartMousePosition = startMousePosition;
            PlayerId = playerId;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref StartPlayerPosition);
            serializer.SerializeValue(ref StartDirectionForward);
            serializer.SerializeValue(ref StartMousePosition);
            serializer.SerializeValue(ref PlayerId);
        }

        public PlayerCastState ToPlayerCastState(Player.Player player) => 
            new (StartPlayerPosition, StartDirectionForward, StartMousePosition, Time.time, player);
    }
}
