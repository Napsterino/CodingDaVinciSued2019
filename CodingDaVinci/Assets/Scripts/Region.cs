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
        /// Deselects the currently selected region
        /// </summary>
        public void Deselect()
        {
            SelectedRegionMask.color = Color.clear;
        }

        public void Select(Color color)
        {
            SelectedRegionMask.color = color;
        }

        /// <summary>
        /// Sets the owner of the region
        /// </summary>
        /// <param name="ownerId">id of the owner</param>
#pragma warning disable 618
        [ClientRpc] public void RpcSetOwner(NetworkInstanceId ownerId)
#pragma warning restore 618
        {
            Assert.IsTrue(isClient, "Run RpcSetOwner only clientside");
            if(Owner && Building)
            {
                Owner.VictoryPoints.RemoveVictoryPoints(Building.GainedVictoryPoints);
            }
#pragma warning disable 618
            if(ownerId != NetworkInstanceId.Invalid)
            {
                var owner = ClientScene.FindLocalObject(ownerId).GetComponent<Player>();
#pragma warning restore 618
                Owner = owner;
                Color color = owner.PlayerColor;
                color.a = 0.3f;
                RegionOwnerOverlay.GetComponent<SpriteRenderer>().color = color;
            }
            else
            {
                Owner = null;
                RegionOwnerOverlay.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
            }
        }

#pragma warning disable 618
        [ClientRpc] public void RpcSetBuilding(NetworkInstanceId buildingId)
        {
            Building = ClientScene.FindLocalObject(buildingId).GetComponent<Building>();
#pragma warning restore 618
        }

        /// <summary>
        /// Mask of that displays the selection mask if the region is selected
        /// </summary>
        [SerializeField] SpriteRenderer SelectedRegionMask;
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
        // CLEANUP: We have a maximum of 6 neighbours per region. Does this have to be 200 large??
        [SerializeField] private Region[] m_NeighbourRegions = new Region[200];
        [SerializeField] private RegionProperty[] Properties;
        [SerializeField] GameObject RegionOwnerOverlay;

        public const int FREE_PRICE = 3;
        public const int ENEMY_OCCUPIED_PRICE = 9;
    }
}