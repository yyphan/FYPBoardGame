using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utilities.Inspector;

namespace BoardGame
{
    public class ChampionController : MonoBehaviour
    {
        [Header("Data")]
        public ChampionData data;

        [Header("Health Settings")]
        public ChampionHealthBehaviour healthBehaviour;
        [SerializeField]
        [ReadOnly] private bool isChampionAlive;

        [Header("Abilities Settings")]
        public ChampionAbilitiesBehaviour abilitiesBehaviour;

        [Header("Position")]
        public TileManager tileManager;
        public int xCoordinate = 0;
        public int yCoordinate = 0;

        [Header("Champion States")]
        public ChampionStatesBehaviour statesBehaviour;

        [Header("Champion Active Events")]
        public ChampionActiveEventsBehaviour activeEventsBehaviour;

        [Header("Accuracy Related")]
        public int accuracyOffsetFromShock;
        public int accuracyOffsetFromAbility;
        public int actualAccuracy;
        public int connectResult;
        public bool abilityConnected;
        const int ACCURACY_PER_SHOCK = 6;
        
        [Header("Events")]
        public UnityEvent championBeingAttackedEvents;
        public UnityEvent<ChampionController> championHasDiedEvent;

        [Header("Flame Shield")]
        [SerializeField]
        [ReadOnly] private ChampionController FSAttacker;
        [SerializeField]
        [ReadOnly] private int FSRange;
        public UnityAction FlameShieldBrokeAction;

        private void Awake()
        {
            healthBehaviour.healthIsZeroEvent.AddListener(ChampionHasDied);
        }

        public void SetAlive()
        {
            ResetChampion();
            isChampionAlive = true;
        }

        private void ResetChampion()
        {
            healthBehaviour.SetupCurrentArmor(data.totalArmor);
            healthBehaviour.SetupCurrentHealth(data.totalHealth);
            abilitiesBehaviour.SetupAbilities();
            activeEventsBehaviour.ResetAllActiveEvents();
            statesBehaviour.ClearAllDebuffs();
        }

        public void UpdateActiveEvents()
        {
            activeEventsBehaviour.UpdateActiveEvents();
            statesBehaviour.ApplyDebuffs();
        }

        public virtual void StartNewRound()
        {
            abilitiesBehaviour.UpdateForNewRound();
        }

        public void SetupListenerForDebuffs(Debuffs debuffs)
        {
            healthBehaviour.healthDifferenceEvent.RemoveAllListeners();
            healthBehaviour.healthDifferenceEvent.AddListener(() => { ReceiveAbilityDebuffs(debuffs); });
        }

        public void ReceiveAbilityValue(ChampionController attacker, int abilityValue, bool isPenetrate, int range)
        {
            FSAttacker = attacker;
            FSRange = range;

            if (isChampionAlive)
            {
                if (isPenetrate)
                {
                    healthBehaviour.ChangeHealth(abilityValue);
                }
                else if (activeEventsBehaviour.isPreventingNonPenetrateDamage)
                {
                    NonPenetrateDamagePrevented();
                }
                else if (abilityValue < 0)
                {
                    healthBehaviour.ChangeArmor(abilityValue);
                }

                championBeingAttackedEvents.Invoke();
            }
        }

        public void SetupFlameShieldListener()
        {
            FlameShieldBrokeAction = FSApplyBurn;
            championBeingAttackedEvents.AddListener(FlameShieldBrokeAction);
        }

        public void UnsetFlameShieldListener()
        {
            championBeingAttackedEvents.RemoveListener(FlameShieldBrokeAction);
        }

        private void FSApplyBurn()
        {
            if (FSAttacker == null) return;
            if (FSRange > 2) return;

            FSAttacker.ReceiveAbilityDebuffs(new Debuffs(false, 0, 1, 0, 0));
            activeEventsBehaviour.UnsetFlameShield();
        }

        public void PreventNextNonPenetrateDamage()
        {
            activeEventsBehaviour.PreventNextNonPenetrateDamage();
        }

        private void NonPenetrateDamagePrevented()
        {
            activeEventsBehaviour.NonPenetrateDamagePrevented();
        }

        public void ReceiveAbilityDebuffs(Debuffs debuffs)
        {
            if (isChampionAlive)
            {
                statesBehaviour.ReceiveDebuffs(debuffs);
            }
        }

        public void ReceiveDebuffValue(int debuffValue)
        {
            if (isChampionAlive)
            {
                healthBehaviour.ChangeHealth(-1 * debuffValue);
            }
        }

        public void ReceiveDebuffSilence()
        {
            abilitiesBehaviour.ReceiveDebuffSilence();
        }

        public void CancelDebuffSilence()
        {
            abilitiesBehaviour.CancelDebuffSilence();
        }

        public void ReceiveDebuffShock(int shockValue)
        {
            accuracyOffsetFromShock = -shockValue * ACCURACY_PER_SHOCK;
        }

        public void ReceiveAccuracyOffsetFromAbility(int accuracyOffset)
        {
            accuracyOffsetFromAbility = accuracyOffset;
        }

        protected int GetActualAccuracy(int range)
        {
            int baseAccuracy = data.baseAccuracy[range];
            int actualAccuracy = baseAccuracy + accuracyOffsetFromShock + accuracyOffsetFromAbility;

            return actualAccuracy;
        }

        public bool TryConnect(int range)
        {
            actualAccuracy = GetActualAccuracy(range);
            Random.InitState((int)System.DateTime.Now.Ticks);
            connectResult = Random.Range(ChampionAbilitiesBehaviour.MIN_ACCURACY, ChampionAbilitiesBehaviour.MAX_ACCURACY);

            abilityConnected = connectResult <= actualAccuracy;
            Debug.Log("IT DID " + (abilityConnected ? "" : "NOT ") + "CONNECT!");

            return abilityConnected;
        }

        public void MoveTo(int targetX, int targetY)
        {
            TileBehaviour currentTile = tileManager.GetTile(xCoordinate, yCoordinate);
            TileBehaviour targetTile = tileManager.GetTile(targetX, targetY);

            if (targetTile.GetObjectOnTile() != null)
            {
                Debug.LogError("Target Tile is occupied");
            }

            currentTile.SetObjectOnTile(null);
            targetTile.SetObjectOnTile(gameObject);
            gameObject.transform.position = targetTile.transform.position;

            xCoordinate = targetX;
            yCoordinate = targetY;

            Debug.Log(gameObject.name + " has moved from " + currentTile.Print() + " to " + targetTile.Print());
        }

        public void MoveTo(TileBehaviour targetTile)
        {
            int targetX = targetTile.xCoordinate;
            int targetY = targetTile.yCoordinate;

            MoveTo(targetX, targetY);
        }

        public int GetCurrentArmor()
        {
            return healthBehaviour.GetCurrentArmor();
        }

        public int GetCurrentHealth()
        {
            return healthBehaviour.GetCurrentHealth();
        }

        public TileBehaviour GetCurrentTile()
        {
            return tileManager.GetTile(xCoordinate, yCoordinate);
        }

        protected int GetIntegerDistance(ChampionController theOtherChampion)
        {
            return tileManager.GetIntegerDistance(this, theOtherChampion);
        }

        public void ChampionHasDied()
        {
            isChampionAlive = false;
            Debug.LogWarning(this.gameObject.name + " has died");
            activeEventsBehaviour.ResetAllActiveEvents();
            statesBehaviour.ClearAllDebuffs();
            championHasDiedEvent.Invoke(this);
        }

        public bool IsChampionAlive()
        {
            return isChampionAlive;
        }
    }
}
