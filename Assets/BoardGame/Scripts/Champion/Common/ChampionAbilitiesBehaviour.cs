using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoardGame
{
    public class ChampionAbilitiesBehaviour : MonoBehaviour
    {
        const int FULL_AP = 3;
        public static int MIN_ACCURACY = 1;
        public static int MAX_ACCURACY = 20;
        public BaseAbilityBehaviour[] abilities;

        public int currentAP;

        public void SetupAbilities()
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                abilities[i].SetupAbilityCooldownTimer();
            }
        }

        public bool canCastAbility(int index)
        {
            BaseAbilityBehaviour ability = abilities[index];
            int APCost = ability.data.APCost;

            if (!ability.abilityReady)
            {
                Debug.LogWarning("Ability in Cooldown");
                return false;
            }
            if (ability.abilitySilenced)
            {
                Debug.LogWarning("Champion is Silenced");
                return false;
            }
            if (APCost > currentAP)
            {
                Debug.LogWarning("No Enough AP");
                return false;
            }

            return true;
        }

        public void StartAbility(int index)
        {
            if (!canCastAbility(index)) return;

            BaseAbilityBehaviour ability = abilities[index];
            int APCost = ability.data.APCost;
            currentAP -= APCost;

            ability.StartAbility();
            Debug.Log(gameObject.name + " has casted " + ability.data.abilityName);
        }

        public void UpdateForNewRound()
        {
            currentAP = FULL_AP;
            UpdateAbilitiesCooldown();
        }

        private void UpdateAbilitiesCooldown()
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                abilities[i].CheckAbilityCooldown();
            }
        }

        public void ReceiveDebuffSilence()
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                abilities[i].abilitySilenced = true;
            }
        }

        public void CancelDebuffSilence()
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                abilities[i].abilitySilenced = false;
            }
        }
    }

}
