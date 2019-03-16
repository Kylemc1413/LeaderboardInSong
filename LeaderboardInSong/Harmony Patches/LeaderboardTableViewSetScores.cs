using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
namespace LeaderboardInSong.Harmony_Patches
{

    [HarmonyPatch(typeof(LeaderboardTableView),
      new Type[] {

        typeof(List<LeaderboardTableView.ScoreData>),
        typeof(int),
})]
    [HarmonyPatch("SetScores", MethodType.Normal)]
    class LeaderboardTableViewSetScores
    {
        static void Postfix(List<LeaderboardTableView.ScoreData> scores, int specialScorePos)
        {
            Plugin.GrabScores();
        }
    }
}
