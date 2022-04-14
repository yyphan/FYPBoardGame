using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Inspector;

namespace BoardGame
{
    public enum GameTurn
    {
        Player,
        Enemy
    }

    public class GameplayManager : MonoBehaviour
    {
        [Header("All Playable Champions")]
        public ChampionController playerArcher;
        public ChampionController playerMage;
        public ChampionController playerBlademaster;

        [Header("Teams")]
        public List<ChampionController> playerTeamChampions;
        public List<ChampionController> enemyTeamChampions;

        private List<ChampionController> alivePlayerChampions;
        private List<ChampionController> aliveEnemyChampions;

        private List<ChampionController> currentTeam;

        [Header("Game States")]
        [SerializeField]
        [ReadOnly] private int championIndex;
        [SerializeField]
        [ReadOnly] private ChampionController currentChampion;
        [SerializeField]
        [ReadOnly] private ChampionController nextChampion;
        [ReadOnly] public GameTurn currentTurn;
        private bool isGameStarted = false;

        [Header("Tiles")]
        public TileManager tileManager;

        [Header("UI")]
        public UIManager uiManager;

        private void Awake()
        {
            Random.InitState((int) System.DateTime.Now.Ticks);
        }

        public bool IsGameStarted()
        {
            return isGameStarted;
        }

        public void StartGame(ChampionName[] championsChosen)
        {
            // hide choose champion screen
            uiManager.ToggleChooseChampionUI();
            // reset tiles
            tileManager.ResetAllTiles();
            // set up champions
            SetupTeamUnits(championsChosen);
            SetupAI();
            // decide which team starts first
            currentTurn = RandomlyDecideTurn();
            Debug.LogWarning("Game Started: " + currentTurn + " Team First");
            switch (currentTurn)
            {
                case GameTurn.Player:
                    currentTeam = alivePlayerChampions;
                    break;
                case GameTurn.Enemy:
                    currentTeam = aliveEnemyChampions;
                    break;
            }
            // give control to next champion
            championIndex = -1;
            NextChampion();
            isGameStarted = true;
        }

        private void SetupTeamUnits(ChampionName[] championsChosen)
        {
            playerArcher.gameObject.SetActive(false);
            playerMage.gameObject.SetActive(false);
            playerBlademaster.gameObject.SetActive(false);

            playerTeamChampions = new List<ChampionController>();
            foreach (ChampionName championName in championsChosen)
            {
                switch (championName)
                {
                    case ChampionName.Archer:
                        playerTeamChampions.Add(playerArcher);
                        break;
                    case ChampionName.Mage:
                        playerTeamChampions.Add(playerMage);
                        break;
                    case ChampionName.Blademaster:
                        playerTeamChampions.Add(playerBlademaster);
                        break;
                }
            }

            alivePlayerChampions = new List<ChampionController>();

            for (int i = 0; i < playerTeamChampions.Count; i++)
            {
                alivePlayerChampions.Add(playerTeamChampions[i]);
                alivePlayerChampions[i].SetAlive();
                alivePlayerChampions[i].championHasDiedEvent.AddListener(UpdateChampionDied);
                alivePlayerChampions[i].gameObject.SetActive(true);
                alivePlayerChampions[i].MoveTo(i, 0);
            }

            aliveEnemyChampions = new List<ChampionController>();

            for (int i = 0; i < enemyTeamChampions.Count; i++)
            {
                aliveEnemyChampions.Add(enemyTeamChampions[i]);
                aliveEnemyChampions[i].SetAlive();
                aliveEnemyChampions[i].championHasDiedEvent.AddListener(UpdateChampionDied);
                aliveEnemyChampions[i].gameObject.SetActive(true);
                aliveEnemyChampions[i].MoveTo(6, 6-i);
            }
        }

