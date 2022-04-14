using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Inspector;

namespace BoardGame
{
    public class ConeOfCombustAbilityBehaviour : AreaAttackAbilityBehaviour
    {
        [Header("Two Rows")]
        [SerializeField]
        [ReadOnly] private List<TileBehaviour> firstRow;
        [SerializeField]
        [ReadOnly] private List<TileBehaviour> secondRow;

        protected override void AbilityHappened(bool abilityConnected = true)
        {
            if (abilityConnected)
            {
                ApplyConeOfCombustToTiles(targetTiles);
            }

            tileManager.RemoveStatusForAll(TileStatus.Active);
            tileManager.RemoveStatusForAll(TileStatus.Selected);

            StartAbilityCooldown();
            isSelectingArea = false;

            thisAbilityFinished.Invoke();
        }

        private void ApplyConeOfCombustToTiles(List<TileBehaviour> tiles)
        {
            firstRow = targetTiles.GetRange(0, 7);
            secondRow = targetTiles.GetRange(7, 7);

            int firstRowAbilityValue = abilityValue;
            int secondRowAbilityValue = firstRowAbilityValue + 1;

            Debuffs firstRowDebuffs = abilityDebuffs;
            Debuffs secondRowDebuffs = new Debuffs(
                firstRowDebuffs.applyDebuffOnlyWhenDamageDealt,
                firstRowDebuffs.bleeding,
                firstRowDebuffs.burn - 1,
                firstRowDebuffs.shock,
                firstRowDebuffs.silence
            );

            foreach (TileBehaviour tile in firstRow)
            {
                GameObject objectOnTile = tile.GetObjectOnTile();
                if (objectOnTile == null) continue;

                ChampionController champion = objectOnTile.GetComponent<ChampionController>();
                if (champion == null) continue;

                ApplyAbilityDebuffsToTarget(firstRowDebuffs, champion);
                ApplyAbilityValueToTarget(firstRowAbilityValue, champion, isPenetrate);
            }

            foreach (TileBehaviour tile in secondRow)
            {
                GameObject objectOnTile = tile.GetObjectOnTile();
                if (objectOnTile == null) continue;

                ChampionController champion = objectOnTile.GetComponent<ChampionController>();
                if (champion == null) continue;

                ApplyAbilityDebuffsToTarget(secondRowDebuffs, champion);
                ApplyAbilityValueToTarget(secondRowAbilityValue, champion, isPenetrate);
            }
        }

        protected override void Update()
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
                        if (tile.HasStatus(TileStatus.Active) && tile.yCoordinate < 6)
                        {
                            tileManager.RemoveStatusForAll(TileStatus.Selected);
                            tileManager.AddStatusForArea(tile, areaWidth, areaLength, TileStatus.Selected);
                        }

                        if (tile.HasStatus(TileStatus.Active) && tile.yCoordinate < 6 && Input.GetMouseButtonDown(0))
                        {
                            targetTiles = tileManager.GetTilesInArea(tile, areaWidth, areaLength);
                            int range = Mathf.Abs(tile.yCoordinate - parentChampionController.yCoordinate);
                            TryConnectAbility(range);
                        }
                    }
                }
            }
        }
    }

}
