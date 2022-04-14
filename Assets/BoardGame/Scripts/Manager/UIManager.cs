using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BoardGame
{
    public class UIManager : MonoBehaviour
    {
        [Header("Gameplay Manager")]
        public GameplayManager gameplayManager;

        [Header("Choose Champion UI")]
        public Canvas chooseChampionCanvas;

        [Header("Game Over UI")]
        public Canvas gameOverCanvas;
        public TextMeshProUGUI gameOverTitle;
        public Button returnButton;

        [Header("Ability UI")]
        public Canvas abilityCanvas;
        public Button abilityButtonPrefab;
        public Button nextButtonPrefab;

        [Header("Status UI")]
        public TextMeshProUGUI championInfoText;
        public Image hoverChampionPortrait;
        public TextMeshProUGUI roundInfoText;
        public Image currentChampionToken;
        public Image nextChampionToken;

        private void Awake()
        {
            returnButton.onClick.AddListener(() =>
            {
                ToggleChooseChampionUI();
                ToggleGameOverScreen(false);
            });
        }

        public void ToggleChooseChampionUI()
        {
            if (chooseChampionCanvas.gameObject.activeSelf)
            {
                chooseChampionCanvas.gameObject.SetActive(false);
            }
            else
            {
                chooseChampionCanvas.gameObject.SetActive(true);
            }
        }

        public void SetupBattleUI(ChampionController currentChampion)
        {
            RemoveAllChildren(abilityCanvas);
            // empty canvas for enemy
            if (currentChampion.CompareTag("Enemy")) return;

            ChampionAbilitiesBehaviour abilitiesBehaviour = currentChampion.GetComponent<ChampionAbilitiesBehaviour>();
            BaseAbilityBehaviour[] abilities = abilitiesBehaviour.abilities;

            // abiliity buttons
            for (int i = 0; i < abilities.Length; i++)
            {
                BaseAbilityBehaviour ability = abilities[i];
                Button button = Instantiate(abilityButtonPrefab);
                button.transform.SetParent(abilityCanvas.transform, false);
                int finalI = i;
                button.onClick.AddListener(() => {
                    abilitiesBehaviour.StartAbility(finalI);
                });

                TextMeshProUGUI tmPro = button.GetComponentInChildren<TextMeshProUGUI>();
                tmPro.SetText(ability.data.abilityName);
            }

            // 'next' button
            Button nextButton = Instantiate(nextButtonPrefab);
            nextButton.transform.SetParent(abilityCanvas.transform, false);
            nextButton.onClick.AddListener(() => {
                gameplayManager.NextChampion();
            });
        }

        public void UpdateStatusText(ChampionController champion)
        {
            if (champion == null)
            {
                championInfoText.SetText("");
                hoverChampionPortrait.enabled = false;
                return;
            }

            string team = champion.tag;
            string championName = champion.data.championName;
            int championHealth = champion.healthBehaviour.GetCurrentHealth();
            int championArmor = champion.healthBehaviour.GetCurrentArmor();

            int bleeding = champion.statesBehaviour.bleedingValue;
            int burn = champion.statesBehaviour.burnValue;
            int shock = champion.statesBehaviour.shockValue;
            int silence = champion.statesBehaviour.silenceValue;

            championInfoText.SetText(
                team + " " + championName + "\n" 
                + "HP: " + championHealth + "\n"
                + "Armor: " + championArmor + "\n"
                + "Bleeding: " + bleeding + "\n"
                + "Burn: " + burn + "\n"
                + "Shock: " + shock + "\n"
                + "Silence: " + silence + "\n"
            );

            hoverChampionPortrait.enabled = true;
            hoverChampionPortrait.sprite = champion.data.championPortrait;
        }

        private void UpdateRoundInfo()
        {
            ChampionController currentChampion = gameplayManager.GetCurrentChampion();
            ChampionController nextChampion = gameplayManager.GetNextChampion();

            string currentChampionName = currentChampion.tag + " " + currentChampion.data.championName;
            int currentAP = currentChampion.abilitiesBehaviour.currentAP;
            int championHealth = currentChampion.healthBehaviour.GetCurrentHealth();
            int championArmor = currentChampion.healthBehaviour.GetCurrentArmor();

            string nextChampionName = nextChampion.tag + " " + nextChampion.data.championName;

            currentChampionToken.sprite = currentChampion.data.championToken;
            nextChampionToken.sprite = nextChampion.data.championToken;

            roundInfoText.SetText(
                "Current: " + currentChampionName + "\n"
                + "HP: " + championHealth + "\n"
                + "Armor: " + championArmor + "\n"
                + "Remaining AP: " + currentAP + "\n"
                + "\n"
                + "Next: " + nextChampionName + "\n"
            ); ;
        }

        private void RemoveAllChildren(Canvas canvas)
        {
            int childCount = canvas.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Destroy(canvas.transform.GetChild(i).gameObject);
            }
        }

        public void ToggleGameOverScreen(bool playerWon)
        {
            if (playerWon)
            {
                gameOverTitle.SetText("VICTORY");
            }
            else
            {
                gameOverTitle.SetText("DEFEAT");
            }

            if (gameOverCanvas.gameObject.activeSelf)
            {
                gameOverCanvas.gameObject.SetActive(false);
            }
            else
            {
                gameOverCanvas.gameObject.SetActive(true);
            }
        }

        private void Update()
        {
            if (!gameplayManager.IsGameStarted()) return;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            ChampionController champion = null;

            // mouse hover on champion
            if (Physics.Raycast(ray, out hit))
            {
                Transform objectHit = hit.transform;
                if (objectHit.gameObject.CompareTag("Tile"))
                {
                    TileBehaviour tile = objectHit.gameObject.GetComponent<TileBehaviour>();
                    GameObject objectOnTile = tile.GetObjectOnTile();
                    if (objectOnTile != null)
                    {
                        champion = objectOnTile.GetComponent<ChampionController>();
                    }
                }
            }

            UpdateStatusText(champion);
            UpdateRoundInfo();
        }
    }
}


