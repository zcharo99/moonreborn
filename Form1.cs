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

        public Form1()
        {
            InitializeComponent();
            InitializeAsync();
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 12, 12));
        }
        Point lastPoint;

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

        private void Form1_Load(object sender, EventArgs e)
        {
            Api.External.RegisterExecutor("moon reborn");
        }

        private void Log(string msg)
        {
            richTextBox1.AppendText("\n" + msg);
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (!Api.External.IsInjected()) {
                MessageBox.Show("Please inject before executing");
            }
            else
            {
                string jsonContent = await webView21.CoreWebView2.ExecuteScriptAsync("getEditorContent();");
                string content = JsonConvert.DeserializeObject<string>(jsonContent);

                Api.External.execute(content);
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            await webView21.CoreWebView2.ExecuteScriptAsync("clearEditor();");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!Api.External.IsInjected())
            {
                try
                {
                    Api.External.inject();
                    Log("Injected.");
                    Log($"Username: {Api.misc.GetUsername()}");
                }
                catch (Exception ex) 
                {
                    Log($"Could not inject. Exception: {ex}");
                }
            }
            else
            {
                MessageBox.Show("Already Injected.");
            }
        }

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
                    }
                    catch (Exception ex)
                    {
                        Log("Error reading file: " + ex.Message);
                    }
                }
            }
        }

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

        private void button10_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

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

        private void button7_Click(object sender, EventArgs e)
        {

        }
    }
}
