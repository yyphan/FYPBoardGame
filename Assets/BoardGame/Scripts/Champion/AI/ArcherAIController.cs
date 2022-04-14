using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Inspector;

namespace BoardGame
{
    public class ArcherAIController : EnemyController
    {
        [Header("Ability Indices")]
        [SerializeField]
        [ReadOnly] private const int snipeAbilityIndex = 0;
        [SerializeField]
        [ReadOnly] private const int twinShotAbilityIndex = 1;
        [SerializeField]
        [ReadOnly] private const int flipShotAbilityIndex = 2;
        [SerializeField]
        [ReadOnly] private const int wrathOfPhoenixAbilityIndex = 3;

        [Header("Archer Debug")]
        public TileBehaviour secondTile;

        [Header("Decision Metrics")]
        [SerializeField]
        [ReadOnly] private int archerSafeDistance = 4;
        [SerializeField]
        [ReadOnly] private ChampionController attackTarget = null;
        [SerializeField]
        [ReadOnly] private TileBehaviour tileToFlip = null;
        [SerializeField]
        [ReadOnly] private float minimalHealth = 12; // aim for minimal expected health of player
        [SerializeField]
        [ReadOnly] private int abilityIndex = -1;

        public override void StartNewRound()
        {
            base.StartNewRound();

            ResetDecisionMetrics();

            // Archer AI's thinking: Stay Alive > Keep Distance >= Attack
            // - Stay Alive:
            //   * Move to Blademaster when health is critical and do not have protection
            // - Keep Distance:
            //   * Try to keep 4 tiles away from all player Champions by the end of turn
            // - Attack:
            //   * My best attack range (according to accuracy) is 2, second bests are 1 and 3. I will not attack in other ranges.
            //   * If I am at least 4 tiles away from all players, cast Wrath of Phoenix (or Twin Shot).
            //   * If not, cast Flip Shot (or Twin Shot) and move away from player champions
            
            // Archer AI's priorities:
            if (healthBehaviour.GetCurrentHealth() <= criticalHP          // 1. if health is critical 
                && !activeEventsBehaviour.isPreventingNonPenetrateDamage) //    and not being protected by Parry Stance, go to campanion Blademaster
            {
                TileBehaviour targetTile = companionChampion.GetCurrentTile();
                int maxSteps = abilitiesBehaviour.currentAP / abilitiesBehaviour.abilities[moveAbilityIndex].data.APCost;
                StartCoroutine(MoveTowardsTile(targetTile, maxSteps));
                return;
            }

            if (IsWithinSafeDistance(archerSafeDistance))                     // 2. if range with all player champions are at least 4                                                              
            {
                if (abilitiesBehaviour.canCastAbility(wrathOfPhoenixAbilityIndex)) // cast WOP if possible
                {
                    ConsiderCastWOP();
                }
                else if (abilitiesBehaviour.canCastAbility(twinShotAbilityIndex))  // better than doing nothing 
                {
                    ConsiderCastTwinShot();
                }
            } 
            else
            {
                if (abilitiesBehaviour.canCastAbility(flipShotAbilityIndex)) // cast Flip Shot if possible, then move away
                {
                    ConsiderCastFlipShot();
                }
                else if (abilitiesBehaviour.canCastAbility(twinShotAbilityIndex))  // Twin Shot is second choice, then move away
                {
                    ConsiderCastTwinShot();
                }
            }

            if (attackTarget != null)
            {
                StartCoroutine(CastAttackAbility(abilityIndex));
            }
            else // no worthy target is found
            {
                MoveAwayFromPlayers();
            }
        }

        private void MoveAwayFromPlayers()
        {
            int maxSteps = abilitiesBehaviour.currentAP / abilitiesBehaviour.abilities[moveAbilityIndex].data.APCost;
            StartCoroutine(MoveAway(maxSteps));
        }

        private void ResetDecisionMetrics()
        {
            attackTarget = null; 
            tileToFlip = null;
            minimalHealth = 12;
            abilityIndex = -1;
        }

        private void ConsiderCastWOP()
        {
            foreach(ChampionController playerChampion in playerChampions)
            {
                CalculateExpectWOP(playerChampion);
            }
        }

        private void ConsiderCastFlipShot()
        {
            foreach (ChampionController playerChampion in playerChampions)
            {
                CalculateExpectFlipShot(playerChampion);
            }
        }

        private void ConsiderCastTwinShot()
        {
            foreach (ChampionController playerChampion in playerChampions)
            {
                CalculateExpectTwinShot(playerChampion);
            }
        }

