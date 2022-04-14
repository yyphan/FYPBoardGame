using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoardGame
{
    public class SnipeActiveEventBehaviour : BaseAbilityBehaviour
    {
        private ArcherActiveEventsBehaviour championActiveEventsBehaviour;
        private SingleTargetAttackAbilityBehaviour snipeNextShot;

        private const int SNIPE_ROUNDS = 1;
        private const int ENHANCE_ACCURACY_OFFSET = 2;
        private const int ENHANCE_VALUE_OFFSET = 2;
        private const int REDUCE_ACCURACY_OFFSET = -14;
        private const int REDUCE_VALUE_OFFSET = -4;

        protected override void Awake()
        {
            base.Awake();

            championActiveEventsBehaviour = parentChampionController.GetComponent<ArcherActiveEventsBehaviour>();
            snipeNextShot = gameObject.transform.parent.GetComponentInChildren<SingleTargetAttackAbilityBehaviour>();

            // define UnityAction for being attacked during sniping
            championActiveEventsBehaviour.snipeAttackedWhenActiveAction += ReduceNextShot;
            // this is called upon deactivation of snipe
            championActiveEventsBehaviour.snipeDeactivationEvents.AddListener(() =>
                {
                    EnhanceNextShot();
                    AbilityHappened();
                }
            );

            // make sure all offsets are effective only for the next one shot and no further shots
            snipeNextShot.thisAbilityFinished.AddListener(() =>
                {
                    snipeNextShot.ResetAccuracyOffset();
                    snipeNextShot.ResetValueOffset();
                }
            );
        }

        public override void StartAbility()
        {
            championActiveEventsBehaviour.SetupSnipeActiveEvent(SNIPE_ROUNDS);
        }

        protected override void AbilityHappened(bool abilityConnected = true)
        {
            StartAbilityCooldown();
            thisAbilityFinished.Invoke();
        }

        private void EnhanceNextShot()
        {
            snipeNextShot.ApplyAccuracyOffset(ENHANCE_ACCURACY_OFFSET);
            snipeNextShot.ApplyValueOffset(ENHANCE_VALUE_OFFSET);
        }

        private void ReduceNextShot()
        {
            snipeNextShot.ApplyAccuracyOffset(REDUCE_ACCURACY_OFFSET);
            snipeNextShot.ApplyValueOffset(REDUCE_VALUE_OFFSET);
        }
    }
}

