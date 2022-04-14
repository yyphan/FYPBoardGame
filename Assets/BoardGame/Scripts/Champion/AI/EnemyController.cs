using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utilities.Inspector;

namespace BoardGame
{
    public class EnemyController : ChampionController
    {
        [Header("Companion Champion")]
        [SerializeField]
        [ReadOnly] protected EnemyController companionChampion;

        [Header("Player Team Champions")]
        [SerializeField]
        [ReadOnly] protected List<ChampionController> playerChampions;

        [Header("Debug")]
        public TileBehaviour tile;
        public int moveAbilityIndex = 4;

        [Header("Constants")]
        [SerializeField]
        [ReadOnly] protected int criticalHP = 5;

        [Header("Pathfinding")]
        public TilePathfinder pathfinder;

        public UnityEvent endTurnEvent;

        public void SetCompanionChampion(EnemyController theOtherEnemyChampion)
        {
            companionChampion = theOtherEnemyChampion;
        }

        public void SetPlayerChampions(List<ChampionController> playerTeamChampions)
        {
            playerChampions = playerTeamChampions;
        }

        public override void StartNewRound()
        {
            base.StartNewRound();
        }

        protected bool TryCastAbilityMove(TileBehaviour targetTile)
        {
            if (!abilitiesBehaviour.canCastAbility(moveAbilityIndex)) return false;
            if (targetTile.GetObjectOnTile() != null)
            {
                Debug.LogWarning("Invalid tile for move. Tile is occupied.");
                return false;
            }

            MoveAbilityBehaviour moveAbility = (MoveAbilityBehaviour)abilitiesBehaviour.abilities[moveAbilityIndex];
            moveAbility.AIChoseTile(targetTile);

            abilitiesBehaviour.StartAbility(moveAbilityIndex);

            return true;
        }

        protected void MoveOnceTowardsTile(TileBehaviour targetTile)
        {
            TileBehaviour nextStep = pathfinder.GetNextTileInPath(GetCurrentTile(), targetTile);
            // if no path is found, I will not move 
            if (nextStep == GetCurrentTile()) return;

            TryCastAbilityMove(nextStep);
        }

        protected bool CanGetCloserToTarget(TileBehaviour startTile, TileBehaviour targetTile)
        {
            int distanceToTarget = tileManager.GetIntegerDistance(startTile, targetTile);

            // I am on target tile, I cannot get closer than 0
            if (GetCurrentTile() == targetTile) return false;
            // target tile is occupied, I cannot get closer than 1
            if (targetTile.GetObjectOnTile() != null && distanceToTarget == 1) return false; 

            // imagine taking the next step towards target
            TileBehaviour nextStep = pathfinder.GetNextTileInPath(startTile, targetTile);
            // no path to target is found, I cannot get closer 
            if (nextStep == GetCurrentTile()) return false;
            int newDistance = tileManager.GetIntegerDistance(nextStep, targetTile);
            // path to target is found, but it does not get me closer
            if (newDistance >= distanceToTarget) return false;

            // I can get closer if I take the next step towards target
            return true;
        }

