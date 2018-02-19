using UnityEngine;

namespace BetterWaterManagement
{
    internal class WaterUtils
    {
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

        internal static string GetWaterSuffix(LiquidItem liquidItem)
        {
            if (Water.IsEmpty(liquidItem))
            {
                return "_empty";
            }

            if (liquidItem.m_LiquidQuality == LiquidQuality.NonPotable)
            {
                return "_nonpotable";
            }

            return "_potable";
        }

        internal static void UpdateWaterBottle(GearItem gearItem)
        {
            UpdateWaterBottleSound(gearItem);
            UpdateWaterBottleTexture(gearItem.m_LiquidItem);
        }

        private static Texture GetTexture(LiquidItem liquidItem)
        {
            return Resources.Load("Textures/GEAR_WaterBottle" + GetWaterSuffix(liquidItem)) as Texture;
        }

        private static void UpdateWaterBottleSound(GearItem instance)
        {
            if (Water.IsEmpty(instance.m_LiquidItem))
            {
                instance.m_PickUpAudio = "Play_SndInvWaterBottle_empty";
                instance.m_PutBackAudio = "Play_SndInvWaterBottle_empty";
            }
            else
            {
                instance.m_PickUpAudio = "Play_SndInvWaterBottle";
                instance.m_PutBackAudio = "Play_SndInvWaterBottle";
            }
        }

        private static void UpdateWaterBottleTexture(LiquidItem liquidItem)
        {
            Texture texture = GetTexture(liquidItem);

            Renderer[] renderers = liquidItem.GetComponentsInChildren<Renderer>();
            foreach (Renderer eachRenderer in renderers)
            {
                foreach (Material eachMaterial in eachRenderer.materials)
                {
                    if ("GEAR_WaterBottle_Mat (Instance)" == eachMaterial.name)
                    {
                        eachMaterial.mainTexture = texture;
                    }
                }
            }
        }
    }
}