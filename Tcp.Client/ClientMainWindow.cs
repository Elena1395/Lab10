using System;
using System.Windows.Forms;
using SomeProject.Library.Client;
using SomeProject.Library;
using System.Net.Sockets;

namespace SomeProject.TcpClient
{
    public partial class ClientMainWindow : Form
    {
        public ClientMainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Вызов отправки собщения на сервер. Вывод результата отправки.
        /// </summary>
        private void OnMsgBtnClick(object sender, EventArgs e)
        {
            Client client = new Client();          
            if (textBox.Text.Length != 0)
            {
                OperationResult res = client.SendMessageToServer("0" + textBox.Text);
                if (res.Result == Result.OK)
                {
                    textBox.Text = "";
                    textBoxMes.Text = /*"serv send:"+*/res.Message;
                }
                else
                {
                    textBoxMes.Text = "Cannot send the message to the server."+Environment.NewLine;
                    textBoxMes.Text += /*"serv mess error: "+ */res.Message;
                }          
                timer.Interval = 2000;
                timer.Start();
            }
        }

        /// <summary>
        /// Очистка поля в которое выводятся сообщения результата отправки.
        /// </summary>
        private void OnTimerTick(object sender, EventArgs e)
        {
            textBoxMes.Text = "";
            timer.Stop();
        }

        /// <summary>
        /// Вызов отправки вайла на сервер. Вывод результата отправки.
        /// </summary>
        private void sendFileBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                Client client = new Client();
                OperationResult res = client.SendFileToServer(fileDialog.FileName);
                if (res.Result == Result.OK)
                {
                    textBoxMes.Text = /*"serv send (file res):" +*/ res.Message;
                }
                else
                {
                    textBoxMes.Text = "Cannot send the file to the server." + Environment.NewLine;
                    textBoxMes.Text += /*"serv mess error: " +*/ res.Message;
                }

                timer.Interval = 2000;
                timer.Start();
            }
        }
    }
}