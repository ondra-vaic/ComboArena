using System;
using System.Collections.Generic;
using Player;
using Unity.Netcode;
using UnityEngine;

namespace Actions.Effectors
{
    [Serializable]
    public class ActionEffector : NetworkBehaviour
    {
        [SerializeField] protected AnimationCurve soundVolumeCurve;
        [SerializeField] protected float timeToDestroy;
        [SerializeField] private int maxNumHits;
        [SerializeField] private int damage;
        [SerializeField] private float colliderDuration;
        [SerializeField] private float colliderStartTime;

        [SerializeField] protected float castMultiplier = 1;

        private Collider effectorCollider;
        private NetworkObject networkObject;
        private AudioSource audioSource;
        
        protected PlayerCastState playerCastState;
        protected float playerMovementDistance;
        
        private Team team;
        private readonly Dictionary<ulong, int> hits = new();
        protected float spawnTime;
        
        private bool initialized;

        public override void OnNetworkSpawn()
        {
            effectorCollider = GetComponent<Collider>();
            audioSource = GetComponent<AudioSource>();
            spawnTime = Time.time;

            if (IsServer)
            {
                networkObject = GetComponent<NetworkObject>();
                
                if (effectorCollider != null)
                {
                    effectorCollider.enabled = false;
                    effectorCollider.isTrigger = true;   
                }
            }
            else if(effectorCollider != null)
            {
                Destroy(effectorCollider);
            }

            initialized = true;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            Player.Player hitPlayer = other.GetComponent<Player.Player>();
            if (hitPlayer == null) return;
            
            ulong playerId = hitPlayer.OwnerClientId;
            
            if (IsServer && hitPlayer.GetTeam() != team &&
                (!hits.ContainsKey(playerId) && maxNumHits > 0 ||
                 hits.ContainsKey(playerId) && hits[playerId] < maxNumHits))
            {
                if (!hits.ContainsKey(playerId))
                {
                    hits.Add(playerId, 1);
                }
                else
                {
                    hits[playerId] += 1;    
                }
                
                hitPlayer.TakeDamage((int)(damage / castMultiplier));
                maxNumHits--;
            }
        }

        protected virtual void Update()
        {
            if(!initialized) return;
            if (IsClient)
            {
                float progress = (Time.time - spawnTime) / (timeToDestroy / castMultiplier);
                audioSource.volume = soundVolumeCurve.Evaluate(progress);    
            }
            
            if (IsServer)
            {
                if (effectorCollider != null)
                {
                    if (Time.time - spawnTime > colliderStartTime / castMultiplier)
                    {
                        effectorCollider.enabled = true;
                    }

                    if (Time.time - spawnTime > (colliderStartTime + colliderDuration) / castMultiplier)
                    {
                        effectorCollider.enabled = false;
                    }
                }

                if (Time.time - spawnTime > timeToDestroy / castMultiplier)
                {
                    networkObject.Despawn();
                }   
            }
        }

        public virtual void PlaceEffector(Team team, PlayerCastState playerCastState, float castMultiplier, float playerMovementDistance)
        {
            this.playerMovementDistance = playerMovementDistance;
            this.team = team;
            this.castMultiplier = castMultiplier;
            this.playerCastState = playerCastState;
        }

        [ClientRpc]
        public virtual void StartVFXClientRPC(float castMultiplier, float playerMovementDistance)
        {
            audioSource.pitch /= castMultiplier;
        }

        public void Break()
        {
            spawnTime = timeToDestroy / castMultiplier - Time.time - 0.5f;
            // networkObject.Despawn();
        }
        
        public Team GetTeam() => team;
    }
}
