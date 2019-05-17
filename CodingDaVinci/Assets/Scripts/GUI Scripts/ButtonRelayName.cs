using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace cdv
{
    public class ButtonRelayName : MonoBehaviour
    {
        public StringEvent OnClick;

        public void Invoke()
        {
            OnClick.Invoke(gameObject.name);
        }

        [Serializable]
        public class StringEvent : UnityEvent<string> { }
    }
}
