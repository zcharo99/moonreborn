using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using CloudyApi;
using Newtonsoft.Json;
using System.Drawing.Drawing2D;
using MoonAPI;
using System.Diagnostics;
using System.Threading;
using Microsoft.Web.WebView2.Core;

namespace moonreborn
{
    public partial class Form1 : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        public class Settings
        {
            public string SelectedAPI { get; set; }
        }

        public Form1()
        {
            InitializeComponent();
            InitializeAsync();
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 12, 12));
        }

        Point lastPoint;
        bool cloudy;
        bool xeno;
        bool moon;

        private Settings GetLoadedAPI()
        {
            Settings updatedSettings = new Settings
            {
                SelectedAPI = "Cloudy"
            };

            if (cloudy)
            {
                updatedSettings.SelectedAPI = "Cloudy";
            }
            else if (xeno)
            {
                updatedSettings.SelectedAPI = "Xeno";
            }
            else if (moon)
            {
                updatedSettings.SelectedAPI = "Moon";
            }
            else
            {
                updatedSettings.SelectedAPI = "Cloudy";
            }
            return updatedSettings;
        }

        private void UpdateSettingsFile()
        {
            Settings updatedSettings = GetLoadedAPI();
            string updatedJson = JsonConvert.SerializeObject(updatedSettings, Formatting.Indented);
            try
            {
                File.WriteAllText("settings.json", updatedJson);
            }
            catch (Exception ex)
            {
                Log("ERROR", Color.Red, $"Could not update settings file: {ex}");
            }
        }

        private async void InitializeAsync()
        {
            try
            {
                await webView21.EnsureCoreWebView2Async(null);
                webView21.CoreWebView2.Navigate(new Uri($"file:///{Directory.GetCurrentDirectory()}/monaco/index.html").ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing WebView2: {ex.Message}", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void SetEditorContent(string content)
        {
            string escapedContent = Newtonsoft.Json.JsonConvert.SerializeObject(content);
            await webView21.CoreWebView2.ExecuteScriptAsync($"window.editor.setValue({escapedContent});");
        }

        void UpdateStatus()
        {
            guna2CircleButton1.DisabledState.FillColor = guna2CircleButton1.FillColor;
            guna2CircleButton1.DisabledState.BorderColor = guna2CircleButton1.FillColor;
            guna2CircleButton1.DisabledState.CustomBorderColor = guna2CircleButton1.FillColor;
            guna2CircleButton1.DisabledState.ForeColor = guna2CircleButton1.FillColor;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            Api.External.RegisterExecutor("moon reborn");
            MoonAPI.MoonAPI.Configure("moon api injected", "moon reborn");
            Log("INFO", Color.Aqua, "Moon Reborn loaded");

            if (!File.Exists("settings.json"))
            {
                Settings defaultSettings = new Settings
                {
                    SelectedAPI = "Moon"
                };
                string defaultJson = JsonConvert.SerializeObject(defaultSettings, Formatting.Indented);
                try
                {
                    File.WriteAllText("settings.json", defaultJson);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error creating file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            string json = File.ReadAllText("settings.json");
            Settings settings = JsonConvert.DeserializeObject<Settings>(json);
            Log("INFO", Color.Aqua, $"Selected API: {settings.SelectedAPI}");
            if (settings.SelectedAPI == "Cloudy")
            {
                xeno = false;
                moon = false;
                cloudy = true;
            }
            else if (settings.SelectedAPI == "Xeno")
            {
                cloudy = false;
                moon = false;
                xeno = true;
            }
            else if (settings.SelectedAPI == "Moon")
            {
                xeno = false;
                cloudy = false;
                moon = true;
            }
            // Log("DEBUG", Color.Pink, $"Cloudy: {cloudy}, Xeno: {xeno}, Moon: {moon}");
            UpdateStatus();
        }

        public void Log(string severity, Color color, string msg)
        {
            string timestamp = DateTime.Now.ToString("[HH:mm:ss]");
            richTextBox1.SelectionColor = Color.Gray;
            richTextBox1.AppendText(timestamp);
            richTextBox1.SelectionColor = color;
            richTextBox1.AppendText(" [" + severity + "] " + msg + "\n");
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        // execute
        private async void button1_Click(object sender, EventArgs e)
        {
            string jsonContent = await webView21.CoreWebView2.ExecuteScriptAsync("getEditorContent();");
            string content = JsonConvert.DeserializeObject<string>(jsonContent);

            if (content == "switchapi(\"Cloudy\")")
            {
                xeno = false;
                moon = false;
                cloudy = true;
                Log("INFO", Color.Aqua, "Switched API to Cloudy");
                UpdateSettingsFile();
                return;
            }
            else if (content == "switchapi(\"Xeno\")")
            {
                cloudy = false;
                moon = false;
                xeno = true;
                UpdateSettingsFile();
                Log("INFO", Color.Aqua, "Switched API to Xeno");
                return;
            }
            else if (content == "switchapi(\"Moon\")")
            {
                xeno = false;
                cloudy = false;
                moon = true;
                UpdateSettingsFile();
                Log("INFO", Color.Aqua, "Switched API to Moon (Nezur)");
                return;
            }

            if (cloudy)
            {
                if (!Api.External.IsInjected())
                {
                    MessageBox.Show("Please inject before executing");
                }
                else
                {
                    Api.External.execute(content);
                }
            }
            else if (xeno)
            {
                if (!XenoFuncs.IsInjected())
                {
                    MessageBox.Show("Please inject before executing");
                }
                else
                {
                    XenoFuncs.ExecuteScript(content);
                }
            }
            else if (moon)
            {
                if (!MoonAPI.MoonAPI.IsInjected())
                {
                    MessageBox.Show("Please inject before executing");
                }
                else
                {
                    MoonAPI.MoonAPI.ExecuteScript(content);
                }
            }
        }

        // clear
        private async void button3_Click(object sender, EventArgs e)
        {
            await webView21.CoreWebView2.ExecuteScriptAsync("clearEditor();");
        }

        // inject
        private void button2_Click(object sender, EventArgs e)
        {
            if (cloudy)
            {
                if (!Api.External.IsInjected() && Api.misc.isRobloxOpen())
                {
                    try
                    {
                        Api.External.inject();
                        Api.External.execute("warn(\"[MOON] Moon Reborn injected with Cloudy API\")");
                        Log("SUCCESS", Color.Green, "Injected.");
                    }
                    catch (Exception ex)
                    {
                        Log("ERROR", Color.Red, $"Could not inject. Exception: {ex}");
                    }
                }
                else
                {
                    MessageBox.Show("Already Injected/Roblox is not open.");
                }
            }
            else if (xeno)
            {
                if (MoonAPI.MoonAPI.IsRobloxOpen() && !XenoFuncs.IsInjected())
                {
                    try
                    {
                        XenoFuncs.Inject();
                        XenoFuncs.ExecuteScript("warn(\"[MOON] Moon Reborn injected with Xeno API (Quorum)\")");
                        Log("SUCCESS", Color.Green, "Injected.");
                        Log("DEBUG", Color.Pink, "IsInjected: " + XenoFuncs.IsInjected().ToString());
                    }
                    catch (Exception ex)
                    {
                        // Log("ERROR", Color.Red, $"Could not inject. Exception: {ex}");
                        MessageBox.Show(ex.ToString());
                    }
                }
                else
                {
                    MessageBox.Show("Already injected/Roblox is not open.");
                }
            }
            else if (moon)
            {
                if (MoonAPI.MoonAPI.IsRobloxOpen() && !MoonAPI.MoonAPI.IsInjected())
                {
                    try
                    {
                        MoonAPI.MoonAPI.Inject();
                        MoonAPI.MoonAPI.ExecuteScript("warn(\"[MOON] Moon Reborn injected with Moon API (Nezur)\")");
                        Log("SUCCESS", Color.Green, "Injected.");
                    }
                    catch (Exception ex)
                    {
                        Log("ERROR", Color.Red, $"Could not inject. Exception: {ex}");
                    }
                }
                else
                {
                    MessageBox.Show("Already Injected/Roblox is not open.");
                }
            }
            guna2CircleButton1.FillColor = Color.FromArgb(128, 255, 128);
            UpdateStatus();
        }

        // open file
        private void button6_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select a text file";
                openFileDialog.Filter = "Text and Lua files (*.txt;*.lua)|*.txt;*.lua|All files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = openFileDialog.FileName;
                        string fileContent = File.ReadAllText(filePath);
                        SetEditorContent(fileContent);
                        Log("SUCCESS", Color.Green, "Opened script from file");
                    }
                    catch (Exception ex)
                    {
                        Log("ERROR", Color.Red, "Error reading file: " + ex.Message);
                    }
                }
            }
        }

        // kill roblox
        private void button8_Click(object sender, EventArgs e)
        {
            if (!Api.misc.isRobloxOpen())
            {
                MessageBox.Show("Roblox is not open");
            } else
            {
                Api.misc.killRoblox();
            }
        }

        // quit button
        private void button10_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // minimize button
        private void button11_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        // dragging behavior
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            lastPoint = new Point(e.X, e.Y);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - lastPoint.X;
                this.Top += e.Y - lastPoint.Y;
            }
        }

        // save btn
        private async void button7_Click(object sender, EventArgs e)
        {
            string jsonContent = await webView21.CoreWebView2.ExecuteScriptAsync("getEditorContent();");
            string content = JsonConvert.DeserializeObject<string>(jsonContent);

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Lua files (*.lua)|*.lua|All files (*.*)|*.*";
            saveFileDialog.DefaultExt = "lua";
            saveFileDialog.AddExtension = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, content);
                    Log("SUCCESS", Color.Green, "Saved script as " + saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    Log("ERROR", Color.Red, "Could not save to file: " + ex);
                }
            }
        }
    }
}
