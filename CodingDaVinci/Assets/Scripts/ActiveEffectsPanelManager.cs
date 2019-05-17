using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveEffectsPanelManager : MonoBehaviour
{
    public GameObject EffectsListEntryPrefab;
    public GameObject ListBaseObject;

    private List<EffectsListEntry> effects = new List<EffectsListEntry>();

    public EffectsListEntry AddEntry(string effect, string description)
    {
        EffectsListEntry newEntry = Instantiate(EffectsListEntryPrefab).GetComponent<EffectsListEntry>();
        newEntry.EffectDisplay.text = effect;
        newEntry.DescriptionDisplay.text = description;
        effects.Add(newEntry);
        return newEntry;
    }

    public void RemoveEntry(EffectsListEntry entry)
    {
        effects.Remove(entry);
        Destroy(entry.gameObject);
    }
}
