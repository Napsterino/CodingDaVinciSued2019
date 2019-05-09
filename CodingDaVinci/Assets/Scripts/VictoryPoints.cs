using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace cdv
{
    public enum VictoryPointCategory
    {
        Science,
        Territory,
        Economy,
        Culture
    }

    /// <summary>
    /// Contains all victory points of a player
    /// </summary>
#pragma warning disable 618
    public sealed class VictoryPoints : NetworkBehaviour
#pragma warning restore 618
    {
        #region Shared Code
        // NOTE: These functions can be used as comparers between two victorypoint types.
        // The higher value is returned as the first value when used for sorting
        public static int CompareSience(VictoryPoints object1, VictoryPoints object2)
        {
            return object2.Science.CompareTo(object1.Science);
        }

        public static int CompareTerritory(VictoryPoints object1, VictoryPoints object2)
        {
            return object2.Territory.CompareTo(object1.Territory);
        }

        public static int CompareEconomy(VictoryPoints object1, VictoryPoints object2)
        {
            return object2.Economy.CompareTo(object1.Economy);
        }

        public static int CompareCulture(VictoryPoints object1, VictoryPoints object2)
        {
            return object2.Culture.CompareTo(object1.Culture);
        }

        public static int CompareWinPoints(VictoryPoints object1, VictoryPoints object2)
        {
            return object2.WinPoints.CompareTo(object1.WinPoints);
        }

        // TODO: We can probably remove this code from a release build later on
        /// <summary>
        /// Prints the current points to the game screen
        /// </summary>
        /// <param name="rect">position where the points get drawn</param>
        public void PrintStats(Rect rect)
        {
            GUILayout.BeginArea(rect);
            GUILayout.Label($"NetID: {netId.Value}");
            GUILayout.Label($"Wissenschaft: {Science}");
            GUILayout.Label($"Territorium: {Territory}");
            GUILayout.Label($"Wirtschaft: {Economy}");
            GUILayout.Label($"Kultur: {Culture}");
            GUILayout.Label($"Siegpunkte: {WinPoints}");
            GUILayout.EndArea();
        }

        private void OnGUI()
        {
            if(isLocalPlayer)
            {
                PrintStats(new Rect(20, 20, 200, 200));
            }
        }

        private void Update()
        {
            if(isLocalPlayer)
            {
                if(Input.GetKeyDown(KeyCode.W))
                {
                    CmdAddScience(1);
                }
                if(Input.GetKeyDown(KeyCode.D))
                {
                    CmdAddTerritory(1);
                }
                if(Input.GetKeyDown(KeyCode.S))
                {
                    CmdAddEconomy(1);
                }
                if(Input.GetKeyDown(KeyCode.A))
                {
                    CmdAddCulture(1);
                }
            }
        }

        public int Science => PermanentScience + TempScience;
        public int Territory => PermanentTerritory + TempTerritory;
        public int Economy => PermanentEconomy + TempEconomy;
        public int Culture => PermanentCulture + TempCulture;

#pragma warning disable 618
        // NOTE: Temp values only stay for one rund while Permanent values stay for the
        // rest of the game
        [SyncVar] private int PermanentScience = 0;
        [SyncVar] public int TempScience = 0;
        [SyncVar] private int PermanentTerritory = 0;
        [SyncVar] public int TempTerritory = 0;
        [SyncVar] private int PermanentEconomy = 0;
        [SyncVar] public int TempEconomy = 0;
        [SyncVar] private int PermanentCulture = 0;
        [SyncVar] public int TempCulture = 0;
        [SyncVar] public uint WinPoints = 0;
#pragma warning restore 618
        #endregion

        #region Server Code
        public void ApplyVictoryPoints(Building.VictoryPoints[] points)
        {
            foreach(var item in points)
            {
                switch(item.Category)
                {
                    case VictoryPointCategory.Science:
                    {
                        PermanentScience += item.Amount;
                        break;
                    }

                    case VictoryPointCategory.Territory:
                    {
                        PermanentTerritory += item.Amount;
                        break;
                    }

                    case VictoryPointCategory.Economy:
                    {
                        PermanentEconomy += item.Amount;
                        break;
                    }

                    case VictoryPointCategory.Culture:
                    {
                        PermanentCulture += item.Amount;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Adds victory points to the science category
        /// </summary>
        /// <param name="amount">amount of points</param>
        /// <param name="permanent">
        /// true: points stay until the end of the game. false: points are added just
        /// for the current round
        /// </param>
        public void AddScience(int amount, bool permanent)
        {
            Assert.IsTrue(isServer, "Only run AddScience on the server");
            if(permanent)
            {
                PermanentScience += amount;
            }
            else
            {
                TempScience += amount;
            }
        }

        /// <summary>
        /// Adds victory points to the territory category
        /// </summary>
        /// <param name="amount">amount of points</param>
        /// <param name="permanent">
        /// true: points stay until the end of the game. false: points are added just
        /// for the current round
        /// </param>
        public void AddTerritory(int amount, bool permanent)
        {
            Assert.IsTrue(isServer, "Only run AddTerritory on the server");
            if(permanent)
            {
                PermanentTerritory += amount;
            }
            else
            {
                TempTerritory += amount;
            }
        }

        /// <summary>
        /// Adds victory points to the economy category
        /// </summary>
        /// <param name="amount">amount of points</param>
        /// <param name="permanent">
        /// true: points stay until the end of the game. false: points are added just
        /// for the current round
        /// </param>
        public void AddEconomy(int amount, bool permanent)
        {
            Assert.IsTrue(isServer, "Only run AddEconomy on the server");
            if(permanent)
            {
                PermanentEconomy += amount;
            }
            else
            {
                TempEconomy += amount;
            }
        }

        /// <summary>
        /// Adds victory points to the culture category
        /// </summary>
        /// <param name="amount">amount of points</param>
        /// <param name="permanent">
        /// true: points stay until the end of the game. false: points are added just
        /// for the current round
        /// </param>
        public void AddCulture(int amount, bool permanent)
        {
            Assert.IsTrue(isServer, "Only run AddCulture on the server");
            if(permanent)
            {
                PermanentCulture += amount;
            }
            else
            {
                TempCulture += amount;
            }
        }

        // TODO: These commands are only used for testing purposes. We could strip them out
        // for a release build

        /// <summary>
        /// Add victory points to the science category
        /// </summary>
        /// <param name="amount">Amount of points to add. Can be negative.</param>
#pragma warning disable 618
        [Command]
#pragma warning restore 618
        private void CmdAddScience(int amount)
        {
            TempScience += amount;
        }

        /// <summary>
        /// Add victory points to the territory category
        /// </summary>
        /// <param name="amount">Amount of points to add. Can be negative.</param>
#pragma warning disable 618
        [Command]
#pragma warning restore 618
        private void CmdAddTerritory(int amount)
        {
            TempTerritory += amount;
        }

        /// <summary>
        /// Add victory points to the economy category
        /// </summary>
        /// <param name="amount">Amount of points to add. Can be negative.</param>
#pragma warning disable 618
        [Command]
#pragma warning restore 618
        private void CmdAddEconomy(int amount)
        {
            TempEconomy += amount;
        }

        /// <summary>
        /// Add victory points to the culture category
        /// </summary>
        /// <param name="amount">Amount of points to add. Can be negative.</param>
#pragma warning disable 618
        [Command]
#pragma warning restore 618
        private void CmdAddCulture(int amount)
        {
            TempCulture += amount;
        }
        #endregion
    }
}