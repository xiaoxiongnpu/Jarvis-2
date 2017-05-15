﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Speech.Recognition;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace TextToDiary
{
    public partial class Main : Form
    {

        SpeechRecognitionEngine mySRE = new SpeechRecognitionEngine();
        string[,] lSites;
        string[] lCommands = { "exit", "new tab", "stop", "jarvis" };
        private System.Windows.Forms.ContextMenu contextMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuItem3;
        bool bRunning = true;

        public Main()
        {
            InitializeComponent();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            bRunning = false;
            Disable();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            bRunning = true;
            Enable();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            WindowSetup();
            LoadFile();

            Choices Commands = new Choices(lCommands);
            Grammar gm = new Grammar(Commands);

            mySRE.RequestRecognizerUpdate();
            mySRE.SetInputToDefaultAudioDevice();
            mySRE.LoadGrammar(gm);
            mySRE.SpeechRecognized += mySR_SpeechRecognized;
            mySRE.RecognizeAsync(RecognizeMode.Multiple);

        }

        void mySR_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Text == "jarvis")
            {
                bRunning = true;
                Enable();
            }

            if (bRunning)
            {
                try
                {
                    switch (e.Result.Text)
                    {
                        case "exit":
                            Application.Exit();
                            break;
                        case "new tab":
                            Process.Start("chrome", "https://www.google.com.au/");
                            break;
                        case "stop":
                            bRunning = false;
                            Disable();
                            break;
                    }

                    for (int i = 0; i < lSites.GetLength(0); i++)
                    {
                        if (lSites[i, 0] == e.Result.Text)
                        {
                            Process.Start("chrome", lSites[i, 1]);
                            break;
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.ToString());
                }
            }
        }

        void WindowSetup()
        {
            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(workingArea.Right - Size.Width,
                                      workingArea.Bottom - Size.Height);
        }

        private void notifyIcon1_DoubleClick(object Sender, EventArgs e)
        {
            // Show the form when the user double clicks on the notify icon.

            // Set the WindowState to normal if the form is minimized.
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;

            // Activate the form.
            this.Activate();
        }

        private void menuItem1_Click(object Sender, EventArgs e)
        {
            btnStart.Enabled = !btnStart.Enabled;
            btnStop.Enabled = !btnStart.Enabled;
        }

        private void menuItem2_Click(object Sender, EventArgs e)
        {
            // Close the form, which closes the application.
            this.Close();
        }

        private void menuItem3_Click(object Sender, EventArgs e)
        {
            // Close the form, which closes the application.
            this.Close();
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                Hide();
            }
        }

        private void ntiTxtDiary_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.BringToFront();
        }

        private void Enable()
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;
        }

        private void Disable()
        {
            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }

        private void LoadFile()
        {
            DialogResult msgBoxResult;
            string path = "websites.json";

            if (!File.Exists(path))
            {
                msgBoxResult = MessageBox.Show("Error websites.txt not found\n Create default websites?", "Error in IO", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                if (msgBoxResult == DialogResult.Cancel)
                {
                    MessageBox.Show("Closing", "Closing");
                    Application.Exit();
                }
                else
                {
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        string lSites = "[\n{\n'name':'facebook',\n'url':'https://facebook.com'\n},";
                        lSites = lSites + "\n{\n'name':'twitch',\n'url': 'https://twitch.com'\n},";
                        lSites = lSites + "\n{\n'name': 'youtube',\n'url': 'https://youtube.com'\n},";
                        lSites = lSites + "\n{\n'name': 'plex',\n'url': 'https://plex.tv/web'\n},";
                        lSites = lSites + "\n{\n'name': 'netflix',\n'url': 'https://netflix.com'\n},";
                        lSites = lSites + "\n{\n'name': 'reddit',\n'url': 'https://reddit.com'\n}\n]";
                        sw.Write(lSites);
                    }
                }
            }

            using (StreamReader sr = File.OpenText(path))
            {
                string json = sr.ReadToEnd();
                List<Item> items = JsonConvert.DeserializeObject<List<Item>>(json);
                lSites = new string[items.Count, items.Count];
                for (int i = 0; i < items.Count; i++)
                {
                    this.lSites[i,0] = items[i].name;
                    this.lSites[i,1] = items[i].url;
                }
            }

            SetupCommand();
        }

        void SetupCommand()
        {
            List<string> tempCommand = new List<string>();

            for (int i = 0; i < lCommands.Length; i++)
            {
                tempCommand.Add(lCommands[i]);
            }

            for (int i = 0; i < lSites.GetLength(0); i++)
            {
                tempCommand.Add(lSites[i,0]);
            }

            lCommands = new string[lCommands.Length + lSites.GetLength(0)];
            for (int i = 0; i < tempCommand.Count; i++)
            {
                lCommands[i] = tempCommand[i];
            }
        }
    }
}
