using CrewToolsCommon.Utilities;
using System.CommandLine;

namespace TC1MissionMaker;

internal class Program
{
    public static void Main(string[] args)
    {
        Logger.Clean();

        Option<bool> debugOption = new Option<bool>("--debug") { Description = "Keeps mission files extracted inside a DEBUG folder." };
        Argument<FileInfo> input = new Argument<FileInfo>("mission folder") { Description = "The folder containing all mission xml's and images." }.AcceptExistingOnly();
        Argument<DirectoryInfo> extraFolder = new Argument<DirectoryInfo>("additional files folder") { Description = "An optional folder to add existing files to the mod.", Arity = ArgumentArity.ZeroOrOne }.AcceptExistingOnly();

        RootCommand command = new RootCommand("TC1MissionMaker") { Description = "A semi-automatic mission creator for The Crew." };
        command.Add(input);
        command.Add(debugOption);
        command.Add(extraFolder);

        if (args.Length == 0)
        {
            Logger.Error("Drag and drop a folder with missions onto this program or use it as an argument in the terminal.");
            command.Parse("-h").Invoke();
            Logger.WriteAndFlush();
            Prompt();
            return;
        }

        //To read the assets folder properly.
        Directory.SetCurrentDirectory(Path.GetDirectoryName(Environment.ProcessPath));

        command.SetAction(parseResult =>
        {
            FileInfo xml = parseResult.GetValue(input);
            DirectoryInfo addedFiles = parseResult.GetValue(extraFolder);
            bool debug = parseResult.GetValue(debugOption);

            if (Path.GetFileName(xml.FullName).Any(char.IsWhiteSpace))
            {
                Logger.Error("Whitespace found in folder name, please remove all whitespace characters.", true);
                Prompt();
                return;
            }

            string[] files = Directory.GetFiles(xml.FullName, "*.xml", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                Logger.Error($"No XML files found in {args[0]}.", true);
                Prompt();
                return;
            }

            Mod mod = new Mod(files, xml.FullName, addedFiles, debug);

            try
            {
                mod.Create();
                mod.Package();
                Logger.WriteAndFlush();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    ex = ex.InnerException;

                Logger.Info("Something went wrong! Printing stacktrace, check log.txt for stacktrace.");
                Logger.Error(ex.Message);
                Logger.Error(ex.StackTrace, true);
                Prompt();
            }
        });

        ParseResult result = command.Parse(args);
        result.Invoke();
    }

    private static void Prompt()
    {
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}