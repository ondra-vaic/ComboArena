using Actions;
using Actions.Effectors;
using UnityEngine;

namespace Networking
{
    public class Libraries : MonoBehaviour
    {
        [SerializeField] private EffectorsLibrary effectorsLibrary;
        [SerializeField] private CastActionsLibrary actionsLibrary;

        public EffectorsLibrary EffectorsLibrary => effectorsLibrary;
        public CastActionsLibrary ActionsLibrary => actionsLibrary;
        
        private void Awake()
        {
            effectorsLibrary.Initialize();
            actionsLibrary.Initialize();   
        }
    }
}
