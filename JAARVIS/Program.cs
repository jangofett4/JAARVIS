using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using System.Speech.Synthesis;
using System.Speech.Recognition;

using RecogLib;

// using JAARVISLib;

namespace JAARVIS
{
    class Program
    {
        public const string ModulesDir = "modules";

        static void Main(string[] args)
        {
            using (var recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US")))
            {
                var synthesizer = new SpeechSynthesizer();
                var recognizers = new RLModuleCollection();

                if (!Directory.Exists(ModulesDir))
                    Directory.CreateDirectory(ModulesDir);
                string[] modules = Directory.GetDirectories(ModulesDir);

                // Add all modules
                foreach (var moddir in modules)
                {
                    string jsonfile = $"{ moddir }/module.json";
                    if (!File.Exists(jsonfile))
                    {
                        Console.WriteLine($"Unable to locate 'module.json' file for '{ moddir }' module");
                        continue;
                    }

                    var path = Path.GetFullPath(moddir);
                    var mod = RLModule.FromFile(jsonfile, path);

                    var asmpath = Path.Combine(path, mod.AssemblyFile);
                    var rulepath = Path.Combine(path, mod.RuleFile);

                    if (!File.Exists(asmpath))
                    {
                        Console.WriteLine($"Unable to locate '{ mod.AssemblyFile }' assembly for '{ mod.Name }'");
                        continue;
                    }
                    
                    if (!File.Exists(rulepath))
                    {
                        Console.WriteLine($"Unable to locate '{ mod.RuleFile }' rules file for '{ mod.Name }'");
                        continue;
                    }

                    mod.AssemblyFile = asmpath;
                    mod.RuleFile = rulepath;

                    if (!mod.LoadAssembly(synthesizer))
                    {
                        Console.WriteLine($"Unable to load assembly details for '{ mod.Name }', check '{ mod.AssemblyFile }' file");
                        continue;
                    }

                    mod.LoadGrammar();

                    recognizers.Add(mod);
                    Console.WriteLine($"[MOD] Loaded { mod.Name } ({ mod.Assembly.FullName })");
                }

                /* Compile & Load */
                var tmp = Environment.CurrentDirectory;
                foreach (var g in recognizers.Collection)
                {
                    Environment.CurrentDirectory = g.ModuleDirectory;
                    recognizer.LoadGrammar(g.Grammar);
                }
                Environment.CurrentDirectory = tmp;

                recognizer.SpeechRecognized += (s, e) =>
                {
                    var mod = recognizers.Find(e.Result.Grammar);
                    if (mod == null)
                    {
                        Console.WriteLine($"Something is wrong, no module found for grammar '{ e.Result.Grammar.Name }'");
                        return;
                    }

                    mod.RecognizerMethod.Invoke(mod.Instance, new object[] { s, e });
                };

                if (recognizers.Collection.Count == 0)
                {
                    Console.WriteLine("Aborting application, no grammar loaded.");
                    return;
                }

                synthesizer.SetOutputToDefaultAudioDevice();
                recognizer.SetInputToDefaultAudioDevice();

                recognizer.RecognizeAsync(RecognizeMode.Multiple);

                Console.WriteLine("Started");
                while (true)
                    Console.ReadKey(true);
            }
        }
    }
}
