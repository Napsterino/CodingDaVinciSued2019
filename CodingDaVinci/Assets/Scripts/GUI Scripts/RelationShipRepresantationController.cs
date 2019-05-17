using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace cdv
{
    public class RelationShipRepresantationController : MonoBehaviour
    {
        public Text Label;
        public SimpleDelegate OnClickDelegate;

        public void Init(string label, SimpleDelegate onClick)
        {
            Label.text = label;
            OnClickDelegate = onClick;
        }

        public void OnClick()
        {
            OnClickDelegate?.Invoke();
            Destroy(gameObject);
        }
    }
}
