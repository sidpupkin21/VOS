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
using VoxSystem.Annotations;
using System.IO;
using System.Xml;

namespace VoxSystem
{
    class MainWindowViewModel:INotifyPropertyChanged
    {
        private double _percentage;
        private bool _isRunning;
        private string _message="";

        public MainWindowViewModel()
        {
            IsRunning = false;
            Percentage = 0.5;
            new Thread(() => Application.Current.Dispatcher.Invoke(() =>
            {
                SetMessage("Welcome To VOS ... How can i help you sir?");
                speakText("Welcome To VOS ... How can i help you sir?");
                Init();
            })){IsBackground = true}.Start();
        }
        object locko=new object();
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
                    {return;}
                    Thread.Sleep(50);

                }
                Thread.Sleep(2000);
                while(Message.Length>0)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Message = Message.Substring(0,Message.Length-1);
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
        string emailsubject = "VOS TEST";
        string emailbody = "Hello Sir";
        string emailtowho = "ahmed.sidahmed234@gmail.com";
        bool emailstart = false;
        bool emailsub = false;
        bool emailbod = false;
        bool emailtow = false;
        SpeechSynthesizer talk = new SpeechSynthesizer();
        PromptBuilder what = new PromptBuilder();
        SpeechRecognitionEngine myvoice = new SpeechRecognitionEngine();
        Process pr = new Process();
        private void Init()
        {
            what.ClearContent();
            //what.AppendText("Hello Sir"); //what will speak
            //talk.Speak(what);
            Choices list = new Choices();
            list.Add(new string[] { "vos", "cancel", "Hello", "Good afternoon", "exit" , "tell me the time", "tell me the date", "Start word", "Open google", "Open hotmail", "Open yahoo", "Start internet explorer", "Tell me today's schedule", "am i free today", "next slide", "Previous slide", "Open gmail"});
            Grammar speech = new Grammar(new GrammarBuilder(list));

            try
            {
                myvoice.LoadGrammar(speech);
                myvoice.RequestRecognizerUpdate();
                myvoice.SpeechRecognized += myvoice_SpeechRecognized;
                myvoice.SetInputToDefaultAudioDevice();
                myvoice.RecognizeAsync(RecognizeMode.Multiple);
                myvoice.AudioStateChanged += MyvoiceOnAudioStateChanged;
            }
            catch
            {
                MessageBox.Show("RESTART");
                return;

            }
        }

        private void MyvoiceOnAudioStateChanged(object sender, AudioStateChangedEventArgs audioStateChangedEventArgs)
        {
            if (audioStateChangedEventArgs.AudioState==AudioState.Stopped)
            {
                Console.Beep();
                myvoice.SetInputToDefaultAudioDevice();
                myvoice.RecognizeAsync(RecognizeMode.Multiple);
            }
        }

        void myvoice_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            bool ignore = false;
            try
            {
                if (e.Result.Text.ToLower() == "vos")
                {
                    //speakText("hi sir , what can i do for you?");
                    ignore = true;
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
                if ((e.Result.Text.ToLower() == "exit") || (e.Result.Text.ToLower() == "goodbye") ||
                    (e.Result.Text.ToLower() == "bye") || (e.Result.Text.ToLower() == "peace out") ||
                    (e.Result.Text.ToLower() == "close"))
                {
                    speakText("Goodbye");
                    Application.Current.Shutdown();
                    return;
                }
                if (e.Result.Text.ToLower() == "Tell me the date")
                {
                    speakText("Today is " + DateTime.Today.ToShortDateString());
                    return;
                }
                else if (e.Result.Text.ToLower() == "next slide")
                {
                    InputSimulator.SimulateKeyDown(VirtualKeyCode.RIGHT);
                    return;
                }
                else if (e.Result.Text.ToLower() == "Previous slide")
                {
                    InputSimulator.SimulateKeyUp(VirtualKeyCode.LEFT);
                    return;
                }
                else if (e.Result.Text.ToLower() == "hello")
                {

                    speakText("hi sir , how are you");
                    return;

                }
                else if (e.Result.Text.ToLower() == "good afternoon")
                {
                    speakText("Good afternoon sir");
                    return;
                }
                else if (e.Result.Text.ToLower() == "Open google")
                {
                    pr.StartInfo.FileName = "http://www.google.com";
                    pr.Start();
                    speakText("here you go sir, Google");
                    return;
                }
                else if (e.Result.Text.ToLower() == "Open hotmail")
                {
                    pr.StartInfo.FileName = "http://www.hotmail.com";
                    pr.Start();
                    speakText("here you go sir, Hotmail");
                    return;
                }
                else if (e.Result.Text.ToLower() == "Open yahoo")
                {
                    pr.StartInfo.FileName = "http://www.yahoo.com";
                    pr.Start();
                    speakText("here you go sir, Yahoo");
                    return;
                }
                else if (e.Result.Text.ToLower() == "Open gmail")
                {
                    pr.StartInfo.FileName = "www.//www.gmail.com";
                    pr.Start();
                    speakText("here you go sir, Gmail. Would you like anything else sir");
                    return;
                }

                else if (e.Result.Text.ToLower().ToLower() == "tell me the time")
                {
                    speakText("it is " + DateTime.Now.ToString("h:mm tt"));
                    return;
                }
                else if (e.Result.Text.ToLower() == "Tell me today's schedule")
                {
                    speakText("yes you are sir");
                    return;
                }
                else if (e.Result.Text.ToLower() == "am i free today")
                {
                    speakText("No your not free today sir.");
                    return;
                }
                else if (e.Result.Text.ToLower() == "Start internet explorer")
                {
                    try
                    {
                        pr.StartInfo.FileName = "iexplore.exe";
                        pr.Start();
                        speakText("INTERNET EXPLORER is starting for you.");
                        return;
                    }
                    catch
                    {
                        speakText("you don't seem to have that installed");
                        return;
                    }

                }
                else if (e.Result.Text.ToLower() == "Start word")
                {
                    try
                    {
                        pr.StartInfo.FileName = "WINWORD.EXE";
                        pr.Start();
                        speakText("Microsoft office word is now starting for you sir.");
                        return;
                    }
                    catch
                    {
                        speakText("you don't seem to have that installed");
                        return;
                    }
                }
      

            }
            finally
            {
                if (!ignore && IsRunning)
                {
                    
                        SetMessage(e.Result.Text);
                    IsRunning = false;
                    
                }
            }

        }
        public void speakText(string s)
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
        //        msg.From = new MailAddress("ahmed.sidahmed234@gmail.com", "HI");
        //        msg.To.Add(new MailAddress(to));
        //        msg.Subject = sub;
        //        msg.Body = body;
        //        msg.IsBodyHtml = false;

        //        SmtpClient gmail = new SmtpClient();
        //        gmail.Host = "smtp.gmail.com";
        //        gmail.Credentials = new NetworkCredential("ahmed.sdiahmed234@gmail.com", "ahmedomer");
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
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
