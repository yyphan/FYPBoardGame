using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoardGame
{
    public class MeteorAbilityBehaviour : AreaAttackAbilityBehaviour
    {
        MageActiveEventsBehaviour activeEventsBehaviour;

        private const int METEOR_ROUNDS = 1;

        protected override void Awake()
        {
            base.Awake();

            activeEventsBehaviour = (MageActiveEventsBehaviour) parentChampionController.activeEventsBehaviour;
        }

        protected override void AbilityHappened(bool abilityConnected = true)
        {
            if (abilityConnected)
            {
                activeEventsBehaviour.SetupMeteor(this, METEOR_ROUNDS);
            }

            tileManager.RemoveStatusForAll(TileStatus.Active);
            tileManager.RemoveStatusForAll(TileStatus.Selected);

            StartAbilityCooldown();
            isSelectingArea = false;

            thisAbilityFinished.Invoke();
        }

        public void ActivateMeteor()
        {
            ApplyAbilityToTargetOnTiles(targetTiles);
            activeEventsBehaviour.UnsetMeteor();
        }

    }
}
