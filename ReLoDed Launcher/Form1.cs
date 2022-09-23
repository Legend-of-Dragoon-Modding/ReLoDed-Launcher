using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReLoDed_Launcher
{
    public partial class ReLoDed : Form
    {
        string[] fileChecks = {
            "isos/1.iso",
            "isos/2.iso",
            "isos/3.iso",
            "isos/4.iso"
        };

        public ReLoDed()
        {
            InitializeComponent();
        }

        private void btnLaunch_Click(object sender, EventArgs e)
        {
            if (CheckFiles())
            {
                if (CheckISO())
                {
                    if (GetJavaVersion())
                    {
                        try
                        {
                            string jarName = "";

                            foreach (string fileName in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.jar", SearchOption.TopDirectoryOnly))
                            {
                                if (fileName.Contains("lod-game"))
                                    jarName = Path.GetFileName(fileName);
                            }
                            if (jarName.Equals(""))
                            {
                                throw new Exception("Incorrect JAR name or JAR not found.");
                            }
                            Process pr = new Process();
                            ProcessStartInfo psi = new ProcessStartInfo();
                            psi.FileName = "cmd.exe";
                            psi.Arguments = @"/K java -cp " + jarName + ";libs/* legend.game.MainWindows -ea"; 
                            pr.StartInfo = psi;
                            pr.Start();
                            pr.WaitForExit();
                        }
                        catch (Exception ex)
                        {
                            rtxtInfoBox.Text += "Something went wrong launching the game";
                            rtxtInfoBox.Text += Environment.NewLine;

                            rtxtInfoBox.Text += ex.ToString();
                            rtxtInfoBox.Text += Environment.NewLine;
                        }

                    }
                }
            }
        }

        private bool CheckFiles()
        {
            bool pass = true;
            string errorBuilder = "You are missing the following files needed to run the game.";

            for (int i = 0; i < fileChecks.Length; i++)
            {
                if (!File.Exists(fileChecks[i]))
                {
                    pass = false;
                    errorBuilder += "\r\n" + fileChecks[i];
                }
            }

            if (!pass)
            {
                rtxtInfoBox.Text += errorBuilder;
                rtxtInfoBox.Text += Environment.NewLine;
            }
            return pass;
        }

        private bool CheckISO()
        {
            bool pass = true;
            string errorBuilder = "Something is wrong with the following ISOs.\r\n";
            try
            {

                for (int i = 1; i < 5; i++)
                {
                    byte[] playstation = new byte[0xB];
                    byte[] region = new byte[0xA];
                    byte[] disc = new byte[0x5];

                    using (BinaryReader reader = new BinaryReader(File.Open($"isos\\{i}.iso", FileMode.Open)))
                    {
                        reader.BaseStream.Seek(0x9320, SeekOrigin.Begin);
                        reader.Read(playstation, 0, 0xB);
                        reader.BaseStream.Seek(0x15, SeekOrigin.Current);
                        reader.Read(region, 0, 0xA);
                        reader.BaseStream.Seek(0x8C, SeekOrigin.Current);
                        reader.Read(disc, 0, 0x5);
                    }

                    errorBuilder += $"Disc {i}\r\n";

                    if (!System.Text.Encoding.Default.GetString(playstation).Equals("PLAYSTATION"))
                    {
                        errorBuilder += "Not a PLAYSTATION disc.\r\n";
                        pass = false;
                    }

                    if (!System.Text.Encoding.Default.GetString(region).Contains("SCUS"))
                    {
                        errorBuilder += "ReLoDed currently only supports NA Region.\r\n";
                        pass = false;
                    }

                    int discCheck = Convert.ToInt32(System.Text.Encoding.Default.GetString(disc).Substring(4, 1));
                    if (discCheck != i)
                    {
                        errorBuilder += $"Your disc {i} is in the wrong order {discCheck}.\r\n";
                        pass = false;
                    }
                }
            }
            catch (Exception ex)
            {
                pass = false;
                errorBuilder = "Something went wrong reading the discs or ReLoDed is already open.";
            }

            if (!pass)
            {
                rtxtInfoBox.Text += errorBuilder;
                rtxtInfoBox.Text += Environment.NewLine;
            }
            return pass;
        }

        private bool GetJavaVersion()
        {
            bool pass = true;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "java.exe";
                psi.Arguments = " -version";
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;

                Process pr = Process.Start(psi);
                string output = pr.StandardError.ReadLine();
                output = output.Replace("\"", "");
                if (Int32.Parse(output.Split(' ')[2].Substring(0, 2)) < 17)
                {
                    pass = false;
                }
            }
            catch (Exception ex)
            {
                rtxtInfoBox.Text += "Open JDK version 17 is needed to run ReLoDed.";
                rtxtInfoBox.Text += Environment.NewLine;
            }
            return pass;
        }
    }
}