        private void CalculateExpectWOP(ChampionController playerChampion)
        {
            TileBehaviour targetTile = playerChampion.GetCurrentTile();
            WrathOfPhoenixBehaviour WOP = (WrathOfPhoenixBehaviour) abilitiesBehaviour.abilities[wrathOfPhoenixAbilityIndex];

            // we focus on minimizing player's(s') health
            int playerHealth = playerChampion.GetCurrentHealth();
            // player's(s') health when ability hit or miss
            int playerHealthOnMiss = playerHealth;
            int playerHealthOnHit = playerHealth;

            // akward solution: using one playerHealh as expect, if more than one player is covered, further decrease the playerHealth to signal more damage is dealt
            List<TileBehaviour> tilesInArea = tileManager.GetTilesInArea(targetTile, WOP.GetAreaWidth(), WOP.GetAreaLength());
            foreach (TileBehaviour tileHit in tilesInArea)
            {
                GameObject objectHit = tileHit.GetObjectOnTile();
                if (objectHit == null) continue;
                if (objectHit.CompareTag("Player")) // reduce playerHealth for every player champion covered in the area
                {
                    ChampionController playerChampionHit = objectHit.GetComponent<ChampionController>();

                    int playerArmor = playerChampionHit.GetCurrentArmor();
                    int armorPierceOverkill = playerArmor + WOP.data.GetAbilityValue();
                    if (armorPierceOverkill < 0) playerHealthOnHit += armorPierceOverkill;
                    playerHealthOnHit -= TileActiveEventsBehaviour.WOPBurn;
                    // WOP applies burning ground even if missed
                    playerHealthOnMiss -= TileActiveEventsBehaviour.WOPBurn;
                }
            }

            // calculate the possibility for this ability to connect (hit)
            int range = tileManager.GetIntegerDistance(this, playerChampion);
            int actualAccuracy = GetActualAccuracy(range);
            int accuracyRange = ChampionAbilitiesBehaviour.MAX_ACCURACY - ChampionAbilitiesBehaviour.MIN_ACCURACY + 1;
            float hitPossibility = (float)actualAccuracy / (float)accuracyRange;

            // calculate playerHealthExpect
            float playerHealthExpect = playerHealthOnHit * hitPossibility + playerHealthOnMiss * (1 - hitPossibility);
            
            // if this ability results in lower expect of player's health 
            // favor this ability
            if (playerHealthExpect < minimalHealth)
            {
                minimalHealth = playerHealthExpect;
                attackTarget = playerChampion;
                abilityIndex = wrathOfPhoenixAbilityIndex;
            }
        }

        private void CalculateExpectFlipShot(ChampionController playerChampion)
        {
            SequentialAbilityBehaviour flipShot = (SequentialAbilityBehaviour) abilitiesBehaviour.abilities[flipShotAbilityIndex];

            // we focus on minimizing player's health
            int playerHealth = playerChampion.GetCurrentHealth();
            // player's health when ability hit or miss
            int playerHealthOnMiss = playerHealth;
            int playerHealthOnHit = playerHealth;

            int playerArmor = playerChampion.GetCurrentArmor();
            int armorPierceOverkill = playerArmor + flipShot.data.GetAbilityValue();
            if (armorPierceOverkill < 0) playerHealthOnHit += armorPierceOverkill;

            // calculate the possibility for this ability to connect (hit)
            int range = tileManager.GetIntegerDistance(this, playerChampion);
            int actualAccuracy = GetActualAccuracy(range);
            int accuracyRange = ChampionAbilitiesBehaviour.MAX_ACCURACY - ChampionAbilitiesBehaviour.MIN_ACCURACY + 1;
            float hitPossibility = (float)actualAccuracy / (float)accuracyRange;

            // calculate playerHealthExpect
            float playerHealthExpect = playerHealthOnHit * hitPossibility + playerHealthOnMiss * (1 - hitPossibility);

            // if this ability results in lower expect of player's health 
            // favor this ability
            if (playerHealthExpect < minimalHealth)
            {
                minimalHealth = playerHealthExpect;
                attackTarget = playerChampion;
                tileToFlip = GetNextStepAwayFromPlayerChampions();
                abilityIndex = flipShotAbilityIndex;
            }
        }

        private void CalculateExpectTwinShot(ChampionController playerChampion)
        {
            SequentialAbilityBehaviour twinShot = (SequentialAbilityBehaviour)abilitiesBehaviour.abilities[twinShotAbilityIndex];
            int twinShotAccuracyOffset = twinShot.abilitySequence[0].data.GetAccuracyOffset();

            // we focus on minimizing player's health
            int playerHealth = playerChampion.GetCurrentHealth();
            // player's health when ability hit or miss
            int playerHealthOnMiss = playerHealth;
            int playerHealthOnHit = playerHealth;

            int playerArmor = playerChampion.GetCurrentArmor();
            int armorPierceOverkill = playerArmor + twinShot.data.GetAbilityValue();
            if (armorPierceOverkill < 0) playerHealthOnHit += armorPierceOverkill;

            // calculate the possibility for this ability to connect (hit)
            int range = tileManager.GetIntegerDistance(this, playerChampion);
            int actualAccuracy = GetActualAccuracy(range) + twinShotAccuracyOffset;
            int accuracyRange = ChampionAbilitiesBehaviour.MAX_ACCURACY - ChampionAbilitiesBehaviour.MIN_ACCURACY + 1;
            float hitPossibility = (float)actualAccuracy / (float)accuracyRange;

            // calculate playerHealthExpect
            float playerHealthExpect = playerHealthOnHit * hitPossibility + playerHealthOnMiss * (1 - hitPossibility);

            // if this ability results in lower expect of player's health 
            // favor this ability
            if (playerHealthExpect < minimalHealth)
            {
                minimalHealth = playerHealthExpect;
                attackTarget = playerChampion;
                abilityIndex = twinShotAbilityIndex;
            }
        }

