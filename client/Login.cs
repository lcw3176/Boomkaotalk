using System;
using System.Windows.Forms;

namespace client
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            Get_User_Name();
        }

        private void Get_User_Name()
        {
            string userName = textBox1.Text;
            Client client = new Client(userName);
            this.Hide();
            client.Show();
            client.FormClosed += new FormClosedEventHandler(child_closed);
        }

        private void textbos_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Get_User_Name();
            }
        }

        private void child_closed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }


    }
}
