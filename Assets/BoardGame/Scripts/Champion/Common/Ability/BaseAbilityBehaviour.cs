using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utilities.Inspector;

namespace BoardGame
{
    public abstract class BaseAbilityBehaviour : MonoBehaviour
    {
        [Header("Ability Data")]
        public BaseAbilityData data;
        public TileManager tileManager;

        private RoundTimer roundTimer;

        [Header("States")]
        [SerializeField]
        [ReadOnly] public bool cooldownActive;
        [SerializeField]
        [ReadOnly] public bool abilityReady;
        [SerializeField]
        [ReadOnly] public bool abilitySilenced;

        protected ChampionController parentChampionController;

        [Header("Events")]
        public UnityEvent thisAbilityFinished;

        [Header("AI")]
        protected TileBehaviour tileChosenByAI = null;
        public static int AI_BASE_DELAY = 1;

        protected virtual void Awake()
        {
            parentChampionController = GetComponentInParent<ChampionController>();
        }

        public void SetupAbilityCooldownTimer()
        {
            roundTimer = new RoundTimer(data.cooldownRounds);
            cooldownActive = false;
            abilityReady = true;
        }

        public void CheckAbilityCooldown()
        {
            if (cooldownActive)
            {
                roundTimer.UpdateTimer();
                if (roundTimer.HasElapsed())
                {
                    AbilityCooldownFinished();
                    roundTimer.EndTimer();
                    roundTimer.Reset();
                }
            }
        }

        public abstract void StartAbility();

        protected abstract void AbilityHappened(bool abilityConnected = true);

        protected void ShowActiveTiles() 
        {
            int range = data.range;
            int x = parentChampionController.xCoordinate;
            int y = parentChampionController.yCoordinate;

            tileManager.RemoveStatusForAll(TileStatus.Active);
            tileManager.AddStatusForRange(x, y, range, TileStatus.Active);
        }

        public void AIChoseTile(TileBehaviour tile)
        {
            tileChosenByAI = tile;
        }

        protected void UpdateSelectedTile(TileBehaviour tile)
        {
            tileManager.RemoveStatusForAll(TileStatus.Selected);
            tileManager.AddStatusForTile(tile, TileStatus.Selected);
        }

        protected void StartAbilityCooldown()
        {
            if (data.cooldownRounds > 0)
            {
                cooldownActive = true;
                abilityReady = false;
            }
        }

        protected void AbilityCooldownFinished()
        {
            cooldownActive = false;
            abilityReady = true;
        }
    }
}
