using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace cdv
{
    public class BuildingSelectorController : MonoBehaviour
    {
        public GameObject Panel;
        public List<BuildingButton> BuildingButtons;

        public void Awake()
        {
            foreach (BuildingButton button in BuildingButtons)
            {
                Translationhelper.translations.Add(button.BuildingName, button.gameObject.name);
            }
        }

        public void Show()
        {
            Panel.SetActive(true);
            UpdateBuildableBuildings();
        }

        public void Hide()
        {
            Panel.SetActive(false);
        }

        public void UpdateBuildableBuildings()
        {
            foreach (BuildingButton buildingButton in BuildingButtons)
            {
                buildingButton.SetInteractable(Player.LocalPlayer.Resources.HasResources(buildingButton.Building.ConstructionCosts));
            }
        }

        public void OnBuildingButton(string name)
        {
            Player.LocalPlayer.BuildingToBuild = name;
            Player.LocalPlayer.ProcessGUINextFrame();
            Player.LocalPlayer.ShowBuildingOverview = false;
        }
    }
}