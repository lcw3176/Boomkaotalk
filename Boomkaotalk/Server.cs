using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Boomkaotalk
{
    public partial class Server : Form
    {
        Socket socket;
        public List<Socket> userlist;

        public Server()
        {
            InitializeComponent();
            userlist = new List<Socket>();
        }

        private void AcceptConnection(IAsyncResult ar)
        {
            try
            {
                Socket client = socket.EndAccept(ar);
                userlist.Add(client);
                this.Invoke(new Action(delegate ()
                {
                    listBox1.Items.Add(client.RemoteEndPoint);
                    listbox_adduser(listBox1.Items.IndexOf(client.RemoteEndPoint));
                }));
                
                socket.BeginAccept(AcceptConnection, null);
            }

            catch(Exception)
            {
                socket.Close();
            }
        }

        public void SendUsers(byte[] msg)
        {

            foreach(Socket user in userlist)
            {
                try
                {
                    user.Send(msg);
                }

                catch(Exception)
                {
                    user.Close();
                }
               
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(socket == null)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Any, 3000));
                socket.Listen(10);
                button3.BackColor = Color.Red;
                button3.ForeColor = Color.White;
                socket.BeginAccept(new AsyncCallback(AcceptConnection), null);
            }
        }

        private void listbox_adduser(int index)
        {
            Echo echo = new Echo(this);
            Thread t = new Thread(() => echo.setClient(userlist[index]));
            t.Start();
        }
    }

    public partial class Echo : Form
    {
        byte[] receive = new byte[1024];

        Socket tsocket;
        Server server;

        public Echo(Server form)
        {
            server = form;
        }

        public void setClient(Socket tclient)
        {
            byte[] accept = new byte[100];
            tsocket = tclient;
            accept = Encoding.Default.GetBytes("접속 승인되었습니다.\r\n");
            tsocket.Send(accept);
            tsocket.BeginReceive(receive, 0, receive.Length, SocketFlags.None, new AsyncCallback(echoClient), null);
        }

        public void echoClient(IAsyncResult ar)
        {
            try
            {
                int recv = tsocket.EndReceive(ar);

                if (recv > 0)
                {
                    string user = Encoding.Default.GetString(receive);
                    
                    server.Invoke(new Action(delegate ()
                    {
                        if (user.Substring(0, 3) == "bye")
                        {
                            byte[] byebye = new byte[100];
                            byebye = Encoding.Default.GetBytes("Good Bye! --> " + user.Substring(3));
                            server.listBox1.Items.Remove(tsocket.RemoteEndPoint);
                            server.userlist.Remove(tsocket);
                            server.SendUsers(byebye);
                            tsocket.Close();
                        }
                        else
                        {
                            server.textBox1.AppendText(user);
                            server.textBox1.AppendText(Environment.NewLine);
                            server.SendUsers(receive);
                            Array.Clear(receive, 0, receive.Length);
                            tsocket.BeginReceive(receive, 0, receive.Length, SocketFlags.None, echoClient, null);
                        }
                       
                    }));
                }
            }

            catch (SocketException)
            {
                tsocket.Close();
            }
        }
    }

}

   
