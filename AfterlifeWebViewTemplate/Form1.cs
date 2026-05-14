using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AfterlifeUnityGitTool
{
    public partial class Form1 : Form
    {
        // =========================
        // Window Drag
        // =========================

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(
            IntPtr hWnd,
            int Msg,
            int wParam,
            int lParam
        );

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        // =========================
        // Resize
        // =========================

        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        private const int resizeAreaSize = 10;

        public Form1()
        {
            InitializeComponent();

            FormBorderStyle = FormBorderStyle.None;
            MinimumSize = new Size(400, 300);

            MouseDown += Form1_MouseDown;

            RunEnsureAsync();
        }

        // =========================
        // Get latest HTML (ALWAYS fresh)
        // =========================
        private string GetLatestHtmlFile()
        {
            string rawFolder = Path.Combine(Application.StartupPath, "raw");

            if (!Directory.Exists(rawFolder))
                return null;

            return Directory
                .GetFiles(rawFolder, "*.html")
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .FirstOrDefault();
        }

        // =========================
        // INIT
        // =========================
        async void RunEnsureAsync()
        {
            await webView21.EnsureCoreWebView2Async();
            await webView22.EnsureCoreWebView2Async();

            // =========================
            // Hide scrollbars (browser)
            // =========================
            await webView22.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
                const style = document.createElement('style');
                style.innerHTML = `
                    ::-webkit-scrollbar {
                        width: 0px !important;
                        height: 0px !important;
                    }
                `;
                document.documentElement.appendChild(style);
            ");

            // =========================
            // Navbar messages
            // =========================
            webView21.CoreWebView2.WebMessageReceived += (s, args) =>
            {
                string msg = args.TryGetWebMessageAsString();

                if (msg.StartsWith("navigate|"))
                {
                    string url = msg.Substring("navigate|".Length);

                    if (!url.StartsWith("http"))
                        url = "https://" + url;

                    webView22.CoreWebView2.Navigate(url);
                }
                else if (msg == "back" && webView22.CoreWebView2.CanGoBack)
                {
                    webView22.CoreWebView2.GoBack();
                }
                else if (msg == "forward" && webView22.CoreWebView2.CanGoForward)
                {
                    webView22.CoreWebView2.GoForward();
                }
                else if (msg == "minimize")
                {
                    WindowState = FormWindowState.Minimized;
                }
                else if (msg == "maximize")
                {
                    WindowState = WindowState == FormWindowState.Maximized
                        ? FormWindowState.Normal
                        : FormWindowState.Maximized;
                }
                else if (msg == "close")
                {
                    Close();
                }
            };

            // =========================
            // LOAD ANY HTML (NO CACHING ISSUES)
            // =========================
            LoadNavbar();

            // =========================
            // Default page
            // =========================
            webView22.CoreWebView2.Navigate("https://www.google.com");
        }

        // =========================
        // NAVBAR LOADER (KEY FIX)
        // =========================
        private void LoadNavbar()
        {
            string htmlFile = GetLatestHtmlFile();

            if (htmlFile == null)
            {
                MessageBox.Show("No HTML files found in /raw folder!");
                return;
            }

            // IMPORTANT: force fresh reload (kills caching issues)
            webView21.CoreWebView2.Navigate("about:blank");

            webView21.CoreWebView2.Navigate(
                new Uri(htmlFile).AbsoluteUri + "?t=" + DateTime.Now.Ticks
            );
        }

        // =========================
        // DRAG
        // =========================
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        // =========================
        // RESIZE
        // =========================
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;

            base.WndProc(ref m);

            if (m.Msg != WM_NCHITTEST)
                return;

            Point p = PointToClient(Cursor.Position);

            bool left = p.X <= resizeAreaSize;
            bool right = p.X >= Width - resizeAreaSize;
            bool top = p.Y <= resizeAreaSize;
            bool bottom = p.Y >= Height - resizeAreaSize;

            if (top && left) m.Result = (IntPtr)HTTOPLEFT;
            else if (top && right) m.Result = (IntPtr)HTTOPRIGHT;
            else if (bottom && left) m.Result = (IntPtr)HTBOTTOMLEFT;
            else if (bottom && right) m.Result = (IntPtr)HTBOTTOMRIGHT;
            else if (left) m.Result = (IntPtr)HTLEFT;
            else if (right) m.Result = (IntPtr)HTRIGHT;
            else if (top) m.Result = (IntPtr)HTTOP;
            else if (bottom) m.Result = (IntPtr)HTBOTTOM;
            else m.Result = (IntPtr)HTCAPTION;
        }

        // =========================
        // Panel dragging
        // =========================
        private void panel1_MouseDown(object sender, MouseEventArgs e) => Form1_MouseDown(sender, e);
        private void panel2_MouseDown(object sender, MouseEventArgs e) => Form1_MouseDown(sender, e);
        private void panel3_MouseDown(object sender, MouseEventArgs e) => Form1_MouseDown(sender, e);
        private void panel4_MouseDown(object sender, MouseEventArgs e) => Form1_MouseDown(sender, e);
        private void panel6_MouseDown(object sender, MouseEventArgs e) => Form1_MouseDown(sender, e);
    }
}