using ModComponentAPI;
using Harmony;

namespace BetterWaterManagement
{
    public class OverrideCookingState : ModSaveBehaviour
    {
        public bool ForceReady;

        public override void Deserialize(string data)
        {
            ForceReady = false;
            bool.TryParse(data, out ForceReady);

            if (!ForceReady)
            {
                return;
            }

            CookingPotItem cookingPotItem = this.GetComponent<CookingPotItem>();
            if (cookingPotItem == null)
            {
                return;
            }

            Traverse.Create(cookingPotItem).Method("SetCookingState", new System.Type[] { typeof(CookingPotItem.CookingState) }).GetValue(CookingPotItem.CookingState.Ready);
            cookingPotItem.m_GrubMeshRenderer.sharedMaterials = cookingPotItem.m_BoilWaterReadyMaterialsList;
        }

        public override string Serialize()
        {
            return ForceReady.ToString();
        }
    }
}