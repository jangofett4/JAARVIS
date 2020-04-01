using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using System.Speech.Recognition;
using System.Speech.Synthesis;

using System.Reflection;

using Newtonsoft.Json;

namespace RecogLib
{
    public abstract class RLModuleProgram
    {
        public SpeechSynthesizer Synthesizer { get; set; }

        public RLModuleProgram(SpeechSynthesizer synth)
        {
            Synthesizer = synth;
        }

        public abstract void Recognize(object sender, SpeechRecognizedEventArgs e);
    }

    [Serializable]
    public class RLModule
    {
        [JsonIgnore]
        public string ModuleDirectory { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Rules")]
        public string RuleFile { get; set; }

        [JsonProperty("Assembly")]
        public string AssemblyFile { get; set; }

        [JsonIgnore]
        public Assembly Assembly { get; set; }
        [JsonIgnore]
        public Grammar Grammar { get; set; }
        [JsonIgnore]
        public object Instance { get; set; }
        [JsonIgnore]
        public MethodInfo RecognizerMethod { get; set; }

        public static RLModule FromFile(string file, string dir)
        {
            string json = File.ReadAllText(file, Encoding.UTF8);
            var obj = JsonConvert.DeserializeObject<RLModule>(json);
            obj.ModuleDirectory = dir;
            return obj;
        }

        public bool LoadAssembly(SpeechSynthesizer synth)
        {
            Assembly = Assembly.LoadFrom(AssemblyFile);
            try
            {
                var types = Assembly.GetTypes();
                Type modtype = null;
                foreach (var t in types)
                    if (t.Name == "Module")
                    {
                        modtype = t;
                        break;
                    }
                if (modtype == null)
                    return false;
                
                var method = modtype.GetMethod("Recognize");
                if (method == null)
                    return false;
                
                Instance = Activator.CreateInstance(modtype, synth);
                if (Instance == null)
                    return false;

                RecognizerMethod = method;
                return true;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void LoadGrammar()
        {
            Grammar = new Grammar(RuleFile)
            {
                Name = Name
            };
        }
    }

    public class RLModuleCollection
    {
        public List<RLModule> Collection { get; set; }
        public string Culture { get; set; }

        public RLModule this[int index]
        {
            get => Collection[index];
            set => Collection[index] = value;
        }

        public RLModuleCollection(string culture = "en-US")
        {
            Collection = new List<RLModule>();
            Culture = culture;
        }

        public void Add(RLModule module)
        {
            Collection.Add(module);
        }

        public void Remove(RLModule module)
        {
            Collection.Remove(module);
        }

        public RLModule Find(Grammar grammar)
        {
            foreach (var mod in Collection)
                if (mod.Grammar == grammar)
                    return mod;
            return null;
        }
    }
}
