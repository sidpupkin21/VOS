using System;
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
        /*public bool IsRunning {
            get
            {
                return _isRunning
            } 
            internal set; 
        }

        internal void SetMessage(string v)
        {
            throw new NotImplementedException();
        }
    }*/

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


