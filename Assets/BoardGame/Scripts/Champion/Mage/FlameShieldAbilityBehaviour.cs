using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoardGame
{
    public class FlameShieldAbilityBehaviour : SingleAllyAbilityBehaviour
    {
        private ChampionActiveEventsBehaviour targetChampionActiveEventsBehaviour;

        private const int FLAME_SHIELD_ROUNDS = 2;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void AbilityHappened(bool abilityConnected = true)
        {
            if (abilityConnected)
            {
                targetChampionActiveEventsBehaviour = abilityTarget.GetComponent<ChampionActiveEventsBehaviour>();
                targetChampionActiveEventsBehaviour.SetupFlameShield(FLAME_SHIELD_ROUNDS);
            }

            base.AbilityHappened(abilityConnected);
        }
    }
}
