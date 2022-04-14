using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoardGame
{
    public class MoveAbilityBehaviour : BaseAbilityBehaviour
    {
        private bool isSelectingTileToMove = false;

        public override void StartAbility()
        {
            if (parentChampionController.CompareTag("Player"))
            {
                isSelectingTileToMove = true;
            }
            else if (tileChosenByAI == null)
            {
                Debug.LogError("AI does not have a target");
            }
            else
            {
                StartCoroutine(AISelectedTile(tileChosenByAI));
            }
            ShowActiveTiles();
        }

        protected override void AbilityHappened(bool abilityConnected = true)
        {
            isSelectingTileToMove = false;

            tileManager.RemoveStatusForAll(TileStatus.Active);
            tileManager.RemoveStatusForAll(TileStatus.Selected);

            // move does not have cooldown but sprint has
            StartAbilityCooldown();
            thisAbilityFinished.Invoke();
        }

        private void TileSelected(TileBehaviour tile)
        {
            GameObject objectOnTile = tile.GetObjectOnTile();
            if (objectOnTile == null)
            {
                parentChampionController.MoveTo(tile);
                AbilityHappened();
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
            if (isSelectingTileToMove)
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

