using ModComponentAPI;
using ModComponentMapper;

namespace BetterWaterManagement
{
    public class CookingModifier : ModSaveBehaviour
    {
        public float additionalMinutes;
        public float potableWaterRequiredLiters;

        public override void Deserialize(string data)
        {
            if (data == null)
            {
                return;
            }

            CookingModifierData cookingModifierData = Utils.DeserializeObject<CookingModifierData>(data);
            this.additionalMinutes = cookingModifierData.additionalMinutes;
            this.potableWaterRequiredLiters = cookingModifierData.potableWaterRequiredLiters;

            this.Apply();
        }

        public override string Serialize()
        {
            CookingModifierData cookingModifierData = new CookingModifierData
            {
                additionalMinutes = this.additionalMinutes,
                potableWaterRequiredLiters = this.potableWaterRequiredLiters
            };

            return Utils.SerializeObject(cookingModifierData);
        }

        public void Apply()
        {
            Cookable cookable = this.GetComponent<Cookable>();
            if (cookable != null && this.additionalMinutes > 0)
            {
                this.potableWaterRequiredLiters = cookable.m_PotableWaterRequiredLiters;

                cookable.m_PotableWaterRequiredLiters = 0;
                cookable.m_CookTimeMinutes += additionalMinutes;
            }
        }

        public void Revert()
        {
            Cookable cookable = this.GetComponent<Cookable>();
            if (cookable != null && this.additionalMinutes > 0)
            {
                cookable.m_PotableWaterRequiredLiters = potableWaterRequiredLiters;
                cookable.m_CookTimeMinutes -= additionalMinutes;
            }
        }
    }

    internal class CookingModifierData
    {
        public float additionalMinutes;
        public float potableWaterRequiredLiters;
    }
}