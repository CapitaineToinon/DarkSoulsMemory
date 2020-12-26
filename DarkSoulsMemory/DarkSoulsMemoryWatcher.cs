using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsMemory {
    abstract class DarkSoulsMemoryWatcher {
        public MemoryWatcher<int> InGameTime { get; protected set; }
        public MemoryWatcher<int> CurrentSaveSlot { get; protected set; }
        public FlagRegionsWatcher BossFlags { get; protected set; }
        public FlagRegionsWatcher ItemFlags { get; protected set; }

        public void GetInGameTime(Action<int> action)
        {
            if (InGameTime != null)
            {
                action.Invoke(InGameTime.Current);
            }
        }

        public void GetCurrentSaveSlot(Action<int> action)
        {
            if (CurrentSaveSlot != null)
            {
                action.Invoke(CurrentSaveSlot.Current);
            }
        }
    }
}
