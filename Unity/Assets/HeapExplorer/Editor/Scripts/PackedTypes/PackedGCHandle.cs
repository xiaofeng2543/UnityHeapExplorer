﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace HeapExplorer
{
    /// <summary>
    /// A description of a GC handle used by the virtual machine.
    /// </summary>
    /// <remarks>
    /// A GCHandle is a struct that contains a handle to an object.
    /// It's mainly used for holding onto a managed object that gets passed to the unmanaged world to prevent the GC from collecting the object.
    /// You can also create a Pinned GCHandle to a managed object and retrieve the object's address in memory. 
    /// https://blogs.msdn.microsoft.com/clyon/2005/03/18/the-truth-about-gchandles/
    /// </remarks>
    [Serializable]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct PackedGCHandle
    {
        // The address of the managed object that the GC handle is referencing.
        public System.UInt64 target;
        
        [NonSerialized] public System.Int32 gcHandlesArrayIndex;
        [NonSerialized] public System.Int32 managedObjectsArrayIndex;

        const System.Int32 k_Version = 1;

        public static void Write(System.IO.BinaryWriter writer, PackedGCHandle[] value)
        {
#if HEAPEXPLORER_WRITE_HEADER
            writer.Write(k_Version);
#endif

            writer.Write(value.Length);

            for (int n = 0, nend = value.Length; n < nend; ++n)
            {
                writer.Write(value[n].target);
            }
        }

        public static void Read(System.IO.BinaryReader reader, out PackedGCHandle[] value)
        {
            value = new PackedGCHandle[0];

#if HEAPEXPLORER_READ_HEADER
            var version = reader.ReadInt32();
            if (version >= 1)
#endif
            {
                var length = reader.ReadInt32();
                value = new PackedGCHandle[length];

                for (int n = 0, nend = value.Length; n < nend; ++n)
                {
                    value[n].target = reader.ReadUInt64();
                    value[n].gcHandlesArrayIndex = n;
                    value[n].managedObjectsArrayIndex = -1;
                }
            }
        }

        public static PackedGCHandle[] FromMemoryProfiler(UnityEditor.MemoryProfiler.PackedGCHandle[] source)
        {
            var value = new PackedGCHandle[source.Length];
            for (int n = 0, nend = source.Length; n < nend; ++n)
            {
                value[n] = new PackedGCHandle
                {
                    target = source[n].target,
                    gcHandlesArrayIndex = n,
                    managedObjectsArrayIndex = -1,
                };
            }
            return value;
        }
    }
}
