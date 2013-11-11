// Filename: Computer.cs
// Author: Taylor Eernisse
// Date: 9/18/12

using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace armsimGUI
{
    /// <summary>
    /// Class: Computer
    /// Purpose: Combines CPU, program memory, and registers into coherent whole and provides
    ///          methods to operate on them
    /// Methods: Computer()
    ///          ResetRam()
    ///          Run(StreamWriter)
    ///          Step(StreamWriter)
    ///          WriteToTraceFile(StreamWriter)
    ///          ReadElf(string, ref Memory)
    ///          WriteElfFileToMemory(FileStream, ELF, byte[] ref Memory)
    ///          ByteArrayToStructure<T>(byte[]) where T : struct
    ///          calculateChecksum(byte[])
    ///          TestLoader()
    /// </summary>
    class Computer
    {
        private static CPU cpu; // CPU object containing main CPU
        public static Observer obs { get; private set; }
        public static CharBuffer charBuffer { get; private set; } // buffer for characters for input
        public static bool crTyped { get; private set; } // set if a carriage return has been typed
        public Memory progRAM { get; private set; } // Memory object containing main program memory
        public Memory registers { get; private set; } // Memory object containing register memory
        public uint PC_value { get { return registers.ReadWord((uint)regs.PC); } } // contains Program Counter value for easy referencing
        public bool end { get; private set; } // tracks if the end of the program has been reached or not
        public string breakpoint { get; private set; } // string address to break at
        public int stepBreakpoint { get; set; } // integer stepnum to break at
        public bool breakpointend { get; private set; } // set if a breakpoint has been reached
        public bool running { get; private set; }
        private static readonly object locker = new object(); // lock for thread safety
        ObserverTest obsTest;

        //--------------------------------------------------------------
        // Purpose: Computer constructor
        //--------------------------------------------------------------
        public Computer()
        {
            progRAM = new Memory(32768);
            registers = new Memory(72);
            cpu = new CPU(progRAM, registers);
            end = false;
            charBuffer = new CharBuffer();
        }

        //--------------------------------------------------------------
        // Purpose: Computer constructor
        //--------------------------------------------------------------
        public Computer(Observer _obs)
        {
            progRAM = new Memory(32768);
            registers = new Memory(72);
            cpu = new CPU(progRAM, registers);
            end = false;
            charBuffer = new CharBuffer();
            obs = _obs;
            obsTest = new ObserverTest();
            obsTest.Attach(obs);
        }

        public static CPU GetCPU() { return cpu; }

        public static Observer GetObserver() { return obs; }

        //--------------------------------------------------------------
        // Purpose: Resets <progRAM> to a new instance of Memory with the default size of 32768
        // Returns: nothing
        //--------------------------------------------------------------
        public void ResetRam()
        {
            progRAM = new Memory(32768);
            cpu = new CPU(progRAM, registers);
        }

        public void ResetEnd()
        {
            end = false;
        }

        public static void AddCharToBuffer(char c)
        {
            charBuffer[charBuffer.Size] = c;
            if (c == '\r')
            {
                crTyped = true;
                charBuffer[charBuffer.Size + 1] = '\0'; // null terminate the string
            }
        }

        //--------------------------------------------------------------
        // Purpose: Executes the program with a loop that runs while Fetch() does not return a 0
        // Returns: nothing
        //--------------------------------------------------------------
        public void Run(StreamWriter file, Observer obs)
        {
        //    ArrayList list = (ArrayList)obj;
        //    StreamWriter file = (StreamWriter)list[0];
        //    Observer obs = (Observer)list[1];
            Instruction instr;
            running = true;

            while (!end)
            {
                cpu.pcCopy = cpu.registers.ReadWord((uint)regs.PC);
                uint data = cpu.Fetch();
                instr = cpu.Decode(data); 
                if (instr.disasm.address == breakpoint || cpu.stepNum == stepBreakpoint)
                {
                    SetLblTexts(instr);
                    SetEnd(true);
                    breakpointend = true;
                    running = false;
                }
                cpu.Execute(instr, false);
                WriteToTraceFile(file);
                if (instr.haltExecution) SetEnd(true);
                if (cpu.stepNum % 100 == 0) SetLblTexts(instr);
            }
            if (breakpointend) SetEnd(false);
            running = false;
        }

        private void SetLblTexts(Instruction instr)
        {
            obsTest.ChangeText("" + cpu.stepNum, "stepNumLbl");
            obsTest.ChangeText(instr.disasm.instrCode, "instrTextLbl");
            obsTest.ChangeText(instr.disasm.instruction + " " + instr.disasm.value, "disassemblyLbl");

            // fetch next instruction
            uint data = cpu.Fetch();
            Instruction i = cpu.Decode(data);
            uint val = registers.ReadWord((uint)regs.PC);
            registers.WriteWord((uint)regs.PC, val - 4); // reset PC
            obsTest.ChangeText(i.disasm.instrCode, "nextInstrLbl");
            obsTest.ChangeText(i.disasm.instruction + " " + i.disasm.value, "nextDisasmLbl");
        }

        public void SetEnd(bool b)
        {
            lock (locker) end = b;
        }

        //--------------------------------------------------------------
        // Purpose: Executes a single fetch-decode-execute cycle
        // Returns: nothing
        //--------------------------------------------------------------
        public void Step(StreamWriter file, Observer obs)
        {
            if (!end)
            {
                uint data = cpu.Fetch();
                if (data == 0) SetEnd(true);
                Instruction i = cpu.Decode(data);
                cpu.Execute(i, false);
                WriteToTraceFile(file); // write registers and other data to trace file
                if (i.haltExecution) SetEnd(true);
                SetLblTexts(i);
            }
        }

        //--------------------------------------------------------------
        // Purpose: Writes register values and other data to trace file <file>
        // Returns: nothing
        //--------------------------------------------------------------
        private void WriteToTraceFile(StreamWriter file)
        {
            if (file.BaseStream == null) { }
            else
            {
                CPU cpu = Computer.GetCPU();
                string stepNum = cpu.stepNum.ToString("000000");
                string progCounter = cpu.pcCopy.ToString("X8");
                string checksum = calculateChecksum(progRAM.memory).ToString("X8");
                char n = cpu.NFlag ? '1' : '0';
                char z = cpu.ZFlag ? '1' : '0';
                char c = cpu.CFlag ? '1' : '0';
                char f = cpu.FFlag ? '1' : '0';
                string flags = "" + n + z + c + f;
                string r0 = "0=" + registers.ReadWord((uint)regs.r0).ToString("X8");
                string r1 = "1=" + registers.ReadWord((uint)regs.r1).ToString("X8");
                string r2 = "2=" + registers.ReadWord((uint)regs.r2).ToString("X8");
                string r3 = "3=" + registers.ReadWord((uint)regs.r3).ToString("X8");
                string r4 = "4=" + registers.ReadWord((uint)regs.r4).ToString("X8");
                string r5 = "5=" + registers.ReadWord((uint)regs.r5).ToString("X8");
                string r6 = "6=" + registers.ReadWord((uint)regs.r6).ToString("X8");
                string r7 = "7=" + registers.ReadWord((uint)regs.r7).ToString("X8");
                string r8 = "8=" + registers.ReadWord((uint)regs.r8).ToString("X8");
                string r9 = "9=" + registers.ReadWord((uint)regs.r9).ToString("X8");
                string r10 = "10=" + registers.ReadWord((uint)regs.r10).ToString("X8");
                string r11 = "11=" + registers.ReadWord((uint)regs.r11).ToString("X8");
                string r12 = "12=" + registers.ReadWord((uint)regs.FP).ToString("X8");
                string r13 = "13=" + registers.ReadWord((uint)regs.SP).ToString("X8");
                string r14 = "14=" + registers.ReadWord((uint)regs.LR).ToString("X8");

                file.WriteLine("{0} {1} {2} {3}  {4,10}  {5,10}  {6,10}  {7,10}", stepNum, progCounter, checksum, flags, r0, r1, r2, r3);
                file.WriteLine("{0,18}  {1,10}  {2,10}  {3,10}  {4,10}  {5,10}", r4, r5, r6, r7, r8, r9);
                file.WriteLine("{0,18} {1,10} {2,10} {3,10} {4,10}", r10, r11, r12, r13, r14);
            }
        }

        //--------------------------------------------------------------
        // Purpose: Reads ELF files into simulated ram after validating their ELFness
        // Returns: bool value true if loading completed successfully
        //--------------------------------------------------------------
        public bool ReadELF(string ELFpath)
        {
            bool success = false;

            try
            {
                Trace.WriteLine("Loader: Opening file " + ELFpath + "...");

                using (FileStream strm = new FileStream(ELFpath, FileMode.Open))
                {
                    ELF elfHeader = new ELF();
                    byte[] data = new byte[Marshal.SizeOf(elfHeader)];

                    // read ELF header data
                    strm.Read(data, 0, data.Length);
                    // convert to struct
                    elfHeader = ByteArrayToStructure<ELF>(data);

                    Trace.WriteLine("Loader: Checking if valid ELF file...");

                    // check to ensure file is an ELF file
                    if (!(data[0] == '\x7f' &&
                          data[1] == 'E' &&
                          data[2] == 'L' &&
                          data[3] == 'F'))
                    {
                        throw new InvalidDataException();
                    }

                    Trace.WriteLine("Loader: File check complete!");
                    Trace.WriteLine("Loader: Number of segments: " + elfHeader.e_phnum);
                    Trace.WriteLine(""); // blank line for file readability

                    registers.WriteWord((uint)regs.PC, elfHeader.e_entry);
                    registers.WriteWord((uint)regs.SP, 0x00007000);

                    // Read first program header entry
                    for (int i = 0; i < elfHeader.e_phnum; i++)
                    {
                        WriteElfFileToMemory(strm, elfHeader, data);
                    }

                    success = true;
                }
            }
            catch (InvalidDataException)
            {
                Console.WriteLine("Loader: The file provided is not an ELF file. Please try again.");
                Trace.WriteLine("Loader: The file provided is not an ELF file. Please try again.");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Loader: Could not locate file " + ELFpath + ". Please try again.");
                Trace.WriteLine("Loader: Could not locate file " + ELFpath + ". Please try again.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loader: Unknown error. " + ex.ToString());
                Trace.WriteLine("Loader: Unknown error. " + ex.ToString());
            }

            return success;
        }

        private void WriteElfFileToMemory(FileStream strm, ELF elfHeader, byte[] data)
        {
            Trace.WriteLine("Loader: Reading ELF File...");
            strm.Seek(elfHeader.e_phoff, SeekOrigin.Begin); // seek to the program header offset
            data = new byte[elfHeader.e_phentsize]; // allocate byte array the size of the program header
            strm.Read(data, 0, (int)elfHeader.e_phentsize); // <data> contains program header

            int p_offset = (int)BitConverter.ToUInt16(data, 4); // read start of segment
            int p_filesz = (int)BitConverter.ToUInt16(data, 16); // read file size
            uint p_vaddr = BitConverter.ToUInt16(data, 8); // read storage address

            Trace.WriteLine("Loader: Reading program header...");
            strm.Seek(p_offset, SeekOrigin.Begin); // seek to beginning of program segment
            byte[] programData = new byte[p_filesz]; // create holder for program data

            Trace.WriteLine("Loader: Program header offset: " + p_offset);
            Trace.WriteLine("Loader: Program file size: " + p_filesz);

            Trace.WriteLine("Loader: Writing program to RAM...");
            strm.Read(programData, 0, p_filesz); // read program into <programData>

            for (uint j = 0; j < programData.Length; j++)
            {
                Computer.GetCPU().progRAM.WriteByte(p_vaddr + j, programData[j]);
            }

            elfHeader.e_phoff += elfHeader.e_phentsize; // move to next program header

            Trace.WriteLine("Loader: Program written to RAM successfully!");
            Trace.WriteLine(""); // blank line for file readability
        }

        // Converts a byte array to a struct
        static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                typeof(T));
            handle.Free();
            return stuff;
        }

        //--------------------------------------------------------------
        // Purpose: Calculates the checksum of data contained in <memory>
        // Returns: integer value of checksum
        //--------------------------------------------------------------
        public static int calculateChecksum(byte[] memory)
        {
            Trace.WriteLine("Loader: Calculating checksum...");
            int chksum = 0;
            for (int i = 0; i < memory.Length; i++)
            {
                chksum += memory[i] ^ i;
            } 

            Trace.WriteLine("Loader: Checksum: " + chksum);
            Trace.WriteLine("-----------------------------------"); // insert line for file readability

            return chksum;
        }

        //--------------------------------------------------------------
        // Purpose: Runs loader tests
        // Returns: nothing
        //--------------------------------------------------------------
        public static void TestLoader()
        {
            Computer comp = new Computer();
            Console.WriteLine("Testing loader...");
            // Loader tests
            string elfpath = "test3.exe";
            comp.ReadELF(elfpath);
            int checksum = calculateChecksum(Computer.GetCPU().progRAM.memory);
            Debug.Assert(checksum == 536860694);

            comp.ResetRam();
            elfpath = "test2.exe";
            comp.ReadELF(elfpath);
            checksum = calculateChecksum(Computer.GetCPU().progRAM.memory);
            Debug.Assert(checksum == 536864418);

            comp.ResetRam();
            elfpath = "test1.exe";
            comp.ReadELF(elfpath);
            checksum = calculateChecksum(Computer.GetCPU().progRAM.memory);
            Debug.Assert(checksum == 536861081);
            Trace.WriteLine("Loader tests passed!");
        }

        public void TestFlags()
        {
            this.registers.WriteWord((uint)regs.r1, 2);
            Instruction d = cpu.Decode(0xe0810004);
            cpu.Execute(d, false);
        }

        public void TestPutChar()
        {
            this.registers.WriteWord((uint)regs.r0, 0x65); // display 'e'
            Instruction i = cpu.Decode(0xef000000); // swi #0x00
            cpu.Execute(i, false);
        }

        internal void SetBreakpoint(string p)
        {
            breakpoint = p;
        }

        internal static void ResetInput()
        {
            charBuffer.Reset();
            crTyped = false;
        }

        public static void TestInstructions()
        {
            CPU cpu = new CPU(new Memory(32768), new Memory(72));
            Instruction i = cpu.Decode(0xe52db004);
            cpu.Execute(i, false);
        }
    }
}
