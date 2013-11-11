// Filename: armsimGUI.cs
// Author: Taylor Eernisse
// Date: 9/18/12

using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using CommandLine;
using System.Diagnostics;
using System.Threading;

namespace armsimGUI
{
    /// <summary>
    /// Class: armsimGUI
    /// Purpose: Contains the logic to run the simulator as well as handle GUI events and updates
    /// Methods: armsimGUI(string[])
    ///          private void openSourceToolBtn_Click(object, EventArgs)
    ///          protected override bool ProcessCmdKey(ref Message, Keys)
    ///          private void runToolBtn_Click(object, EventArgs)
    ///          private void stepToolBtn_Click(object, EventArgs)
    ///          private void stopToolBtn_Click(object, EventArgs)
    ///          private void resetToolBtn_Click(object, EventArgs)
    ///          private void findBtn_Click(object, EventArgs)
    ///          private void traceBtn_Click(object, EventArgs)
    ///          private void StartCPU()
    ///          private void BreakExecution()
    ///          private void SingleStep()
    ///          private void ResetToLastExe()
    ///          private void OpenFileDialog()
    ///          private void LoadFile(string)
    ///          private void updateGUI()
    ///          private void updateProgramCounterTextBox()
    ///          private void updateChecksumTextBox()
    ///          private int getChecksum()
    ///          public void updateRegistersPanel()
    ///          public void updateMemoryPanel()
    ///          public void updateFlagsPanel()
    ///          public void updateDisassemblyPanel()
    ///          private void updateFileOpenLabel()
    /// </summary>
    public partial class armsimGUI : Form, Observer
    {
        delegate void SetTextCallback(char text);
        delegate void SetLabelTextCallback(string text, string dest);

        Computer comp; // reference to the main Computer instance
        StreamWriter tracefile = new StreamWriter(Directory.GetCurrentDirectory() + "\\trace.log", false); // trace file object to write to
        string filename; // filename of opened executable
        uint origPC; // holds original PC value for disassembly panel updating
        uint finalPC; // hold final PC value for diassembly panel updating
        private BackgroundWorker runThread;
        private BackgroundWorker stepThread;

        public armsimGUI(string[] args)
        {
            InitializeComponent();

            comp = new Computer(this);
            runThread = new BackgroundWorker();
            runThread.DoWork += new DoWorkEventHandler(runThread_DoWork);
            runThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(runThread_RunWorkerCompleted);

            stepThread = new BackgroundWorker();
            stepThread.DoWork += new DoWorkEventHandler(stepThread_DoWork);
            stepThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(stepThread_RunWorkerCompleted);

            updateRegistersPanel();

            var options = new Options();

            if (CommandLineParser.Default.ParseArguments(args, options)) // if command line arguments are valid...
            {
                if (options.Test)
                {
                    testRAM.RunTests();
                    Computer.TestLoader();
                    CPU.TestCPU();
                    Trace.WriteLine("All tests passed!");
                    Trace.WriteLine("-----------------------------------"); // insert line for file readability
                }
                else if (options.TestFlags)
                {
                    //comp.TestFlags();
                    //disassemblyListView.Items.Add(new ListViewItem(
                    //updateFlagsPanel();
                    //updateRegistersPanel();
                    comp.TestPutChar();
                }

                if (options.ELFInputFile != "")
                {
                    if (options.Exec)
                    {
                        LoadAndRunFile(options.ELFInputFile);
                    }
                    else
                    {
                        LoadFile(options.ELFInputFile);
                        runToolBtn.Enabled = true;
                        stepToolBtn.Enabled = true;
                        resetToolBtn.Enabled = true;
                    }
                }
            }
            // If any command line arguments are invalid, the CommandLine library displays the message as commented in the Options class.
            // Additional error handling can be implemented by including an "else" statement here.
        }


        #region Event Handlers

        //--------------------------------------------------------------
        // Purpose: Event handler to open file open clicking the "Open Source" button
        // Returns: nothing
        //--------------------------------------------------------------
        private void openSourceToolBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog();
            runToolBtn.Enabled = true;
            stepToolBtn.Enabled = true;
            resetToolBtn.Enabled = true;
        }

