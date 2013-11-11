// Filename: Options.cs
// Author: Taylor Eernisse
// Date: 9/18/12

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace armsimGUI
{
    /// <summary>
    /// Class: Options
    /// Purpose: Provides values for the CommandLine library to perform proper command line parsing.
    ///          Please refer to http://commandline.codeplex.com for documentation.
    /// </summary>
    class Options
    {
        [Option("l", "load", DefaultValue = "", HelpText = "ELF file to read")]
        public string ELFInputFile { get; set; }

        [Option("m", "mem", DefaultValue = 32768U, HelpText = "The amount of memory to intialize")]
        public uint MaxMemory { get; set; }

        [Option("t", "test", DefaultValue = false, HelpText = "Run unit tests")]
        public bool Test { get; set; }

        [Option("e", "exec", DefaultValue = false, HelpText = "Execute the file specified by [--load]")]
        public bool Exec { get; set; }

        [Option("f", "testflags", DefaultValue = false, HelpText = "Run flag tests")]
        public bool TestFlags { get; set; }

        [HelpOption(HelpText = "Display this help screen.")]
        public string GetUsage() // help message to be displayed upon receipt of invalid command line arguments
        {
            var help = new StringBuilder();
            help.AppendLine("Usage: -l|oad parameter MUST be supplied.");
            help.AppendLine("       -m|em parameter must be an integer value less than 32769.");
            help.AppendLine("        If not supplied, default is 32768.");
            help.AppendLine("       -t|est 'true' to run tests; 'false' to skip");
            return help.ToString();
        }
    }
}
