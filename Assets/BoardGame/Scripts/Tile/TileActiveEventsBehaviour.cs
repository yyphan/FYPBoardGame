using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Inspector;

namespace BoardGame
{
    public class TileActiveEventsBehaviour : BaseActiveEventsBehaviour
    {
        private TileBehaviour tile;

        [SerializeField]
        [ReadOnly] private int WOPBurnRemainingRounds;

        public static int WOPBurn = 3;

        private void Awake()
        {
            tile = GetComponent<TileBehaviour>();
        }

        public void UpdateActiveEvents(GameTurn currentTurn)
        {
            if (WOPBurnRemainingRounds > 0)
            {
                ApplyWOPBurn(currentTurn);
                // decrement count at the start of player turn
                if (currentTurn == GameTurn.Player) WOPBurnRemainingRounds--;
            }
            else if (WOPBurnRemainingRounds == 0)
            {
                tile.RemoveStatus(TileStatus.Burning);
            }
        }

        public void SetupWOPBurnActiveEvent(int roundsToGo)
        {
            WOPBurnRemainingRounds += roundsToGo;
            tile.AddStatus(TileStatus.Burning);
        }

        private void ApplyWOPBurn(GameTurn currentTurn)
        {
            string activeTeamTag = "";
            switch(currentTurn)
            {
                case GameTurn.Player:
                    activeTeamTag = "Player";
                    break;
                case GameTurn.Enemy:
                    activeTeamTag = "Enemy";
                    break;
            }

            GameObject objectOnTile = tile.GetObjectOnTile();
            if (objectOnTile != null)
            {
                ChampionController champion = objectOnTile.GetComponent<ChampionController>();
                if (champion != null && champion.CompareTag(activeTeamTag))
                {
                    Debuffs WOPBurnDebuffs = new Debuffs(false, 0, WOPBurn, 0, 0);
                    champion.ReceiveAbilityDebuffs(WOPBurnDebuffs);
                }
            }
        }
    }
}
