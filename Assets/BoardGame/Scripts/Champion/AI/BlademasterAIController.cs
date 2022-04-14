using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Inspector;

namespace BoardGame
{
    public class BlademasterAIController : EnemyController
    {
        [Header("Ability Indices")]
        [SerializeField]
        [ReadOnly] private const int slashAbilityIndex = 0;
        [SerializeField]
        [ReadOnly] private const int elbowStrikeAbilityIndex = 1;
        [SerializeField]
        [ReadOnly] private const int sprintAbilityIndex = 2;
        [SerializeField]
        [ReadOnly] private const int parryStanceAbilityIndex = 3;

        [Header("Decision Metrics")]
        [SerializeField]
        [ReadOnly] private ChampionController attackTarget = null;
        [SerializeField]
        [ReadOnly] private TileBehaviour attackPosition = null; // my position when I attack (for the best outcome)
        [SerializeField]
        [ReadOnly] private float minimalHealth = 12; // aim for minimal expected health of player
        [SerializeField]
        [ReadOnly] private int abilityIndex = -1;


        public override void StartNewRound()
        {
            base.StartNewRound();

            ResetDecisionMetrics();

            // Blademaster AI's thinking: Protecting Companinon > Attack >= Take Best Position
            // - Protecting Companinon:
            //   * I should prioritize protecting my companion (over attacking playerChampion)
            // - Attack:
            //   * I will choose action that results in the lowest health of playerChampion
            // - Take Best Position:
            //   * I should get as close to the best position (center tile) as possible when I have unused AP

            // Blademaster AI's priorities:
            if (CheckProtectingCompanion()) // 1. should I cast Parry Stance to my companion?
            {
                return;
            }
            else if (CheckAttacking())      // 2. attack the best target, then move towards best position
            {
                return;
            }
            else                            // 3. if I cannot do 1 nor 2, move towards the best position
            {
                TakeBestPossiblePosition();
            }
        }

        private void ResetDecisionMetrics()
        {
            attackTarget = null;
            attackPosition = null;
            minimalHealth = 12;
            abilityIndex = -1;
        }

        private bool CheckProtectingCompanion()
        {
            // check if companion is in low health
            if (companionChampion.IsChampionAlive() 
                && companionChampion.GetCurrentHealth() <= criticalHP
                && !companionChampion.activeEventsBehaviour.isPreventingNonPenetrateDamage)
            {
                // if within range and can cast Parry Stance, cast it
                if (GetIntegerDistance(companionChampion) == 1 && abilitiesBehaviour.canCastAbility(parryStanceAbilityIndex))
                {
                    TileBehaviour companionTile = companionChampion.GetCurrentTile();
                    StartCoroutine(CastParryStance(companionTile));
                    return true;
                }
            }

            return false;
        }

        private bool CheckAttacking()
        {
            // 1. check if we can attack any player champion, choose target with best outcome 
            //    ImagineSmth() means I am thinking about doing smth, including moving myself or casting abilities
            //    everything inside DOES NOT actually happen
            foreach (ChampionController playerChampion in playerChampions)
            {
                ImagineCastAbilitySlash(playerChampion);
                ImagineCastAbilityElbowStrike(playerChampion);
            }

            // 2. this is where attack will ACTUALLY HAPPEN (if a target was found in 1.)
            if (attackTarget != null) // not null means worthy target has been found, attack now
            {
                StartCoroutine(CastAttackAbility(abilityIndex));
                return true;
            }

            return false;
        }

        private void TakeBestPossiblePosition()
        {
            TileBehaviour bestPosition = tileManager.GetCenterTile();
            int maxSteps = abilitiesBehaviour.currentAP / abilitiesBehaviour.abilities[moveAbilityIndex].data.APCost;
            StartCoroutine(MoveTowardsTile(bestPosition, maxSteps));
        }