        //--------------------------------------------------------------
        // Purpose: Handles logic for hotkeys
        // Returns: true if key combination is found
        //--------------------------------------------------------------
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.O))
            {
                OpenFileDialog();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.F5))
            {
                StartCPU();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.F10))
            {
                SingleStep();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.B))
            {
                BreakExecution();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.R))
            {
                ResetToLastExe();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        //--------------------------------------------------------------
        // Purpose: Runs the CPU when "Run" button is clicked
        // Returns: nothing
        //--------------------------------------------------------------
        private void runToolBtn_Click(object sender, EventArgs e)
        {
            comp.stepBreakpoint = -1;
            StartCPU();
        }

        //--------------------------------------------------------------
        // Purpose: Executes a single step of the program when "Step" button is clicked
        // Returns: nothing
        //--------------------------------------------------------------
        private void stepToolBtn_Click(object sender, EventArgs e)
        {
            SingleStep();
        }

        //--------------------------------------------------------------
        // Purpose: Stops execution of the program when "Stop" button is clicked
        // Returns: nothing
        //--------------------------------------------------------------
        private void stopToolBtn_Click(object sender, EventArgs e)
        {
            BreakExecution();
        }

        //--------------------------------------------------------------
        // Purpose: Reloads last program into memory and resets all windows
        // Returns: nothing
        //--------------------------------------------------------------
        private void resetToolBtn_Click(object sender, EventArgs e)
        {
            ResetToLastExe();
        }

        //--------------------------------------------------------------
        // Purpose: Finds address in memoryListView when "Find" button is clicked
        // Returns: nothing
        //--------------------------------------------------------------
        private void findBtn_Click(object sender, EventArgs e)
        {
            string address = findAddressTxtBox.Text;
            int num;
            if (address != "" && address.Length != 10 && (address[0] != 0 && address[1] != 'x') && !int.TryParse(address.Remove(0, 2), out num))
            {
                findAddressTxtBox.Text = "Invalid address; must follow syntax \"0x00000000\"";
            }
            else
            {
                updateMemoryPanel(true);
            }
        }

        //--------------------------------------------------------------
        // Purpose: Enables or disables trace when "Enable/Disable Trace" button is clicked 
        //          based on the text currently displayed on the button
        // Returns: nothing
        //--------------------------------------------------------------
        private void traceBtn_Click(object sender, EventArgs e)
        {
            if (!comp.running)
            {
                if (traceBtn.Text == "Disable Trace") tracefile.Dispose();
                else tracefile = new StreamWriter(Directory.GetCurrentDirectory() + "\\trace.log", false);

                traceBtn.Text = (traceBtn.Text == "Disable Trace") ? "Enable Trace" : "Disable Trace";
            }
        }

        private void findAddressTxtBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (findAddressTxtBox.Text == "Invalid address; must follow syntax \"0x00000000\"")
            {
                findAddressTxtBox.Text = "";
            }
        }

        private void outputTxtBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            Computer.AddCharToBuffer(e.KeyChar);
        }

        private void stepThread_DoWork(object sender, DoWorkEventArgs e)
        {
            ArrayList list = (ArrayList)e.Argument;
            StreamWriter file = (StreamWriter)list[0];
            Observer obs = (Observer)list[1];
            comp.Step(file, obs);
        }

        private void stepThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            updateProgramCounterTextBox();
            updateDisassemblyPanel();
            updateMemoryPanel(false);
            updateRegistersPanel();
            updateFlagsPanel();
            if (comp.end) stepToolBtn.Enabled = false;
        }

        private void runThread_DoWork(object sender, DoWorkEventArgs e)
        {
            ArrayList list = (ArrayList)e.Argument;
            StreamWriter file = (StreamWriter)list[0];
            Observer obs = (Observer)list[1];
            comp.Run(file, obs);
        }

        private void runThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            updateGUI();
            if (comp.breakpointend)
            {
                runToolBtn.Enabled = true;
            }
            else
            {
                stopToolBtn.Enabled = false;
                stepToolBtn.Enabled = false;
            }
        }

        private void breakPtRunBtn_Click(object sender, EventArgs e)
        {
            int val;
            bool success = Int32.TryParse(stepNumTxtBox.Text, out val);
            if (success && val > 0)
            {
                comp.stepBreakpoint = val - 1;
                StartCPU();
            }
            else
            {
                stepNumTxtBox.Text = "Invalid";
            }
        }

        private void executeBtn_Click(object sender, EventArgs e)
        {
            string data = executeTxtBox.Text;
            if (data.Substring(0, 2) == "0x" && data.Length == 10)
            {
                try
                {
                    string str = data.Substring(2);
                    Instruction i = Computer.GetCPU().Decode(UInt32.Parse(str, System.Globalization.NumberStyles.AllowHexSpecifier));
                    Computer.GetCPU().Execute(i, false);
                }
                catch { }
            }
            else { executeTxtBox.Text = "Invalid"; }
        }

        #endregion


        #region Methods

        //--------------------------------------------------------------
        // Purpose: Runs the program currently loaded into memory
        // Returns: nothing
        //--------------------------------------------------------------
        private void StartCPU()
        {
            runToolBtn.Enabled = false;
            stopToolBtn.Enabled = true;
            ArrayList items = new ArrayList();
            items.Add(tracefile);
            items.Add(this);
            try { runThread.RunWorkerAsync(items); }
            catch { }
        }

        public void SetLabelText(string text, string dest)
        {
            switch (dest)
            {
                case "stepNumLbl":
                    if (this.stepNumLbl.InvokeRequired)
                    {
                        SetLabelTextCallback d = new SetLabelTextCallback(SetLabelText);
                        this.Invoke(d, new object[] { text, dest });
                    }
                    else
                    {
                        this.stepNumLbl.Text = "StepNum: " + text;
                    }
                    break;
                case "instrTextLbl":
                    if (this.instrTextLbl.InvokeRequired)
                    {
                        SetLabelTextCallback d = new SetLabelTextCallback(SetLabelText);
                        this.Invoke(d, new object[] { text, dest });
                    }
                    else
                    {
                        this.instrTextLbl.Text = "Instruction: " + text;
                    }
                    break;
                case "disassemblyLbl":
                    if (this.disassemblyLbl.InvokeRequired)
                    {
                        SetLabelTextCallback d = new SetLabelTextCallback(SetLabelText);
                        this.Invoke(d, new object[] { text, dest });
                    }
                    else
                    {
                        this.disassemblyLbl.Text = "Disassembly: " + text;
                    }
                    break;
                case "nextInstrLbl":
                    if (this.nextInstrLbl.InvokeRequired)
                    {
                        SetLabelTextCallback d = new SetLabelTextCallback(SetLabelText);
                        this.Invoke(d, new object[] { text, dest });
                    }
                    else
                    {
                        this.nextInstrLbl.Text = "Next instruction: " + text;
                    }
                    break;

                case "nextDisasmLbl":
                    if (this.nextDisasmLbl.InvokeRequired)
                    {
                        SetLabelTextCallback d = new SetLabelTextCallback(SetLabelText);
                        this.Invoke(d, new object[] { text, dest });
                    }
                    else
                    {
                        this.nextDisasmLbl.Text = "Disassembly: " + text;
                    }
                    break;
            }
        }

        public void SetText(char text)
        {
            if (this.outputTxtBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.outputTxtBox.Text += text;
            }
        }

        //--------------------------------------------------------------
        // Purpose: Stops execution of the current program
        // Returns: nothing
        //--------------------------------------------------------------
        private void BreakExecution()
        {
            comp.SetEnd(true);
            tracefile.Close();
            runToolBtn.Enabled = true;
        }

        //--------------------------------------------------------------
        // Purpose: Executes a single step of the program in memory
        // Returns: nothing
        //--------------------------------------------------------------
        private void SingleStep()
        {
            Computer.TestInstructions();
            ArrayList items = new ArrayList();
            items.Add(tracefile);
            items.Add(this);
            try { stepThread.RunWorkerAsync(items); }
            catch { }
        }

        //--------------------------------------------------------------
        // Purpose: Loads the last file loaded
        // Returns: nothing
        //--------------------------------------------------------------
        private void ResetToLastExe()
        {
            ResetRegisters();
            LoadFile(filename);
            ResetDisasmTesting();
            updateGUI();
            runToolBtn.Enabled = true;
            stepToolBtn.Enabled = true;
            stopToolBtn.Enabled = false;
            comp.ResetEnd();
            Computer.ResetInput();
            try
            {
                tracefile = new StreamWriter(Directory.GetCurrentDirectory() + "\\trace.log", false);
            }
            catch { tracefile.Dispose(); }
        }

        private void ResetDisasmTesting()
        {
            stepNumTxtBox.Text = "";
            stepNumLbl.Text = "StepNum:";
            instrTextLbl.Text = "Instruction:";
            disassemblyLbl.Text = "Disassembly:";
            nextInstrLbl.Text = "Next instruction:";
            nextDisasmLbl.Text = "Disassembly:";
        }

        //--------------------------------------------------------------
        // Purpose: Opens a file dialog at the current directory to open executable files
        // Returns: nothing
        //--------------------------------------------------------------
        private void OpenFileDialog()
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Executable files (*.exe)|*.exe";
            if (open.ShowDialog(this) == DialogResult.OK)
            {
                ResetRegisters();
                ResetDisasmTesting();
                LoadFile(open.FileName);
                stepToolBtn.Enabled = true;
            }
        }

        //--------------------------------------------------------------
        // Purpose: Loads the file <fileToLoad> and reads it into RAM
        // Returns: nothing
        //--------------------------------------------------------------
        private void LoadFile(string fileToLoad)
        {
            try
            {
                comp.ResetRam();
                comp.ResetEnd();
                Computer.GetCPU().ClearFlags();
                Memory ram = comp.progRAM;
                filename = fileToLoad;
                if (comp.ReadELF(fileToLoad))
                {
                    origPC = Computer.GetCPU().registers.ReadWord((uint)regs.PC);
                    findAddressTxtBox.Text = "0x" + Computer.GetCPU().registers.ReadWord((uint)regs.PC).ToString("x8");
                    initializeDisassemblyPanel();
                    initializeMemoryPanel();
                    updateGUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadAndRunFile(string file)
        {
            LoadFile(file);
            StartCPU();
            tracefile.Dispose();
            Environment.Exit(0);
        }

        private void FindItemInMemoryPanel(string address)
        {
            uint n = ParseAddress(address.Length == 8 ? address : address.Substring(2, address.Length - 2));
            FindItemInMemoryPanel(n);
        }

        private void FindItemInMemoryPanel(uint address)
        {
            string addr = "0x" + address.ToString("x8");

            // locate the address in the Memory panel
            ListViewItem i = memoryListView.FindItemWithText(addr, false, 0, false);
            if (i != null) memoryListView.TopItem = i;
        }

        private static uint ParseAddress(string address)
        {
            // if <address> is not evenly divisible by 16, subtract the extra amount to ensure address location will succeed in ListView search
            int n = Int32.Parse(address, System.Globalization.NumberStyles.AllowHexSpecifier);
            int j = n % 16;
            n -= j;
            return (uint)n;
        }

        //--------------------------------------------------------------
        // Purpose: Updates GUI components with current data
        // Returns: nothing
        //--------------------------------------------------------------
        private void updateGUI()
        {
            updateChecksumTextBox();
            updateProgramCounterTextBox();
            updateRegistersPanel();
            updateMemoryPanel(false);
            updateFlagsPanel();
            updateDisassemblyPanel();
            updateFileOpenLabel();
        }

        //--------------------------------------------------------------
        // Purpose: Updates Program Counter text to current value
        // Returns: nothing
        //--------------------------------------------------------------
        private void updateProgramCounterTextBox()
        {
            Memory registers = comp.registers;
            progCounterToolstripLbl.Text = "Program Counter: 0x" + registers.ReadWord((uint)regs.PC).ToString("x8");
        }

        //--------------------------------------------------------------
        // Purpose: Updates Checksum textbox to current value
        // Returns: nothing
        //--------------------------------------------------------------
        private void updateChecksumTextBox()
        {
            int checksum = getChecksum();
            checksumToolStripLbl.Text = "Checksum: 0x" + checksum.ToString("x8");
        }

        //--------------------------------------------------------------
        // Purpose: Gets the current checksum value
        // Returns: int value of checksum
        //--------------------------------------------------------------
        private int getChecksum()
        {
            int checksum = Computer.calculateChecksum(comp.progRAM.memory);
            return checksum;
        }

        private void updateStackPanel(bool findBtnClick)
        {
            uint stackpointer = ParseAddress(Computer.GetCPU().registers.ReadWord((uint)regs.SP).ToString("x8"));
            stackpointer -= 0x00000050;

            stackPanel.Items.Clear();
             
                for (int i = 0; i < 0x00000b0; i += 16, stackpointer += 16)
                {
               ListViewItem item = new ListViewItem("0x" + stackpointer.ToString("x8"));

                        for (uint j = 0; j < 16; j++)
                        {
                            byte b = comp.progRAM.ReadByte(stackpointer | j);
                            item.SubItems.Add(b.ToString("x2"));
                        }

                        stackPanel.Items.Add(item);
                    
                }

                //item.SubItems.Add("0x" + Computer.GetCPU().progRAM.ReadWord(stackpointer).ToString("x8"));
                //item.SubItems.Add("0x" + Computer.GetCPU().progRAM.ReadWord(stackpointer + 4).ToString("x8"));
                //item.SubItems.Add("0x" + Computer.GetCPU().progRAM.ReadWord(stackpointer + 8).ToString("x8"));
                //item.SubItems.Add("0x" + Computer.GetCPU().progRAM.ReadWord(stackpointer + 12).ToString("x8"));
                //stackPanel.Items.Add(item);
            
        }

        //--------------------------------------------------------------
        // Purpose: Clears and repopulates the registers panel with current register data
        // Returns: nothing
        //--------------------------------------------------------------
        private void updateRegistersPanel()
        {
            registerListView.Items.Clear();

            foreach (regs value in Enum.GetValues(typeof(regs)))
            {
                uint val = comp.registers.ReadWord((uint)value);
                ListViewItem item = new ListViewItem(value.ToString());
                item.SubItems.Add("0x" + val.ToString("x8"));
                registerListView.Items.Add(item);
            }
        }

        //--------------------------------------------------------------
        // Purpose: Updates the values of the rows within 50 lines before and 
        //          after the top item of list. This is performed any time the 
        //          "Find" button is clicked; thus, if the user scrolls to a position,
        //          the data will be false with the current implementation. This 
        //          is performed to reduce lag between steps. An ideal 
        //          solution would be to update only the rows currently in
        //          view, and connect the scroll event handlers to this method to
        //          update the newly-exposed rows, but I don't have time to
        //          implement this.
        // Returns: nothing
        //--------------------------------------------------------------
        private void updateMemoryPanel(bool findBtnClick)
        {
            string address = findBtnClick ? findAddressTxtBox.Text : "";
            if (findBtnClick) FindItemInMemoryPanel(address);
            ListViewItem backup = memoryListView.TopItem;
            int index = memoryListView.TopItem.Index > 50 ? memoryListView.TopItem.Index - 50 : memoryListView.TopItem.Index; // adjust index to start refreshing 50 slots before the top item
            uint uaddress = 0;
            int loopVal = index > 50 ? 100 : 100 - (50 - index); // to avoid referencing an index that doesn't exist
            try
            {
                if (!findBtnClick) address = backup.SubItems[0].Text.Substring(2);
                uaddress = Convert.ToUInt32(address, 16);
            }
            catch
            {
                if (!findBtnClick) address = comp.registers.ReadWord((uint)regs.PC).ToString("x8");
                uaddress = Convert.ToUInt32(address, 16);
            }

            uint addr = (uaddress > 0x320) ? uaddress - 0x320 : uaddress; // subtract 50 * 16 from address

            for (int i = 0; i < loopVal; i++, addr += 16)
            {
                if (addr < comp.progRAM.memory.Length)
                {
                    backup = memoryListView.Items[index];
                    ListViewItem item = new ListViewItem(backup.Text);
                    //item.SubItems.Add(""); // populate extra space column with nothing

                    //uint firstWord = comp.progRAM.ReadWord(addr);
                    //uint secondWord = comp.progRAM.ReadWord(addr + 4);
                    //uint thirdWord = comp.progRAM.ReadWord(addr + 8);
                    //uint fourthWord = comp.progRAM.ReadWord(addr + 12);
                    for (uint j = 0; j < 16; j++)
                    {
                        byte b = comp.progRAM.ReadByte(addr | j);
                        item.SubItems.Add(b.ToString("x2"));
                    }

                    //item.SubItems.Add("0x" + firstWord.ToString("x8"));
                    //item.SubItems.Add("0x" + secondWord.ToString("x8"));
                    //item.SubItems.Add("0x" + thirdWord.ToString("x8"));
                    //item.SubItems.Add("0x" + fourthWord.ToString("x8"));

                    memoryListView.Items[index++] = item;
                }
                else break;
            }

            updateStackPanel(findBtnClick);
        }

        //--------------------------------------------------------------
        // Purpose: Initializes the memory panel with data from memory
        // Returns: nothing
        //--------------------------------------------------------------
        private void initializeMemoryPanel()
        {
            for (uint addr = 0; addr < comp.progRAM.memory.Length; )
            {
                uint firstWord = comp.progRAM.ReadWord(addr);
                uint secondWord = comp.progRAM.ReadWord(addr + 4);
                uint thirdWord = comp.progRAM.ReadWord(addr + 8);
                uint fourthWord = comp.progRAM.ReadWord(addr + 12);

                ListViewItem item = new ListViewItem("0x" + addr.ToString("x8"));
                item.SubItems.Add("0x" + firstWord.ToString("x8"));
                item.SubItems.Add("0x" + secondWord.ToString("x8"));
                item.SubItems.Add("0x" + thirdWord.ToString("x8"));
                item.SubItems.Add("0x" + fourthWord.ToString("x8"));

                memoryListView.Items.Add(item);

                addr += 16;
            }

            FindItemInMemoryPanel(Computer.GetCPU().registers.ReadWord((uint)regs.PC));
        }

        //--------------------------------------------------------------
        // Purpose: Currently clears and updates flags panel with flag names and "false" values
        // Returns: nothing
        //--------------------------------------------------------------
        private void updateFlagsPanel()
        {
            flagsListView.Items.Clear();
            CPU c = Computer.GetCPU();

            ListViewItem NFlag = new ListViewItem("N");
            NFlag.SubItems.Add(c.NFlag.ToString());
            ListViewItem ZFlag = new ListViewItem("Z");
            ZFlag.SubItems.Add(c.ZFlag.ToString());
            ListViewItem CFlag = new ListViewItem("C");
            CFlag.SubItems.Add(c.CFlag.ToString());
            ListViewItem FFlag = new ListViewItem("F");
            FFlag.SubItems.Add(c.FFlag.ToString());
            ListViewItem IFlag = new ListViewItem("I");
            IFlag.SubItems.Add(c.IFlag.ToString());

            flagsListView.Items.AddRange(new ListViewItem[] { NFlag, ZFlag, CFlag, FFlag, IFlag });
        }

        //--------------------------------------------------------------
        // Purpose: Highlights the row corresponding with the current PC value
        // Returns: nothing
        //--------------------------------------------------------------
        private void updateDisassemblyPanel()
        {
            uint pc = Computer.GetCPU().registers.ReadWord((uint)regs.PC);
            if (pc > origPC && pc < finalPC - 4)
            {
                string strpc = "0x" + pc.ToString("x8");

                ListViewItem i = disassemblyListView.FindItemWithText(strpc);
                int index1 = i.Index;
                for (int j = 0; j < disassemblyListView.Items.Count; j++) disassemblyListView.Items[j].Selected = false;
                disassemblyListView.Items[index1].Selected = true;
            }
        }

        //--------------------------------------------------------------
        // Purpose: Clears and updates the disassembly panel with fake data to look good
        // Returns: nothing
        //--------------------------------------------------------------
        private void initializeDisassemblyPanel()
        {
            disassemblyListView.Items.Clear();
            disassemblyListView.FullRowSelect = true;
            // back up program counter
            uint data = 1;
            uint pc = origPC; // create PC temp outside loop for efficiency reasons
            bool end = false;
            Memory mem = Computer.GetCPU().progRAM;
            Computer.GetCPU().registers.WriteWord((uint)regs.PC, origPC); // ensure accurate PC value is stored in PC
            while (!end)
            {
                data = Computer.GetCPU().progRAM.ReadWord(pc);
                Instruction i = Computer.GetCPU().Decode(data);

                ListViewItem item = new ListViewItem(i.disasm.address);
                item.SubItems.Add(i.disasm.instrCode);
                item.SubItems.Add(i.disasm.instruction);
                item.SubItems.Add(i.disasm.value);
                disassemblyListView.Items.Add(item);

                pc += 4;
                Computer.GetCPU().registers.WriteWord((uint)regs.PC, pc); // update PC for disassembly generation
                if (data == 0)
                {
                    end = true;
                    // erase the last row added to the ListView
                    disassemblyListView.Items.RemoveAt(disassemblyListView.Items.Count - 1);
                }
            }

            finalPC = pc; // store final PC value
            Computer.GetCPU().registers.WriteWord((uint)regs.PC, origPC); // restore value of program counter
        }

        //--------------------------------------------------------------
        // Purpose: Sets the "Executable: " label at the bottom of the GUI 
        //          to reflect the current filename
        // Returns: nothing
        //--------------------------------------------------------------
        private void updateFileOpenLabel()
        {
            string[] parts = filename.Split('\\');
            filenameLbl.Text = "Executable: " + parts[parts.Length - 1];
        }

        private void ResetRegisters()
        {
            for (uint i = 0; i < 64; i += 4)
            {
                Computer.GetCPU().registers.WriteWord(i, 0);
            }
        }

        #endregion

        private void disassemblyListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < disassemblyListView.Items.Count; i++)
            {
                if (disassemblyListView.Items[i].Selected)
                {
                    comp.SetBreakpoint(disassemblyListView.Items[i].Text);
                }
            }
        }
    }
}
