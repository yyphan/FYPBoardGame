using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoardGame
{

    [CreateAssetMenu(fileName = "Data_Ability_", menuName = "BoardGame/Champion/Ability Data/Move Or Sequential Ability", order = 1)]
    public class BaseAbilityData : ScriptableObject
    {
        [Header("Display Info")]
        public string abilityName;

        [Header("Cooldown")]
        public int cooldownRounds;

        [Header("Range")]
        public int range;

        [Header("Cost")]
        public int APCost;

        public virtual int GetAbilityValue() { return 0; }

        public virtual int GetAccuracyOffset() { return 0; }

        public virtual int GetValueOffset() { return 0; }

        protected virtual int ValueModifierResult(ValueModifier modifier) { return 0; }

        public virtual void GetArea(out int areaWidth, out int areaLength)
        {
            areaWidth = 0;
            areaLength = 0;
        }

        public virtual Debuffs GetDebuffs() { return new Debuffs(false, 0, 0, 0, 0); }

        public virtual bool IsPenetrate() { return false; }
    }
}
