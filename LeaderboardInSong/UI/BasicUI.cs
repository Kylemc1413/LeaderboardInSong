using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomUI.GameplaySettings;
using CustomUI.Settings;
using IllusionPlugin;
namespace LeaderboardInSong.UI
{
    class BasicUI
    {
        public static bool enabled;
        public static bool sortByAcc;
        public static bool simpleNames;
        public static float x;
        public static float y;
        public static float z;
        public static float scale;
        public static float refreshTime;
        public static void ReadPrefs()
        {
            enabled = Plugin.Config.GetBool("Options", "Enabled", true, true);
            sortByAcc = Plugin.Config.GetBool("Options", "SortByAcc", true, true);
            simpleNames = Plugin.Config.GetBool("Options", "SimpleNames", false, true);
            x = Plugin.Config.GetFloat("Options", "X", -2f, true);
            y = Plugin.Config.GetFloat("Options", "Y", 0.5f, true);
            z = Plugin.Config.GetFloat("Options", "Z", 7f, true);
            scale = Plugin.Config.GetFloat("Options", "Scale", 0.3f, true);
            refreshTime = Plugin.Config.GetFloat("Options", "RefreshTime", 0.5f, true);
        }
        public static void CreateUI()
        {
            //This will create the UI for the plugin when called, keep in mind that the mod will require CustomUI when executing this as it calls functions etc from the library
            CreateGameplayOptionsUI();



        }

        public static void CreateGameplayOptionsUI()
        {
            //Example submenu option
            var pluginSubmenu = GameplaySettingsUI.CreateSubmenuOption(GameplaySettingsPanels.ModifiersLeft, "LeaderboardInSong", "MainMenu", "pluginMenu1", "Settings for the LeaderboardInSong Plugin");

            //Example Toggle Option within a submenu
            var enableOption = GameplaySettingsUI.CreateToggleOption(GameplaySettingsPanels.ModifiersLeft, "Enabled", "pluginMenu1", "Enable the in song leaderboard. Compares your score in the song against those on the leaderboard page selected when starting the song");
            enableOption.GetValue = Plugin.Config.GetBool("Options", "Enabled", true, true);
            enableOption.OnToggle += (value) => { enabled = value; Plugin.Config.SetBool("Options", "Enabled", value); };

            var accOption = GameplaySettingsUI.CreateToggleOption(GameplaySettingsPanels.ModifiersLeft, "Order by Percentage", "pluginMenu1", "Order the scores in the leaderboard based on your current accuracy compared to that of the other scores rather than the scores itself");
            accOption.GetValue = sortByAcc;
            accOption.OnToggle += (value) => { sortByAcc = value; Plugin.Config.SetBool("Options", "SortByAcc", value); };

            var simpleNameOption = GameplaySettingsUI.CreateToggleOption(GameplaySettingsPanels.ModifiersLeft, "Simplify Names", "pluginMenu1", "Removes excess information that scoresaber adds to scores, such as percentage, modifiers, and performance points");
            simpleNameOption.GetValue = simpleNames;
            simpleNameOption.OnToggle += (value) => { simpleNames = value; Plugin.Config.SetBool("Options", "SimpleNames", value); };


        }





    }
}
