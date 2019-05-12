using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace cdv
{
    public enum RegionProperty
    {
        SeaAccess,
        NoGameField,
        StartRegion
    }

    /// <summary>
    /// Representation of on region of the map the game is played on
    /// </summary>
#pragma warning disable 618
    public sealed class Region : NetworkBehaviour
#pragma warning restore 618
    {

        #region Client Code
        /// <summary>
        /// Sets the owner of the region
        /// </summary>
        /// <param name="ownerId">id of the owner</param>
#pragma warning disable 618
        [ClientRpc] public void RpcSetOwner(NetworkInstanceId ownerId)
#pragma warning restore 618
        {
            Assert.IsTrue(isClient, "Run RpcSetOwner only clientside");
#pragma warning disable 618
            var owner = ClientScene.FindLocalObject(ownerId).GetComponent<Player>();
#pragma warning restore 618
            Owner = owner;
            // TODO: Check if the region already has an owner and/or a building on it and then
            // remove and add the building values to the new owner 
        }

#pragma warning disable 618
        [ClientRpc] public void RpcSetBuilding(NetworkInstanceId buildingId)
        {
            Building = ClientScene.FindLocalObject(buildingId).GetComponent<Building>();
#pragma warning restore 618
        }
        #endregion

        public bool FulFillsRequirements(RegionProperty[] requirements)
        {
            foreach(var requirement in requirements)
            {
                if(Array.IndexOf(Properties, requirement) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns whether or not the region has acces to the sea
        /// </summary>
        public bool HasSeaAccess()
        {
            foreach(var element in Properties)
            {
                if(element == RegionProperty.SeaAccess)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsStartRegion()
        {
            foreach(var element in Properties)
            {
                if(element == RegionProperty.StartRegion)
                {
                    return true;
                }
            }

            return false;
        }

        public IReadOnlyCollection<Region> NeighbourRegions => Array.AsReadOnly(m_NeighbourRegions);

        public Building Building;
        public Player Owner;
        [SerializeField] private Region[] m_NeighbourRegions = new Region[200];
        [SerializeField] private RegionProperty[] Properties; 

        public const int FREE_PRICE = 3;
        public const int ENEMY_OCCUPIED_PRICE = 9;
    }
}