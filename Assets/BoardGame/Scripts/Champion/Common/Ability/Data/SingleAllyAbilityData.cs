using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoardGame
{
    [CreateAssetMenu(fileName = "Data_Ability_", menuName = "BoardGame/Champion/Ability Data/Single Ally Ability", order = 4)]
    public class SingleAllyAbilityData : BaseAbilityData
    {
        [Header("Value Settings")]
        public ValueModifier valueModifier;
        public int value;

        [Header("Value Offsets")]
        public ValueModifier accuracyOffsetModifier;
        public int accuracyOffset;
        public ValueModifier valueOffsetModifier;
        public int valueOffset;
    }
}