        // ImagineSmth() means I am thinking about doing smth, including moving myself or casting abilities
        // everything inside DOES NOT actually happen
        private void ImagineCastAbilitySlash(ChampionController playerChampion)
        {
            if (!abilitiesBehaviour.canCastAbility(slashAbilityIndex)) return;

            // 1. get as close as possible, before I run out of AP (still need enough AP to cast the ability)
            TileBehaviour closestPositionSoFar = GetCurrentTile();
            TileBehaviour targetTile = playerChampion.GetCurrentTile();
            int remainingAP = abilitiesBehaviour.currentAP;
            int slashAP = abilitiesBehaviour.abilities[slashAbilityIndex].data.APCost;
            int moveAP = abilitiesBehaviour.abilities[moveAbilityIndex].data.APCost;
            // total AP available for moving
            remainingAP = remainingAP - slashAP;
            // get as close as possible
            while (remainingAP > 0 && CanGetCloserToTarget(closestPositionSoFar, targetTile))
            {
                TileBehaviour nextTile = pathfinder.GetNextTileInPath(closestPositionSoFar, targetTile);
                // update the closest position so far
                closestPositionSoFar = nextTile;
                remainingAP -= moveAP;
            }

            // 2. now we are at the closest position, attack if we can
            int abilityRange = abilitiesBehaviour.abilities[slashAbilityIndex].data.range;
            int distanceToTarget = tileManager.GetIntegerDistance(playerChampion, closestPositionSoFar);

            if (distanceToTarget <= abilityRange)
            {
                CalculateExpectSlash(closestPositionSoFar, playerChampion);
            }
        }

        private void ImagineCastAbilityElbowStrike(ChampionController playerChampion)
        {
            if (!abilitiesBehaviour.canCastAbility(elbowStrikeAbilityIndex)) return;

            // 1. get as close as possible, before I run out of AP (still need enough AP to cast the ability)
            TileBehaviour closestPositionSoFar = GetCurrentTile();
            TileBehaviour targetTile = playerChampion.GetCurrentTile();
            int remainingAP = abilitiesBehaviour.currentAP;
            int elbowStrikeAP = abilitiesBehaviour.abilities[elbowStrikeAbilityIndex].data.APCost;
            int moveAP = abilitiesBehaviour.abilities[moveAbilityIndex].data.APCost;
            // total AP available for moving
            remainingAP = remainingAP - elbowStrikeAP;
            // get as close as possible
            while (remainingAP > 0 && CanGetCloserToTarget(closestPositionSoFar, targetTile))
            {
                TileBehaviour nextTile = pathfinder.GetNextTileInPath(closestPositionSoFar, targetTile);
                // update the closest position so far
                closestPositionSoFar = nextTile;
                remainingAP -= moveAP;
            }

            // 2. now we are at the closest position, attack if we can
            int abilityRange = abilitiesBehaviour.abilities[elbowStrikeAbilityIndex].data.range;
            int distanceToTarget = tileManager.GetIntegerDistance(playerChampion, closestPositionSoFar);

            if (distanceToTarget <= abilityRange)
            {
                CalculateExpectElbowStrike(closestPositionSoFar, playerChampion);
            }
        }

        private void CalculateExpectSlash(TileBehaviour attackFromTile, ChampionController playerChampion)
        {
            if (!abilitiesBehaviour.canCastAbility(slashAbilityIndex)) return;

            // we focus on minimizing player's health
            int playerHealth = playerChampion.GetCurrentHealth();
            // player's health does not change when the ability does not connect
            int playerHealthOnMiss = playerHealth;
            int playerHealthOnHit = playerHealth;

            int playerArmor = playerChampion.GetCurrentArmor();
            SingleTargetAttackAbilityBehaviour slash = (SingleTargetAttackAbilityBehaviour)abilitiesBehaviour.abilities[slashAbilityIndex];
            int armorPierceOverkill = playerArmor + slash.data.GetAbilityValue();

            // calculate player's health when the ability connects
            if (armorPierceOverkill < 0)
            {
                // damage and bleeding
                playerHealthOnHit = playerHealth + armorPierceOverkill - slash.data.GetDebuffs().bleeding;
            }

            // calculate the possibility for this ability to connect (hit)
            int range = tileManager.GetIntegerDistance(playerChampion, attackFromTile);
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
                attackPosition = attackFromTile;
                abilityIndex = slashAbilityIndex;
            }
        }