        // if there exists one step that will take me farther away, return that step
        // if there is none (e.g. cornered), return next step towards the companion champion
        protected TileBehaviour GetNextStepAwayFromPlayerChampions()
        {
            // 1. highest score is the best move (gets me farthest to all player champions)
            //    orgin place is at [1, 1]
            int[,] tileScores = new int[3,3];
            foreach (ChampionController playerChampion in playerChampions)
            {
                int originalDistance = tileManager.GetIntegerDistance(this, playerChampion);

                //----------- 1st Possible Move (North) ------------
                if (tileManager.IsValid(xCoordinate, yCoordinate + 1))
                {
                    int newDistance = tileManager.GetIntegerDistance(xCoordinate, yCoordinate + 1, playerChampion.xCoordinate, playerChampion.yCoordinate);
                    int delta = newDistance - originalDistance;
                    tileScores[1, 2] += delta;
                }

                //----------- 2nd Possible Move (South) ------------
                if (tileManager.IsValid(xCoordinate, yCoordinate - 1))
                {
                    int newDistance = tileManager.GetIntegerDistance(xCoordinate, yCoordinate - 1, playerChampion.xCoordinate, playerChampion.yCoordinate);
                    int delta = newDistance - originalDistance;
                    tileScores[1, 0] += delta;
                }

                //----------- 3rd Possible Move (East) ------------
                if (tileManager.IsValid(xCoordinate + 1, yCoordinate))
                {
                    int newDistance = tileManager.GetIntegerDistance(xCoordinate + 1, yCoordinate, playerChampion.xCoordinate, playerChampion.yCoordinate);
                    int delta = newDistance - originalDistance;
                    tileScores[2, 1] += delta;
                }

                //----------- 4th Possible Move (West) ------------
                if (tileManager.IsValid(xCoordinate - 1, yCoordinate))
                {
                    int newDistance = tileManager.GetIntegerDistance(xCoordinate - 1, yCoordinate, playerChampion.xCoordinate, playerChampion.yCoordinate);
                    int delta = newDistance - originalDistance;
                    tileScores[0, 1] += delta;
                }
                
                //----------- 5th Possible Move (NorthEast) ------------
                if (tileManager.IsValid(xCoordinate + 1, yCoordinate + 1))
                {
                    int newDistance = tileManager.GetIntegerDistance(xCoordinate + 1, yCoordinate + 1, playerChampion.xCoordinate, playerChampion.yCoordinate);
                    int delta = newDistance - originalDistance;
                    tileScores[2, 2] += delta;
                }

                //----------- 6th Possible Move (SouthEast) ------------
                if (tileManager.IsValid(xCoordinate + 1, yCoordinate - 1))
                {
                    int newDistance = tileManager.GetIntegerDistance(xCoordinate + 1, yCoordinate - 1, playerChampion.xCoordinate, playerChampion.yCoordinate);
                    int delta = newDistance - originalDistance;
                    tileScores[2, 0] += delta;
                }

                //----------- 7th Possible Move (NorthWest) ------------
                if (tileManager.IsValid(xCoordinate - 1, yCoordinate + 1))
                {
                    int newDistance = tileManager.GetIntegerDistance(xCoordinate - 1, yCoordinate + 1, playerChampion.xCoordinate, playerChampion.yCoordinate);
                    int delta = newDistance - originalDistance;
                    tileScores[0, 2] += delta;
                }

                //----------- 8th Possible Move (SouthWest) ------------
                if (tileManager.IsValid(xCoordinate - 1, yCoordinate - 1))
                {
                    int newDistance = tileManager.GetIntegerDistance(xCoordinate - 1, yCoordinate - 1, playerChampion.xCoordinate, playerChampion.yCoordinate);
                    int delta = newDistance - originalDistance;
                    tileScores[0, 0] += delta;
                }
            }

            // 2. if highest score is bigger than 0, take that step
            int highestScore = 0;
            int nextX = xCoordinate, nextY = yCoordinate;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (tileScores[i, j] > highestScore)
                    {
                        highestScore = tileScores[i, j];
                        nextX = xCoordinate + i - 1;
                        nextY = yCoordinate + j - 1;
                    }
                }
            }

            // 3. if the highest score is no bigger than 0, it means no next step can take me farther
            //    I will use the next step to go to my companion
            if (nextX == xCoordinate && nextY == yCoordinate)
            {
                return pathfinder.GetNextTileInPath(GetCurrentTile(), companionChampion.GetCurrentTile());
            }
            else
            {
                return tileManager.GetTile(nextX, nextY);
            }
        }

        // check if I am at least safeDistance from all player champions
        protected bool IsWithinSafeDistance(int safeDistance)
        {
            bool isSafe = true;
            foreach (ChampionController playerChampion in playerChampions)
            {
                if (tileManager.GetIntegerDistance(this, playerChampion) < safeDistance) isSafe = false;
            }

            return isSafe;
        }

        protected IEnumerator MoveTowardsTile(TileBehaviour targetTile, int maxSteps)
        {
            while (maxSteps > 0 && CanGetCloserToTarget(GetCurrentTile(), targetTile))
            {
                MoveOnceTowardsTile(targetTile);
                maxSteps--;

                // hardcoded, this must be longer than the AI_BASE_DELAY
                yield return new WaitForSeconds(BaseAbilityBehaviour.AI_BASE_DELAY * 2);
            }

            endTurnEvent.Invoke();
        }
    }
}

