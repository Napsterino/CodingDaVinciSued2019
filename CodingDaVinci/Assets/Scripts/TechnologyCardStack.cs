using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace cdv
{
    public sealed class TechnologyCardStack : CardStack
    {
        #region Shared Code
        public override void PreStartClient()
        {
            base.PreStartClient();
            Graveyard = GameObject.Find("TechnologyCards/Graveyard").transform;
        }
        #endregion

        #region Server Code
        #endregion    
    }
}