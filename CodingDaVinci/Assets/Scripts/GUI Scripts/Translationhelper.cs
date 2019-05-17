using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cdv
{
    public static class Translationhelper
    {
        public static Dictionary<string, string> translations = new Dictionary<string, string>();

        static Translationhelper()
        {
            translations.Add(ResourceType.ConstructionMaterial.ToString(), "Baumaterial");
            translations.Add(ResourceType.Technology.ToString(), "Technologie");
            translations.Add(ResourceType.Power.ToString(), "Einfluss");
        }

        public static string Get(string key)
        {
            if (translations.TryGetValue(key, out string result))
            {
                return result;
            }
            else
            {
                return key;
            }
        }
    }
}