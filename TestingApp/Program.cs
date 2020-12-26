using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DarkSoulsMemory;

namespace TestingApp {
    class Program {
        static Dictionary<Static.Bosses, bool> bosses = Enum.GetValues(typeof(Static.Bosses)).Cast<Static.Bosses>().ToDictionary(key => key, key => false);

        static void Main(string[] args)
        {
            var darksouls = DarkSouls.GetInstance();
            darksouls.OnBossDefeated += DarkSouls_OnBossDefeated;

            while (true)
            {
                darksouls.Update();

                int killed = bosses.Where(boss => boss.Value).Count();
                Console.WriteLine(string.Format("You've killed {0} out of {1} bosses", killed, bosses.Count));

                Thread.Sleep(1000);
            }
        }

        private static void DarkSouls_OnBossDefeated(Static.Bosses boss)
        {
            bosses[boss] = true;
        }
    }
}
