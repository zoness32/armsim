// Filename: Memory.cs
// Author: Taylor Eernisse
// Date: 9/18/12

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace armsimGUI
{
    /// <summary>
    /// Class: Memory
    /// Purpose: Handles logic to load and read from simulated memory
    /// Methods: Memory(uint)
    ///          ReadWord(uint)
    ///          WriteWord(uint, uint)
    ///          ReadHalfWord(uint)
    ///          WriteHalfWord(uint, uint)
    ///          ReadByte(uint)
    ///          WriteByte(uint, byte)
    ///          TestFlagInMemory(uint, byte)
    ///          SetFlag(uint, byte, bool)
    ///          ExtractBits(uint, byte, byte)
    ///          RunTests()
    /// </summary>
    class Memory
    {
        public byte[] memory { get; private set; } // holds the byte array of simulated memory

        //--------------------------------------------------------------
        // Purpose: Memory constructor; initiates <memory> to size <size>
        //--------------------------------------------------------------
        public Memory(uint size)
        {
            memory = new byte[size];
        }


        //--------------------------------------------------------------
        // Purpose: Reads a word of data starting at location <addr> in simulated ram
        // Returns: an unsigned integer of data read
        //--------------------------------------------------------------
        public uint ReadWord(uint addr)
        {
            if (addr % 4 == 0 && addr + 1 < memory.Length)
            {
                return (uint)(memory[addr] + ((uint)memory[addr + 1] << 8) + ((uint)memory[addr + 2] << 16) + ((uint)memory[addr + 3] << 24));
            }
            return 0;
        }

        //--------------------------------------------------------------
        // Purpose: Writes a word <data> into simulated ram at <addr>
        // Returns: true if write succeeds, false if it fails
        //--------------------------------------------------------------
        public bool WriteWord(uint addr, uint data)
        {
            if (addr % 4 == 0 && addr + 3 < memory.Length)
            {
                memory[addr] = (byte)(data & 0x00ff);
                memory[addr + 1] = (byte)((data >> 8) & 0x00ff);
                memory[addr + 2] = (byte)((data >> 16) & 0x00ff);
                memory[addr + 3] = (byte)((data >> 24) & 0x00ff);
                return true;
            }
            Trace.WriteLine("WriteWord: Write out of bounds: address = " + addr + " value = " + data);
            return false;
        }

        //--------------------------------------------------------------
        // Purpose: Reads a half byte of data starting at location <addr> in simulated ram
        // Returns: an unsigned short of data read
        //--------------------------------------------------------------
        public ushort ReadHalfWord(uint addr)
        {
            if (addr % 2 == 0 && addr + 1 < memory.Length)
            {
                return (ushort)(memory[addr] + (memory[addr + 1] << 8));
            }
            return 0;
        }

        //--------------------------------------------------------------
        // Purpose: Writes a half word <data> into simulated ram starting at <addr>
        // Returns: true if write succeeds, false if it fails
        //--------------------------------------------------------------
        public bool WriteHalfWord(uint addr, ushort data)
        {
            if (addr % 2 == 0 && addr + 1 < memory.Length)
            {
                memory[addr] = (byte)(data & 0x00ff);
                memory[addr + 1] = (byte)(data >> 8);
                return true;
            }
            Trace.WriteLine("WriteHalfWord: Write out of bounds: address = " + addr + " value = " + data);
            return false;
        }

        //--------------------------------------------------------------
        // Purpose: Reads a byte of data at location <addr> in simulated ram
        // Returns: the byte of data read
        //--------------------------------------------------------------
        public byte ReadByte(uint addr)
        {
            if (addr < memory.Length) return memory[addr];
            return 0;
        }

        //--------------------------------------------------------------
        // Purpose: Writes a byte <data> into simulated ram at <addr>
        // Returns: true if the write succeeds, false if it fails
        //--------------------------------------------------------------
        public bool WriteByte(uint addr, byte data)
        {
            if (addr < memory.Length)
            {
                memory[addr] = data;
                return true;
            }
            Trace.WriteLine("WriteByte: Write out of bounds: address = " + addr + " value = " + data);
            return false;
        }

        //--------------------------------------------------------------
        // Purpose: Tests the bit at location <bit> in data stored at location <addr>
        // Returns: true if bit is 1, false if bit is 0
        //--------------------------------------------------------------
        public bool TestFlagInMemory(uint addr, byte bit)
        {
            uint bits = ReadWord(addr) & (1U << bit);

            return (bits & (1 << bit)) != 0;
        }

        public static bool TestFlagInData(uint data, byte bit)
        {
            uint bits = data & (1U << bit);

            return (bits & (1 << bit)) != 0;
        }

        //--------------------------------------------------------------
        // Purpose: Sets bit at location <bit> in data stored at location <addr>;
        //          If <flag> is true, bit is set to 1, else bit is set to 0
        // Returns: true is the bit setting succeeds, false if it fails
        //--------------------------------------------------------------
        public bool SetFlag(uint addr, byte bit, bool flag)
        {
            uint data = ReadWord(addr);

            if (flag)
            {
                data = data | (1U << bit);
            }
            else
            {
                data = data & (uint)~(1 << bit);
            }
            return WriteWord(addr, data);
        }

        //--------------------------------------------------------------
        // Purpose: Receives <word> of data and extracts bit values between 
        //          <startBit> and <endBit>
        // Details: Of the two, <startBit> is the lsb and <endBit> is the msb
        // Returns: an unsigned integer containing the bits within designated range
        //--------------------------------------------------------------
        public static uint ExtractBits(uint word, byte startBit, byte endBit)
        {
            int numbits = endBit - startBit + 1;
            uint mask = (1U << numbits) - 1;
            mask = mask << startBit;

            return word & mask;
        }
    }

    class testRAM
    {
        //--------------------------------------------------------------
        // Purpose: Runs unit tests for RAM class
        // Returns: nothing
        //--------------------------------------------------------------
        public static void RunTests()
        {
            uint wdata = 0x3B9AC9FF;
            Memory mem = new Memory(32768);

            Console.WriteLine("Testing WriteWord...");
            // WriteWord tests
            mem.WriteWord(300, wdata);
            Debug.Assert(mem.memory[300] == 0xFF, "Error: WriteWord");
            Debug.Assert(mem.memory[301] == 0xC9, "Error: WriteWord");
            Debug.Assert(mem.memory[302] == 0x9A, "Error: WriteWord");
            Debug.Assert(mem.memory[303] == 0x3B, "Error: WriteWord");
            Trace.WriteLine("WriteWord tests passed!");

            Console.WriteLine("Testing ReadWord...");
            // ReadWord tests
            uint readWordData = mem.ReadWord(300);
            Debug.Assert(readWordData == wdata, "Error: ReadWord");
            Trace.WriteLine("ReadWord tests passed!");

            Console.WriteLine("Testing WriteHalfWord...");
            // WriteHalfWord tests
            ushort hwdata = 0x1234;
            mem.WriteHalfWord(13470, hwdata);
            Debug.Assert(mem.memory[13470] == 0x34, "Error: WriteHalfWord");
            Debug.Assert(mem.memory[13471] == 0x12, "Error: WriteHalfWord");
            Trace.WriteLine("WriteHalfWord tests passed!");

            Console.WriteLine("Testing ReadHalfWord...");
            // ReadHalfWord tests
            ushort readHWdata = mem.ReadHalfWord(13470);
            Debug.Assert(readHWdata == hwdata, "Error: ReadHalfWord");
            Trace.WriteLine("ReadHalfWord tests passed!");

            Console.WriteLine("Testing WriteByte...");
            // WriteByte tests
            byte b = 0x4B;
            mem.WriteByte(1800, b);
            Debug.Assert(mem.memory[1800] == 0x4B, "Error: WriteByte");
            Trace.WriteLine("WriteByte tests passed!");

            Console.WriteLine("Testing ReadByte...");
            // ReadByte tests
            byte bdata = mem.ReadByte(1800);
            Debug.Assert(bdata == b, "Error: ReadByte");
            Trace.WriteLine("ReadByte tests passed!");

            Console.WriteLine("Testing TestFlagInMemory...");
            // TestFlagInMemory tests
            uint address = 100;
            mem.WriteWord(address, 0x84746B8E);
            bool flag = mem.TestFlagInMemory(address, 16);
            Debug.Assert(flag == false);
            flag = mem.TestFlagInMemory(address, 14);
            Debug.Assert(flag == true);
            flag = mem.TestFlagInMemory(address, 31);
            Debug.Assert(flag == true);
            flag = mem.TestFlagInMemory(address, 27);
            Debug.Assert(flag == false);
            Trace.WriteLine("TestFlagInMemory tests passed!");

            Console.WriteLine("Testing SetFlag...");
            // SetFlag tests
            uint addr = 52;
            byte bit = 11;
            flag = false;
            mem.WriteWord(addr, 0xFFFF);
            mem.SetFlag(addr, bit, flag);
            Debug.Assert(mem.ReadWord(addr) == 0xF7FF);

            flag = true;
            mem.SetFlag(addr, bit, flag);
            Debug.Assert(mem.ReadWord(addr) == 0xFFFF);

            bit = 4;
            flag = false;
            mem.SetFlag(addr, bit, flag);
            Debug.Assert(mem.ReadWord(addr) == 0xFFEF);

            flag = true;
            mem.SetFlag(addr, bit, flag);
            Debug.Assert(mem.ReadWord(addr) == 0xFFFF);

            bit = 27;
            flag = false;
            mem.WriteWord(addr, 0xFFFFFFFF);
            mem.SetFlag(addr, bit, flag);
            Debug.Assert(mem.ReadWord(addr) == 0xF7FFFFFF);

            flag = true;
            mem.SetFlag(addr, bit, flag);
            Debug.Assert(mem.ReadWord(addr) == 0xFFFFFFFF);
            Trace.WriteLine("SetFlag tests passed!");

            Console.WriteLine("Testing ExtractBits...");
            // ExtractBits tests
            uint word = 0xF5893624;
            byte startbit = 1;
            byte endbit = 7;
            uint result = Memory.ExtractBits(word, startbit, endbit);
            Debug.Assert(result == 0x24);

            word = 0x13;
            startbit = 2;
            endbit = 6;
            result = Memory.ExtractBits(word, startbit, endbit);
            Debug.Assert(result == 0x10);
            Trace.WriteLine("ExtractBits tests passed!");
        }
    }
}
