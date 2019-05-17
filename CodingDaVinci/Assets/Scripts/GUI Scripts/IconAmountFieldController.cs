using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconAmountFieldController : MonoBehaviour
{
    public Text AmountDisplay;
    public string UnitString;
    public string IntFormatString = "### ##0";
    public string FloatFormatString = "### ###,00";

    public void SetAmount(int amount)
    {
        SetText(amount.ToString(IntFormatString));
    }

    public void SetAmount(float amount)
    {
        SetText(amount.ToString(FloatFormatString));
    }

    private void SetText(string amountStr)
    {
        if (string.IsNullOrEmpty(UnitString))
        {
            AmountDisplay.text = amountStr;
        }
        else
        {
            AmountDisplay.text = string.Format("{0} {1}", amountStr, UnitString);
        }
    }
}
