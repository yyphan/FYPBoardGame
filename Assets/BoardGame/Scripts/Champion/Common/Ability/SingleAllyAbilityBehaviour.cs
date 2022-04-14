using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Inspector;

namespace BoardGame
{
    public class SingleAllyAbilityBehaviour : BaseAbilityBehaviour
    {
        [Header("Ability Target")]
        [SerializeField]
        [ReadOnly] protected ChampionController abilityTarget;
        [SerializeField]
        [ReadOnly] protected int abilityValue;

        [Header("Offsets")]
        [SerializeField]
        [ReadOnly] private int accuracyOffset;
        [SerializeField]
        [ReadOnly] private int valueOffset;

        private bool isSelectingSingleAlly = false;

        protected override void Awake()
        {
            base.Awake();

            // ability values
            abilityValue = data.GetAbilityValue();
            ResetAccuracyOffset();
            ResetValueOffset();
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
                isSelectingSingleAlly = true;
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
            tileManager.RemoveStatusForAll(TileStatus.Active);
            tileManager.RemoveStatusForAll(TileStatus.Selected);

            StartAbilityCooldown();
            isSelectingSingleAlly = false;

            thisAbilityFinished.Invoke();
        }

        protected IEnumerator AISelectedTile(TileBehaviour tile)
        {
            UpdateSelectedTile(tile);

            yield return new WaitForSeconds(AI_BASE_DELAY);

            TileSelected(tile);
        }

        protected void TileSelected(TileBehaviour tile)
        {
            GameObject objectOnTile = tile.GetObjectOnTile();
            Debug.Log("The target is on " + tile.Print());
            // make sure it is ally
            if (objectOnTile != null && objectOnTile.CompareTag(parentChampionController.gameObject.tag))
            {
                abilityTarget = objectOnTile.GetComponent<ChampionController>();
                int range = tileManager.GetIntegerDistance(parentChampionController, abilityTarget);
                TryConnectAbility(range);
            }
        }

        protected void TryConnectAbility(int range)
        {
            parentChampionController.ReceiveAccuracyOffsetFromAbility(accuracyOffset);
            bool abilityConnected = parentChampionController.TryConnect(range);
            AbilityHappened(abilityConnected);
        }

        private void Update()
        {
            if (isSelectingSingleAlly)
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
