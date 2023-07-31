using System.Text;
using SharpDenizenTools.MetaHandlers;
using SharpDenizenTools.ScriptAnalysis;

namespace DenizenLinter
{
    /// <summary>General program entry and handler.</summary>
    public class Program
    {
        private static int _errors = 0;
        
        /// <summary>SW Entry point.</summary>
        static int Main(string[] args)
        {
            MetaDocs.CurrentMeta = MetaDocsLoader.DownloadAll();

            string rootPath = args.Length == 0 ? "." : args[0];
            ProcessFiles(rootPath);    

            if (_errors >= 0)
            {
                Console.WriteLine($"\n\nLinter error: {_errors} errors found.");
                return 1;
            }

            return 0;
        }

        static void ProcessFiles(string path)
        {
            string[] legacyFiles = Directory.GetFiles(path, "*.yml");
            foreach (string file in legacyFiles)
            {
                CheckFile(file, true);
            }

            string[] files = Directory.GetFiles(path, "*.dsc");
            foreach (string file in files)
            {
                CheckFile(file);
            }
            
            string[] directories = Directory.GetDirectories(path);
            foreach (string dir in directories)
            {
                ProcessFiles(dir);
            }
        }

        static void CheckFile(string file, bool legacy=false)
        {
            if (legacy)
                Console.WriteLine($"\n\nValidating file with LEGACY file extension (.yml) {file}:\n");
            else
                Console.WriteLine($"\n\nValidating file {file}:\n");
            
            ScriptChecker checker = new(File.ReadAllText(file));
            checker.Run();

            _errors += checker.Errors.Count;

            int totalWarns = checker.Errors.Count + checker.Warnings.Count + checker.MinorWarnings.Count;
            Console.WriteLine($"Script Check Results with {totalWarns} Warnings:");

            void EmbedList(List<ScriptChecker.ScriptWarning> list, string title)
            {
                if (list.Count == 0)
                {
                    return;
                }
                StringBuilder thisListResult = new(list.Count * 200);
                foreach (var message in list.Select(entry => $"On line {entry.Line}: {entry.CustomMessageForm}\n"))
                {
                    thisListResult.Append(message);
                }
                if (thisListResult.Length > 0)
                {
                    Console.WriteLine($"{title}:\n{thisListResult}");
                }
            }
            EmbedList(checker.Errors, "Encountered Critical Errors");
            EmbedList(checker.Warnings, "Script Warnings");
            EmbedList(checker.MinorWarnings, "Minor Warnings");
            foreach (string debug in checker.Debugs)
            {
                Console.WriteLine($"Script checker debug: {debug}");
            }

            Console.WriteLine($"Summary:");
            foreach (ScriptChecker.ScriptWarning info in checker.Infos)
            {
                Console.WriteLine(info.CustomMessageForm);
            }
        }
    }

}