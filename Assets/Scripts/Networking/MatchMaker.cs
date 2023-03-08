using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using ParrelSync;
#endif


// code adapted from https://www.youtube.com/watch?v=fdkvm21Y0xE by Tarodev 
namespace Networking
{
    public class MatchMaker : MonoBehaviour
    {
        [SerializeField] private int maxPlayers = 2;
        
        [HideInInspector] public UnityEvent OnMatchmakingStarted = new ();
        
        private Lobby currentLobby;
        private QueryResponse lobbies;
        private UnityTransport unityTransport;
        private const string JoinCodeKey = "JoinCode";
        private string _playerId;
        
        public int MaxPlayers => maxPlayers;
        
        private void Awake()
        {
            unityTransport = FindObjectOfType<UnityTransport>();
        }

        public async void FindMatch()
        {
            OnMatchmakingStarted.Invoke();
            await Authenticate();
            currentLobby = await QuickJoinLobby() ?? await CreateLobby();
        }

        private async Task<Lobby> QuickJoinLobby()
        {
            try
            {
                var lobby = await Lobbies.Instance.QuickJoinLobbyAsync();

                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);
                SetTransformAsClient(joinAllocation);

                NetworkManager.Singleton.StartClient();
                return lobby;
            }
            catch (Exception e)
            {
                Debug.Log("Didn't find a lobby to join");
                return null;
            }
        }

        private void SetTransformAsClient(JoinAllocation joinAllocation)
        {
            unityTransport.SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData);
        }

        private async Task<Lobby> CreateLobby()
        {
            try
            {
                var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
                var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                var options = new CreateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                    }
                };
                
                var lobby = await Lobbies.Instance.CreateLobbyAsync("Lobby Name", maxPlayers, options);

                StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));
                
                unityTransport.SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData);

                NetworkManager.Singleton.StartHost();
                return lobby;
            }
            catch(Exception e)
            {
                Debug.Log("Error creating lobby " + e.Message);
                return null;
            }
        }

        private static IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
        {
            var delay = new WaitForSecondsRealtime(waitTimeSeconds);
            while (true)
            {
                Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
                yield return delay;
            }
        }

        private async Task Authenticate()
        {
            var initializationOptions = new InitializationOptions();
            
            initializationOptions.SetProfile("Random profile " + UnityEngine.Random.Range(0, 100000000));
            
            await UnityServices.InitializeAsync(initializationOptions);
            
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();   
            }
            catch(Exception e)
            {
                Debug.LogWarning("Error authenticating " + e.Message);
            }
            _playerId = AuthenticationService.Instance.PlayerId;
        }

        private void OnDestroy()
        {
            try
            {
                StopAllCoroutines();
                if(currentLobby != null)
                {
                    if (currentLobby.HostId == _playerId)
                    {
                        Lobbies.Instance.DeleteLobbyAsync(currentLobby.Id);
                    }
                    else
                    {
                        Lobbies.Instance.RemovePlayerAsync(currentLobby.Id, _playerId);
                    }
                }
            } catch (Exception e)
            {
                Debug.Log("Error deleting lobby " + e.Message);
            }
        }

        public void SetSoloPractice()
        {
            maxPlayers = 1;
        }
    }
}
