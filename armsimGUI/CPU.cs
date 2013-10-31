// Filename: CPU.cs
// Author: Taylor Eernisse
// Date: 9/18/12

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace armsimGUI
{
    /// <summary>
    /// Class: CPU
    /// Purpose: Handles the main CPU logic.
    /// Methods: Fetch()
    ///          Decode()
    ///          Execute()
    ///          TestCPU()
    /// </summary>
    class CPU
    {
        public Memory progRAM { get; private set; } // Memory object containing main program memory
        public Memory registers { get; private set; } // Memory object containing register memory
        public int stepNum { get; private set; } // contains current step counter value
        public uint pcCopy { get; set; } // copy of program counter for proper trace output
        public bool NFlag { get; private set; } // N flag
        public bool ZFlag { get; private set; } // Z flag
        public bool CFlag { get; private set; } // C flag
        public bool FFlag { get; private set; } // F flag
        public bool IFlag { get; private set; } // I flag

        //--------------------------------------------------------------
        // Purpose: CPU constructor
        //--------------------------------------------------------------
        public CPU(Memory _ram, Memory _registers)
        {
            progRAM = _ram;
            registers = _registers;
            stepNum = 0;
            NFlag = false;
            ZFlag = false;
            CFlag = false;
            FFlag = false;
            IFlag = false;
        }

        //--------------------------------------------------------------
        // Purpose: references data stored in memory at the current Program Counter address
        // Returns: the value in the address stored in the Program Counter
        //--------------------------------------------------------------
        public uint Fetch()
        {
            uint val = registers.ReadWord((uint)regs.PC);
            uint data = progRAM.ReadWord(val);

            // increment program counter
            registers.WriteWord((uint)regs.PC, val + 4);

            return data;
        }

        //--------------------------------------------------------------
        // Purpose: Processes <data>, creating an Instruction object with appropriate
        //          property values filled
        // Returns: An Instruction representing <data>
        //--------------------------------------------------------------
        public Instruction Decode(uint data)
        {
            InstructionFactory IF = new InstructionFactory();
            Instruction i = IF.Create(data);
            return i;
        }

        //--------------------------------------------------------------
        // Purpose: Executes <instr> 
        // Returns: nothing
        //--------------------------------------------------------------
        public void Execute(Instruction instr, bool generateDisasm)
        {
            instr.ExecuteInstruction(false);
                        
            stepNum += 1;
        }

        //--------------------------------------------------------------
        // Purpose: Performs an addition according to data in <instr>
        // Returns: nothing
        //--------------------------------------------------------------
        public static void TestCPU()
        {
            Trace.WriteLine("Testing CPU...");
            Memory ram = new Memory(32768);
            Memory reg = new Memory(64);
            reg.WriteWord((uint)regs.PC, 2300);
            ram.WriteWord((uint)2300, 0x12345678);
            CPU cpu = new CPU(ram, reg);
            uint data = cpu.Fetch();
            Debug.Assert(data == 0x12345678);
            Trace.WriteLine("Fetch() test passed!");

            // TODO: cpu.Execute();
            Debug.Assert(reg.ReadWord((uint)regs.PC) == 2304);
            Trace.WriteLine("Execute() test passed!");
        }

        public void SetNFlag(bool b)
        {
            NFlag = b;
        }

        public void SetZFlag(bool b)
        {
            ZFlag = b;
        }

        public void SetCFlag(bool b)
        {
            CFlag = b;
        }

        public void SetFFlag(bool b)
        {
            FFlag = b;
        }

        public void SetIFlag(bool b)
        {
            IFlag = b;
        }

        public void ClearFlags()
        {
            NFlag = false;
            ZFlag = false;
            CFlag = false;
            FFlag = false;
        }
    }
}
