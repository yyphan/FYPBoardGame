using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BoardGame
{
    public enum ChampionName
    {
        Null,
        Archer,
        Mage,
        Blademaster
    }

    public class ChooseChampionUIManager : MonoBehaviour
    {
        public Button archerButton;
        public Button mageButton;
        public Button blademasterButton;
        public Button confirmButton;

        public GameplayManager gameplayManager;

        ChampionName[] championChosen;

        public void Start()
        {
            archerButton.onClick.AddListener(() => { ToggleChoice(ChampionName.Archer); });
            mageButton.onClick.AddListener(() => { ToggleChoice(ChampionName.Mage); });
            blademasterButton.onClick.AddListener(() => { ToggleChoice(ChampionName.Blademaster); });
            confirmButton.onClick.AddListener(() => { ConfirmChoice(); });
            championChosen = new ChampionName[2];
        }

        public void ToggleChoice(ChampionName championName)
        {
            if (championChosen[0] == ChampionName.Null)
            {
                championChosen[0] = championName;
            }
            else if (championChosen[0] == championName)
            {
                championChosen[0] = championChosen[1];
                championChosen[1] = ChampionName.Null;
            }
            else if (championChosen[1] == ChampionName.Null)
            {
                championChosen[1] = championName;
            }
            else if (championChosen[1] == championName)
            {
                championChosen[1] = ChampionName.Null;
            }
            
            UpdateButtonText();
            ToggleConfirmButton();
        }

        private Button GetButton(ChampionName championName)
        {
            switch(championName)
            {
                case ChampionName.Archer:
                    return archerButton;
                case ChampionName.Mage:
                    return mageButton;
                case ChampionName.Blademaster:
                    return blademasterButton;
                default:
                    Debug.LogError("Champion Name Error");
                    return null;
            }
        }

        private void UpdateButtonText()
        {
            ResetButtonText();
            if (championChosen[0] != ChampionName.Null)
            {
                GetButton(championChosen[0]).GetComponentInChildren<TextMeshProUGUI>().SetText("1st Champion");
            }

            if (championChosen[1] != ChampionName.Null)
            {
                GetButton(championChosen[1]).GetComponentInChildren<TextMeshProUGUI>().SetText("2nd Champion");
            }
        }

        private void ResetButtonText()
        {
            archerButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Choose\nArcher");
            mageButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Choose\nMage");
            blademasterButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Choose\nBlademaster");
        }

        private void ToggleConfirmButton()
        {
            confirmButton.gameObject.SetActive(false);
            if (championChosen[0] != ChampionName.Null
                && championChosen[1] != ChampionName.Null)
            {
                confirmButton.gameObject.SetActive(true);
            }
        }

        private void ConfirmChoice()
        {
            gameplayManager.StartGame(championChosen);
        }
    }
}
