using LiveSplit.ComponentUtil;
using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace DarkSoulsMemory {
    /// <summary>
    /// A FlagRegion is a pointer to a int that is used as a bit field
    /// with the ids of all the flags in that region
    /// </summary>
    public struct FlagRegion {
        public MemoryWatcher<int> Watcher;
        public List<int> Flags;
    }

    /// <summary>
    /// Based on MemoryWatcherList
    /// </summary>
    public class FlagRegionsWatcher : List<FlagRegion> {
        public delegate void FlagRegionDataChangedEventHandler(FlagRegion region);
        public event FlagRegionDataChangedEventHandler OnWatcherDataChanged;

        public void UpdateAll(Process process)
        {
            if (OnWatcherDataChanged != null)
            {
                var changedList = new List<FlagRegion>();

                foreach (var region in this)
                {
                    bool changed = region.Watcher.Update(process);
                    if (changed)
                        changedList.Add(region);
                }

                // only report changes when all of the other watches are updated too
                foreach (var region in changedList)
                {
                    OnWatcherDataChanged(region);
                }
            }
            else
            {
                foreach (var region in this)
                {
                    region.Watcher.Update(process);
                }
            }
        }

        public void ResetAll()
        {
            foreach (var region in this)
            {
                region.Watcher.Reset();
            }
        }

        /// <summary>
        /// Creates a FlagRegionsWatcher based on a list of flags
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="basePointer"></param>
        /// <returns></returns>
        public static FlagRegionsWatcher From(List<int> flags, IntPtr basePointer)
        {
            var flagRegionWatcher = new FlagRegionsWatcher();

            flags
                .Select(id => Flags.GetOffset(id, out uint mask))
                .Distinct()
                .ToList()
                .ForEach(offset => flagRegionWatcher.Add(new FlagRegion
                {
                    Watcher = new MemoryWatcher<int>(IntPtr.Add(basePointer, offset)),
                    Flags = flags.ToList().FindAll(id => Flags.GetOffset(id, out uint mask) == offset),
                }));

            return flagRegionWatcher;
        }

        public static FlagRegionsWatcher From(int[] flags, IntPtr basePointer)
        {
            return From(flags.ToList(), basePointer);
        }
    }
}
