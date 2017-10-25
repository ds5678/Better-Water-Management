using UnityEngine;

namespace BetterWaterManagement
{
    internal class WaterUtils
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

        internal static bool ContainsWater(GearItem gearItem)
        {
            if (gearItem == null || gearItem.m_LiquidItem == null)
            {
                return false;
            }

            if (gearItem.m_LiquidItem.m_LiquidType != GearLiquidTypeEnum.Water)
            {
                return false;
            }

            return gearItem.m_LiquidItem.m_LiquidLiters > 0;
        }
    }
}