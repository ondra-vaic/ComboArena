using System.Collections.Generic;
using Actions;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Networking
{
    public class PlayerManager : NetworkBehaviour
    {
        [SerializeField] private Vector3 blueTeamStartPoint;
        [SerializeField] private Vector3 blueTeamStartForward;
        [SerializeField] private Vector3 redTeamStartPoint;
        [SerializeField] private Vector3 redTeamStartForward;
        
        public UnityEvent<ulong> OnPlayerAdded = new();
        public UnityEvent<ulong> OnBeforePlayerRemoved = new();
        public UnityEvent OnLocalPlayerClientIdSet = new();
        public UnityEvent OnLocalPlayerInitialized = new();
        public UnityEvent OnAllPlayersInitialized = new();
        public UnityEvent OnRematchSessionStart = new();
        public UnityEvent<bool> OnGameEnded = new();

        private MatchMaker matchMaker;

        private Team previouslyGivenTeam = Team.Red;
        private readonly Dictionary<ulong, Player.Player> players = new();
        private ulong clientId = ulong.MaxValue;
        private bool isLocalPlayerInitialized;

        private int numInitializedPlayers = 0;
        private bool isBattleOn = false;
        private int numPlayersWantingRematch = 0;

        public bool IsBattleOn => isBattleOn;

        private void Awake()
        {
            matchMaker = FindObjectOfType<MatchMaker>();
        }

        public void SetTeam(Player.Player player)
        {
            previouslyGivenTeam = previouslyGivenTeam == Team.Blue ? Team.Red : Team.Blue;
            player.SetTeam(previouslyGivenTeam);
            
            setPlayerPosition(player);
        }

        private void setPlayerPosition(Player.Player player)
        {
            if (player.GetTeam() == Team.Blue)
            {
                player.SetStartTransform(blueTeamStartPoint, blueTeamStartForward);
            }
            else
            {
                player.SetStartTransform(redTeamStartPoint, redTeamStartForward);
            }
        }

        public void AddPlayer(ulong playerId, Player.Player player)
        {
            players.Add(playerId, player);
            OnPlayerAdded.Invoke(playerId);
        }

        public void RemovePlayer(ulong playerId)
        {
            OnBeforePlayerRemoved.Invoke(playerId);
            players.Remove(playerId);
        }
        
        public void SetClientId(ulong clientId)
        {
            this.clientId = clientId;
            OnLocalPlayerClientIdSet.Invoke();
        }

        public Player.Player GetPlayer(ulong playerId) => players[playerId];
        
        public Player.Player GetMyPlayer() => players[clientId];
        
        public Team GetMyTeam() => GetMyPlayer().GetTeam();
        
        public bool IsLocalPlayerInitialized() => isLocalPlayerInitialized;
        
        public Dictionary<ulong, Player.Player> GetPlayers() => players;

        [ServerRpc(RequireOwnership = false)]
        public void setPlayerActionGroupServerRpc(KeyType key, ActionGroupNetworkData actionGroupNetworkData, ulong senderId)
        {
            Player.Player player = players[senderId];
            player.SetActionGroup(key, actionGroupNetworkData);
        }
        
        public void SetPlayerActionGroup(KeyType key, ActionGroupNetworkData actionGroupNetworkData)
        {
            setPlayerActionGroupServerRpc(key, actionGroupNetworkData, clientId);
        }

        public void SetLocalPlayerInitialized() => setPlayerInitializedServerRpc(clientId);

        [ServerRpc(RequireOwnership = false)]
        public void setPlayerInitializedServerRpc(ulong senderId)
        {
            ClientRpcParams rpcParams = new ClientRpcParams();
            rpcParams.Send.TargetClientIds = new[] { senderId };
            setLocalPlayerInitializedClientRpc(rpcParams);

            numInitializedPlayers++;
            if (numInitializedPlayers == matchMaker.MaxPlayers)
            {
                onAllPlayersInitializedClientRpc();
                isBattleOn = true;
            }
        }

        [ClientRpc]
        private void setLocalPlayerInitializedClientRpc(ClientRpcParams rpcParams = default)
        {
            isLocalPlayerInitialized = true;
            OnLocalPlayerInitialized.Invoke();
        }
        
        [ClientRpc]
        private void onAllPlayersInitializedClientRpc()
        {
            OnAllPlayersInitialized.Invoke();
        }

        public void OnPlayerDeath(Player.Player player)
        {
            isBattleOn = false;
            foreach (var idPlayerPair in players)
            {
                ClientRpcParams rpcParams = new ClientRpcParams();
                rpcParams.Send.TargetClientIds = new[] { idPlayerPair.Key };

                bool isWin = player.GetTeam() != idPlayerPair.Value.GetTeam();
                onPlayerDeathClientRpc(isWin, rpcParams);
            }
        }

        [ClientRpc]
        private void onPlayerDeathClientRpc(bool isWin, ClientRpcParams rpcParams = default)
        {
            isBattleOn = false;
            isLocalPlayerInitialized = false;
            OnGameEnded.Invoke(isWin);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestRematchServerRpc()
        {
            numPlayersWantingRematch++;
            if(numPlayersWantingRematch == matchMaker.MaxPlayers)
            {
                initializeRematch();
            }
        }

        private void initializeRematch()
        {
            numInitializedPlayers = 0;
            numPlayersWantingRematch = 0;
            isBattleOn = false;
            
            foreach (var idPlayerPair in players)
            {
                idPlayerPair.Value.ResetHealth();
                SetTeam(idPlayerPair.Value);
            }

            onRematchClientRpc();
        }

        [ClientRpc]
        private void onRematchClientRpc()
        {
            OnRematchSessionStart.Invoke();
        }
    }
}
