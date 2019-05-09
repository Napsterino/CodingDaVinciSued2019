using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace cdv
{
    public enum ResourceType
    {
        ConstructionMaterial,
        Technology,
        Power,
        Count
    }

#pragma warning disable 618
    public sealed class Resources : NetworkBehaviour
#pragma warning restore 618
    {
        #region Server Code
#pragma warning disable 618
        [Command] private void CmdAddConstructionMaterial(int amount)
#pragma warning restore 618
        {
            ConstructionMaterial += amount;
        }

#pragma warning disable 618
        [Command] private void CmdAddTechnology(int amount)
#pragma warning restore 618
        {
            Technology += amount;
        }

#pragma warning disable 618
        [Command] private void CmdAddPower(int amount)
#pragma warning restore 618
        {
            Power += amount;
        }

        /// <summary>
        /// Applies all the resources a player gets per round to the current resources
        /// </summary>
        public void ApplyPerRoundValues()
        {
            ConstructionMaterial += ConstructionMaterialPerRound;
            Power += PowerPerRound;
            Technology += TechnologyPerRound;
        }

        public void AddConstructionMaterial(int amount, bool permanent)
        {
            Assert.IsTrue(isServer, "Only call AddConstructionMaterial on Server");
            if(permanent)
            {
                ConstructionMaterialPerRound += amount;
            }
            else
            {
                ConstructionMaterial += amount;
            }
        }

        public void AddPower(int amount, bool permanent)
        {
            Assert.IsTrue(isServer, "Only call AddPower on Server");
            if(permanent)
            {
                PowerPerRound += amount;
            }
            else
            {
                Power += amount;
            }
        }

        public void AddTechnology(int amount, bool permanent)
        {
            Assert.IsTrue(isServer, "Only call AddTechnology on Server");
            if(permanent)
            {
                TechnologyPerRound += amount;
            }
            else
            {
                Technology += amount;
            }
        }
        #endregion

        #region Shared Code
        public void AddResources(ResourceInfo[] resources)
        {
            foreach(var resource in resources)
            {
                switch(resource.Type)
                {
                    case ResourceType.ConstructionMaterial:
                    {
                        ConstructionMaterial += resource.Amount;
                        break;
                    }

                    case ResourceType.Power:
                    {
                        Power += resource.Amount;
                        break;
                    }

                    case ResourceType.Technology:
                    {
                        Technology += resource.Amount;
                        break;
                    }
                }
            }
        }

        public void ApplyConstructionCosts(IReadOnlyCollection<ResourceInfo> costs,
            int technologyCostReduction)
        {
            foreach(var cost in costs)
            {
                switch(cost.Type)
                {
                    case ResourceType.ConstructionMaterial:
                    {
                        ConstructionMaterial -= cost.Amount;
                        break;
                    }

                    case ResourceType.Power:
                    {
                        Power -= cost.Amount;
                        break;
                    }

                    case ResourceType.Technology:
                    {
                        int amount = Mathf.Clamp(cost.Amount - technologyCostReduction, 0,
                            cost.Amount);
                        Technology -= amount;
                        break;
                    }
                }
            }
        }

        public bool HasResources(IReadOnlyCollection<ResourceInfo> costs)
        {
            foreach(var cost in costs)
            {
                switch(cost.Type)
                {
                    case ResourceType.ConstructionMaterial:
                    {
                        if(ConstructionMaterial < cost.Amount)
                        {
                            return false;
                        }
                        break;
                    }

                    case ResourceType.Power:
                    {
                        if(Power < cost.Amount)
                        {
                            return false;
                        }
                        break;
                    }

                    case ResourceType.Technology:
                    {
                        if(Technology < cost.Amount)
                        {
                            return false;
                        }
                        break;
                    }
                }
            }

            return true;
        }

        private void Update()
        {
            if(isLocalPlayer)
            {
                if(Input.GetKeyDown(KeyCode.UpArrow))
                {
                    CmdAddConstructionMaterial(1);
                }
                if(Input.GetKeyDown(KeyCode.RightArrow))
                {
                    CmdAddTechnology(1);
                }
                if(Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    CmdAddPower(1);
                }
            }
        }

        private void OnGUI()
        {
            if(isLocalPlayer)
            {
                GUILayout.BeginArea(new Rect(20, 250, 200, 200));
                GUILayout.Label($"Baustoffe: {ConstructionMaterial}");
                GUILayout.Label($"Technologie: {Technology}");
                GUILayout.Label($"Einfluss: {Power}");
                GUILayout.EndArea();
            }
        }

        public int ConstructionMaterial
        {
            get => m_ConstructionMaterial;
            set
            {
                if(value < 0)
                {
                    m_ConstructionMaterial = 0;
                }
                else
                {
                    m_ConstructionMaterial = value;
                }
            }
        }

        public int Power
        {
            get => m_Power;
            set
            {
                if(value < 0)
                {
                    m_Power = 0;
                }
                else
                {
                    m_Power = value;
                }
            }
        }

        public int Technology
        {
            get => m_Technology;
            set
            {
                if(value < 0)
                {
                    m_Technology = 0;
                }
                else
                {
                    m_Technology = value;
                }
            }
        }

#pragma warning disable 618
        [SyncVar] private int m_ConstructionMaterial = 0;
        [SyncVar] private int m_Technology = 0;
        [SyncVar] private int m_Power = 0;
        [SyncVar] int ConstructionMaterialPerRound;
        [SyncVar] int PowerPerRound;
        [SyncVar] int TechnologyPerRound;
#pragma warning restore 618
        #endregion
    }
}