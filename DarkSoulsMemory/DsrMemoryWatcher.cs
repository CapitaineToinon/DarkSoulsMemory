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

        private Target FLAGS = new Target()
        {
            SigScan = new SigScanTarget(3, "48 8B 0D ?? ?? ?? ?? 99 33 C2 45 33 C0 2B C2 8D 50 F6"),
            offset = 3,
            intructionSize = 7,
        };

        public DsrMemoryWatcher(Process process) : base()
        {
            if (process == null)
                throw new NullReferenceException("Process cannot be null");

            int val;
            var scanner = new SignatureScanner(process, process.MainModule.BaseAddress, process.MainModule.ModuleMemorySize);

            IntPtr pChrClassBase;
            if ((pChrClassBase = scanner.Scan(CHR_CLASS_BASE.SigScan)) == IntPtr.Zero)
                throw new NullReferenceException("Failed to Scan CHR_CLASS_BASE_AOB");

            process.ReadValue(pChrClassBase, out val);
            pChrClassBase = IntPtr.Add(pChrClassBase, val - CHR_CLASS_BASE.offset + CHR_CLASS_BASE.intructionSize);

            InGameTime = new MemoryWatcher<int>(new DeepPointer(pChrClassBase, 0xA4));

            IntPtr pFlags;
            if ((pFlags = scanner.Scan(FLAGS.SigScan)) == IntPtr.Zero)
                throw new NullReferenceException("Failed to Scan FLAGS");

            process.ReadValue(pFlags, out val);
            pFlags = IntPtr.Add(pFlags, val - FLAGS.offset + FLAGS.intructionSize);
            new DeepPointer(pFlags, 0).Deref(process, out val);
            pFlags = new IntPtr(val);

            BossFlags = FlagRegionsWatcher.From(Flags.Bosses, pFlags);
            ItemFlags = FlagRegionsWatcher.From(Flags.Items, pFlags);

            // todo
            CurrentSaveSlot = new MemoryWatcher<int>(IntPtr.Zero)
            {
                Enabled = false
            };
        }
    }
}
