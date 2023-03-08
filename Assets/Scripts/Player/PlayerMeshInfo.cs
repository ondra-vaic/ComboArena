using UnityEngine;

namespace Player
{
    public class PlayerMeshInfo : MonoBehaviour
    {
        [SerializeField] private Transform head;
        [SerializeField] private Transform visualRoot;
        public Transform Head => head;
        public Transform VisualRoot => visualRoot;
    }
}
