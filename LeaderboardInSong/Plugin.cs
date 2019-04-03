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
using CustomUI.BeatSaber;
using BS_Utils;
using Harmony;

namespace LeaderboardInSong
{
    public class Plugin : IPlugin
    {
        public string Name => "LeaderboardInSong";
        public string Version => "1.1.2";
        public static List<LeaderboardInfo> playerScores = new List<LeaderboardInfo>();
        internal static StandardLevelDetailViewController standardLevelDetailView;
        internal static BeatmapDifficultyViewController DifficultyViewController;
        public static LeaderboardInfo playerScore;
        public static CustomListViewController board;
        internal static ScoreController scoreController;
        internal static string PlayerName = "";
        internal static int CurrentScore;
        internal static int maxPossibleScore;
        internal static int currentMaxPossibleScore;
        internal static BS_Utils.Gameplay.LevelData levelSceneSetupDataSO;
        internal static bool gameScene;
        internal static BS_Utils.Utilities.Config Config = new BS_Utils.Utilities.Config("LeaderboardInSong");
        public static HarmonyInstance harmony;
        public void OnApplicationStart()
        {
            UI.BasicUI.ReadPrefs();
            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            harmony = HarmonyInstance.Create("com.kyle1413.BeatSaber.LeaderboardInSong");
            ApplyPatches();
        }

        private void SceneManagerOnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            UI.BasicUI.ReadPrefs();
            CurrentScore = 0;
            maxPossibleScore = 0;
            currentMaxPossibleScore = 0;
            gameScene = false;
            if (newScene.name == "MenuCore")
            {
                if (standardLevelDetailView == null)
                    standardLevelDetailView = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().FirstOrDefault();
                if (standardLevelDetailView != null)
                {
                    standardLevelDetailView.didPressPlayButtonEvent += StandardLevelDetailView_didPressPlayButtonEvent;
                }
                if (DifficultyViewController == null)
                    DifficultyViewController = Resources.FindObjectsOfTypeAll<BeatmapDifficultyViewController>().FirstOrDefault();
                if (DifficultyViewController != null)
                {
                    DifficultyViewController.didSelectDifficultyEvent += DifficultyViewController_didSelectDifficultyEvent;
                }
                if (PlayerName == "")
                    PlayerName = BS_Utils.Gameplay.GetUserInfo.GetUserName();

            }

            if (newScene.name == "GameCore")
            {
                gameScene = true;
                if (UI.BasicUI.enabled && !BS_Utils.Gameplay.Gamemode.IsIsolatedLevel)
                {
                    levelSceneSetupDataSO = BS_Utils.Plugin.LevelData;
                    if (levelSceneSetupDataSO != null)
                    {
                        maxPossibleScore = ScoreController.MaxScoreForNumberOfNotes(levelSceneSetupDataSO.GameplayCoreSceneSetupData.difficultyBeatmap.beatmapData.notesCount);
                    }
                    scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().FirstOrDefault();
                    if (scoreController != null)
                        scoreController.scoreDidChangeEvent += ScoreController_scoreDidChangeEvent;
                    CreateBoard();
                    SharedCoroutineStarter.instance.StartCoroutine(UpdateBoardFixed());
                }


            }


        }

        private void DifficultyViewController_didSelectDifficultyEvent(BeatmapDifficultyViewController arg1, IDifficultyBeatmap arg2)
        {
            playerScores.Clear();
        }

        private IEnumerator UpdateBoardFixed()
        {
            yield return new WaitForSeconds(UI.BasicUI.refreshTime);
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
            if (scene.name == "MenuCore")
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
                Log("PlayerScores Count: " + playerScores.Count);
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
                //      Log("Player Index: " + playerIndex);
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
            int lastPos = playerScores.Count - 1;
            int secondLastPos = playerScores.Count - 2;
            if (oldIndex != playerIndex)
                update = true;
            if (update)
            {
                //      Log("Updating");
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
                    default:
                        if (playerIndex == secondLastPos)
                        {
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
                        }
                        else if (playerIndex == lastPos)
                        {
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
                        }
                        else
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
                //      Log("Successful Update");
                board._customListTableView.ReloadData();
            }
            else
            {
                //   Log("Not Updating");
                int playerPos = 4;
                switch (playerIndex)
                {
                    case 0:
                        playerPos = 0;
                        break;
                    case 1:
                        playerPos = 1;
                        break;
                    default:
                        if (playerIndex == secondLastPos)
                            playerPos = 3;
                        else if (playerIndex == lastPos)
                            playerPos = 4;
                        else
                            playerPos = 2;
                        break;
                }
                // Log("Not Updating 2");
                //   Log("Count: " + board.Data.Count);
                //     Log("PlayerPos: " + playerPos);
                board.Data[playerPos].text = $"<#59B0F4>{playerScore.playerName}" + " | " + $"{playerScore.playerScore}";
                //       Log("Successful Not Updating");
                board._customListTableView.ReloadData();
            }

        }

        public static void GrabScores()
        {
            var boards = Resources.FindObjectsOfTypeAll<PlatformLeaderboardViewController>().First()?.GetComponentInChildren<LeaderboardTableView>()?.gameObject?
                .transform?.Find("Viewport")?.Find("Content")?.GetComponentsInChildren<LeaderboardTableCell>();
            if (boards != null)
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
                            if (UI.BasicUI.simpleNames)
                            {
                                if (text.text.Contains("<size=85%>"))
                                {
                                    playerName = text.text.Split('>', '<')[2];
                                    playerName = playerName.Remove(Mathf.Clamp(playerName.Length - 3, 0, playerName.Length), 3);

                                }
                                else if (text.text.Contains("<size=75%>"))
                                {
                                    playerName = text.text.Split('<')[0];
                                    playerName = playerName.Remove(Mathf.Clamp(playerName.Length - 3, 0, playerName.Length), 3);
                                }

                            }

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
                    LeaderboardInfo entry = new LeaderboardInfo(playerName, score, pos);
                    if (!playerScores.Any(x => (x.playerPosition == entry.playerPosition && x.playerScore == entry.playerScore)))
                        playerScores.Add(entry);
                    //        else
                    //          Log("Entry already present");
                }
            if (!playerScores.Contains(playerScore))
            {
                playerScore = new LeaderboardInfo(PlayerName, 0, 0);
                if (!playerScores.Any(x => (x.playerPosition == 0)))
                    playerScores.Add(playerScore);
            }

            //foreach (LeaderboardInfo entry in playerScores)
            //  {
            //      Log("Yoinking Leaderboard Entry for Position: " + entry.playerPosition);
            //      Log("Name: " + entry.playerName);
            //      Log("Score: " + entry.playerScore);
            //}
        }
        public static void Log(string message)
        {
            Console.WriteLine("[{0}] {1}", "LeaderboardInSong", message);
        }

        public static void ApplyPatches()
        {

            try
            {

                harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

    }
}
