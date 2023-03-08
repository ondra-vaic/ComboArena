using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Networking
{
    public class DebugLagSetter : MonoBehaviour
    {
        void Awake()
        {
#if UNITY_EDITOR
            GetComponent<UnityTransport>().SetDebugSimulatorParameters(packetDelay:30, packetJitter:0, dropRate:0);
#endif
        }
    }
}
