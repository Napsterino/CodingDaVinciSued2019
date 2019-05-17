using cdv;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonRelayWithPlayer : MonoBehaviour
{
    public Player AssociatedPlayer;
    public PlayerEvent OnClick;

    public void InvokeEvent()
    {
        OnClick.Invoke(AssociatedPlayer);
    }
}

[Serializable]
public class PlayerEvent : UnityEvent<Player> { }
