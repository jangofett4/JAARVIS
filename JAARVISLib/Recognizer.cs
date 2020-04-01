using System;

using System.Speech.Recognition;
using System.Speech.Synthesis;

using RecogLib;

namespace JAARVISLib
{
    public class Module : RLModuleProgram
    {
        public Module(SpeechSynthesizer s) : base(s) { }

        public override void Recognize(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine("[BASE] Received command: {0}", e.Result);
            if (e.Result.Text.Contains("recognition"))
            {
                Synthesizer.Speak("Exiting Jarvis, thanks for using");
                Environment.Exit(0);
            }
        }
    }
}
