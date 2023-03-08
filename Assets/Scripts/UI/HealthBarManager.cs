using System.Collections.Generic;
using Networking;
using UnityEngine;

namespace UI
{
    public class HealthBarManager : MonoBehaviour
    {
        [SerializeField] private Color AllyTeamColor;
        [SerializeField] private Color EnemyTeamColor;
        
        [SerializeField] private HealthBar healthBarPrefab;

        private readonly Dictionary<ulong, HealthBar> healthBars = new();
        private PlayerManager playerManager;
       
        private void Start()
        {
            playerManager = FindObjectOfType<PlayerManager>();
            playerManager.OnAllPlayersInitialized.AddListener(createHealthBars);
        }

        private void createHealthBars()
        {
            foreach (var idPlayerPair in playerManager.GetPlayers())
            {
                createHealthBar(idPlayerPair.Key);
            }
        }

        private void createHealthBar(ulong playerId)
        {
            if(healthBars.ContainsKey(playerId)) return;
            
            HealthBar newHealthBar = Instantiate(healthBarPrefab, transform);
            healthBars.Add(playerId, newHealthBar);
            
            Player.Player player = playerManager.GetPlayer(playerId);
            
            Color healthBarColor = player.GetTeam() == playerManager.GetMyTeam() ? AllyTeamColor : EnemyTeamColor;
            newHealthBar.Initialize(playerManager.GetPlayer(playerId), healthBarColor);
        }
    }
}