        protected bool TryCastAbilitySnipe()
        {
            // gg
            return false;
        }

        protected bool TryCastAbilityTwinShot(TileBehaviour target1, TileBehaviour target2)
        {
            if (!abilitiesBehaviour.canCastAbility(twinShotAbilityIndex)) return false;

            if (target1.GetObjectOnTile() == null || target2.GetObjectOnTile() == null)
            {
                Debug.LogWarning("Invalid tile. No valid target on tile.");
                return false;
            }

            SequentialAbilityBehaviour twinShotAbility = (SequentialAbilityBehaviour) abilitiesBehaviour.abilities[twinShotAbilityIndex];
            SingleTargetAttackAbilityBehaviour firstShot = (SingleTargetAttackAbilityBehaviour) twinShotAbility.abilitySequence[0];
            SingleTargetAttackAbilityBehaviour secondShot = (SingleTargetAttackAbilityBehaviour) twinShotAbility.abilitySequence[1];

            firstShot.AIChoseTile(target1);
            secondShot.AIChoseTile(target2);

            abilitiesBehaviour.StartAbility(twinShotAbilityIndex);

            return true;
        }

        protected bool TryCastAbilityFlipShot(TileBehaviour shotTarget, TileBehaviour moveTarget)
        {
            if (!abilitiesBehaviour.canCastAbility(flipShotAbilityIndex)) return false;

            if (shotTarget.GetObjectOnTile() == null)
            {
                Debug.LogWarning("Invalid tile. No valid target on tile.");
                return false;
            }
            else if (moveTarget.GetObjectOnTile() != null)
            {
                Debug.LogWarning("Invalid tile for move. Tile is occupied.");
                return false;
            }

            SequentialAbilityBehaviour flipShotAbility = (SequentialAbilityBehaviour) abilitiesBehaviour.abilities[flipShotAbilityIndex];
            SingleTargetAttackAbilityBehaviour shot = (SingleTargetAttackAbilityBehaviour) flipShotAbility.abilitySequence[0];
            MoveAbilityBehaviour move = (MoveAbilityBehaviour)flipShotAbility.abilitySequence[1];

            shot.AIChoseTile(shotTarget);
            move.AIChoseTile(moveTarget);

            abilitiesBehaviour.StartAbility(flipShotAbilityIndex);

            return true;
        }

        protected bool TryCastAbilityWOP(TileBehaviour centerTile)
        {
            if (!abilitiesBehaviour.canCastAbility(wrathOfPhoenixAbilityIndex)) return false;

            WrathOfPhoenixBehaviour WOPAbility = (WrathOfPhoenixBehaviour)abilitiesBehaviour.abilities[wrathOfPhoenixAbilityIndex];
            WOPAbility.AIChoseTile(centerTile);

            abilitiesBehaviour.StartAbility(wrathOfPhoenixAbilityIndex);

            return true;
        }

        private IEnumerator CastAttackAbility(int index)
        {
            TileBehaviour targetTile = attackTarget.GetCurrentTile();
            switch (index)
            {
                case wrathOfPhoenixAbilityIndex:
                    TryCastAbilityWOP(targetTile);
                    break;
                case flipShotAbilityIndex:
                    TryCastAbilityFlipShot(targetTile, tileToFlip);
                    break;
                case twinShotAbilityIndex:
                    TryCastAbilityTwinShot(targetTile, targetTile);
                    break;
            }

            // wait for animation to finish
            yield return new WaitForSeconds(BaseAbilityBehaviour.AI_BASE_DELAY * 2);

            // always end the turn by getting away from players
            MoveAwayFromPlayers();
        }

        private IEnumerator MoveAway(int maxSteps)
        {
            while (maxSteps > 0)
            {
                TileBehaviour nextStep = GetNextStepAwayFromPlayerChampions();
                TryCastAbilityMove(nextStep);
                maxSteps--;

                yield return new WaitForSeconds(BaseAbilityBehaviour.AI_BASE_DELAY * 2);
            }

            endTurnEvent.Invoke();
        }
    }
}
