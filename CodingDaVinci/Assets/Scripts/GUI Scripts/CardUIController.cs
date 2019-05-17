using cdv;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardUIController : MonoBehaviour
{
    public Card SourceCard;

    public Text Title;
    public Image Icon;

    private void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        Title.text = SourceCard.name;
        Icon.sprite = SourceCard.artwork;
    }
}
