using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace cdv {
    public class TerritoryContextMenuController : MonoBehaviour
    {
        public GameObject Panel;
        public Button ContextButton;
        public Text ContextButtonLabel;
        public Text ContextText;

        private SimpleDelegate OnButtonPress;

        public void UpdateContextMenu(string buttonLabel, string text, SimpleDelegate onButtonPress)
        {
            Panel.SetActive(true);
            OnButtonPress = onButtonPress;
            ContextButton.interactable = OnButtonPress != null;
            ContextButtonLabel.text = buttonLabel;
            ContextText.text = text;
        }

        public void HideContextMenu()
        {
            Panel.SetActive(false);
        }

        public void OnButton()
        {
            ContextText.text = string.Empty;
            OnButtonPress?.Invoke();
            OnButtonPress = null;
            gameObject.SetActive(false);
        }
    }
}