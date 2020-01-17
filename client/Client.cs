using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace client
{
    public partial class Client : Form
    {
        Socket socket;
        byte[] send;
        byte[] receive;
        string userName;

        // 음성 합성 코드
        WMPLib.WindowsMediaPlayer wplyaer = new WMPLib.WindowsMediaPlayer();
        bool voiceOn = false;
        DirectoryInfo di = new DirectoryInfo(Application.StartupPath + "/tts.mp3");

        public Client(string user)
        {
            InitializeComponent();
            userName = user;
            send = new byte[1024];
            receive = new byte[1024];
            Wait_Connection();
        }

        private void Wait_Connection()
        {
            while(true)
            {
                try
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                    socket.Connect(new IPEndPoint(IPAddress.Loopback, 3000));
                    socket.BeginReceive(receive, 0, receive.Length, SocketFlags.None, new AsyncCallback(receiveServer), null);
                    break;
                }

                catch (SocketException)
                {
                    continue;
                }
            }
        }

        private void receiveServer(IAsyncResult ar)
        {
            try
            {
                int recv = socket.EndReceive(ar);

                Console.WriteLine(recv);
                if (recv > 0)
                {
                    string user = Encoding.Default.GetString(receive);
                    this.Invoke(new Action(delegate ()
                    {
                        textBox2.AppendText(user);
                        textBox2.AppendText(Environment.NewLine);

                    }));

                    if(voiceOn == true)
                    {
                        string[] text = user.Trim('\0', '\n').Split(':');
                        Voice_Reader(text[2]);
                    }

                    
                    Array.Clear(receive, 0, receive.Length);
                    socket.BeginReceive(receive, 0, receive.Length, SocketFlags.None, receiveServer, null);
                }
            }

            catch (IndexOutOfRangeException)
            {
                socket.BeginReceive(receive, 0, receive.Length, SocketFlags.None, receiveServer, null);
            }

            catch (Exception)
            {
                socket.Close();
            }


        }

        private void sendCallback(IAsyncResult ar)
        {
            socket.EndSend(ar);
            Array.Clear(send, 0, send.Length);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SendMessege();
        }

        private void SendMessege()
        {
            send = Encoding.Default.GetBytes(DateTime.Now.ToString("[HH:mm] ") + userName + ": "  + textBox1.Text.TrimEnd());
            textBox1.Clear();
            socket.BeginSend(send, 0, send.Length, SocketFlags.None, new AsyncCallback(sendCallback), null);
        }

        private void textBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendMessege();
            }
        }

        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            if(MessageBox.Show("붐카오톡이 종료됩니다.", "종료", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                byte[] bye;
                bye = Encoding.Default.GetBytes("bye" + userName);
                socket.Send(bye);
                socket.Close();
                Dispose(true);
                this.Close();
            }

            else
            {
                e.Cancel = true;
                return;
            }
           
        }

        private void Voice_Reader(string message)
        {
            // 음성인식 코드  
            wplyaer.close();
            string path = di.ToString();
            string url = "https://kakaoi-newtone-openapi.kakao.com/v1/synthesize";
            string VoiceName = "MAN_READ_CALM";

            if(radioButton2.Checked == true)
            {
                VoiceName = "MAN_DIALOG_BRIGHT";
            }

            else if(radioButton3.Checked == true)
            {
                VoiceName = "WOMAN_READ_CALM";
            }
            
            else if(radioButton4.Checked == true)
            {
                VoiceName = "WOMAN_DIALOG_BRIGHT";
            }
            string text = message;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            string rkey = "개발자 키";

            request.Method = "POST";
            request.ContentType = "application/xml";
            request.Headers.Add("Authorization", rkey);

            byte[] byteData = Encoding.UTF8.GetBytes("<speak><voice name='" + VoiceName + "'>" + text + "</voice></speak>");
            request.ContentLength = byteData.Length;
            Stream st = request.GetRequestStream();
            st.Write(byteData, 0, byteData.Length);
            st.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            File.Delete(path);
            using (Stream output = File.OpenWrite(path))
            using (Stream input = response.GetResponseStream())
            {
                input.CopyTo(output);

            }
            wplyaer.URL = path;
        }

        private void voiceButton_Click(object sender, EventArgs e)
        {
            if (voiceOn == false)
            {
                voiceOn = true;
                voiceButton.BackColor = Color.Red;
                voiceButton.ForeColor = Color.White;
            }

            else
            {
                voiceOn = false;
                voiceButton.BackColor = Color.White;
                voiceButton.ForeColor = Color.Black;
            }
        }
    }
}
