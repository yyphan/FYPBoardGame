using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoardGame
{
    public class ParryStanceAbilityBehaviour : SingleAllyAbilityBehaviour
    {
        protected override void AbilityHappened(bool abilityConnected = true)
        {
            if (abilityConnected)
            {
                abilityTarget.PreventNextNonPenetrateDamage();
            }

            base.AbilityHappened();
        }
    }
}
