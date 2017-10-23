using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace BetterWaterManagement
{
    internal class CookingUtils
    {
        internal static UILabel GetUILabel(string name)
        {
            UILabel[] labels = Resources.FindObjectsOfTypeAll<UILabel>();
            foreach (UILabel eachLabel in labels)
            {
                if (eachLabel.name == name)
                {
                    return eachLabel;
                }
            }

            return null;
        }

        internal static UITexture GetUITexure(string name)
        {
            UITexture[] textures = Resources.FindObjectsOfTypeAll<UITexture>();
            foreach (UITexture eachTexture in textures)
            {
                if (eachTexture.name == name)
                {
                    return eachTexture;
                }
            }

            return null;
        }
    }
}
