using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utilities.Inspector;

namespace BoardGame
{
    public class ChampionActiveEventsBehaviour : BaseActiveEventsBehaviour
    {
        protected ChampionController thisChampion;

        [Header("Preventing Non Penetrate Damage")]
        [ReadOnly] public bool isPreventingNonPenetrateDamage = false;
        [SerializeField]
        [ReadOnly] private int preventingNPDRemainingRounds = 0;

        [Header("Flame Shield")]
        [ReadOnly] public bool isFlameShielded = false;
        [SerializeField]
        [ReadOnly] protected int flameShieldRemainingRounds = 0;

        [Header("Events")]
        public UnityEvent snipeDeactivationEvents;
        public UnityAction snipeAttackedWhenActiveAction;

        public GameObject FSSprite;
        public GameObject PSSprite;

        private void Awake()
        {
            thisChampion = GetComponent<ChampionController>();
        }

        public virtual void UpdateActiveEvents()
        {
            if (isFlameShielded)
            {
                if (flameShieldRemainingRounds > 0)
                {
                    flameShieldRemainingRounds--;
                    if (flameShieldRemainingRounds == 0) UnsetFlameShield();
                }
            }
        }

        public void PreventNextNonPenetrateDamage()
        {
            preventingNPDRemainingRounds++;
            isPreventingNonPenetrateDamage = true;
            // enable the image to display
             PSSprite.SetActive(true);
        }

        public void NonPenetrateDamagePrevented()
        {
            preventingNPDRemainingRounds--;
            if (preventingNPDRemainingRounds == 0)
            {
                isPreventingNonPenetrateDamage = false;
                // disable the image
                PSSprite.SetActive(false);
            }
        }

        public void SetupFlameShield(int rounds)
        {
            flameShieldRemainingRounds += rounds;
            if (!isFlameShielded)
            {
                isFlameShielded = true;
                thisChampion.SetupFlameShieldListener();
                FSSprite.SetActive(true);
            }
        }

        public void UnsetFlameShield()
        {
            if (isFlameShielded)
            {
                isFlameShielded = false;
                flameShieldRemainingRounds = 0;
                thisChampion.UnsetFlameShieldListener();
                FSSprite.SetActive(false);
            }
        }

        public virtual void ResetAllActiveEvents()
        {
            UnsetFlameShield();
            preventingNPDRemainingRounds = 0;
            isPreventingNonPenetrateDamage = false;
        }
    }
}
