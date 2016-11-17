using StarterDaemon.Properties;
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
using System.Reflection;
using System.Text;
using System.Threading;
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



        

        bool alreadyOpen = false;
        private ContextMenu trayMenu;

        public StartForm()
        {
            InitializeComponent();

            listener = new TcpListener(IPAddress.Any, PORT_NO);

            // Create a simple tray menu with only one item.
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Show", Show);
            trayMenu.MenuItems.Add("Start", btn_start_Click);
            trayMenu.MenuItems.Add("Stop", btn_stop_Click);
            trayMenu.MenuItems.Add("Exit", OnExit);

            // Add menu to tray icon and show it.
            notifyIcon1.ContextMenu = trayMenu;

            this.WindowState = FormWindowState.Minimized;

            String strAppPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            String strFilePath = Path.Combine(strAppPath, "Resources");
            String strFullFilename = Path.Combine(strFilePath, "path.txt");

            //using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("StarterDaemon.Resources.path.txt"))
            //{
            //    TextReader tr = new StreamReader(stream);
            //    tbx_file.Text = tr.ReadToEnd();
            //}

            tbx_file.Text = File.ReadAllText(strFullFilename);

            

        }

        #region *** Metodi per il Menu della Systray ***
        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void Show (object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }
        #endregion



        private void select_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Select the file to be executed";
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() != DialogResult.Cancel)
            {
                String strAppPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                String strFilePath = Path.Combine(strAppPath, "Resources");
                String strFullFilename = Path.Combine(strFilePath, "path.txt");

                File.WriteAllText(strFullFilename, openFileDialog1.FileName);

                tbx_file.Text = openFileDialog1.FileName;


                //using (Stream manifestResourceStream =  Assembly.GetExecutingAssembly().GetManifestResourceStream("StarterDaemon.Resources.path.txt"))
                //{
                //    using (StreamWriter sw = new StreamWriter(manifestResourceStream))
                //    {
                //        sw.Write(openFileDialog1.FileName);
                //    }
                //}

            }
        }




        async void btn_start_Click(object sender, EventArgs e)  // Start Server
        {
            btn_start.Enabled = false;
            trayMenu.MenuItems[1].Enabled=false;
            btn_stop.Enabled = true;
            trayMenu.MenuItems[2].Enabled = true;

            //backgroundWorker1.WorkerSupportsCancellation = true;
            //backgroundWorker1.CancelAsync();
            //client.Close();
            //listener.Stop();

            //listener = new TcpListener(IPAddress.Any, PORT_NO);
            //listener.Stop();
            lbl_status.Text = "ACCESO";
            lbl_status.ForeColor = Color.Green;

            await Task.Run(() =>
            {
                listener.Stop();
                listener.Start();
                client = listener.AcceptTcpClient();
                STR = new StreamReader(client.GetStream());
                STW = new StreamWriter(client.GetStream());
                STW.AutoFlush = true;

                    
                backgroundWorker1.WorkerSupportsCancellation = true;      // Ability to cancel this thread
                backgroundWorker1.RunWorkerAsync();                       // Start receiving data in background 
                //backgroundWorker2.WorkerSupportsCancellation = true;      // Ability to cancel this thread
            });
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) //receive data
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            for (int i = 0; i < 10; i++)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
            }


            while (client.Connected)
            {
                try
                {
                    receive = STR.ReadLine();

                    if (!string.IsNullOrEmpty(receive) && !alreadyOpen)
                    {
                        //Process.Start(@"C:\WebSite\CoffeeBreakWebApp\Asset\autoplaySpotify.vbs");
                        Process.Start(openFileDialog1.FileName);  // Apro il file selezionato
                        alreadyOpen = true;
                    }

                    this.textBox2.Invoke(new MethodInvoker(delegate () { textBox2.AppendText("Dispositivo rilevato in data: " + receive + "\n"); }));
                    receive = "";


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

        //private void btn_send_Click(object sender, EventArgs e)   // Send Button
        //{
        //    if (textBox2.Text != "")
        //    {
        //        text_to_send = textBox2.Text;
        //        lbl_status.Text = text_to_send;
        //        backgroundWorker2.RunWorkerAsync();
        //    }
        //    textBox2.Text = "";
        //}

        //private void btn_connect_Click(object sender, EventArgs e) // Connect to Server
        //{
        //    client = new TcpClient();
        //    IPEndPoint IP_End = new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT_NO);

        //    try
        //    {
        //        client.Connect(IP_End);
        //        if (client.Connected)
        //        {
        //            lbl_status.Text = "Connected to Server" + "\n";
        //            STW = new StreamWriter(client.GetStream());
        //            STR = new StreamReader(client.GetStream());
        //            STW.AutoFlush = true;
        //            backgroundWorker1.RunWorkerAsync(); // Start receiving data in background 
        //        }

        //    }
        //    catch (Exception x)
        //    {
        //        MessageBox.Show(x.Message.ToString());
        //    }
        //}

        //private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)  // send data
        //{
        //    if (client.Connected)
        //    {
        //        STW.WriteLine(text_to_send);
        //        this.textBox2.Invoke(new MethodInvoker(delegate () { textBox2.AppendText("Me: " + text_to_send + "\n"); }));
        //    }
        //    else
        //    {
        //        MessageBox.Show("Send failed!");
        //    }
        //    backgroundWorker2.CancelAsync();
        //}

        private void btn_stop_Click(object sender, EventArgs e)
        {
            btn_start.Enabled = true;
            trayMenu.MenuItems[1].Enabled = true;
            btn_stop.Enabled = false;
            trayMenu.MenuItems[2].Enabled = false;

            backgroundWorker1.CancelAsync();


            if (client != null)
            {
                client.Close();
                listener.Stop();
            }
            

            alreadyOpen = false;
            lbl_status.Text = "SPENTO";
            lbl_status.ForeColor = Color.Firebrick;
        }
    }
}