        private void CalculateExpectElbowStrike(TileBehaviour attackFromTile, ChampionController playerChampion)
        {
            if (!abilitiesBehaviour.canCastAbility(elbowStrikeAbilityIndex)) return;

            // we focus on minimizing player's health
            int playerHealth = playerChampion.GetCurrentHealth();
            // player's health does not change when the ability does not connect
            int playerHealthOnMiss = playerHealth;
            int playerHealthOnHit = playerHealth;

            int playerArmor = playerChampion.GetCurrentArmor();
            SingleTargetAttackAbilityBehaviour elbowStrike = (SingleTargetAttackAbilityBehaviour) abilitiesBehaviour.abilities[elbowStrikeAbilityIndex];
            int armorPierceOverkill = playerArmor + elbowStrike.data.GetAbilityValue();

            // calculate player's health when the ability connects
            if (armorPierceOverkill < 0)
            {
                // damage
                // TODO: he does not recognize the value of shock
                playerHealthOnHit = playerHealth + armorPierceOverkill;
            }

            // calculate the possibility for this ability to connect (hit)
            int range = tileManager.GetIntegerDistance(playerChampion, attackFromTile);
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
                attackPosition = attackFromTile;
                abilityIndex = elbowStrikeAbilityIndex;
            }
        }

        private bool TryCastAbilitySlash(TileBehaviour targetTile)
        {
            if (!abilitiesBehaviour.canCastAbility(slashAbilityIndex)) return false;

            GameObject targetObject = targetTile.GetObjectOnTile();
            if (targetObject == null || !targetObject.CompareTag("Player"))
            {
                Debug.LogWarning("Invalid tile. No valid target on tile.");
                return false;
            }

            SingleTargetAttackAbilityBehaviour slashAbility =
                (SingleTargetAttackAbilityBehaviour)abilitiesBehaviour.abilities[slashAbilityIndex];
            slashAbility.AIChoseTile(targetTile);

            abilitiesBehaviour.StartAbility(slashAbilityIndex);
            
            return true;
        }

        private bool TryCastAbilityElbowStrike(TileBehaviour targetTile)
        {
            if (!abilitiesBehaviour.canCastAbility(elbowStrikeAbilityIndex)) return false;

            GameObject targetObject = targetTile.GetObjectOnTile();
            if (targetObject == null || !targetObject.CompareTag("Player"))
            {
                Debug.LogWarning("Invalid tile. No valid target on tile.");
                return false;
            }

            SingleTargetAttackAbilityBehaviour elbowStrikeAbility =
                (SingleTargetAttackAbilityBehaviour)abilitiesBehaviour.abilities[elbowStrikeAbilityIndex];
            elbowStrikeAbility.AIChoseTile(targetTile);

            abilitiesBehaviour.StartAbility(elbowStrikeAbilityIndex);

            return true;
        }

        private bool TryCastAbilitySprint(TileBehaviour targetTile)
        {
            // should I do range check here?
            if (!abilitiesBehaviour.canCastAbility(sprintAbilityIndex)) return false;

            MoveAbilityBehaviour sprintAbility = (MoveAbilityBehaviour)abilitiesBehaviour.abilities[sprintAbilityIndex];
            sprintAbility.AIChoseTile(targetTile);

            abilitiesBehaviour.StartAbility(sprintAbilityIndex);

            return true;
        }

        private bool TryCastAbilityParryStance(TileBehaviour targetTile)
        {
            if (!abilitiesBehaviour.canCastAbility(parryStanceAbilityIndex)) return false;

            GameObject targetObject = targetTile.GetObjectOnTile();
            if (targetObject == null || !targetObject.CompareTag("Enemy"))
            {
                Debug.LogWarning("Invalid tile. No valid target on tile.");
                return false;
            }

            SingleAllyAbilityBehaviour parryStanceAbility =
                (SingleAllyAbilityBehaviour)abilitiesBehaviour.abilities[parryStanceAbilityIndex];
            parryStanceAbility.AIChoseTile(targetTile);

            abilitiesBehaviour.StartAbility(parryStanceAbilityIndex);

            return true;
        }

        private IEnumerator CastAttackAbility(int index)
        {
            TileBehaviour targetTile = attackTarget.GetCurrentTile();
            // 1. get to the planned attack position
            while (GetCurrentTile() != attackPosition)
            {
                MoveOnceTowardsTile(attackPosition);

                // hardcoded, this must be longer than the AI_BASE_DELAY
                yield return new WaitForSeconds(BaseAbilityBehaviour.AI_BASE_DELAY * 2);
            }

            // 2. cast the ability
            switch (index)
            {
                case slashAbilityIndex:
                    TryCastAbilitySlash(targetTile);
                    break;
                case elbowStrikeAbilityIndex:
                    TryCastAbilityElbowStrike(targetTile);
                    break;
            }

            yield return new WaitForSeconds(BaseAbilityBehaviour.AI_BASE_DELAY);

            // 3. always end the turn by trying to get to the best position
            TakeBestPossiblePosition();
        }

        private IEnumerator CastParryStance(TileBehaviour targetTile)
        {
            TryCastAbilityParryStance(targetTile);

            yield return new WaitForSeconds(BaseAbilityBehaviour.AI_BASE_DELAY);

            TakeBestPossiblePosition();
        }
    }
}
