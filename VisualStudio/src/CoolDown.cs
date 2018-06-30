using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Harmony;
using UnityEngine;

namespace BetterWaterManagement
{
    public class CoolDown : MonoBehaviour
    {
        private float elapsed;
        private CookingPotItem cookingPotItem;
        private CookingPotItem.CookingState originalState;
        private CookingPotItem.CookingState nextState;
        private float lastUpdate;

        public void Start()
        {
            this.cookingPotItem = this.GetComponent<CookingPotItem>();
            this.originalState = this.cookingPotItem.GetCookingState();
            this.nextState = originalState - 1;
        }

        public void FixedUpdate()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            elapsed += Time.fixedDeltaTime;
            if (elapsed - lastUpdate > 5)
            {
                Debug.Log("elapsed = " + elapsed);
                lastUpdate = elapsed;
            }

            if (elapsed < 20)
            {
                return;
            }

            if (nextState < CookingPotItem.CookingState.Cooking)
            {
                AccessTools.Method(typeof(CookingPotItem), "TurnOnParticles").Invoke(this.cookingPotItem, new object[] { null });
                this.enabled = false;
                return;
            }

            System.Reflection.FieldInfo fieldInfo = AccessTools.Field(typeof(CookingPotItem), "m_CookingState");
            fieldInfo.SetValue(this.cookingPotItem, nextState);
            AccessTools.Method(typeof(CookingPotItem), "UpdateParticles").Invoke(this.cookingPotItem, null);
            fieldInfo.SetValue(this.cookingPotItem, originalState);
            nextState--;
            this.elapsed = 0;
        }

        public void SetEnabled(bool enable)
        {
            if (this.enabled == enable)
            {
                return;
            }

            if (enable)
            {
                this.enabled = true;
                this.elapsed = 0;
                this.lastUpdate = 0;
                this.originalState = this.cookingPotItem.GetCookingState();
                this.nextState = originalState - 1;
            }
            else
            {
                this.enabled = false;
            }
        }
    }
}
