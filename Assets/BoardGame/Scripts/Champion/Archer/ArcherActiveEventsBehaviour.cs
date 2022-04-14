using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Inspector;

namespace BoardGame
{
    public class ArcherActiveEventsBehaviour : ChampionActiveEventsBehaviour
    {
        [Header("Snipe")]
        [ReadOnly] public bool isSnipeActive = false;
        [SerializeField]
        [ReadOnly] private int snipeRemainingRounds = 0;

        public override void UpdateActiveEvents()
        {
            base.UpdateActiveEvents();

            if (snipeRemainingRounds > 0)
            {
                snipeRemainingRounds--;
                if (snipeRemainingRounds == 0) UnsetSnipeActiveEvent();
            }
        }

        public void SetupSnipeActiveEvent(int roundsToGo)
        {
            snipeRemainingRounds += roundsToGo;
            isSnipeActive = true;
            thisChampion.championBeingAttackedEvents.AddListener(snipeAttackedWhenActiveAction);
        }

        private void UnsetSnipeActiveEvent()
        {
            if (isSnipeActive)
            {
                isSnipeActive = false;
                thisChampion.championBeingAttackedEvents.RemoveListener(snipeAttackedWhenActiveAction);
                snipeDeactivationEvents.Invoke();
                snipeDeactivationEvents.RemoveAllListeners();
            }
        }

        public override void ResetAllActiveEvents()
        {
            base.ResetAllActiveEvents();

            if (isSnipeActive)
            {
                isSnipeActive = false;
                thisChampion.championBeingAttackedEvents.RemoveListener(snipeAttackedWhenActiveAction);
                snipeDeactivationEvents.RemoveAllListeners();
            }
        }
    }
}

