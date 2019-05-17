using cdv;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsPanelManager : MonoBehaviour
{
    public GameObject LeaderCardPrefab;
    public GameObject ListBaseObject;

    private List<CardUIController> cards = new List<CardUIController>();

    public CardUIController AddEntry(Card source)
    {
        CardUIController newEntry = Instantiate(LeaderCardPrefab).GetComponent<CardUIController>();
        newEntry.SourceCard = source;
        newEntry.UpdateUI();
        cards.Add(newEntry);
        return newEntry;
    }

    public void RemoveEntry(Card entry)
    {
        foreach (CardUIController cardUI in cards)
        {
            if (cardUI.SourceCard == entry || cardUI.SourceCard.name == entry.name)
            {
                Destroy(cardUI.gameObject);
                cards.Remove(cardUI);
                break;
            }
        }
    }
}
