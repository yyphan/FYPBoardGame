using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Inspector;

namespace BoardGame
{
    public class MageActiveEventsBehaviour : ChampionActiveEventsBehaviour
    {
        [SerializeField]
        [ReadOnly] private bool isMeteorActive = false;
        [SerializeField]
        [ReadOnly] private int meteorRemainingRounds = 0;
        [SerializeField]
        [ReadOnly] private MeteorAbilityBehaviour abilityBehaviour;

        public override void UpdateActiveEvents()
        {
            base.UpdateActiveEvents();
            if (isMeteorActive)
            {
                meteorRemainingRounds--;
                if (meteorRemainingRounds == 0) abilityBehaviour.ActivateMeteor();
            }
        }

        public void SetupMeteor(MeteorAbilityBehaviour ability, int rounds)
        {
            abilityBehaviour = ability;
            meteorRemainingRounds += rounds;
            isMeteorActive = true;
        }

        public void UnsetMeteor()
        {
            isMeteorActive = false;
            meteorRemainingRounds = 0;
        }

        public override void ResetAllActiveEvents()
        {
            base.ResetAllActiveEvents();

            UnsetMeteor();
        }
    }
}

