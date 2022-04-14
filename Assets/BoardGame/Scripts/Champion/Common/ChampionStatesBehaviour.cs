using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Inspector;

namespace BoardGame
{
    public class ChampionStatesBehaviour : MonoBehaviour
    {
        [Header("Debuffs")]
        [ReadOnly] public int bleedingValue;
        [ReadOnly] public int burnValue;
        [ReadOnly] public int shockValue;
        [ReadOnly] public int silenceValue;

        private ChampionController parentChampionController;

        private void Awake()
        {
            parentChampionController = GetComponent<ChampionController>();
        }

        public void ReceiveDebuffs(Debuffs debuffs)
        {
            bleedingValue += debuffs.bleeding;
            burnValue += debuffs.burn;
            shockValue += debuffs.shock;
            silenceValue += debuffs.silence;
        }

        public void ClearAllDebuffs()
        {
            bleedingValue = 0;
            burnValue = 0;
            shockValue = 0;
            silenceValue = 0;
        }

        public void ApplyDebuffs()
        {
            if (bleedingValue > 0)
            {
                parentChampionController.ReceiveDebuffValue(bleedingValue);
                bleedingValue--;
            }

            if (burnValue > 0)
            {
                parentChampionController.ReceiveDebuffValue(burnValue);
                burnValue--;
            }

            parentChampionController.ReceiveDebuffShock(shockValue);
            if (shockValue > 0)
            {
                shockValue--;
            }

            if (silenceValue > 0)
            {
                parentChampionController.ReceiveDebuffSilence();
                silenceValue--;
            }
            else if (silenceValue == 0)
            {
                parentChampionController.CancelDebuffSilence();
            }
        }
    }
}
