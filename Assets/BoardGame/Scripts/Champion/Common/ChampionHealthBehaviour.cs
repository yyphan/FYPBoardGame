using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utilities.Inspector;

namespace BoardGame
{
    public class ChampionHealthBehaviour : MonoBehaviour
    {
        [Header("Health Info")]
        [SerializeField]
        [ReadOnly] private int currentArmor;
        [SerializeField]
        [ReadOnly] private int currentHealth;

        [Header("Events")]
        public UnityEvent healthDifferenceEvent;
        public UnityEvent healthIsZeroEvent;

        // delegates?

        public void SetupCurrentHealth(int totalHealth)
        {
            currentHealth = totalHealth;
        }

        public void SetupCurrentArmor(int totalArmor)
        {
            currentArmor = totalArmor;
        }

        public void ChangeArmor(int armorDifference)
        {
            int overkillValue = currentArmor + armorDifference;
            if (overkillValue < 0)
            {
                ChangeHealth(overkillValue);
                healthDifferenceEvent.Invoke();
                currentArmor--;
            }
            if (currentArmor < 0)
            {
                currentArmor = 0;
            }
        }

        public void ChangeHealth(int healthDifference)
        {
            currentHealth = currentHealth + healthDifference;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                healthIsZeroEvent.Invoke();
            }
        }

        public int GetCurrentArmor()
        {
            return currentArmor;
        }

        public int GetCurrentHealth()
        {
            return currentHealth;
        }
    }
}

