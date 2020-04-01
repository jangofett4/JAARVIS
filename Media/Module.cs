using System;
using System.Runtime.InteropServices;
using System.Speech.Recognition;
using System.Speech.Synthesis;

using RecogLib;

namespace Media
{
    public class Module : RLModuleProgram
    {
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);

        public const int KEYEVENTF_EXTENTEDKEY = 1;
        public const int KEYEVENTF_KEYUP = 0;
        public const int VK_MEDIA_NEXT_TRACK = 0xB0;// code to jump to next track
        public const int VK_MEDIA_PLAY_PAUSE = 0xB3;// code to play or pause a song
        public const int VK_MEDIA_PREV_TRACK = 0xB1;// code to jump to prev track

        public Module(SpeechSynthesizer s) : base(s) { }

        public override void Recognize(object sender, SpeechRecognizedEventArgs e)
        {
            string result = e.Result.Text;
            if (result.Contains("next"))
            {
                keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
            } 
            else if (result.Contains("previous"))
            {
                keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
            } 
            else if (result.Contains("start"))
            {
                Console.WriteLine("[Media] Received command start/stop", result);
                keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
            }
            else if (result.Contains("stop"))
            {
                Console.WriteLine("[Media] Received command start/stop", result);
                keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
            }
        }
    }
}
