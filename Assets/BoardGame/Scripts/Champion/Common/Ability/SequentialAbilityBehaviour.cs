using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Inspector;

namespace BoardGame
{
    public class SequentialAbilityBehaviour : BaseAbilityBehaviour
    {
        public List<BaseAbilityBehaviour> abilitySequence;
        [SerializeField]
        [ReadOnly] private int currentAbilityIndex;
        private const float SEQ_ABILITY_DELAY = 0.5f;

        public override void StartAbility()
        {
            currentAbilityIndex = -1;
            NextAbilityInSequence();
        }

        public void NextAbilityInSequence()
        {
            currentAbilityIndex++;
            if (currentAbilityIndex == abilitySequence.Count) // if all sub abilities had happened
            {
                AbilityHappened();
            }
            else if (currentAbilityIndex == 0) // no delay for the first sub ability
            {
                abilitySequence[currentAbilityIndex].StartAbility();
            }
            else // set delay for subsequent abilties
            {
                StartCoroutine(StartNextAbility());
            }
        }

        protected override void AbilityHappened(bool abilityConnected = true)
        {
            StartAbilityCooldown();
        }

        private IEnumerator StartNextAbility()
        {
            yield return new WaitForSeconds(SEQ_ABILITY_DELAY);

            abilitySequence[currentAbilityIndex].StartAbility();
        }
    }
}
