using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PersistentSettingsManager : MonoBehaviour
{
    public const int PLAYERNAME_MAXLENGTH = 16;

    public static string playerName;
    public static Color32 playerColor;

    public static bool SettingsLoaded { get; private set; }

    public InputField playerNameSource;
    public ColorSelector playerColorSource;

    public List<Image> ColorPreviews;
    public List<Text> NamePreviews;

    // Start is called before the first frame update
    void Start()
    {
        RetrieveSettings();
        playerNameSource.text = playerName;
        playerColorSource.color = playerColor;
        UpdatePreviews();
    }

    public static void RetrieveSettings()
    {
        if (!SettingsLoaded)
        {
            playerName = PlayerPrefs.GetString("PlayerName", "New Player");
            playerColor = intToColor32(PlayerPrefs.GetInt("PlayerColor", MASK_BLUE));
            SettingsLoaded = true;
        }
    }

    public static void ApplySettings()
    {
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.SetInt("PlayerColor", color32ToInt(playerColor));
    }

    public void GetFromUIAndSave()
    {
        playerColor = playerColorSource.color;
        playerName = playerNameSource.text;
        if (playerName.Length > PLAYERNAME_MAXLENGTH)
        {
            playerName = playerName.Substring(0, PLAYERNAME_MAXLENGTH);
        }
        ApplySettings();
        UpdatePreviews();
    }

    public void UpdatePreviews()
    {
        foreach (Image image in ColorPreviews)
        {
            image.color = playerColor;
        }
        foreach (Text text in NamePreviews)
        {
            text.text = playerName;
        }
    }

    const int MASK_BLUE = 0xFF0000, MASK_GREEN = 0xFF00, MASK_RED = 0xFF;

    private static Color32 intToColor32(int i)
    {
        return new Color32()
        {
            a = 255,
            r = (byte)(i & MASK_RED),
            g = (byte)((i & MASK_GREEN) >> 8),
            b = (byte)((i & MASK_BLUE) >> 16)
        };
    }

    private static int color32ToInt(Color32 color)
    {
        return (color.b << 16) + (color.g << 8) + color.r;
    }
}
