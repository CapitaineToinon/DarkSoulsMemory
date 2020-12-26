using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using DarkSoulsMemory.Util;
using DarkSoulsMemory.Data;

namespace DarkSoulsMemory {
    class PtdeMemoryWatcher : DarkSoulsMemoryWatcher {
        private SigScanTarget CHAR_DATA = new SigScanTarget(2, "8B 0D ?? ?? ?? ?? 8B 7E 1C 8B 49 08 8B 46 20 81 C1 B8 01 00 00 57 51 32 DB");
        private SigScanTarget CURRENT_SAVE_SLOT = new SigScanTarget(2, "8B 0D ?? ?? ?? ?? 80 B9 4F 0B 00 00 00 C6 44 24 28 00");
        private SigScanTarget FLAGS = new SigScanTarget(8, "56 8B F1 8B 46 1C 50 A1 ?? ?? ?? ?? 32 C9");
        private SigScanTarget LOADED = new SigScanTarget(4, "83 EC 14 A1 ?? ?? ?? ?? 8B 48 04 8B 40 08 53 55 56 57 89 4C 24 1C 89 44 24 20 3B C8");

        public PtdeMemoryWatcher(Process process)
        {
            if (process == null)
                throw new NullReferenceException("Process cannot be null");

            int val;
            var scanner = new SignatureScanner(process, process.MainModule.BaseAddress, process.MainModule.ModuleMemorySize);

            IntPtr pCharData = scanner.Scan(CHAR_DATA);
            if (pCharData == IntPtr.Zero)
                throw new NullReferenceException("Failed to Scan CHAR_DATA");

            IntPtr pCurrentSaveSlot = scanner.Scan(CURRENT_SAVE_SLOT);
            if (pCurrentSaveSlot == IntPtr.Zero)
                throw new NullReferenceException("Failed to Scan CURRENT_SAVE_SLOT");

            IntPtr pFlags = scanner.Scan(FLAGS);
            if (pFlags == IntPtr.Zero)
                throw new NullReferenceException("Failed to Scan FLAGS");

            IntPtr pLoaded = scanner.Scan(LOADED);
            if (pLoaded == IntPtr.Zero)
                throw new NullReferenceException("Failed to Scan LOADED");

            InGameTime = new MemoryWatcher<int>(new DeepPointer(pCharData, 0x0, 0x68));
            CurrentSaveSlot = new MemoryWatcher<int>(new DeepPointer(pCurrentSaveSlot, 0x0, 0xA70));
            Loaded = new MemoryWatcher<int>(new DeepPointer(pLoaded, 0, 4, 0))
            {
                // Pointer not working indicates that player is not loaded
                FailAction = MemoryWatcher.ReadFailAction.SetZeroOrNull
            };

            new DeepPointer(pFlags, 0, 0).Deref(process, out val);
            pFlags = new IntPtr(val);

            BossFlags = FlagRegionsWatcher.From(Flags.Bosses, pFlags);
            ItemFlags = FlagRegionsWatcher.From(Flags.Items, pFlags);
        }
    }
}
