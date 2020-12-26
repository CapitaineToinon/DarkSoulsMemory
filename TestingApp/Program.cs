using System;
using System.Threading;
using DarkSoulsMemory;

namespace TestingApp {
    class Program {
        static void Main(string[] args)
        {
            var DarkSouls = new DarkSouls();
            DarkSouls.OnInGameTimeChanged += DarkSouls_OnInGameTimeChanged; ;

            while (true)
            {
                DarkSouls.Update();
                Thread.Sleep(1000);
            }
        }

        private static void DarkSouls_OnInGameTimeChanged(int old, int current)
        {
            Console.WriteLine("IGT in ms: " + current);
        }
    }
}
