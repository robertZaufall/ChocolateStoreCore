using CommandLine;

namespace ChocolateStoreCore
{
    internal class ArgsOptions
    {
        [Option('w', "whatif", Required = false, HelpText = "WhatIf - only log results. dont't delete or download other than nupkg files.")]
        public bool WhatIf { get; set; }

        [Option('p', "purge", Required = false, HelpText = "Only execute purge operation.")]
        public bool Purge { get; set; }

        [Option('v', "purgevscode", Required = false, HelpText = "Only execute purge operation on VSCode extension folders.")]
        public bool PurgeVsCode { get; set; }

        [Option('d', "path", Required = false, HelpText = "Only execute purge operation on VSCode extension folders.")]
        public string Path { get; set; }
    }
}