        private void SetupAI()
        {
            EnemyController firstEnemy = (EnemyController)aliveEnemyChampions[0];
            EnemyController secondEnemy = (EnemyController)aliveEnemyChampions[1];

            firstEnemy.SetCompanionChampion(secondEnemy);
            secondEnemy.SetCompanionChampion(firstEnemy);

            firstEnemy.SetPlayerChampions(alivePlayerChampions);
            secondEnemy.SetPlayerChampions(alivePlayerChampions);

            firstEnemy.endTurnEvent.RemoveAllListeners();
            firstEnemy.endTurnEvent.AddListener(NextChampion);
            secondEnemy.endTurnEvent.RemoveAllListeners();
            secondEnemy.endTurnEvent.AddListener(NextChampion);
        }

        private GameTurn RandomlyDecideTurn()
        {
            return (Random.Range(0, 2) == 0) ? (GameTurn.Player) : (GameTurn.Enemy);
        }

        public void NextChampion()
        {
            // if this is called after game over, ignore it
            if (aliveEnemyChampions.Count == 0 || alivePlayerChampions.Count == 0) return;
            // switch team if previous champion was the last in his/her team
            if (championIndex == currentTeam.Count - 1)
            {
                SwitchTeam();
            }
            // update the current champion and the next champion (for display)
            championIndex++;
            currentChampion = currentTeam[championIndex];
            nextChampion = PeekNextChampion();
            Debug.LogWarning("New Turn! Now " + currentChampion.gameObject.name);
            // apply active events (including debuffs)
            currentChampion.UpdateActiveEvents();
            // if champion dies after applying active events
            // skip this champion
            // else
            // start new round as per normal
            if (currentChampion.IsChampionAlive())
            {
                currentChampion.StartNewRound();
                uiManager.SetupBattleUI(currentChampion);
            }
            else
            {
                championIndex--;
                NextChampion();
            }
        }

        public ChampionController PeekNextChampion()
        {
            int nextChampionIndex = championIndex + 1;
            if (nextChampionIndex == currentTeam.Count)
            {
                switch (currentTurn)
                {
                    case GameTurn.Player:
                        return aliveEnemyChampions[0];
                    case GameTurn.Enemy:
                        return alivePlayerChampions[0];
                }
            }

            return currentTeam[nextChampionIndex];
        }

        private void SwitchTeam()
        {
            switch (currentTurn)
            {
                case GameTurn.Player:
                    currentTurn = GameTurn.Enemy;
                    currentTeam = aliveEnemyChampions;
                    break;
                case GameTurn.Enemy:
                    currentTurn = GameTurn.Player;
                    currentTeam = alivePlayerChampions;
                    break;
            }

            // update at the start of each turn
            UpdateNonChampionActiveEvents(currentTurn);

            championIndex = -1;
        }

        private void UpdateNonChampionActiveEvents(GameTurn nextTurn)
        {
            tileManager.UpdateActiveEvents(nextTurn);
        }

        public ChampionController GetCurrentChampion()
        {
            return currentChampion; 
        }

        public ChampionController GetNextChampion()
        {
            return nextChampion;
        }

        public void UpdateChampionDied(ChampionController champion)
        {
            // hide the champion from the chessboard
            champion.gameObject.SetActive(false);
            // remove the champion from his/her team
            if (alivePlayerChampions.Contains(champion))
            {
                alivePlayerChampions.Remove(champion);
                if (alivePlayerChampions.Count == 0)
                {
                    GameOver(GameTurn.Enemy);
                    return;
                }
            }
            else if (aliveEnemyChampions.Contains(champion))
            {
                aliveEnemyChampions.Remove(champion);
                if (aliveEnemyChampions.Count == 0)
                {
                    GameOver(GameTurn.Player);
                    return;
                }
            }
        }

        private void GameOver(GameTurn winnerTeam)
        {
            isGameStarted = false;
            bool playerWon = winnerTeam == GameTurn.Player;
            uiManager.ToggleGameOverScreen(playerWon);
        }
    }
}
