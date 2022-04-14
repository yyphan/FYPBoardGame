using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utilities.Inspector;

namespace BoardGame
{
    public class SingleTargetAttackAbilityBehaviour : BaseAbilityBehaviour
    {
        [Header("Value Information")]
        [SerializeField]
        [ReadOnly] private ChampionController abilityTarget;
        [SerializeField]
        [ReadOnly] protected int abilityValue;
        [SerializeField]
        [ReadOnly] protected bool isPenetrate;
        [SerializeField]
        [ReadOnly] protected int rangeOfAttack;

        private bool isSelectingSingleEnemy = false;

        protected Debuffs abilityDebuffs;
        [Header("Debuffs")]
        [SerializeField]
        [ReadOnly] protected bool applyDebuffOnlyWhenDamageDealt;
        [SerializeField]
        [ReadOnly] protected int bleeding;
        [SerializeField]
        [ReadOnly] protected int burn;
        [SerializeField]
        [ReadOnly] protected int shock;
        [SerializeField]
        [ReadOnly] protected int silence;

        [Header("Offsets")]
        [SerializeField]
        [ReadOnly] private int accuracyOffset;
        [SerializeField]
        [ReadOnly] private int valueOffset;

        protected override void Awake()
        {
            base.Awake();

            // ability values
            abilityValue = data.GetAbilityValue();
            ResetAccuracyOffset();
            ResetValueOffset();
            isPenetrate = data.IsPenetrate();

            // ability debuffs
            abilityDebuffs = data.GetDebuffs();
            applyDebuffOnlyWhenDamageDealt = abilityDebuffs.applyDebuffOnlyWhenDamageDealt;
            bleeding = abilityDebuffs.bleeding;
            burn = abilityDebuffs.burn;
            shock = abilityDebuffs.shock;
            silence = abilityDebuffs.silence;
        }

        public void ResetAccuracyOffset()
        {
            accuracyOffset = data.GetAccuracyOffset();
        }

        public void ApplyAccuracyOffset(int value)
        {
            accuracyOffset += value;
        }

        public void ResetValueOffset()
        {
            valueOffset = data.GetValueOffset();
        }

        public void ApplyValueOffset(int value)
        {
            valueOffset += value;
        }

        public override void StartAbility()
        {
            ShowActiveTiles();
            if (parentChampionController.CompareTag("Player"))
            {
                isSelectingSingleEnemy = true;
            }
            else if (tileChosenByAI == null)
            {
                Debug.LogError("AI does not have a target");
            }
            else
            {
                StartCoroutine(AISelectedTile(tileChosenByAI));
            }
        }

        protected override void AbilityHappened(bool abilityConnected = true)
        {
            if (abilityConnected)
            {
                ApplyAbilityDebuffsToTarget(abilityTarget);
                ApplyAbilityValueToTarget(abilityValue, abilityTarget, isPenetrate);
            }

            tileManager.RemoveStatusForAll(TileStatus.Active);
            tileManager.RemoveStatusForAll(TileStatus.Selected);

            StartAbilityCooldown();
            isSelectingSingleEnemy = false;

            thisAbilityFinished.Invoke();
        }

        protected void ApplyAbilityValueToTarget(int abilityValue, ChampionController abilityTarget, bool isPenetrate)
        {
            int actualAbilityValue = abilityValue + valueOffset;
            abilityTarget.ReceiveAbilityValue(parentChampionController, actualAbilityValue, isPenetrate, rangeOfAttack);
        }

        protected void ApplyAbilityDebuffsToTarget(ChampionController abilityTarget)
        {
            if (abilityDebuffs.applyDebuffOnlyWhenDamageDealt)
            {
                abilityTarget.SetupListenerForDebuffs(abilityDebuffs);
            }
            else
            {
                abilityTarget.ReceiveAbilityDebuffs(abilityDebuffs);
            }
        }

        protected void ApplyAbilityDebuffsToTarget(Debuffs debuffs, ChampionController abilityTarget)
        {
            if (debuffs.applyDebuffOnlyWhenDamageDealt)
            {
                abilityTarget.SetupListenerForDebuffs(debuffs);
            }
            else
            {
                abilityTarget.ReceiveAbilityDebuffs(debuffs);
            }
        }

        protected void TryConnectAbility(int range)
        {
            parentChampionController.ReceiveAccuracyOffsetFromAbility(accuracyOffset);
            bool abilityConnected = parentChampionController.TryConnect(range);
            AbilityHappened(abilityConnected);
        }

        private void TileSelected(TileBehaviour tile)
        {
            GameObject objectOnTile = tile.GetObjectOnTile();
            Debug.Log("The target is on " + tile.Print());
            if (objectOnTile != null)
            {
                abilityTarget = objectOnTile.GetComponent<ChampionController>();
                rangeOfAttack = tileManager.GetIntegerDistance(parentChampionController, abilityTarget);
                TryConnectAbility(rangeOfAttack);
            }
        }

        private IEnumerator AISelectedTile(TileBehaviour tile)
        {
            UpdateSelectedTile(tile);

            yield return new WaitForSeconds(AI_BASE_DELAY);

            TileSelected(tile);
        }

        private void Update()
        {
            if (isSelectingSingleEnemy)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                // mouse hover means selected
                if (Physics.Raycast(ray, out hit))
                {
                    Transform objectHit = hit.transform;
                    if (objectHit.gameObject.CompareTag("Tile"))
                    {
                        TileBehaviour tile = objectHit.gameObject.GetComponent<TileBehaviour>();
                        if (tile.HasStatus(TileStatus.Active))
                        {
                            UpdateSelectedTile(tile);
                        }

                        if (tile.HasStatus(TileStatus.Active) && Input.GetMouseButtonDown(0))
                        {
                            TileSelected(tile);
                        }
                    }
                }
            }
        }
    }
}

