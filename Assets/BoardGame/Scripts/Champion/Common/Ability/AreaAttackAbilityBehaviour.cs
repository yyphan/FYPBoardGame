using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Inspector;

namespace BoardGame
{
    public class AreaAttackAbilityBehaviour : SingleTargetAttackAbilityBehaviour
    {
        protected bool isSelectingArea = false;

        [Header("Area Information")]
        [SerializeField]
        [ReadOnly] public int areaWidth;
        [SerializeField]
        [ReadOnly] public int areaLength;
        [SerializeField]
        [ReadOnly] protected List<TileBehaviour> targetTiles;

        protected override void Awake()
        {
            base.Awake();
            data.GetArea(out areaWidth, out areaLength);
        }

        public int GetAreaWidth() { return areaWidth; }

        public int GetAreaLength() { return areaLength; }

        public override void StartAbility()
        {
            ShowActiveTiles();

            if (parentChampionController.CompareTag("Player"))
            {
                isSelectingArea = true;
            }
            else if (tileChosenByAI == null)
            {
                Debug.LogError("AI does not have a target");
            }
            else
            {
                StartCoroutine(AISelectedTiles(tileChosenByAI));
            }
        }

        protected override void AbilityHappened(bool abilityConnected = true)
        {
            if (abilityConnected)
            {
                ApplyAbilityToTargetOnTiles(targetTiles);
            }

            tileManager.RemoveStatusForAll(TileStatus.Active);
            tileManager.RemoveStatusForAll(TileStatus.Selected);

            StartAbilityCooldown();
            isSelectingArea = false;

            thisAbilityFinished.Invoke();
        }

        protected void ApplyAbilityToTargetOnTiles(List<TileBehaviour> tiles)
        {
            foreach (TileBehaviour tile in tiles)
            {
                GameObject objectOnTile = tile.GetObjectOnTile();
                if (objectOnTile == null) continue;

                ChampionController champion = objectOnTile.GetComponent<ChampionController>();
                if (champion == null) continue;

                ApplyAbilityDebuffsToTarget(champion);
                ApplyAbilityValueToTarget(abilityValue, champion, isPenetrate);
            }
        }

        private void TileSelected(TileBehaviour centerTile)
        {
            targetTiles = tileManager.GetTilesInArea(centerTile, areaWidth, areaLength);

            string debugMsg = "The targets are ";
            foreach (TileBehaviour tile in targetTiles)
            {
                debugMsg += tile.Print() + " ";
            }
            Debug.Log(debugMsg);

            rangeOfAttack = tileManager.GetIntegerDistance(parentChampionController, centerTile);
            TryConnectAbility(rangeOfAttack);
        }

        private IEnumerator AISelectedTiles(TileBehaviour centerTile)
        {
            UpdateSelectedTiles(centerTile);

            yield return new WaitForSeconds(AI_BASE_DELAY);

            TileSelected(centerTile);
        }

        private void UpdateSelectedTiles(TileBehaviour centerTile)
        {
            tileManager.RemoveStatusForAll(TileStatus.Selected);
            tileManager.AddStatusForArea(centerTile, areaWidth, areaLength, TileStatus.Selected);
        }

        protected virtual void Update()
        {
            if (isSelectingArea)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    Transform objectHit = hit.transform;
                    if (objectHit.transform.CompareTag("Tile"))
                    {
                        TileBehaviour tile = objectHit.gameObject.GetComponent<TileBehaviour>();
                        if (tile.HasStatus(TileStatus.Active))
                        {
                            UpdateSelectedTiles(tile);
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
