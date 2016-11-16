using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarterDaemon
{
    public partial class StartForm : Form
    {
        TcpListener listener;
        private TcpClient client;
        public StreamReader STR;
        public StreamWriter STW;
        public string receive;
        public string text_to_send;
        const int PORT_NO = 5000;


        public StartForm()
        {
            InitializeComponent();
        }

        private void select_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Select the file to be executed";
            if (openFileDialog1.ShowDialog() != DialogResult.Cancel)
            {
                openFileDialog1.FileName = "";
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void btn_start_Click(object sender, EventArgs e)  // Start Server
        {
            listener = new TcpListener(IPAddress.Any, PORT_NO);
            listener.Start();
            client =  listener.AcceptTcpClient();
            STR = new StreamReader(client.GetStream());
            STW = new StreamWriter(client.GetStream());
            STW.AutoFlush = true;

            backgroundWorker1.RunWorkerAsync();                       // Start receiving data in background     
            backgroundWorker2.WorkerSupportsCancellation = true;      // Ability to cancel this thread

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) //receive data
        {
            while (client.Connected)
            {
                try
                {
                    receive = STR.ReadLine();

                    if (!string.IsNullOrEmpty(receive))
                    {
                        Process.Start(@"C:\WebSite\CoffeeBreakWebApp\Asset\autoplaySpotify.vbs");
                    }

                    this.textBox2.Invoke(new MethodInvoker(delegate () { textBox2.AppendText("You: " + receive + "\n"); }));
                    receive = "";

                    listener.Stop();
                    listener = new TcpListener(IPAddress.Any, PORT_NO);
                    listener.Start();
                    client = listener.AcceptTcpClient();
                    STR = new StreamReader(client.GetStream());
                    STW = new StreamWriter(client.GetStream());
                    STW.AutoFlush = true;

                }
                catch(Exception x)
                {
                    MessageBox.Show(x.Message.ToString());
                }
            }
        }

        private void btn_send_Click(object sender, EventArgs e)   // Send Button
        {
            if (textBox2.Text != "")
            {
                text_to_send = textBox2.Text;
                lbl_status.Text = text_to_send;
                backgroundWorker2.RunWorkerAsync();
            }
            textBox2.Text = "";
        }

        private void btn_connect_Click(object sender, EventArgs e) // Connect to Server
        {
            client = new TcpClient();
            IPEndPoint IP_End = new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT_NO);

            try
            {
                client.Connect(IP_End);
                if (client.Connected)
                {
                    lbl_status.Text = "Connected to Server" + "\n";
                    STW = new StreamWriter(client.GetStream());
                    STR = new StreamReader(client.GetStream());
                    STW.AutoFlush = true;
                    backgroundWorker1.RunWorkerAsync(); // Start receiving data in background 
                }
                
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)  // send data
        {
            if (client.Connected)
            {
                STW.WriteLine(text_to_send);
                this.textBox2.Invoke(new MethodInvoker(delegate () { textBox2.AppendText("Me: " + text_to_send + "\n"); }));
            }
            else
            {
                MessageBox.Show("Send failed!");
            }
            backgroundWorker2.CancelAsync();
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            client.Close(); ;
        }
    }
}
