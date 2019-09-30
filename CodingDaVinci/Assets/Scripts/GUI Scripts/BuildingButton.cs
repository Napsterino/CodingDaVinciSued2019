using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace cdv
{
    public class BuildingButton : MonoBehaviour
    {
        public Building Building;
        public Button Button;
        public Text Label;

        public string BuildingName => Building?.name;

        private void Awake()
        {
            IReadOnlyCollection<ResourceInfo> constructionCosts = Building.ConstructionCosts;
            string costString = string.Join(", ", constructionCosts);
            //Label.text = $"{Label.text} - {costString}";

            TooltipContentProvider tooltip = GetComponent<TooltipContentProvider>();

            if (tooltip)
            {
                string effectString = (Building.GeneratedResources.Length == 0 ? "" : "Jede Runde: " +  string.Join(", ", Building.GeneratedResources) + '\n') + (Building.GainedVictoryPoints.Length == 0 ? "" : "Einmalig: " + string.Join(", ", Building.GainedVictoryPoints) + '\n');

                tooltip.Text = string.Format(tooltip.Text, BuildingName, costString, effectString);
            }
        }

        public void OnClick()
        {
            TooltipInfoController.Hide();
            MainUIRoot.Instance.BuildingMenu.OnBuildingButton(BuildingName);
        }

        public void SetInteractable(bool interactable)
        {
            Button.interactable = interactable;
        }
    }
}