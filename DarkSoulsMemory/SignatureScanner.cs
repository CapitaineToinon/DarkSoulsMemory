using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;

namespace DarkSoulsMemory {
    class SignatureScanner : LiveSplit.ComponentUtil.SignatureScanner {

        public SignatureScanner(Process proc, IntPtr addr, int size) : base(proc, addr, size)
        {

        }

        public SignatureScanner(byte[] mem) : base(mem)
        {
  
        }

        public IntPtr ScanRelative(SigScanTarget target, int offset, int instructionSize)
        {
            IntPtr ptr = Scan(target);

            if (ptr != IntPtr.Zero)
            {
                Process.ReadValue(ptr, out int val);
                ptr = IntPtr.Add(ptr, val - offset + instructionSize);
            }

            return ptr;
        }
    }
}
