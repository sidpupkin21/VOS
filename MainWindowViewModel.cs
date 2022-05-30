using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WindowsInput;
//using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using VOS.Annotations;
using System.Globalization;
namespace VOS
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private double _percentage;
        private bool _isRunning;
        private bool _continue;
        private string _message = " ";
        public MainWindowViewModel()
        {
            IsRunning = false;
            Percentage = 0.5;
            SetMessage(""); //How may i be of service
            Init();
        }

        private readonly object locko = new object();

        public void SetMessage(string message)
        {

            new Thread(() =>
            {
                for (int i = 0; i < message.Count(); i++)
                {
                    try
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Message += message[i];
                        });
                    }
                    catch
                    { return; }
                    Thread.Sleep(50);

                }
                Thread.Sleep(2000);
                while (Message.Length > 0)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Message = Message.Substring(0, Message.Length - 1);
                    });
                    Thread.Sleep(50);
                }
            }).Start();
        }

        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                if (value.Equals(_isRunning)) return;
                Console.Beep();
                if (IsRunning)
                {
                    Console.Beep();
                }
                _isRunning = value;
                Application.Current.Dispatcher.Invoke(() => NotifyPropertyChanged("IsRunning"));
            }
        }
        public bool continues
        {
            get { return _continue; }
            set
            {
                if (value.Equals(_continue)) return;
                if (IsRunning)
                {
                    Console.Beep();
                }
                _continue = value;
                Application.Current.Dispatcher.Invoke(() => NotifyPropertyChanged("continues"));
            }
        }
        public double Percentage
        {
            get { return _percentage; }
            set
            {
                if (value.Equals(_percentage)) return;
                _percentage = value;
                NotifyPropertyChanged();
            }
        }
        public static class getSet
        {
            #region wordBefore
            private static string _WordBefore;
            public static string wordBefore
            {
                get { return _WordBefore; }
                set { _WordBefore = value; }
            }
            #endregion
        }
        public string Message
        {
            get { return _message; }
            set
            {
                if (value == _message) return;
                _message = value;
                NotifyPropertyChanged();
            }
        }
        #region VOS
        //string emailsubject = ""; string emailbody = "";string emailtowho = "";bool emailstart = false;bool emailsub = false;bool emailbod = false;bool emailtow = false;
        readonly SpeechSynthesizer talk = new SpeechSynthesizer();
        readonly PromptBuilder what = new PromptBuilder();
        readonly SpeechRecognitionEngine myvoice = new SpeechRecognitionEngine();
        private readonly Process pr = new Process();
        private void Init()
        {
            what.ClearContent();
            //read from csv
            //string[] path = System.IO.File.ReadAllLines(@"C:\Users\sidam\source\repos\VOS\readexample.txt");
            //what.AppendText("Hello Sir"); //what will speak
            //talk.Speak(what);
            Choices list = new Choices();
            list.Add(new string[] { 
                "hey vos","vos", "hey there", "hi vos", "can you hear me","can i ask for something","can you help me","stop","cancel","stop listening","i'm done","sleep", "restart","restart program","exit","exit program","close","close application","close program","help me","i need help","help me","shutdown","shutdown system","shutdown computer","can you shutdown the computer","restart system","restart computer","can you restart the computer","hibernate","hibernate computer","hibernate system","can you put the computer to hibernation mode","go to sleep", "can you put the computer to sleep", "time","what is the time", "tell me the time", "day", "what is today","date", "what is the date today", "tell me the date","year", "what year is it"  , "what is this year"  , "tell me the year","location"  , "where am i?" , "what is my current location","weather", "what is it like outside", "what is the weather ", "what is the weather today", "volume up", "increase volume", "volume max","volume down", "decrease volume", "mute" , "can you mute",
                "unmute", "can you unmute", "num lock on", "turn num lock on","num lock off", "turn num lock off","caps on", "cap lock on", "caps off", "cap lock off","brightness up" , "increase brightness", "turn up brightnesss","brightness down", "decrease brightness", "turn down brightness",
                "acrobat", "access","antivirius","brave","c lion","discord","edge", "eclipse", "excel","git", "chrome", "intellij", "itunes", "m audio",  "nord vpn", "nord pass","note", "pad plus plus", "outlook", "paint","powershell", "powerpoint", "publisher", "pycharm" ,"privado vpn","r studio", "reaper", "safewatch", "steam","visual studio", "v s code",  "vlc", "word", "web storm" ,"winrar","zoom","calculator","terminal","screenshot","copy", "paste","new window","new tab","search page","close tab",  "close window","bookmark page","print page","new private tab","reopen tab", "refresh page", "file explorer", "lock computer","settings","full screen","pause playing","continue playing","previous track", "previous video","previous song","next track","next video","next song","zoom in","zoom out", "debug","debug again","stop debugging"
            });

            Grammar speech = new Grammar(new GrammarBuilder(list));
            try
            {
                myvoice.LoadGrammar(speech);
                myvoice.RequestRecognizerUpdate();
                myvoice.SpeechRecognized += Myvoice_SpeechRecognized;
                myvoice.SetInputToDefaultAudioDevice();
                myvoice.RecognizeAsync(RecognizeMode.Multiple);
                myvoice.AudioStateChanged += MyvoiceOnAudioStateChanged;
            }
            catch
            {
                //MessageBox.Show("");//RESTART
                return;

            }
        }

        private void MyvoiceOnAudioStateChanged(object sender, AudioStateChangedEventArgs audioStateChangedEventArgs)
        {
            if (audioStateChangedEventArgs.AudioState == AudioState.Stopped)
            {
                // Console.Beep();
                myvoice.SetInputToDefaultAudioDevice();
                myvoice.RecognizeAsync(RecognizeMode.Multiple);
            }
        }
        /*
         * Voice Commands and their possible outcome
         */
        void Myvoice_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            bool ignore = false;
            //if((e.Result.Text.ToLower() == "vos")|| (e.Result.Text.ToLower() == "Vos") || (e.Result.Text.ToLower() == "hey vos")){
            // ignore = false; IsRunning = true; return;}
            if ((e.Result.Text == "hey vos") || (e.Result.Text == "vos") ||(e.Result.Text=="hey there")||(e.Result.Text=="hi vos"))
            {
                ignore = false;
                IsRunning = true;
                return;
            }
            if (IsRunning == true)
            {
                //Launch Apps
                if (e.Result.Text.Contains("acrobat"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "AcroRd32.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("access"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "MSACCESS.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("antivirius"))
                {
                    try
                    {
                        pr.StartInfo.FileName = @"C:\Program Files (x86)\Avira\Antivirus\startui.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("brave"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "brave.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("c lion"))
                {
                    try
                    {
                        pr.StartInfo.FileName = @"C:\Program Files\JetBrains\CLion 2021.2.1\bin\clion64.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("discord"))
                {
                    try
                    {
                        pr.StartInfo.FileName = @"C:\Users\sidam\AppData\Local\Discord\app-1.0.9004\Discord.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("edge"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "msedge.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("eclipse"))
                {
                    try
                    {
                        pr.StartInfo.FileName = @"C:\Users\sidam\eclipse\java-2020-12\eclipse\eclipse.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("excel"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "EXCEL.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("git"))
                {
                    try
                    {
                        pr.StartInfo.FileName = @"C:\Program Files\Git\git-bash.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("chrome"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "chrome.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("intellij"))
                {
                    try
                    {
                        pr.StartInfo.FileName = @"C:\Program Files\JetBrains\IntelliJ IDEA 2021.2.1\bin\idea64.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("itunes"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "iTunes.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("m audio"))
                {
                    try
                    {
                        pr.StartInfo.FileName = @"C:\Program Files (x86)\M-Audio\Fast Track Pro\Panel.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("nord vpn"))
                {
                    try
                    {
                        pr.StartInfo.FileName = @"C:\Program Files\NordVPN\NordVPN.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("nord pass"))
                {
                    try
                    {
                        pr.StartInfo.FileName = @"C:\Users\sidam\AppData\Local\Programs\nordpass\NordPass.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("note"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "Notepad.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("pad plus plus"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "notepad++.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("outlook"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "outlook.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("paint"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "mspaint.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("powershell"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "powershell.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("powerpoint"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "powerpnt.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("publisher"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "MSPUB.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("pycharm"))
                {
                    try
                    {
                        pr.StartInfo.FileName = @"C:\Program Files\JetBrains\PyCharm 2021.2.1\bin\pycharm64.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("privado vpn"))
                {
                    try
                    {
                        pr.StartInfo.FileName = @"C:\Program Files (x86)\PrivadoVPN\PrivadoVPN.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("r studio"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "rstudio.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("reaper"))
                {
                    try
                    {
                        pr.StartInfo.FileName = @"C:\Program Files\REAPER (x64)\reaper.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("safewatch"))
                {
                    try
                    {
                        pr.StartInfo.FileName = @"C:\Users\sidam\AppData\Local\Programs\safe-watch\safe-watch.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("steam"))
                {
                    try
                    {
                        pr.StartInfo.FileName = @"C:\Program Files (x86)\Steam\Steam.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("visual studio"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "devenv.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("v s code"))
                {
                    try
                    {
                        pr.StartInfo.FileName = @"C:\Users\sidam\AppData\Local\Programs\Microsoft VS Code\Code.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("vlc"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "vlc.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("word"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "WINWORD.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("web storm"))
                {
                    try
                    {
                        pr.StartInfo.FileName = @"C:\Program Files\JetBrains\WebStorm 2021.2.1\bin\webstorm64.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("winrar"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "winRAR.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text=="zoom")
                {
                    try
                    {
                        pr.StartInfo.FileName = @"C:\Users\sidam\AppData\Roaming\Zoom\bin\Zoom.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("calculator"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "calc.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }
                else if (e.Result.Text.Contains("terminal"))
                {
                    try
                    {
                        pr.StartInfo.FileName = "cmd.exe";
                        pr.Start();
                    }
                    catch
                    {
                        SpeakText("I was unable to locate this application");
                    }
                    IsRunning = false;
                    return;
                }

               
                switch (e.Result.Text.ToLower())
                {//Starts with
                    //navigation and activation & termination of program
                    //case "hi":case "hey":case "hello":
                    // {
                    //     ignore = true;
                    //     SpeakText("hi " + Environment.UserName); //Environment. UserDomainName + "\\" + Environment. UserName
                    //     IsRunning = false;
                    //     break;
                    // }
                    //Main controls
                    case "can you hear me": case "can i ask for something": case "can you help me":
                        {
                            SpeakText("Yes " + Environment.UserName);
                            //ignore = true;
                            IsRunning = false;
                            break;
                        }
                    case "stop": case "cancel": case "stop listening": case "i'm done": case "sleep":
                        {
                            //ignore = true;
                            IsRunning = false;
                            break;
                        }
                    case "exit": case "exit program": case "close": case "close application": case "close program":
                        {
                            Application.Current.Shutdown();
                            break;
                        }
                    case "restart": case "restart program":
                        {
                            System.Windows.Forms.Application.Restart();
                            Application.Current.Shutdown();
                            IsRunning = false;
                            return;
                        }
                    case "help me": case "i need help": case "help menu":
                        {
                            string help = "Tips: \n\n"+
                                            "Activate by saying VOS or pressing on the goggles on the bottom right of the screen\n"+
                                            "Goggle Movement indicates the program is listening\n"+
                                            "Stop the program by saying cancel or stop\n"+
                                            "Launch Applications by saying their name\n"+
                                            "Execute keyboard shortcuts by saying their function\n";
                            System.Windows.Forms.MessageBox.Show(help);
                            //System.Windows.Forms.ComboBoxStyle.DropDown;
                            IsRunning = false;
                            //break;
                            return;
                        }
                    //control system i.e Restart/Shutdown/Sleep/Hybernate
                    case "shutdown system": case "shutdown computer": case "can you shutdown the computer":
                        {/*
                            SpeakText("Do you want to shutdown the system "); //+Environment.UserName;
                            if (e.Result.Text.ToString() == "yes")
                            {
                                SpeakText("I will now Shutdown the computer, see you soon " + Environment.UserName);
                                Process.Start("shutdown", "/s /t 0");
                            }
                            else if (e.Result.Text.ToLower() == "no")
                            {
                                IsRunning = false;
                            }
                            else
                            {
                                SpeakText("I didnt get that, did you say yes or no? ");
                                IsRunning = true;
                            }*/
                            //Process.Start("shutdown", "/s /t 0");
                            IsRunning = false;
                            return;
                        }
                    case "restart system": case "restart computer": case "can you restart the computer":
                        {
                            //SpeakText("I will now restart the computer, see you soon sir");
                            //Process.Start("shutdown", "/r /t 0"); // r-restart,
                            /*SpeakText("Do you want to shutdown the system?");
                            if (e.Result.Text.ToString() == "yes")
                            {
                                SpeakText("I will now restart the computer, see you soon " + Environment.UserName);
                                Process.Start("shutdown", "/r /t 0"); // r-restart,
                            }
                            else if (e.Result.Text.ToString() == "no")
                            {
                                IsRunning = false;
                            }
                            else
                            {
                                SpeakText("I didnt get that, did you say yes or no? ");
                                IsRunning = true;
                            }
                            SpeakText("I will now restart the computer, see you soon " + Environment.UserName);
                            Process.Start("shutdown", "/r /t 0"); // r-restart,*/
                            SpeakText("I will now restart the computer, see you soon " + Environment.UserName);
                            Process.Start("shutdown", "/r /t 0");
                            IsRunning = false;
                            return;
                        }
                    case "hibernate computer": case "hibernate system": case "can you put the computer to hibernation mode":
                        {
                            //SpeakText("Computer is going into hibernation mode now");
                            //SetSuspendState(true, true, true);
                            //Application.Current.Shutdown();
                            /*if (e.Result.Text.ToString() == "yes")
                            {
                                SpeakText("Computer is going into hibernation mode now");
                                SetSuspendState(true, true, true);
                                Application.Current.Shutdown();
                            }
                            else if (e.Result.Text.ToString() == "no")
                            {
                                IsRunning = false;
                            }
                            else
                            {
                                SpeakText("I didnt get that, did you say yes or no? ");
                                IsRunning = true;
                            }*/
                            IsRunning = false;
                            return;
                        }
                    case "go to sleep": case "can you put the computer to sleep":
                        {
                            /* SpeakText("Do you want to go to sleep");
                             IsRunning = true;
                             if((IsRunning == true) && (e.Result.Text == "yes"))
                             {
                                 SpeakText("Okay i will");

                             } //while (IsRunning == true);
                               //SpeakText("Computer is going into sleep mode now");
                               //SetSuspendState(false, true, true);
                               //Application.Current.Shutdown();
                               //time/date/weather/location*/
                            return;
                        }
                    case "time": case "what is the time": case "tell me the time":
                        {
                            SpeakText("The time is " + DateTime.Now.ToLongTimeString());
                            IsRunning = false;
                            return;
                        }
                    case "day": case "what is today":
                        {
                            // SpeakText(DateTime.Now.To);
                            SpeakText("Today is " + DateTime.Now.Day.ToString() + "th day of this month");
                            IsRunning = false;
                            return;
                        }
                    case "date": case "what is the date today": case "tell me the date":
                        {
                            SpeakText("The date today is " + DateTime.Today.ToShortDateString());
                            IsRunning = false;
                            return;
                        }
                    case "year": case "what year is it": case "what is this year": case "tell me the year":
                        {
                            SpeakText("This year is " + DateTime.Today.Year);
                            IsRunning = false;
                            return;
                        }
                    //on/off
                    case "volume up": case "increase volume":
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.VOLUME_UP, VirtualKeyCode.VOLUME_UP);
                            }
                            IsRunning = false;
                            return;
                        }
                    case "volume max":
                        {
                            for (int i = 0; i < 100; i++)
                            {
                                InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.VOLUME_UP, VirtualKeyCode.VOLUME_UP);
                            }
                            IsRunning = false;
                            return;
                        }
                    case "volume down": case "decrease volume":
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.VOLUME_DOWN, VirtualKeyCode.VOLUME_DOWN);
                            }
                            IsRunning = false;
                            return;
                        }
                    case "mute": case "can you mute":
                        {
                            InputSimulator.SimulateKeyPress(VirtualKeyCode.VOLUME_MUTE);
                            IsRunning = false;
                            return;
                        }
                    case "unmute": case "can you unmute":
                        {
                            var isKeyDown = InputSimulator.IsTogglingKeyInEffect((VirtualKeyCode.VOLUME_MUTE));//checks if mute
                            if (isKeyDown == true)
                            {
                                InputSimulator.SimulateKeyPress(VirtualKeyCode.VOLUME_MUTE);
                            }
                            IsRunning = false;
                            return;
                        }
                    case "num lock on": case "turn num lock on":
                        {
                            InputSimulator.SimulateKeyPress(VirtualKeyCode.NUMLOCK);
                            IsRunning = false;
                            return;
                        }
                    case "num lock off": case "turn num lock off":
                        {
                            var isKeyDown = InputSimulator.IsTogglingKeyInEffect((VirtualKeyCode.NUMLOCK)); //checks if pressed
                            if (isKeyDown == true)
                            {
                                InputSimulator.SimulateKeyPress(VirtualKeyCode.NUMLOCK);
                            }
                            IsRunning = false;
                            return;
                        }
                    case "caps on": case "cap lock on":
                        {
                            InputSimulator.SimulateKeyPress(VirtualKeyCode.CAPITAL);
                            IsRunning = false;
                            return;
                        }
                    case "caps off": case "cap lock off":
                        {
                            var isKeyDown = InputSimulator.IsTogglingKeyInEffect(VirtualKeyCode.CAPITAL);
                            if (isKeyDown == true)
                            {
                                InputSimulator.SimulateKeyPress(VirtualKeyCode.CAPITAL);
                            }
                            IsRunning = false;
                            return;
                        }
                    //keyboard short cuts for browsers and documents
                    case "copy": //control+c
                        {
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                            IsRunning = false;
                            return;
                        }
                    case "paste": //control+V 
                        {
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
                            IsRunning = false;
                            return;
                        }
                    case "screenshot": //LWin+Printscreen
                        {
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.SNAPSHOT);
                            System.Diagnostics.Process.Start("explorer.exe", @"C:\Users\sidam\OneDrive\Pictures\Screenshots");
                            IsRunning = false;
                            return;
                        }
                    case "new window": //control + n
                        {
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_N);
                            IsRunning = false;
                            return;
                        }
                    case "new tab": //control + t
                        {
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_T);
                            IsRunning = false;
                            return;
                        }
                    case "search page": //control + f
                        {
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_F);
                            IsRunning = false;
                            return;
                        }
                    case "close tab": //control + w
                        {
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_W);
                            IsRunning = false;
                            return;
                        }
                    case "close window": //control + q
                        {
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_Q);
                            IsRunning = false;
                            return;
                        }
                    case "bookmark page": //control + d
                        {
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_D);
                            IsRunning = false;
                            return;
                        }
                    case "print page": //control + p
                        {
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_P);
                            IsRunning = false;
                            return;
                        }
                    case "new private tab"://control + shift + n
                        {
                            InputSimulator.SimulateModifiedKeyStroke( new[] { VirtualKeyCode.CONTROL, VirtualKeyCode.SHIFT}, new[] { VirtualKeyCode.VK_N});
                            IsRunning = false;
                            return;
                        }
                    case "reopen tab"://control + shift + t
                        {
                            InputSimulator.SimulateModifiedKeyStroke(new[] {VirtualKeyCode.CONTROL, VirtualKeyCode.SHIFT }, new[] { VirtualKeyCode.VK_T});
                            IsRunning = false;
                            return;
                        }
                    case "refresh page":case "debug"://f5
                        {
                            InputSimulator.SimulateKeyPress(VirtualKeyCode.F5);
                            IsRunning = false;
                            return;
                        }
                    case "file explorer": //windows + e
                        {
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_E);
                            IsRunning = false;
                            return;
                        }
                    case "lock computer": //windows + l
                        {
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_L);
                            IsRunning = false;
                            return;
                        }
                    case "settings"://windows + i
                        {
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_I);
                            IsRunning = false;
                            return;
                        }
                    case "full screen"://f11
                        {
                            InputSimulator.SimulateKeyPress(VirtualKeyCode.F11);
                            IsRunning = false;
                            return;
                        }
                    case "pause playing"://pause
                        {
                            InputSimulator.SimulateKeyPress(VirtualKeyCode.MEDIA_STOP);
                            IsRunning = false;
                            return;
                        }
                    case "continue playing": //continue playing 
                        {
                            InputSimulator.SimulateKeyPress(VirtualKeyCode.MEDIA_PLAY_PAUSE);
                            IsRunning = false;
                            return;
                        }
                    case "next track":case "next video":case "next song":
                        {
                            InputSimulator.SimulateKeyPress(VirtualKeyCode.MEDIA_NEXT_TRACK);
                            IsRunning = false;
                            return;
                        }
                    case "previous track": case "previous video": case "previous song":
                        {
                            InputSimulator.SimulateKeyPress(VirtualKeyCode.MEDIA_PREV_TRACK);
                            IsRunning = false;
                            return;
                        }
                    case "zoom in": //control + (+)
                        {
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.ADD);
                            IsRunning = false;
                            return;
                        }
                    case "zoom out": //control + (-)
                        {
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.SUBTRACT);
                            IsRunning = false;
                            return;
                        }
                    case "stop debugging": //shift+ f5
                        {
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.SHIFT, VirtualKeyCode.F5);
                            IsRunning = false;
                            return;
                        }
                    case "debug again": //control+shift+f5
                        {
                            InputSimulator.SimulateModifiedKeyStroke(new[] { VirtualKeyCode.CONTROL, VirtualKeyCode.SHIFT }, new[] { VirtualKeyCode.F5});
                            IsRunning = false;
                            return;

                        }
                    default:
                        {
                            SpeakText("I could not recognize the command, try another one");
                            ignore = true;
                            IsRunning = false;
                            return;
                        }
                }
                
            }
        }
        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);
        public void SpeakText(string s)
        {
            what.ClearContent();
            what.AppendText(s.ToString());
            try
            {
                talk.Speak(what);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
//public void emailText(string to, string sub, string body)
//{
//    try
//    {
//        MailMessage msg = new MailMessage();
//        msg.From = new MailAddress("mr.proudknownothing@gmail.com", "Miro");
//        msg.To.Add(new MailAddress(to));
//        msg.Subject = sub;
//        msg.Body = body;
//        msg.IsBodyHtml = false;

//        SmtpClient gmail = new SmtpClient();
//        gmail.Host = "smtp.gmail.com";
//        gmail.Credentials = new NetworkCredential("mr.proudknownothing@gmail.com", "slyME3newyear2011*_*");
//        gmail.EnableSsl = true;
//        gmail.Send(msg);
//        speakText("okay sir , your email has been sent successfuly");
//        return;

//    }

//    catch
//    {
//        speakText("");
//        return;
//    }
//}
//string emailsubject = ""; string emailbody = "";string emailtowho = "";bool emailstart = false;bool emailsub = false;bool emailbod = false;bool emailtow = false;




//implement later

