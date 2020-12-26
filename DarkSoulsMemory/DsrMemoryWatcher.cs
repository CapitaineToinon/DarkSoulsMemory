using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;

namespace DarkSoulsMemory {
    class DsrMemoryWatcher : DarkSoulsMemoryWatcher {
        struct Target {
            public SigScanTarget SigScan;
            public int offset;
            public int instructionSize;
        }

        private Target CHR_CLASS_BASE = new Target()
        {
            SigScan = new SigScanTarget(3, "48 8B 05 ?? ?? ?? ?? 48 85 C0 ?? ?? F3 0F 58 80 AC 00 00 00"),
            offset = 3,
            instructionSize = 7,
        };

        private Target FLAGS = new Target()
        {
            SigScan = new SigScanTarget(3, "48 8B 0D ?? ?? ?? ?? 99 33 C2 45 33 C0 2B C2 8D 50 F6"),
            offset = 3,
            instructionSize = 7,
        };

        public DsrMemoryWatcher(Process process) : base()
        {
            if (process == null)
                throw new NullReferenceException("Process cannot be null");

            int val;
            var scanner = new SignatureScanner(process, process.MainModule.BaseAddress, process.MainModule.ModuleMemorySize);

            IntPtr pChrClassBase = scanner.ScanRelative(CHR_CLASS_BASE.SigScan, CHR_CLASS_BASE.offset, CHR_CLASS_BASE.instructionSize);
            if (pChrClassBase == IntPtr.Zero)
                throw new NullReferenceException("Failed to Scan CHR_CLASS_BASE_AOB");

            IntPtr pFlags = scanner.ScanRelative(FLAGS.SigScan, FLAGS.offset, FLAGS.instructionSize);
            if (pFlags == IntPtr.Zero)
                throw new NullReferenceException("Failed to Scan FLAGS");

            InGameTime = new MemoryWatcher<int>(new DeepPointer(pChrClassBase, 0xA4));

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
