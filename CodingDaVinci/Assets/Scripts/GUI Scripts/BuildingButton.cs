using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace cdv {
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
           Label.text = $"{Label.text} - {costString}";
        }

        public void OnClick()
        {
            MainUIRoot.Instance.BuildingMenu.OnBuildingButton(BuildingName);
        }

        public void SetInteractable(bool interactable)
        {
            Button.interactable = interactable;
        }
    }
}