using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace cdv
{
    public class TradeSelectorController : MonoBehaviour
    {
        public int MaxAmount_Construction;
        public int MaxAmount_Technology;
        public int MaxAmount_Influence;
        public ResourceType SelectedType { get; private set; }
        public int Amount { get; private set; } = 1;

        [Header("Selected Button Colorblock")]
        public ColorBlock SelectedTypeColor;
        [Header("Unselected Button Colorblock")]
        public ColorBlock UnselectedTypeColor;

        [Header("Buttons")]
        public Button Resource_Construction;
        public Button Resource_Technology;
        public Button Resource_Influence;

        public Button Amount_Increment;
        public Button Amount_Decrement;
        public Text Amount_Display;

        private void Awake()
        {
            ResetTypeAndAmount(1, 1, 1);
        }

        public void ResetTypeAndAmount(int mConstruction, int mTechnology, int mInfluence)
        {
            SetSelection(ResourceType.ConstructionMaterial);
            MaxAmount_Construction = mConstruction;
            MaxAmount_Technology = mTechnology;
            MaxAmount_Influence = mInfluence;
            Amount = 1;
            UpdateAmountUI();
        }

        public void SelectConstruction() {
            SetSelection(ResourceType.ConstructionMaterial);
        }

        public void SelectTechnology()
        {
            SetSelection(ResourceType.Technology);
        }

        public void SelectInfluence()
        {
            SetSelection(ResourceType.Power);
        }

        public void SetSelection(ResourceType type)
        {
            SelectedType = type;
            Resource_Construction.colors = UnselectedTypeColor;
            Resource_Technology.colors = UnselectedTypeColor;
            Resource_Influence.colors = UnselectedTypeColor;
            switch (type)
            {
                case ResourceType.ConstructionMaterial:
                    Resource_Construction.colors = SelectedTypeColor;
                    break;
                case ResourceType.Technology:
                    Resource_Technology.colors = SelectedTypeColor;
                    break;
                case ResourceType.Power:
                    Resource_Influence.colors = SelectedTypeColor;
                    break;
            }
            UpdateAmountUI();
        }

        public void IncrementAmount()
        {
            Amount++;
            UpdateAmountUI();
        }

        public void DecrementAmount()
        {
            Amount--;
            UpdateAmountUI();
        }

        private void UpdateAmountUI()
        {
            int MaxAmount;
            switch (SelectedType)
            {
                case ResourceType.ConstructionMaterial:
                    MaxAmount = MaxAmount_Construction;
                    break;
                case ResourceType.Technology:
                    MaxAmount = MaxAmount_Technology;
                    break;
                case ResourceType.Power:
                    MaxAmount = MaxAmount_Influence;
                    break;
                default:
                    MaxAmount = 0;
                    break;
            }

            if (Amount >= MaxAmount)
            {
                Amount = MaxAmount;
                Amount_Increment.interactable = false;
            }
            else
            {
                Amount_Increment.interactable = true;
            }

            if (Amount <= 1)
            {
                Amount = 1;
                Amount_Decrement.interactable = false;
            }
            else
            {
                Amount_Decrement.interactable = true;
            }
            Amount_Display.text = Amount.ToString();
        }
    }
}
