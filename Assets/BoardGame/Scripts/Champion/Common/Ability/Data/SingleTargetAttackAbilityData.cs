using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoardGame
{
    public enum ValueModifier
    { 
        Positive,
        Negative
    }

    public struct Debuffs
    {
        public Debuffs(bool applyDebuffOnlyWhenDamageDealt, int bleeding, int burn, int shock, int silence)
        {
            this.applyDebuffOnlyWhenDamageDealt = applyDebuffOnlyWhenDamageDealt;
            this.bleeding = bleeding;
            this.burn = burn;
            this.shock = shock;
            this.silence = silence;
        }

        public bool applyDebuffOnlyWhenDamageDealt { get; }
        public int bleeding { get; }
        public int burn { get; }
        public int shock { get; }
        public int silence { get; }
    }

    [CreateAssetMenu(fileName = "Data_Ability_", menuName = "BoardGame/Champion/Ability Data/Single Target Attack Ability", order = 2)]
    public class SingleTargetAttackAbilityData : BaseAbilityData
    {
        [Header("Value Settings")]
        public ValueModifier valueModifier;
        public int value;

        [Header("Value Offsets")]
        public ValueModifier accuracyOffsetModifier;
        public int accuracyOffset;
        public ValueModifier valueOffsetModifier;
        public int valueOffset;

        [Header("Special Effects")]
        public bool applyDebuffOnlyWhenDamageDealt;
        public bool isPenetrate;
        public int bleeding;
        public int burn;
        public int shock;
        public int silence;

        public override int GetAbilityValue()
        {
            return value * ValueModifierResult(valueModifier);
        }

        public override int GetAccuracyOffset()
        {
            return accuracyOffset * ValueModifierResult(accuracyOffsetModifier);
        }
        public override int GetValueOffset()
        {
            return valueOffset * ValueModifierResult(valueOffsetModifier);
        }

        protected override int ValueModifierResult(ValueModifier modifier)
        {
            int modifierValue = 0;

            switch (modifier)
            {
                case ValueModifier.Positive:
                    modifierValue = 1;
                    break;
                case ValueModifier.Negative:
                    modifierValue = -1;
                    break;
            }

            return modifierValue;
        }

        public override Debuffs GetDebuffs()
        {
            Debuffs debuffs = new Debuffs(applyDebuffOnlyWhenDamageDealt, bleeding, burn, shock, silence);
            return debuffs;
        }

        public override bool IsPenetrate()
        {
            return isPenetrate;
        }
    }
}
