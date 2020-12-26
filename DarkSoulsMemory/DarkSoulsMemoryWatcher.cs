using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsMemory {
    abstract class DarkSoulsMemoryWatcher {
        public abstract MemoryWatcher<int> InGameTime { get; }
        public abstract MemoryWatcher<int> CurrentSaveSlot { get; }
    }
}
