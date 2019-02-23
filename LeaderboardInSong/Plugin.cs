using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using IllusionPlugin;
using UnityEngine.UI;
using TMPro;
using CustomUI.BeatSaber;
using BS_Utils;
namespace LeaderboardInSong
{
    public class Plugin : IPlugin
    {
        public string Name => "LeaderboardInSong";
        public string Version => "0.5.0";
        public static List<LeaderboardInfo> playerScores = new List<LeaderboardInfo>();
        internal static StandardLevelDetailViewController standardLevelDetailView;
        public static LeaderboardInfo playerScore;
        public static CustomListViewController board;
        internal static ScoreController scoreController;
        internal static string PlayerName = "";
        internal static int CurrentScore;
        internal static int maxPossibleScore;
        internal static int currentMaxPossibleScore;
        internal static StandardLevelSceneSetupDataSO levelSceneSetupDataSO;
        internal static bool gameScene;
        internal static BS_Utils.Utilities.Config Config = new BS_Utils.Utilities.Config("LeaderboardInSong");
        public void OnApplicationStart()
        {
            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void SceneManagerOnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            UI.BasicUI.ReadPrefs();
            CurrentScore = 0;
            maxPossibleScore = 0;
            currentMaxPossibleScore = 0;
            gameScene = false;
            if (newScene.name == "Menu")
            {
                if (standardLevelDetailView == null)
                    standardLevelDetailView = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().FirstOrDefault();
                if (standardLevelDetailView != null)
                {
                    standardLevelDetailView.didPressPlayButtonEvent += StandardLevelDetailView_didPressPlayButtonEvent;
                }
                if (PlayerName == "")
                    PlayerName = BS_Utils.Gameplay.GetUserInfo.GetUserName();

            }

            if (newScene.name == "GameCore")
            {
                gameScene = true;
                if (UI.BasicUI.enabled && !BS_Utils.Gameplay.Gamemode.IsIsolatedLevel)
                {
                    levelSceneSetupDataSO = Resources.FindObjectsOfTypeAll<StandardLevelSceneSetupDataSO>().FirstOrDefault();
                    if (levelSceneSetupDataSO != null)
                    {
                        maxPossibleScore = ScoreController.MaxScoreForNumberOfNotes(levelSceneSetupDataSO.difficultyBeatmap.beatmapData.notesCount);
                    }
                    scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().FirstOrDefault();
                    if (scoreController != null)
                        scoreController.scoreDidChangeEvent += ScoreController_scoreDidChangeEvent;
                    CreateBoard();
                    SharedCoroutineStarter.instance.StartCoroutine(UpdateBoardFixed());
                }


            }


        }
        private IEnumerator UpdateBoardFixed()
        {
            yield return new WaitForSeconds(0.2f);
            if (gameScene)
            {
                UpdateBoard();
                SharedCoroutineStarter.instance.StartCoroutine(UpdateBoardFixed());
            }

        }
        private void ScoreController_scoreDidChangeEvent(int num)
        {
            CurrentScore = num;
            currentMaxPossibleScore = scoreController.GetField<int>("_immediateMaxPossibleScore");
        }

        private void StandardLevelDetailView_didPressPlayButtonEvent(StandardLevelDetailViewController obj)
        {
            GrabScores();
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            //Create GameplayOptions/SettingsUI if using either
            if (scene.name == "Menu")
                UI.BasicUI.CreateUI();

        }

