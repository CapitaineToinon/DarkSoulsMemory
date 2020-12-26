using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsMemory {
    class PtdeMemoryWatcher : DarkSoulsMemoryWatcher {
        private SigScanTarget CHAR_DATA = new SigScanTarget(2, "8B 0D ?? ?? ?? ?? 8B 7E 1C 8B 49 08 8B 46 20 81 C1 B8 01 00 00 57 51 32 DB");
        private SigScanTarget CURRENT_SAVE_SLOT = new SigScanTarget(2, "8B 0D ?? ?? ?? ?? 80 B9 4F 0B 00 00 00 C6 44 24 28 00");

        public override MemoryWatcher<int> InGameTime { get; }
        public override MemoryWatcher<int> CurrentSaveSlot { get; }

        public PtdeMemoryWatcher(Process process)
        {
            if (process == null)
                throw new NullReferenceException("Process cannot be null");

            var scanner = new SignatureScanner(process, process.MainModule.BaseAddress, process.MainModule.ModuleMemorySize);

            IntPtr pCharData;
            if ((pCharData = scanner.Scan(CHAR_DATA)) == IntPtr.Zero)
                throw new NullReferenceException("Failed to Scan CHAR_DATA");

            IntPtr pCurrentSaveSlot;
            if ((pCurrentSaveSlot = scanner.Scan(CURRENT_SAVE_SLOT)) == IntPtr.Zero)
                throw new NullReferenceException("Failed to Scan CURRENT_SAVE_SLOT");

            InGameTime = new MemoryWatcher<int>(new DeepPointer(pCharData, 0x0, 0x68));
            CurrentSaveSlot = new MemoryWatcher<int>(new DeepPointer(pCurrentSaveSlot, 0x0, 0xA70));
        }
    }
}
