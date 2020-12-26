using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

namespace DarkSoulsMemory {
    public class DarkSouls
    {
        /// <summary>
        /// Singleton
        /// </summary>
        private static readonly Lazy<DarkSouls> lazy = new Lazy<DarkSouls>(() => new DarkSouls());
        public static DarkSouls GetInstance() => lazy.Value;

        private Process process = null;
        private DarkSoulsMemoryWatcher state = null;

        public delegate void IntChangedEventHandler(int old, int current);
        public delegate void FlagChangedEventHandler(int flag, bool old, bool current);
        public delegate void BossDefeatedEventHandler(Static.Bosses boss);
        public delegate void ItemPickupEventHandler(int flag);
        public delegate void OnProcessHooked(Process process);
        public delegate void OnProcessUnHooked();
        public event IntChangedEventHandler OnInGameTimeChanged;
        public event IntChangedEventHandler OnCurrentSaveSlotChanged;
        public event FlagChangedEventHandler OnFlagChanged;
        public event BossDefeatedEventHandler OnBossDefeated;
        public event ItemPickupEventHandler OnItemPickup;

        private event OnProcessHooked OnHooked;

        public bool isHooked => this.process != null;

        private DarkSouls()
        {
            this.process = null;
            this.OnHooked += DarkSouls_OnHooked;
        }

        private void DarkSouls_OnHooked(Process process)
        {
            if (process == null)
                throw new ArgumentNullException("Process cannot be null");

            this.process = process;
            this.process.EnableRaisingEvents = true;

            if (ExtensionMethods.Is64Bit(this.process))
            {
                state = new DsrMemoryWatcher(this.process);
            }
            else
            {
                state = new PtdeMemoryWatcher(this.process);
            }

            // Listen to various memory changes and fire our own events
            state.InGameTime.OnChanged += InGameTime_OnChanged;
            state.CurrentSaveSlot.OnChanged += CurrentSaveSlot_OnChanged;
            state.BossFlags.OnWatcherDataChanged += BossFlags_OnWatcherDataChanged;
            state.ItemFlags.OnWatcherDataChanged += ItemFlags_OnWatcherDataChanged;

            // Read and trigger events manually as memorywatchers only activate on changed value
            Update();
            OnInGameTimeChanged?.Invoke(state.InGameTime.Old, state.InGameTime.Current);
            OnCurrentSaveSlotChanged?.Invoke(state.CurrentSaveSlot.Old, state.CurrentSaveSlot.Current);
            state.BossFlags.ForEach(region => RaiseBosses(region, true));
            state.ItemFlags.ForEach(region => RaiseItems(region, true));

            // stop listening when program closes
            this.process.Exited += Process_Exited;
        }

        /// <summary>
        /// Clears event and local variables when process is unhooked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Process_Exited(object sender, EventArgs e)
        {
            state.InGameTime.OnChanged -= InGameTime_OnChanged;
            state.CurrentSaveSlot.OnChanged -= CurrentSaveSlot_OnChanged;
            state.BossFlags.OnWatcherDataChanged -= BossFlags_OnWatcherDataChanged;
            state.ItemFlags.OnWatcherDataChanged -= ItemFlags_OnWatcherDataChanged;
            this.process = null;
            this.state = null;
        }

        private void BossFlags_OnWatcherDataChanged(FlagRegion region)
        {
            RaiseBosses(region, false);
        }

        private void ItemFlags_OnWatcherDataChanged(FlagRegion region)
        {
            RaiseItems(region, false);
        }

        private void InGameTime_OnChanged(int old, int current)
        {
            OnInGameTimeChanged?.Invoke(old, current);
        }

        private void CurrentSaveSlot_OnChanged(int old, int current)
        {
            OnCurrentSaveSlotChanged?.Invoke(old, current);
        }

        /// <summary>
        /// Raises the OnItemPickup if the flags for item picks up changed
        /// or if the force option is on
        /// </summary>
        /// <param name="region">The flag memory region watched</param>
        /// <param name="force">Forces to raise the event</param>
        public void RaiseItems(FlagRegion region, bool force = false)
        {
            region.Flags.ForEach(flag =>
            {
                Flags.GetOffset(flag, out uint mask);
                bool old = (region.Watcher.Old & mask) != 0;
                bool current = (region.Watcher.Current & mask) != 0;

                if (!old && current || force && current)
                {
                    OnItemPickup?.Invoke(flag);
                }

                if (old != current || force)
                {
                    OnFlagChanged?.Invoke(flag, old, current);
                }
            });
        }

        /// <summary>
        /// Raises the OnBossDefeated if the flags for bosses defeated changed
        /// or if the force option is on
        /// </summary>
        /// <param name="region">The flag memory region watched</param>
        /// <param name="force">Forces to raise the event</param>
        public void RaiseBosses(FlagRegion region, bool force = false)
        {
            region.Flags.ForEach(flag =>
            {
                Flags.GetOffset(flag, out uint mask);
                bool old = (region.Watcher.Old & mask) != 0;
                bool current = (region.Watcher.Current & mask) != 0;

                if (!old && current || force && current)
                {
                    OnBossDefeated?.Invoke((Static.Bosses)flag);
                }

                if (old != current || force)
                {
                    OnFlagChanged?.Invoke(flag, old, current);
                }
            });
        }

        /// <summary>
        /// Hook to PTDE or DSR
        /// </summary>
        public void Hook()
        {
            Process ptde, dsr;

            if ((ptde = Process.GetProcessesByName("DARKSOULS").FirstOrDefault()) != null)
            {
                OnHooked?.Invoke(ptde);
                return;
            }

            if ((dsr = Process.GetProcessesByName("DarkSoulsRemastered").FirstOrDefault()) != null)
            {
                OnHooked?.Invoke(dsr);
                return;
            }
        }

        /// <summary>
        /// Main Update Loop
        /// </summary>
        public void Update()
        {
            if (!this.isHooked)
                this.Hook();

            this.state?.InGameTime.Update(this.process);
            this.state?.CurrentSaveSlot.Update(this.process);
            this.state?.BossFlags.UpdateAll(this.process);
            this.state?.ItemFlags.UpdateAll(this.process);
        }
    }
}