        public void OnApplicationQuit()
        {
            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        public void OnLevelWasLoaded(int level)
        {

        }

        public void OnLevelWasInitialized(int level)
        {
        }

        public void OnUpdate()
        {


        }

        public void OnFixedUpdate()
        {
        }

        public static void CreateBoard()
        {
            Log("Creating Board");
            try
            {
                //    Log("Sorting Scores");
                playerScores.Sort(LeaderboardInfo.CompareScore);
                GameObject canvasobj = new GameObject("LeaderboardInSong");
                Canvas canvas = canvasobj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                CanvasScaler cs = canvasobj.AddComponent<CanvasScaler>();
                cs.scaleFactor = 1f;
                cs.dynamicPixelsPerUnit = 10f;
                GraphicRaycaster gr = canvasobj.AddComponent<GraphicRaycaster>();
                canvasobj.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1f);
                canvasobj.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1f);
                //      Log("Created Canvas");
                board = BeatSaberUI.CreateViewController<CustomListViewController>();
                board.transform.SetParent(canvasobj.GetComponent<RectTransform>());
                board.transform.localScale *= 0.15f * UI.BasicUI.scale;
                board.transform.localPosition = new Vector3(UI.BasicUI.x, UI.BasicUI.y, UI.BasicUI.z);
                //           Log("Created board");
                int playerIndex = playerScores.IndexOf(playerScore);
                for (int i = playerIndex - 4; i <= playerIndex; i++)
                {

                    if (i >= 0 && i < playerScores.Count)
                    {
                        if (i != playerIndex)
                            board.Data.Add(new CustomCellInfo($"{playerScores[i].playerPosition}" + " | " + $"{playerScores[i].playerName}" + " | " + $"{playerScores[i].playerScore}", ""));
                        else
                            board.Data.Add(new CustomCellInfo($"<#59B0F4>{playerScores[i].playerName}" + " | " + $"{playerScores[i].playerScore}", ""));

                    }
                }
                //                  Log("Filled Data");
                //   foreach (LeaderboardInfo player in playerScores)
                //           {

                //                  board.Data.Add(new CustomCellInfo(player.playerPosition + '\t' + player.playerName + '\t' + player.playerScore, ""));
                //             }
                board.__Activate(VRUI.VRUIViewController.ActivationType.AddedToHierarchy);
                //            Log("Activated Board");
                (board._customListTableView.transform.parent as RectTransform).sizeDelta = new Vector2(100f, 120f);
                //              Log("SizeDelta");
                board._pageDownButton.gameObject.SetActive(false);
                board._pageUpButton.gameObject.SetActive(false);
                //                Log("Buttons");
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        public static void UpdateBoard()
        {
            bool update = false;
            playerScore.playerScore = CurrentScore;
            int oldIndex = playerScores.IndexOf(playerScore);
            playerScores.Sort(LeaderboardInfo.CompareScore);
            int playerIndex = playerScores.IndexOf(playerScore);
            if (oldIndex != playerIndex)
                update = true;
            if (update)
            {
                board.Data.Clear();
                switch (playerIndex)
                {
                    case 0:
                        for (int i = playerIndex; i <= playerIndex + 4; i++)
                        {
                            if (i >= 0 && i < playerScores.Count)
                            {
                                if (i != playerIndex)
                                    board.Data.Add(new CustomCellInfo($"{playerScores[i].playerPosition}" + " | " + $"{playerScores[i].playerName}" + " | " + $"{playerScores[i].playerScore}", ""));
                                else
                                    board.Data.Add(new CustomCellInfo($"<#59B0F4>{playerScores[i].playerName}" + " | " + $"{playerScores[i].playerScore}", ""));
                            }
                        }
                        break;
                    case 1:
                        for (int i = playerIndex - 1; i <= playerIndex + 3; i++)
                        {
                            if (i >= 0 && i < playerScores.Count)
                            {
                                if (i != playerIndex)
                                    board.Data.Add(new CustomCellInfo($"{playerScores[i].playerPosition}" + " | " + $"{playerScores[i].playerName}" + " | " + $"{playerScores[i].playerScore}", ""));
                                else
                                    board.Data.Add(new CustomCellInfo($"<#59B0F4>{playerScores[i].playerName}" + " | " + $"{playerScores[i].playerScore}", ""));
                            }
                        }
                        break;
                    case 9:
                        for (int i = playerIndex - 3; i <= playerIndex + 1; i++)
                        {
                            if (i >= 0 && i < playerScores.Count)
                            {
                                if (i != playerIndex)
                                    board.Data.Add(new CustomCellInfo($"{playerScores[i].playerPosition}" + " | " + $"{playerScores[i].playerName}" + " | " + $"{playerScores[i].playerScore}", ""));
                                else
                                    board.Data.Add(new CustomCellInfo($"<#59B0F4>{playerScores[i].playerName}" + " | " + $"{playerScores[i].playerScore}", ""));
                            }
                        }
                        break;
                    case 10:
                        for (int i = playerIndex - 4; i <= playerIndex; i++)
                        {
                            if (i >= 0 && i < playerScores.Count)
                            {
                                if (i != playerIndex)
                                    board.Data.Add(new CustomCellInfo($"{playerScores[i].playerPosition}" + " | " + $"{playerScores[i].playerName}" + " | " + $"{playerScores[i].playerScore}", ""));
                                else
                                    board.Data.Add(new CustomCellInfo($"<#59B0F4>{playerScores[i].playerName}" + " | " + $"{playerScores[i].playerScore}", ""));
                            }
                        }
                        break;
                    default:
                        for (int i = playerIndex - 2; i <= playerIndex + 2; i++)
                        {
                            if (i >= 0 && i < playerScores.Count)
                            {
                                if (i != playerIndex)
                                    board.Data.Add(new CustomCellInfo($"{playerScores[i].playerPosition}" + " | " + $"{playerScores[i].playerName}" + " | " + $"{playerScores[i].playerScore}", ""));
                                else
                                    board.Data.Add(new CustomCellInfo($"<#59B0F4>{playerScores[i].playerName}" + " | " + $"{playerScores[i].playerScore}", ""));
                            }
                        }
                        break;

                }

                board._customListTableView.ReloadData();
            }
            else
            {
                int playerPos = 4;
                switch (playerIndex)
                {
                    case 0:
                        playerPos = 0;
                        break;
                    case 1:
                        playerPos = 1;
                        break;
                    case 10:
                        playerPos = 4;
                        break;
                    case 9:
                        playerPos = 3;
                        break;
                    default:
                        playerPos = 2;
                        break;
                }
                board.Data[playerPos].text = $"<#59B0F4>{playerScore.playerName}" + "\t" + $"{playerScore.playerScore}";
                board._customListTableView.ReloadData();
            }

        }

        public static void GrabScores()
        {
            playerScores.Clear();
            var boards = Resources.FindObjectsOfTypeAll<PlatformLeaderboardViewController>().First().GetComponentInChildren<LeaderboardTableView>().gameObject
                .transform.Find("Viewport").Find("Content").GetComponentsInChildren<LeaderboardTableCell>();
            foreach (LeaderboardTableCell cell in boards)
            {
                var cellTexts = cell.GetComponentsInChildren<TextMeshProUGUI>();
                string playerName = "";
                int pos = -1;
                int score = -1;
                foreach (TextMeshProUGUI text in cellTexts)
                {
                    if (text.name == "PlayerName")
                    {
                        playerName = text.text;
                    }
                    if (text.name == "Rank")
                    {
                        pos = int.Parse(text.text);
                    }
                    if (text.name == "Score")
                    {
                        score = int.Parse(text.text.Replace(" ", ""));
                    }

                }
                playerScores.Add(new LeaderboardInfo(playerName, score, pos));
            }
            playerScore = new LeaderboardInfo(PlayerName, 0, 0);
            playerScores.Add(playerScore);
        //    foreach (LeaderboardInfo entry in playerScores)
        //    {
        //        Log("Yoinking Leaderboard Entry for Position: " + entry.playerPosition);
       //         Log("Name: " + entry.playerName);
      //          Log("Score: " + entry.playerScore);
      //      }
        }
        public static void Log(string message)
        {
            Console.WriteLine("[{0}] {1}", "LeaderboardInSong", message);
        }

    }
}
