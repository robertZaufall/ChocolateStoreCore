using CommandLine;

namespace ChocolateStoreCore
{
    internal class ArgsOptions
    {
        [Option('w', "whatif", Required = false, HelpText = "WhatIf - only log results. dont't delete or download other than nupkg files.")]
        public bool WhatIf { get; set; }

        [Option('p', "purge", Required = false, HelpText = "Only execute purge operation.")]
        public bool Purge { get; set; }
    }
}
