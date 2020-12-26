using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;

namespace DarkSoulsMemory {
    class DsrMemoryWatcher : DarkSoulsMemoryWatcher {
        struct Target {
            public SigScanTarget SigScan;
            public int offset;
            public int intructionSize;
        }

        private Target CHR_CLASS_BASE = new Target()
        {
            SigScan = new SigScanTarget(3, "48 8B 05 ?? ?? ?? ?? 48 85 C0 ?? ?? F3 0F 58 80 AC 00 00 00"),
            offset = 3,
            intructionSize = 7,
        };

        public override MemoryWatcher<int> InGameTime { get; }
        public override MemoryWatcher<int> CurrentSaveSlot { get; }

        public DsrMemoryWatcher(Process process)
        {
            if (process == null)
                throw new NullReferenceException("Process cannot be null");

            var scanner = new SignatureScanner(process, process.MainModule.BaseAddress, process.MainModule.ModuleMemorySize);

            IntPtr pChrClassBase;
            if ((pChrClassBase = scanner.Scan(CHR_CLASS_BASE.SigScan)) == IntPtr.Zero)
                throw new NullReferenceException("Failed to Scan CHR_CLASS_BASE_AOB");

            ExtensionMethods.ReadValue<int>(process, pChrClassBase, out int value);
            pChrClassBase = IntPtr.Add(pChrClassBase, value - CHR_CLASS_BASE.offset + CHR_CLASS_BASE.intructionSize);

            InGameTime = new MemoryWatcher<int>(new DeepPointer(pChrClassBase, 0xA4));

            // todo
            CurrentSaveSlot = new MemoryWatcher<int>(IntPtr.Zero)
            {
                Enabled = false
            };
        }
    }
}
