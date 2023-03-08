using System.Collections.Generic;
using Actions;
using Unity.Netcode;

namespace Networking
{
    public struct ActionGroupNetworkData : INetworkSerializable
    {
        ActionGroupNetworkData[] actionGroupChildren;
        int leafActionId;
        int castMultiplier;
        int depth;
        
        public void SetChildren(ActionGroupNetworkData[] children)
        {
            actionGroupChildren = children;
        }
        
        public void SetLeafActionId(int leafActionId)
        {
            this.leafActionId = leafActionId;
        }
        
        public void SetMultiplier(int castMultiplier)
        {
            this.castMultiplier = castMultiplier;
        }

        public CastActionGroup CreateCastActionGroup(CastActionsLibrary actionsLibrary, CastActionGroup parent)
        {
            CastActionGroup[] castActionChildren = new CastActionGroup[actionGroupChildren.Length];
            CastActionGroup castActionGroup = new CastActionGroup();
            
            for (int i = 0; i < actionGroupChildren.Length; i++)
            {
                castActionChildren[i] = actionGroupChildren[i].CreateCastActionGroup(actionsLibrary, castActionGroup);
            }
            
            CastActionPrototype leafCastActionPrototype = leafActionId == -1 ? null : actionsLibrary.GetActionPrototype(leafActionId);
            
            castActionGroup.Initialize(new List<CastActionGroup>(castActionChildren), leafCastActionPrototype, parent, castMultiplier);
            return castActionGroup;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref leafActionId);
            serializer.SerializeValue(ref castMultiplier);
            serializer.SerializeValue(ref depth);
            serializer.SerializeValue(ref actionGroupChildren);
        }
    }
}
