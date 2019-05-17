using UnityEngine.Networking;

namespace cdv
{
    public struct NetworkInstance
    {
        public NetworkInstanceId Id;
    }
    
    /// <summary>
    /// Data structure to syncronize a list of NetworkInstanceIds
    /// </summary>
    public sealed class NetworkIdList : SyncListStruct<NetworkInstance>
    {
        public bool Contains(NetworkInstanceId id)
        {
            foreach(NetworkInstance instance in this)
            {
                if(instance.Id == id)
                {
                    return true;
                }
            }
            
            return false;
        }
    }
}