using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoardGame
{
    public class WrathOfPhoenixBehaviour : AreaAttackAbilityBehaviour
    {
        public const int WOP_BURN_ROUNDS = 3;

        protected override void Awake()
        {
            base.Awake();
            // apply burn to tiles no matter if WOP connected
            thisAbilityFinished.AddListener(AssignWOPActiveEventToTiles);
        }

        private void AssignWOPActiveEventToTiles()
        {
            foreach (TileBehaviour tile in targetTiles)
            {
                TileActiveEventsBehaviour tileActiveBehaviour = tile.GetComponent<TileActiveEventsBehaviour>();
                tileActiveBehaviour.SetupWOPBurnActiveEvent(WOP_BURN_ROUNDS);
            }
        }
    }
}
