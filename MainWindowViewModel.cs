/*using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VOS.Annotations;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Diagnostics;

namespace VOS
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private double _percentage;
        private bool _isRunning;
        private string _message = " ";
        //public bool IsRunning {
        //  get
           // {
            //    return _isRunning
           // }
           // internal set;
       // }

        //internal void SetMessage(string v)
        //{
         //   throw new NotImplementedException();
       // }
    //}

        //default
        public MainWindowViewModel()
        {
            IsRunning = false;
            Percentage = 0.5;
            SetMessage("How may i be of service");
            InIt();
        }


        object locko = new object();

        //IsRunning methods
        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        //Percentage Methods
        public double Percentage
        {
            get
            {
                return _percentage; ;
            }
            set
            {
                if (value.Equals(_percentage)) return;
                _percentage = value;
                NotifyPropertyChanged();
            }
        }

        //send message
        public void SetMessage(string message)
        {
            new Thread(()=>
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
                    {
                        return;
                    }
                    Thread.Sleep(50);
                    while (Message.Length > 0)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Message = Message.Substring(0, Message.Length - 1);
                        });
                        Thread.Sleep(50);
                    }
                }
            }).Start();
        }


        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                if (value == _message) return;
                _message = value;
                NotifyPropertyChanged();
            }
        }



        #region VOS
        //init
        SpeechSynthesizer speaking = new SpeechSynthesizer(); // talk to client
        PromptBuilder getSpeech = new PromptBuilder();//what to client
        SpeechRecognitionEngine voice = new SpeechRecognitionEngine(); //myvoice
        Process pros = new Process(); //pr

        private void InIt()
        {
            getSpeech.ClearContent();
            //getSpeech.AppendText("Hello Sir");// output to user
            //speaking.Speak(getSpeech);
            Choices commandLib = new Choices();
            commandLib.Add(new string[]
            {
                //voice commands to get vos's attention
                "vos", "hi vos", "hey vos", "hello vos", "is there anyone out there",  "anyone there", "whatsup", "whatsup vos", "yo", "yo vos",
                //exit commands
                "exit", "goodbye","bye","peace out", "close", "cancel",
                //getdate commands
                "date", "whats the date", "what is the date", "what is the data today", "tell me the date"
                //Old Commands
                // "Good morning", "Good afternoon",  "Good bye", "bye",  "exit", "Close","Go away","Can you tell me the time", "Can you tell me the date"  "Can you open word","Can you open word", "Can you make a note of this", "Can you play music for me", "Can you open Facebook",   "can you open google",   "can you open hotmail",    "can you open yahoo",    "can you open youtube",     "can you open linkedIn",      "can you open Microsoft visual studio",       "can you open paint",        "can you turn on wifi",       "can you start an email",        "can you launch firefox",    "can  you launch internet explorer",              "can you open itunes",         "can you tell me today's schedule",        "am i free today",       "restart system",     "shutdown system",       "close programs",      "read this document for me",     "open my favourite website",      "can you open Photoshop",  "can you write down this memo for me","can you start an email","next slide"

            });
            Grammar grammar = new Grammar(new GrammarBuilder(commandLib));
            try
            {
                voice.LoadGrammar(grammar);
                voice.RequestRecognizerUpdate();
                voice.SpeechRecognized += voice_SpeechRecognized;
                voice.SetInputToDefaultAudioDevice();
                voice.RecognizeAsync(RecognizeMode.Multiple);
                voice.AudioStateChanged += voice_AudioStateChanged;
            }
            catch
            {
                MessageBox.Show("RESET");
                return;
;            }
        }

        private void voice_AudioStateChanged(object sender, AudioStateChangedEventArgs e)
        {
            //throw new NotImplementedException();
            if(e.AudioState == AudioState.Stopped)
            {
                Console.Beep();
                voice.SetInputToDefaultAudioDevice();
                voice.RecognizeAsync(RecognizeMode.Multiple);
            }
        }

        private void voice_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // throw new NotImplementedException();
            bool check = false;
            try
            {
                if ((e.Result.Text.ToLower() == "hey vos")
                    || (e.Result.Text.ToLower() == "hi vos")
                    || (e.Result.Text.ToLower() == "hello vos")
                    || (e.Result.Text.ToLower() == "is there anyone out there")
                    || (e.Result.Text.ToLower() == "anyone there")
                    || (e.Result.Text.ToLower() == "whatsup")
                    || (e.Result.Text.ToLower() == "whatsup vos")
                    || (e.Result.Text.ToLower() == "yo")
                    || (e.Result.Text.ToLower() == "yo vos")
                    )
                {
                    isTalking("Yes, I'm here to help. Please let me know what you want me to do for you, Just Say VOS and i'll respone");
                    check = true;
                    IsRunning = true;
                    return;
                }
                if (e.Result.Text.ToLower() == "vos")
                {
                    check = true;
                    IsRunning = true;
                    return;
                }
                if (!IsRunning)
                {
                    return;
                }
                if (e.Result.Text.ToLower() == "cancel")
                {
                    IsRunning = false;
                    return;
                }
                if (
                    (e.Result.Text.ToLower() == "exit")
                    || (e.Result.Text.ToLower() == "goodbye")
                    || (e.Result.Text.ToLower() == "bye")
                    || (e.Result.Text.ToLower() == "peace out")
                    || (e.Result.Text.ToLower() == "close")
                    )
                {
                    isTalking("Goodbye");
                    Application.Current.Shutdown();
                    return;
                }
                if ((e.Result.Text.ToLower() == "date")
                    || (e.Result.Text.ToLower() == "whats the date")
                    || (e.Result.Text.ToLower() == "what is the date")
                    || (e.Result.Text.ToLower() == "what is the date today")
                    || (e.Result.Text.ToLower() == "tell me the date")
                    )
                {
                    isTalking("Today is " + DateTime.Today.ToShortDateString());
                    check = true;
                    IsRunning = true;
                    return;
                }

            }
            finally
            {
                if (!check && IsRunning)
                {

                    SetMessage(e.Result.Text);
                    IsRunning = false;

                }
            }

        }

        private void isTalking(string v)
        {
            getSpeech.ClearContent();
            getSpeech.AppendText(v.ToString());
            try
            {
                speaking.Speak(getSpeech);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
        //        speakText("sorry sir , the message couldn't be send , check your network connection and try again");
        //        return;
        //    }
        //}


        #endregion

    }
}


*/
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
using System.Diagnostics;
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
        //string emailsubject = "";
        //string emailbody = "";
        //string emailtowho = "";
        //bool emailstart = false;
        //bool emailsub = false;
        //bool emailbod = false;
        //bool emailtow = false;

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
            list.Add(new string[] { "hey vos", "vos", "hi", "hey","hello","can you hear me", "can i ask for something", "can you help me","stop", "cancel","stop listening","i'm done","sleep","exit","goodbye","bye","close","close application","help me","i need help","help menu",
                                    "shutdown","shutdown system","shutdown computer","can you shutdown the computer","restart","restart system","restart computer","can you restart the computer","can you put the computer to sleep","go to sleep","hibernate","hibernate computer","hibernate system","can you put the computer to hibernation mode",
                                    "time","what is the time","tell me the time","day","what is today","date","what is the date today","tell me the date","month","what is this month","what month are we in ","what month is it","Year","what year is it","what is this year","tell me the year",
                                    "location","where am i","what is my current location","weather","what is it like outside","what is the weather","what is the weather today","bluetooth on","can you turn on bluetooth","bluetooth off","can you turn off bluetooth","wifi on","turn the wifi on","turn on the wifi","turn on wifi","connect to the wifi",
                                    "wifi off","turn off the wifi","turn the wifi off","turn off wifi","disconnect from wifi","airplane on","airplane mode on","airplane off","airplane mode off","brightness up","increas brightness","turn up brightness","brightness down","decrease brightness","turn down brightness","display off","turn off display","turn off screen",
                                    "display on","turn on display","turn on screen","volume up","increase volume","turn up the volume","volume down","decrease volume","turn down the volume","mute ","can you mute","unmute","can you unmute","keyboard lights up","turn keyboard lights up","keyboard lgiths down","turn keyboard lights down","caps on","cap lock on","caps off",
                                    "cap lock off","num lock on","turn num lock on","num lock off","turn num lock off","nightlight on","turn nightlight on","nightlight off","turn nightlight off"
                    ,"volume max","yes i do","no","yes", "no i dont", "yes sir"
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
            //if((e.Result.Text.ToLower() == "vos")|| (e.Result.Text.ToLower() == "Vos") || (e.Result.Text.ToLower() == "hey vos"))
            //{
            // ignore = false;
            //IsRunning = true;
            //  return;
            //}

            if ((e.Result.Text.ToString() == "hey vos") || (e.Result.Text.ToString() == "vos"))
            {
                ignore = false;
                IsRunning = true;
                return;
            }

            if (IsRunning == true)
            {
                switch (e.Result.Text.ToLower())//Starts with
                {
                    //navigation and activation & termination of program
                    //case "hi":case "hey":case "hello":
                    // {
                    //     ignore = true;
                    //     SpeakText("hi " + Environment.UserName); //Environment. UserDomainName + "\\" + Environment. UserName
                    //     IsRunning = false;
                    //     break;
                    // }
                    case "can you hear me":
                    case "can i ask for something":
                    case "can you help me":
                        {
                            SpeakText("Yes " + Environment.UserName);
                            //ignore = true;
                            IsRunning = false;
                            break;
                        }
                    case "stop":
                    case "cancel":
                    case "stop listening":
                    case "i'm done":
                        {
                            //ignore = true;
                            IsRunning = false;
                            break;
                        }
                    case "exit":
                    case "goodbye":
                    case "bye":
                    case "close application":
                    case "close":
                        {
                            Application.Current.Shutdown();
                            break;
                        }
                    case "help me":
                    case "i need help":
                    case "help menu":
                        {
                            string help = "VOS Basics:" +
                                          "";
                            System.Windows.Forms.MessageBox.Show(help);
                            IsRunning = false;
                            break;
                            //case "help me": case "i need help":case  "help menu":
                            // return;
                            // case "restart system": case "restart computer": case "can you restart the computer":
                            //   {
                            //    System.Windows.Forms.Application.SetSuspendState(PowerState.Suspend, false, true);
                            //  return;
                            //}
                            //case: "sleepy System","can you put the computer to sleep", "system sleep","computer sleep", "shutdown system", "shutdown computer", "can you shutdown the computer",

                        }
                    //control system i.e Restart/Shutdown/Sleep/Hybernate
                    case "shutdown":
                    case "shutdown system":
                    case "shutdown computer":
                    case "can you shutdown the computer":
                        {
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
                            }
                            //Process.Start("shutdown", "/s /t 0");
                            IsRunning = false;
                            return;
                        }
                    case "restart":
                    case "restart system":
                    case "restart computer":
                    case "can you restart the computer":
                        {
                            //SpeakText("I will now restart the computer, see you soon sir");
                            //Process.Start("shutdown", "/r /t 0"); // r-restart,
                            SpeakText("Do you want to shutdown the system?");
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
                            IsRunning = false;
                            return;
                        }

                    case "hibernate":
                    case "hibernate computer":
                    case "hibernate system":
                    case "can you put the computer to hibernation mode":
                        {
                            //SpeakText("Computer is going into hibernation mode now");
                            //SetSuspendState(true, true, true);
                            //Application.Current.Shutdown();
                            if (e.Result.Text.ToString() == "yes")
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
                            }
                            IsRunning = false;
                            return;
                        }
                    /*case "sleep":
                        {
                            SpeakText("Do you want to go to sleep");
                            IsRunning = true;
                            if((IsRunning == true) && (e.Result.Text == "yes"))
                            {
                                SpeakText("Okay i will");

                            } //while (IsRunning == true);
                            return;
                        }*/
                    //SpeakText("Computer is going into sleep mode now");
                    //SetSuspendState(false, true, true);
                    //Application.Current.Shutdown();
                    //time/date/weather/location
                    case "time":
                    case "what is the time":
                    case "tell me the time":
                        {
                            SpeakText("The time is " + DateTime.Now.ToLongTimeString());
                            IsRunning = false;
                            return;
                        }
                    case "day":
                    case "what is today":
                        {
                            // SpeakText(DateTime.Now.To);
                            SpeakText("Today is " + DateTime.Now.Day.ToString() + "th day of this month");
                            IsRunning = false;
                            return;
                        }
                    case "date":
                    case "what is the date today":
                    case "tell me the date":
                        {
                            SpeakText("The date today is " + DateTime.Today.ToShortDateString());
                            IsRunning = false;
                            return;
                        }
                    case "year":
                    case "what year is it":
                    case "what is this year":
                    case "tell me the year":
                        {
                            SpeakText("This year is " + DateTime.Today.Year);
                            IsRunning = false;
                            return;
                        }
                    case "location":
                    case "where am i?":
                    case "what is my current location":
                        {

                            return;
                        }
                    case "weather":
                    case "what is it like outside":
                    case "what is the weather ":
                    case "what is the weather today":
                        {
                            return;
                        }
                    //on/off
                    case "volume up":
                    case "increase volume"://case "turn up the volume":
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
                    case "volume down":
                    case "decrease volume":
                    case "turn down the volume":
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.VOLUME_DOWN, VirtualKeyCode.VOLUME_DOWN);
                            }
                            IsRunning = false;
                            return;
                        }
                    case "mute":
                    case "can you mute":
                        {
                            InputSimulator.SimulateKeyPress(VirtualKeyCode.VOLUME_MUTE);
                            IsRunning = false;
                            return;
                        }
                    case "unmute":
                    case "can you unmute":
                        {
                            var isKeyDown = InputSimulator.IsTogglingKeyInEffect((VirtualKeyCode.VOLUME_MUTE));//checks if mute
                            if (isKeyDown == true)
                            {
                                InputSimulator.SimulateKeyPress(VirtualKeyCode.VOLUME_MUTE);
                            }
                            IsRunning = false;
                            return;
                        }
                    case "num lock on":
                    case "turn num lock on":
                        {
                            InputSimulator.SimulateKeyPress(VirtualKeyCode.NUMLOCK);
                            IsRunning = false;
                            return;
                        }
                    case "num lock off":
                    case "turn num lock off":
                        {
                            var isKeyDown = InputSimulator.IsTogglingKeyInEffect((VirtualKeyCode.NUMLOCK)); //checks if pressed
                            if (isKeyDown == true)
                            {
                                InputSimulator.SimulateKeyPress(VirtualKeyCode.NUMLOCK);
                            }
                            IsRunning = false;
                            return;
                        }
                    case "caps on":
                    case "cap lock on":
                        {
                            InputSimulator.SimulateKeyPress(VirtualKeyCode.CAPITAL);
                            IsRunning = false;
                            return;
                        }
                    case "caps off":
                    case "cap lock off":
                        {
                            var isKeyDown = InputSimulator.IsTogglingKeyInEffect(VirtualKeyCode.CAPITAL);
                            if (isKeyDown == true)
                            {
                                InputSimulator.SimulateKeyPress(VirtualKeyCode.CAPITAL);
                            }
                            IsRunning = false;
                            return;
                        }
                    case "brightness up":
                    case "increase brightness":
                    case "turn up brightnesss":
                        {
                            IsRunning = true;
                            return;
                        }
                    case "brightness down":
                    case "decrease brightness":
                    case "turn down brightness":
                        {
                            return;
                        }


                    case "bluetooth on":
                    case "can you turn on bluetooth":
                        {
                            
                            IsRunning = false;
                            return;
                        }
                    case "bluetooth off":
                    case "can you turn off bluetooth":
                        {
                            return;
                        }
                    case "wifi on":
                    case "turn the wifi on":
                    case "turn on the wifi":
                    case "turn on wifi":
                    case "connect to the wifi":
                        {
                            return;
                        }
                    case "wifi off":
                    case "turn off the wifi":
                    case "turn the wifi off":
                    case "turn off wifi":
                    case "disconnect from the wifi":
                        {
                            return;
                        }
                    case "airplane on":
                    case "airplane mode on":
                        {
                            return;
                        }
                    case "airplane off":
                    case "airplane mode off":
                        {
                            return;
                        }
                    case "display off":
                    case "turn off display":
                    case "turn off sceeen":
                        {
                            return;
                        }
                    case "display on":
                    case "turn on display":
                    case "turn on screen":
                        {
                            return;
                        }
                    case "keyboard lights up":
                    case "turn keyboard lights up":
                        {
                            return;
                        }
                    case "keyboard lights down":
                    case "turn keyboard lights down":
                        {
                            return;
                        }
                    case "nightlight on":
                    case "turn nightlight on":
                        {
                            return;
                        }
                    case "nightlight off":
                    case "turn nightlight off":
                        {
                            return;
                        }
                    default:
                        {
                            SpeakText("I could not recognize the command");
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
        //        speakText("sorry sir , the message couldn't be send , check your network connection and try again");
        //        return;
        //    }
        //}
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
////////////////////to control applications
//for chrome and other browser usesfullscreen
//search page
//copy
//paste
//go back
//new tab
//new incagnito tab
//new window
/*else if (e.Result.Text.ToLower() == "next slide")
     {
        InputSimulator.SimulateKeyDown(VirtualKeyCode.RIGHT);
        return;}


else if (e.Result.Text.ToLower() == "Open google")
        {
            pr.StartInfo.FileName = "http://www.google.com";
            pr.Start();
            speakText("here you go sir, Google");
            return;
}
*/
/*Implement later on
case "postgres": case"launch postgres": case"database management": case"database software": case"database":
  {
      try
      {
          pr.StartInfo.FileName = @;
          pr.Start();

      }
      catch
      {
          SpeakText("I cannot locate this application, please install it and try again");
      }
      IsRunning = false;
      return;
  }*/
//Launch and Control applications
/* case "acrobat":case"adobe acrobat": case"launch acrobat": case "launch adobe acrobat": case"pdf": case "make a pdf": case"new pdf":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files (x86)\Adobe\Acrobat Reader DC\Reader\AcroRd32.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "access":case "launch access": case"open access":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files\Microsoft Office\root\Office16\MSACCESS.EXE";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "alarm app": case"alarm": case"launch alarm app": case"open alarm app": case"i want to make an alarm":
     {
         try
         {
            // pr.StartInfo.FileName = @;
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "armoury": case"armoury crate": case"launch armoury": case"launch armoury crate":
     {
         try
         {
            // pr.StartInfo.FileName = @;
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "antivirus": case"avira": case"launch antivirius": case"launch avira":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files (x86)\Avira\Launcher\Avira.Systray.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "brave": case"brave browser": case"launch brave": case"launch brave browser":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files\BraveSoftware\Brave-Browser\Application\brave.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "calculator": case"launch calculator": case"open calculator": case"i want to do math":
     {
         try
         {
             //pr.StartInfo.FileName = @;
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "calendar": case"launch calendar": case"open calendar": case"i want to check my schedule": case"my schedule":
     {
         try
         {
            // pr.StartInfo.FileName = @;
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "camera": case"launch camera": case"open camera": case"i want to take a photo":case"i want to take a picture":
     {
         try
         {
            // pr.StartInfo.FileName = @;
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "c lion": case"launch c lion": case"i want to write c code": case"code in c":case"make a c program": case "make a c project":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files\JetBrains\CLion 2021.2.1\bin\clion64.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "discord": case"launch discord": case"i want to check my messages":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Users\sidam\AppData\Local\Discord\app-1.0.9004\Discord.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "edge": case"launch edge": case"open edge browser": case"microsoft edge": case"launch microsoft edge": case"open microsoft edge":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "eclipse": case"launch eclipse":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Users\sidam\eclipse\java-2020-12\eclipse\eclipse.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "excel":case "launch excel":case"make a spreadsheet": case"microsoft excel":case"launch microsoft excel": case"i want to make a spreadsheet":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case"film app": case"launch film app":
     {
         try
         {
            // pr.StartInfo.FileName = @;
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "git": case"launch git": case"open git":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files\Git\git-bash.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "chrome": case"launch chrome": case"open chrome": case"google chrome": case"launch google chrome": case"open google chrome": case"web browser": case"main browser":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "intellij": case"launch intellij": case"open intellij": case"java coding":case"i want to write java code": case"make a java project":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files\JetBrains\IntelliJ IDEA 2021.2.1\bin\idea64.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "itunes": case"launch itunes":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files\iTunes\iTunes.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "mail": case"launch mail": case"check my mail": case"check mail": case"check email": case"i want to write email": case"write email":case"email":
     {
         try
         {
            // pr.StartInfo.FileName = @;
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "m audio": case"launch m audio": case"midi": case"launch midi": case"midi controller":case"launch midi controller":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files (x86)\M-Audio\Fast Track Pro\Panel.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "microsoft store":case "launch microsoft store": case"app store": case"launch app store":
     {
         try
         {
            // pr.StartInfo.FileName = @;
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "maps": case"launch maps": case"open maps":
     {
         try
         {
            // pr.StartInfo.FileName = @;
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "netflix": case"launch netflix": case"i want to watch netflix":case"i want to watch something on netflix":
     {
         try
         {
           //  pr.StartInfo.FileName = @;
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "nord vpn": case"launch nord vpn": case"connect to nord vpn":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files\NordVPN\NordVPN.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "nord pass": case"launch nord pass": case"password manager":case"open password manager": case"i want to check my passwords":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Users\sidam\AppData\Local\Programs\nordpass\NordPass.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "notepad": case"launch notepad": case"make notes": case"make a note":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Accessories\Notepad.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "notepad plus plus": case"launch notepad plus plus":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files (x86)\Notepad++\notepad++.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "office": case"launch office": case"microsoft office":
     {
         try
         {
           //  pr.StartInfo.FileName = @;
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "outlook": case"launch outlook": case"microsoft outlook":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files\Microsoft Office\root\Office16\OUTLOOK.EXE";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "paint": case"launch paint": case"i want to paint":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Accessories\Paint.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "powershell": case"launch powershell":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Windows PowerShell\WindowsPowerShell ISE (x86).exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "photos": case"launch photos": case"photo library": case"open photo library": case"photos app": case"launch photos app":
     {
         try
         {
            // pr.StartInfo.FileName = @;
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "powerpoint": case"launch powerpoint": case"microsoft powerpoint":  case"i want to make a presentation": case"powerpoint presentation": case "new powerpoint presentation":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files\Microsoft Office\root\Office16\POWERPNT.EXE";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "publisher": case"launch publisher": case"microsoft publisher": case"publish something":case"i want to publish":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files\Microsoft Office\root\Office16\MSPUB.EXE";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "pycharm": case"launch pycharm": case"python": case"python coding":case"make a python project":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files\JetBrains\PyCharm 2021.2.1\bin\pycharm64.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "privado vpn":case"launch privado": case"connect to vpn":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files (x86)\PrivadoVPN\PrivadoVPN.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "r studio": case"launch r studio": case"create r project": case"write r code":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files\RStudio\bin\rstudio.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "reaper": case"launch reaper": case"record guitar": case"play guitar":case"i want to play guitar": case"record music":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files\REAPER (x64)\reaper.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "safewatch": case"launch safewatch": case"download torrents": case"i want to download torrents":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Users\sidam\AppData\Local\Programs\safe-watch\safe-watch.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "skype": case"launch skype": case"skype meeting": case"make a skype meeting":
     {
         try
         {
           //  pr.StartInfo.FileName = @;
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "settings": case"launch settings": case"open settings": case"system settings":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Settings.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "screenshot": case"screenshot this page": case"take a screenshot": case"make a screenshot":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Accessories\SnippingTool.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case"spotify":case "launch spotify": case"i want to listen to music": case"listen to music": case"play music":
     {
         try
         {
            // pr.StartInfo.FileName = @;
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "steam": case "launch steam": case"play games": case"i want to play a game":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files (x86)\Steam\Steam.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "stickynotes": case"make a stickynote": case"open stickynotes": case"my stickynotes":
     {
         try
         {
           //  pr.StartInfo.FileName = @;
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "visual studios": case"microsoft visual studios": case"coding i.d.e": case"launch visual studios":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "v.s code": case"launch v.s code": case"visual studio code":case"launch visual studio code":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Users\sidam\AppData\Local\Programs\Microsoft VS Code\Code.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "vlc": case"launch vlc": case"vlc media player": case"video player": case"movie player": case"open a video player": case"play a video":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "voice recorder": case"launch voice recorder": case"record voice": case"make a voice note": case"record a voice note":
     {
         try
         {
            // pr.StartInfo.FileName = @;
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "weather app": case"launch the weather app": case"open the weather app":
     {
         try
         {
            // pr.StartInfo.FileName = @;
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "word": case"launch word ": case"microsoft word": case"launch microsoft word":  case"make a word document": case"new word document": case"word document":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files\Microsoft Office\root\Office16\WINWORD.EXE";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "webstorm": case"launch webstorm": //case"i want to design a website": case"make a website":case"new website project":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files\JetBrains\WebStorm 2021.2.1\bin\webstorm64.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "winrar": case"launch winrar": //case"extraction software": case"extract files":case "compress files": case"decompress files":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Program Files\WinRAR\WinRAR.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }
 case "zoom": case"launch zoom": //case"join zoom meeting": case"make a new zoom meeting":case"zoom meeting": case"zoom call":
     {
         try
         {
             pr.StartInfo.FileName = @"C:\Users\sidam\AppData\Roaming\Zoom\bin\Zoom.exe";
             pr.Start();

         }
         catch
         {
             SpeakText("I cannot locate this application, please install it and try again");
         }
         IsRunning = false;
         return;
     }*/