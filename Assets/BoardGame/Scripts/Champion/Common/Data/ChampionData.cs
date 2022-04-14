using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoardGame
{
    [CreateAssetMenu(fileName = "Data_Champion_", menuName = "BoardGame/Champion/Champion Data", order = 1)]
    public class ChampionData : ScriptableObject
    {
        [Header("Display Infos")]
        public string championName;

        [Header("Health Settings")]
        public int totalArmor;
        public int totalHealth;

        [Header("Action Points")]
        public int totalAP;

        [Header("Accuracy")]
        public int[] baseAccuracy;

        [Header("Sprites")]
        public Sprite championPortrait;
        public Sprite championToken;
    }
}
