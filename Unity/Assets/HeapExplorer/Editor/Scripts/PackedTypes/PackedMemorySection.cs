﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace HeapExplorer
{
    // A dump of a piece of memory from the player that's being profiled.
    [Serializable]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct PackedMemorySection
    {
        // The actual bytes of the memory dump.
        public System.Byte[] bytes;

        // The start address of this piece of memory.
        public System.UInt64 startAddress;

        public ulong size
        {
            get
            {
                if (bytes != null)
                    return (ulong)bytes.LongLength;
                return 0;
            }
        }

        const System.Int32 k_Version = 1;

        public static void Write(System.IO.BinaryWriter writer, PackedMemorySection[] value)
        {
#if HEAPEXPLORER_WRITE_HEADER
            writer.Write(k_Version);
#endif

            writer.Write(value.Length);

            for (int n = 0, nend = value.Length; n < nend; ++n)
            {
                writer.Write((System.Int32)value[n].bytes.Length);
                writer.Write(value[n].bytes);
                writer.Write(value[n].startAddress);
            }
        }

        public static void Read(System.IO.BinaryReader reader, out PackedMemorySection[] value)
        {
            value = new PackedMemorySection[0];

#if HEAPEXPLORER_READ_HEADER
            var version = reader.ReadInt32();
            if (version >= 1)
#endif
            {
                var length = reader.ReadInt32();
                value = new PackedMemorySection[length];

                for (int n = 0, nend = value.Length; n < nend; ++n)
                {
                    var count = reader.ReadInt32();
                    value[n].bytes = reader.ReadBytes(count);
                    value[n].startAddress = reader.ReadUInt64();
                }
            }
        }

        public static PackedMemorySection[] FromMemoryProfiler(UnityEditor.MemoryProfiler.MemorySection[] source)
        {
            var value = new PackedMemorySection[source.Length];

            for (int n = 0, nend = source.Length; n < nend; ++n)
            {
                value[n] = new PackedMemorySection
                {
                    bytes = source[n].bytes,
                    startAddress = source[n].startAddress
                };
            }
            return value;
        }
    }

}
