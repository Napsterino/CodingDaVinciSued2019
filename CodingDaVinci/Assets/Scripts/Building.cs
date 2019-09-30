using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace cdv
{
    [Serializable]
    public struct ResourceInfo
    {
        public ResourceType Type;
        public int Amount;

        public override string ToString()
        {
            return $"{Amount} x {Translationhelper.Get(Type.ToString())}";
        }
    }

#pragma warning disable 618
    public sealed class Building : NetworkBehaviour
#pragma warning restore 618
    {
        public IReadOnlyCollection<ResourceInfo> ConstructionCosts => Array.AsReadOnly(m_ConstructionCosts);

        public string Type => m_Type;

        [SerializeField] private ResourceInfo[] m_ConstructionCosts;
        public ResourceInfo[] GeneratedResources;
        public VictoryPoints[] GainedVictoryPoints;
        public RegionProperty[] ConstructionRequirements;
        [SerializeField] private string m_Type;

        [Serializable] public struct VictoryPoints
        {
            public VictoryPointCategory Category;
            public int Amount;

            public override string ToString()
            {
                return $"{Amount} x {Translationhelper.Get(Category.ToString())}";
            }
        }
    }
}