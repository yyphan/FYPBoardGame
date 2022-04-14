using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoardGame
{
    [CreateAssetMenu(fileName = "Data_Ability_", menuName = "BoardGame/Champion/Ability Data/Area Attack Ability", order = 3)]
    public class AreaAttackAbilityData : SingleTargetAttackAbilityData
    {
        [Header("Area Settings")]
        public int areaWidth;
        public int areaLength;

        public override void GetArea(out int areaWidth, out int areaLength)
        {
            areaWidth = this.areaWidth;
            areaLength = this.areaLength;
        }
    }
}

