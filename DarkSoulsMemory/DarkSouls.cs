using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

namespace DarkSoulsMemory {
    public class DarkSouls
    {
        private Process process = null;
        private DarkSoulsMemoryWatcher state = null;

        public delegate void IntChangedEventHandler(int old, int current);
        public delegate void OnProcessHooked(Process process);
        public delegate void OnProcessUnHooked();
        public event IntChangedEventHandler OnInGameTimeChanged;
        public event IntChangedEventHandler OnCurrentSaveSlotChanged;

        private event OnProcessHooked OnHooked;

        public bool isHooked => this.process != null;

        public DarkSouls()
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

            // Support for both DSR and PTDE
            if (ExtensionMethods.Is64Bit(this.process))
                this.state = new DsrMemoryWatcher(this.process);
            else
                this.state = new PtdeMemoryWatcher(this.process);

            // Listen to various memory changes and fire our own events
            this.state.InGameTime.OnChanged += InGameTime_OnChanged;
            this.state.CurrentSaveSlot.OnChanged += CurrentSaveSlot_OnChanged;

            // Read and trigger events manually as memorywatchers only activate on changed value
            Update();
            InGameTime_OnChanged(state.InGameTime.Old, state.InGameTime.Current);
            CurrentSaveSlot_OnChanged(state.CurrentSaveSlot.Old, state.CurrentSaveSlot.Current);

            // stop listening when program closes
            this.process.Exited += (o, sender) =>
            {
                state.InGameTime.OnChanged -= InGameTime_OnChanged;
                state.CurrentSaveSlot.OnChanged -= CurrentSaveSlot_OnChanged;
                this.process = null;
                this.state = null;
            };
        }

        private void InGameTime_OnChanged(int old, int current)
        {
            OnInGameTimeChanged?.Invoke(old, current);
        }

        private void CurrentSaveSlot_OnChanged(int old, int current)
        {
            OnCurrentSaveSlotChanged?.Invoke(old, current);
        }

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

        public void Update()
        {
            if (!this.isHooked)
                this.Hook();

            this.state?.InGameTime.Update(this.process);
            this.state?.CurrentSaveSlot.Update(this.process);
        }
    }
}
