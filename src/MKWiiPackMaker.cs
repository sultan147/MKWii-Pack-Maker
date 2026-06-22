// MKWii Pack Maker - HNS Edition
// Local C# Windows Forms desktop application. No browser wrapper. No Python runtime.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MKWiiPackMaker
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e)
                {
                    ShowStartupError(e.Exception);
                };
                AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
                {
                    ShowStartupError(e.ExceptionObject as Exception);
                };

                MainForm form = new MainForm();
                form.Load += delegate
                {
                    try
                    {
                        form.WindowState = FormWindowState.Normal;
                        form.ShowInTaskbar = true;
                        form.TopMost = true;
                        form.Activate();
                        form.BringToFront();

                        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                        timer.Interval = 900;
                        timer.Tick += delegate
                        {
                            timer.Stop();
                            timer.Dispose();
                            if (!form.IsDisposed) form.TopMost = false;
                        };
                        timer.Start();
                    }
                    catch { }
                };

                Application.Run(form);
            }
            catch (Exception ex)
            {
                ShowStartupError(ex);
            }
        }

        private static void ShowStartupError(Exception ex)
        {
            try
            {
                string root = AppContext.BaseDirectory;
                string logDir = Path.Combine(root, "logs");
                Directory.CreateDirectory(logDir);
                string text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] STARTUP ERROR" + Environment.NewLine +
                              (ex == null ? "Unknown startup error." : ex.ToString()) + Environment.NewLine;
                File.AppendAllText(Path.Combine(logDir, "startup_error.log"), text, Encoding.UTF8);
                File.AppendAllText(Path.Combine(logDir, "latest.log"), text, Encoding.UTF8);
            }
            catch { }

            try
            {
                MessageBox.Show(
                    "MKWii Pack Maker could not open correctly.\n\n" +
                    "A startup error log was written to logs/startup_error.log.\n" +
                    "Send that file if you need help.\n\n" +
                    (ex == null ? "Unknown error." : ex.Message),
                    "MKWii Pack Maker Startup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch { }
        }
    }

    public class TrackSlotInfo
    {
        public string SlotType { get; set; } = "Race";
        public string Cup { get; set; } = "";
        public string TrackName { get; set; } = "";
        public string GameFile { get; set; } = "";
        public string SourcePath { get; set; } = "";
        public string CustomName { get; set; } = "";

        public TrackSlotInfo() { }
        public TrackSlotInfo(string slotType, string cup, string trackName, string gameFile)
        {
            SlotType = slotType;
            Cup = cup;
            TrackName = trackName;
            GameFile = gameFile;
        }
    }

    public class AssetFile
    {
        public string Category { get; set; } = "Music";
        public string SourcePath { get; set; } = "";
        public string OutputName { get; set; } = "";
        public string DiscPath { get; set; } = "";
        public string Notes { get; set; } = "";
    }

    public class ProjectState
    {
        public string PackName { get; set; } = "New MKWii HNS Pack";
        public string PackId { get; set; } = "mkwii_hns_pack";
        public string Author { get; set; } = "";
        public string Version { get; set; } = "1.0.0";
        public string Description { get; set; } = "";
        public bool CopyMultiplayerCourseCopies { get; set; } = false;
        public bool IncludeCommunityFallbackFolderPatch { get; set; } = false;
        public List<TrackSlotInfo> Tracks { get; set; } = new List<TrackSlotInfo>();
        public List<AssetFile> Music { get; set; } = new List<AssetFile>();
        public List<AssetFile> Characters { get; set; } = new List<AssetFile>();
        public List<AssetFile> UiRelAssets { get; set; } = new List<AssetFile>();
        public Dictionary<string, string> CupIcons { get; set; } = new Dictionary<string, string>();
        public string ExportOutputParentFolder { get; set; } = "";
        public string PatchBaseUiFolder { get; set; } = "";
        public string PatchWorkspaceFolder { get; set; } = "";
        public string PatchBrawlCratePath { get; set; } = "";
        public string PatchWbmgtPath { get; set; } = "";
        public string PatchWszstPath { get; set; } = "";
        public string PatchLanguage { get; set; } = "U - USA English";
        public bool PatchIncludeAward { get; set; } = false;
    }

    public class MainForm : Form
    {
        private const string AppVersion = "12.0.0";

        private readonly Color Bg = Color.FromArgb(14, 17, 24);
        private readonly Color Bg2 = Color.FromArgb(18, 22, 32);
        private readonly Color Sidebar = Color.FromArgb(10, 13, 20);
        private readonly Color Card = Color.FromArgb(28, 33, 46);
        private readonly Color Card2 = Color.FromArgb(33, 39, 54);
        private readonly Color Border = Color.FromArgb(55, 64, 83);
        private readonly Color TextMain = Color.FromArgb(245, 248, 255);
        private readonly Color TextMuted = Color.FromArgb(165, 177, 199);
        private readonly Color Accent = Color.FromArgb(93, 156, 255);
        private readonly Color AccentPurple = Color.FromArgb(133, 91, 255);
        private readonly Color AccentGreen = Color.FromArgb(42, 176, 147);
        private readonly Color Danger = Color.FromArgb(246, 91, 91);
        private readonly Color Warn = Color.FromArgb(255, 194, 87);

        private string AppRoot = Application.StartupPath;
        private string ProjectFile = "";
        private string PackName = "New MKWii HNS Pack";
        private string PackId = "mkwii_hns_pack";
        private string Author = "";
        private string PackVersion = "1.0.0";
        private string Description = "";
        private bool CopyMultiplayerCourseCopies = false;
        private bool IncludeCommunityFallbackFolderPatch = false;
        private string ExportOutputParentFolder = "";
        private string LastExportFolder = "";
        private string PatchBaseUiFolder = "";
        private string PatchWorkspaceFolder = "";
        private string PatchBrawlCratePath = "";
        private string PatchWbmgtPath = "";
        private string PatchWszstPath = "";
        private string PatchLanguage = "U - USA English";
        private bool PatchIncludeAward = false;
        private int LastAutoPatchedCharacterUi = 0;
        private int LastAutoPatchedCharacterDriver = 0;

        private readonly List<TrackSlotInfo> Slots = new List<TrackSlotInfo>();
        private readonly List<AssetFile> MusicAssets = new List<AssetFile>();
        private readonly List<AssetFile> CharacterAssets = new List<AssetFile>();
        private readonly List<AssetFile> UiRelAssets = new List<AssetFile>();
        private readonly Dictionary<string, string> CupIconPaths = new Dictionary<string, string>();

        private readonly string[] CupNames = new string[]
        {
            "Mushroom Cup", "Flower Cup", "Star Cup", "Special Cup",
            "Shell Cup", "Banana Cup", "Leaf Cup", "Lightning Cup"
        };

        private TableLayoutPanel Root;
        private Panel PageHost;
        private Label HeaderTitle;
        private Label HeaderSubtitle;
        private Label HeaderReadyLabel;
        private Label HeaderVersionLabel;
        private readonly List<Button> NavButtons = new List<Button>();
        private RichTextBox LogBox;

        private TextBox TxtPackName;
        private TextBox TxtPackId;
        private TextBox TxtAuthor;
        private TextBox TxtPackVersion;
        private TextBox TxtDescription;
        private Label DashboardReadyLabel;
        private Label DashboardAssetLabel;

        private DataGridView SlotGrid;
        private DataGridView MusicGrid;
        private DataGridView CharacterGrid;
        private DataGridView UiRelGrid;
        private ComboBox ExportModeCombo;
        private TextBox OutputFolderText;
        private Label ExportEstimateLabel;
        private CheckBox CopyMultiplayerCheck;
        private CheckBox CommunityFallbackCheck;

        private ListBox CupList;
        private PictureBox CupPreview;
        private Label CupPreviewTitle;
        private ListView IconList;
        private ImageList IconImageList;
        private readonly Dictionary<string, Image> CharacterIconCache = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        private TextBox PatchBaseUiFolderText;
        private TextBox PatchOutputFolderText;
        private TextBox PatchBrawlCrateText;
        private TextBox PatchWbmgtText;
        private TextBox PatchWszstText;
        private ComboBox PatchLanguageCombo;
        private CheckBox PatchAwardCheck;
        private RichTextBox PatchLogBox;
        private RichTextBox SetupStatusBox;

        public MainForm()
        {
            AppRoot = Application.StartupPath;
            InitializeSlots();
            InitializeCupIcons();
            BuildWindow();
            ShowDashboard();
            AddLog("Ready. Build: " + AppVersion + " | simple one-click HNS pack workflow.");
            // Show the first-run tutorial only after the form has a real window handle.
            // Calling BeginInvoke from the constructor can fail before the handle exists.
            Shown += delegate
            {
                try { ShowFirstRunTutorialIfNeeded(); }
                catch (Exception ex) { AddLog("First-run tutorial skipped: " + ex.Message); }
            };
        }

        private void InitializeSlots()
        {
            Slots.Clear();
            Slots.Add(new TrackSlotInfo("Race", "Mushroom Cup", "Luigi Circuit", "beginner_course.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Mushroom Cup", "Moo Moo Meadows", "farm_course.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Mushroom Cup", "Mushroom Gorge", "kinoko_course.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Mushroom Cup", "Toad's Factory", "factory_course.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Flower Cup", "Mario Circuit", "castle_course.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Flower Cup", "Coconut Mall", "shopping_course.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Flower Cup", "DK Summit", "boardcross_course.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Flower Cup", "Wario's Gold Mine", "truck_course.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Star Cup", "Daisy Circuit", "senior_course.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Star Cup", "Koopa Cape", "water_course.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Star Cup", "Maple Treeway", "treehouse_course.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Star Cup", "Grumble Volcano", "volcano_course.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Special Cup", "Dry Dry Ruins", "desert_course.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Special Cup", "Moonview Highway", "ridgehighway_course.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Special Cup", "Bowser's Castle", "koopa_course.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Special Cup", "Rainbow Road", "rainbow_course.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Shell Cup", "GCN Peach Beach", "old_peach_gc.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Shell Cup", "DS Yoshi Falls", "old_falls_ds.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Shell Cup", "SNES Ghost Valley 2", "old_obake_sfc.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Shell Cup", "N64 Mario Raceway", "old_mario_64.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Banana Cup", "N64 Sherbet Land", "old_sherbet_64.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Banana Cup", "GBA Shy Guy Beach", "old_heyho_gba.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Banana Cup", "DS Delfino Square", "old_town_ds.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Banana Cup", "GCN Waluigi Stadium", "old_waluigi_gc.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Leaf Cup", "DS Desert Hills", "old_desert_ds.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Leaf Cup", "GBA Bowser Castle 3", "old_koopa_gba.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Leaf Cup", "N64 DK's Jungle Parkway", "old_donkey_64.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Leaf Cup", "GCN Mario Circuit", "old_mario_gc.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Lightning Cup", "SNES Mario Circuit 3", "old_mario_sfc.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Lightning Cup", "DS Peach Gardens", "old_garden_ds.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Lightning Cup", "GCN DK Mountain", "old_donkey_gc.szs"));
            Slots.Add(new TrackSlotInfo("Race", "Lightning Cup", "N64 Bowser's Castle", "old_koopa_64.szs"));
        }

        private void InitializeCupIcons()
        {
            CupIconPaths.Clear();
            foreach (string cup in CupNames) CupIconPaths[cup] = "";
        }

        private void BuildWindow()
        {
            Text = "MKWii Pack Maker";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1280, 760);
            Size = new Size(1500, 900);
            BackColor = Bg;
            ForeColor = TextMain;
            AutoScaleMode = AutoScaleMode.Dpi;
            DoubleBuffered = true;

            string ico = Path.Combine(AppRoot, "assets", "app.ico");
            if (File.Exists(ico)) { try { Icon = new Icon(ico); } catch { } }

            Root = new TableLayoutPanel();
            Root.Dock = DockStyle.Fill;
            Root.BackColor = Bg;
            Root.ColumnCount = 2;
            Root.RowCount = 1;
            Root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 252));
            Root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Controls.Add(Root);

            BuildSidebar();
            BuildMainShell();
        }

        private void BuildSidebar()
        {
            Panel side = new Panel();
            side.Dock = DockStyle.Fill;
            side.BackColor = Sidebar;
            Root.Controls.Add(side, 0, 0);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.BackColor = Sidebar;
            layout.ColumnCount = 1;
            layout.RowCount = 3;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 155));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            side.Controls.Add(layout);

            Panel brand = new Panel();
            brand.Dock = DockStyle.Fill;
            brand.Padding = new Padding(26, 24, 20, 10);
            brand.BackColor = Sidebar;
            layout.Controls.Add(brand, 0, 0);

            Label title = new Label();
            title.Text = "MKWii\nPack Maker";
            title.ForeColor = TextMain;
            title.Font = new Font("Segoe UI Semibold", 22f, FontStyle.Bold);
            title.Location = new Point(25, 23);
            title.Size = new Size(210, 78);
            brand.Controls.Add(title);

            Label sub = new Label();
            sub.Text = "HNS toolkit";
            sub.ForeColor = TextMuted;
            sub.Font = new Font("Segoe UI", 10f);
            sub.Location = new Point(28, 104);
            sub.Size = new Size(205, 26);
            brand.Controls.Add(sub);

            FlowLayoutPanel nav = new FlowLayoutPanel();
            nav.Dock = DockStyle.Fill;
            nav.FlowDirection = FlowDirection.TopDown;
            nav.WrapContents = false;
            nav.BackColor = Sidebar;
            nav.Padding = new Padding(0, 12, 0, 0);
            layout.Controls.Add(nav, 0, 1);

            AddNav(nav, "Dashboard", ShowDashboard);
            AddNav(nav, "Tracks", ShowTrackSlots);
            AddNav(nav, "Music", delegate { ShowAssetPage("Music"); });
            AddNav(nav, "Characters", delegate { ShowAssetPage("Characters"); });
            AddNav(nav, "Advanced Files", delegate { ShowAssetPage("UI / REL Assets"); });
            AddNav(nav, "Cup Icons", ShowCupIcons);
            AddNav(nav, "Setup", ShowUiPatcher);
            AddNav(nav, "Export", ShowExportBuilder);
            AddNav(nav, "Logs", ShowLogs);

            Label ver = new Label();
            ver.Text = AppVersion;
            ver.ForeColor = TextMuted;
            ver.Font = new Font("Segoe UI", 9f);
            ver.Dock = DockStyle.Fill;
            ver.TextAlign = ContentAlignment.MiddleCenter;
            layout.Controls.Add(ver, 0, 2);
        }

        private void AddNav(FlowLayoutPanel nav, string text, Action action)
        {
            Button btn = CreateNavButton(text);
            btn.Click += delegate { SaveMetadataFromFields(); action(); };
            nav.Controls.Add(btn);
        }

        private Button CreateNavButton(string text)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Width = 252;
            btn.Height = 46;
            btn.Margin = new Padding(0, 0, 0, 7);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Sidebar;
            btn.ForeColor = TextMuted;
            btn.Font = new Font("Segoe UI Semibold", 10.3f, FontStyle.Bold);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(30, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            NavButtons.Add(btn);
            return btn;
        }

        private void SetActiveNav(string text)
        {
            foreach (Button btn in NavButtons)
            {
                bool active = btn.Text == text;
                btn.BackColor = active ? Accent : Sidebar;
                btn.ForeColor = active ? Color.White : TextMuted;
            }
        }

        private void BuildMainShell()
        {
            TableLayoutPanel main = new TableLayoutPanel();
            main.Dock = DockStyle.Fill;
            main.BackColor = Bg;
            main.ColumnCount = 1;
            main.RowCount = 2;
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 126));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Root.Controls.Add(main, 1, 0);

            Panel header = new Panel();
            header.Dock = DockStyle.Fill;
            header.BackColor = Bg2;
            header.Padding = new Padding(34, 20, 34, 20);
            string heroPath = Path.Combine(AppRoot, "assets", "theme", "brand_hero.png");
            if (File.Exists(heroPath))
            {
                try { using (Image hero = Image.FromFile(heroPath)) header.BackgroundImage = new Bitmap(hero); header.BackgroundImageLayout = ImageLayout.Stretch; } catch { }
            }
            main.Controls.Add(header, 0, 0);

            HeaderTitle = new Label();
            HeaderTitle.ForeColor = TextMain;
            HeaderTitle.Font = new Font("Segoe UI Semibold", 22f, FontStyle.Bold);
            HeaderTitle.Location = new Point(34, 21);
            HeaderTitle.Size = new Size(720, 42);
            header.Controls.Add(HeaderTitle);

            HeaderSubtitle = new Label();
            HeaderSubtitle.ForeColor = TextMuted;
            HeaderSubtitle.Font = new Font("Segoe UI", 10.5f);
            HeaderSubtitle.Location = new Point(36, 67);
            HeaderSubtitle.Size = new Size(850, 28);
            header.Controls.Add(HeaderSubtitle);

            HeaderReadyLabel = new Label();
            HeaderReadyLabel.ForeColor = TextMain;
            HeaderReadyLabel.Font = new Font("Segoe UI Semibold", 13f, FontStyle.Bold);
            HeaderReadyLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            HeaderReadyLabel.Size = new Size(320, 28);
            HeaderReadyLabel.TextAlign = ContentAlignment.MiddleRight;
            header.Controls.Add(HeaderReadyLabel);

            HeaderVersionLabel = new Label();
            HeaderVersionLabel.ForeColor = TextMuted;
            HeaderVersionLabel.Font = new Font("Segoe UI", 9f);
            HeaderVersionLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            HeaderVersionLabel.Size = new Size(320, 24);
            HeaderVersionLabel.TextAlign = ContentAlignment.MiddleRight;
            HeaderVersionLabel.Text = AppVersion;
            header.Controls.Add(HeaderVersionLabel);

            header.Resize += delegate
            {
                HeaderReadyLabel.Left = Math.Max(560, header.ClientSize.Width - HeaderReadyLabel.Width - 36);
                HeaderReadyLabel.Top = 32;
                HeaderVersionLabel.Left = Math.Max(560, header.ClientSize.Width - HeaderVersionLabel.Width - 36);
                HeaderVersionLabel.Top = 64;
            };

            PageHost = new Panel();
            PageHost.Dock = DockStyle.Fill;
            PageHost.Padding = new Padding(34);
            PageHost.BackColor = Bg;
            main.Controls.Add(PageHost, 0, 1);
        }

        private void SetHeader(string title, string subtitle)
        {
            HeaderTitle.Text = title;
            HeaderSubtitle.Text = subtitle;
            UpdateReadyLabels();
        }

        private void ClearPage()
        {
            PageHost.Controls.Clear();
        }


        private void ShowFirstRunTutorialIfNeeded()
        {
            try
            {
                string data = Path.Combine(AppRoot, "Data");
                Directory.CreateDirectory(data);
                string marker = Path.Combine(data, ".first_run_tutorial_seen_10_3_5");
                if (File.Exists(marker)) return;
                ShowGettingStartedTutorial(true);
                File.WriteAllText(marker, DateTime.Now.ToString("u"));
            }
            catch { }
        }

        private void ShowGettingStartedTutorial(bool firstRun)
        {
            using (Form dialog = new Form())
            {
                dialog.Text = firstRun ? "First Run Setup - MKWii Pack Maker" : "Setup Tutorial - MKWii Pack Maker";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.Size = new Size(980, 720);
                dialog.MinimumSize = new Size(880, 640);
                dialog.BackColor = Bg;
                dialog.ForeColor = TextMain;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowIcon = true;
                try
                {
                    string ico = Path.Combine(AppRoot, "assets", "app.ico");
                    if (File.Exists(ico)) dialog.Icon = new Icon(ico);
                }
                catch { }

                TableLayoutPanel root = new TableLayoutPanel();
                root.Dock = DockStyle.Fill;
                root.BackColor = Bg;
                root.Padding = new Padding(22);
                root.ColumnCount = 1;
                root.RowCount = 4;
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
                dialog.Controls.Add(root);

                Panel header = new Panel();
                header.Dock = DockStyle.Fill;
                header.BackColor = Bg;
                root.Controls.Add(header, 0, 0);

                Label title = new Label();
                title.Text = "First-time setup";
                title.Dock = DockStyle.Top;
                title.Height = 42;
                title.ForeColor = TextMain;
                title.Font = new Font("Segoe UI Semibold", 24f, FontStyle.Bold);
                header.Controls.Add(title);

                Label subtitle = new Label();
                subtitle.Text = "Each user adds their own Mario Kart Wii base files once. After that: drop tracks, characters, music, icons, then export.";
                subtitle.Dock = DockStyle.Top;
                subtitle.Height = 30;
                subtitle.ForeColor = TextMuted;
                subtitle.Font = new Font("Segoe UI", 10.5f);
                header.Controls.Add(subtitle);

                TableLayoutPanel body = new TableLayoutPanel();
                body.Dock = DockStyle.Fill;
                body.BackColor = Bg;
                body.ColumnCount = 2;
                body.RowCount = 1;
                body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
                body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
                root.Controls.Add(body, 0, 1);

                RoundedPanel stepsCard = CreateCard();
                stepsCard.Margin = new Padding(0, 0, 14, 0);
                stepsCard.Padding = new Padding(22);
                body.Controls.Add(stepsCard, 0, 0);

                RichTextBox steps = new RichTextBox();
                steps.Dock = DockStyle.Fill;
                steps.ReadOnly = true;
                steps.BorderStyle = BorderStyle.None;
                steps.BackColor = Card;
                steps.ForeColor = TextMain;
                steps.Font = new Font("Segoe UI", 10.2f);
                stepsCard.Controls.Add(steps);
                steps.AppendText("WHAT YOU NEED\n");
                steps.SelectionStart = 0;
                steps.SelectionLength = "WHAT YOU NEED".Length;
                steps.SelectionFont = new Font("Segoe UI Semibold", 13f, FontStyle.Bold);
                steps.SelectionColor = AccentGreen;
                steps.SelectionStart = steps.TextLength;
                steps.SelectionLength = 0;
                steps.SelectionFont = new Font("Segoe UI", 10.2f);
                steps.SelectionColor = TextMain;
                steps.AppendText("\nYou need your own legally dumped Mario Kart Wii WBFS/ISO or an already extracted MKWii dump. Do not include Nintendo base files when you share the app.\n\n");
                steps.AppendText("FAST METHOD - Let this app extract the required files\n");
                steps.AppendText("1. Go to Setup.\n2. Click Extract WBFS/ISO.\n3. Choose your own Mario Kart Wii .wbfs or .iso.\n4. The app uses tools\\wit.exe and copies only the required base folders into base_files.\n\n");
                steps.SelectionFont = new Font("Consolas", 9.5f, FontStyle.Bold);
                steps.SelectionColor = Color.FromArgb(255, 224, 140);
                steps.AppendText("Required folders copied automatically:\n");
                steps.AppendText("Scene\\UI        -> base_files\\Scene\\UI\n");
                steps.AppendText("Scene\\Model     -> base_files\\Scene\\Model\n");
                steps.AppendText("Race\\Kart       -> base_files\\Race\\Kart\n");
                steps.AppendText("Race\\Course     -> base_files\\Race\\Course\n");
                steps.AppendText("Sound            -> base_files\\Sound\n\n");
                steps.SelectionFont = new Font("Segoe UI", 10.2f);
                steps.SelectionColor = TextMain;
                steps.AppendText("MANUAL METHOD - If you already extracted the game\n");
                steps.AppendText("Open your extracted dump's files folder, then copy the same folders above into this app's base_files folder.\n\n");
                steps.AppendText("USE THE APP\n");
                steps.AppendText("Tracks: add .szs and type custom names.\nCharacters: import complete character packs.\nMusic: add .brstm if your track download includes music.\nCup Icons: click a cup, click an icon, or Reset Cup to keep original.\nExport: one click builds the HNS/Riivolution pack.\n");

                RoundedPanel statusCard = CreateCard();
                statusCard.Margin = new Padding(14, 0, 0, 0);
                statusCard.Padding = new Padding(22);
                body.Controls.Add(statusCard, 1, 0);

                TableLayoutPanel statusLayout = new TableLayoutPanel();
                statusLayout.Dock = DockStyle.Fill;
                statusLayout.BackColor = Card;
                statusLayout.ColumnCount = 1;
                statusLayout.RowCount = 3;
                statusLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
                statusLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                statusLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
                statusCard.Controls.Add(statusLayout);

                Label statusTitle = CreateSectionTitle("Base file detection");
                statusLayout.Controls.Add(statusTitle, 0, 0);

                RichTextBox statusBox = new RichTextBox();
                statusBox.Dock = DockStyle.Fill;
                statusBox.ReadOnly = true;
                statusBox.BorderStyle = BorderStyle.None;
                statusBox.BackColor = Color.FromArgb(16, 20, 29);
                statusBox.ForeColor = TextMain;
                statusBox.Font = new Font("Consolas", 9.4f);
                statusLayout.Controls.Add(statusBox, 0, 1);

                Action refreshStatus = delegate
                {
                    FillBaseFileStatusBox(statusBox);
                };
                refreshStatus();

                FlowLayoutPanel statusButtons = new FlowLayoutPanel();
                statusButtons.Dock = DockStyle.Fill;
                statusButtons.BackColor = Card;
                statusButtons.Padding = new Padding(0, 16, 0, 0);
                statusLayout.Controls.Add(statusButtons, 0, 2);
                statusButtons.Controls.Add(CreateActionButton("Refresh", Accent, delegate { refreshStatus(); }));
                statusButtons.Controls.Add(CreateActionButton("Open base_files", Color.FromArgb(76, 86, 108), delegate { OpenBaseFilesFolder(); }));

                RoundedPanel warningCard = CreateCard();
                warningCard.Margin = new Padding(0, 16, 0, 0);
                warningCard.Padding = new Padding(20, 14, 20, 12);
                warningCard.BackColor = Color.FromArgb(35, 27, 18);
                warningCard.BorderColor = Color.FromArgb(120, 85, 40);
                root.Controls.Add(warningCard, 0, 2);

                Label warning = new Label();
                warning.Dock = DockStyle.Fill;
                warning.ForeColor = Color.FromArgb(255, 222, 158);
                warning.Font = new Font("Segoe UI Semibold", 10.2f, FontStyle.Bold);
                warning.Text = "Important: do not distribute Nintendo base_files with the public app. Share the tool with an empty base_files folder. Each user must provide their own MKWii dump or extract base files from their own WBFS/ISO.";
                warningCard.Controls.Add(warning);

                FlowLayoutPanel bottom = new FlowLayoutPanel();
                bottom.Dock = DockStyle.Fill;
                bottom.FlowDirection = FlowDirection.RightToLeft;
                bottom.BackColor = Bg;
                bottom.Padding = new Padding(0, 12, 0, 0);
                root.Controls.Add(bottom, 0, 3);

                Button start = CreateActionButton("Got it", AccentGreen, delegate { dialog.Close(); });
                Button openSetup = CreateActionButton("Go to Setup", Accent, delegate { dialog.Close(); ShowUiPatcher(); });
                Button extractNow = CreateActionButton("Extract WBFS/ISO", AccentGreen, delegate { dialog.Close(); ExtractRequiredBaseFilesFromDiscImage(); });
                Button openBase = CreateActionButton("Open base_files", Color.FromArgb(76, 86, 108), delegate { OpenBaseFilesFolder(); });
                bottom.Controls.Add(start);
                bottom.Controls.Add(openSetup);
                bottom.Controls.Add(extractNow);
                bottom.Controls.Add(openBase);

                try { dialog.ShowDialog(this); }
                catch { dialog.ShowDialog(); }
            }
        }


        private void ShowReleaseSafetyCheck()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("GitHub / public release safety check");
            sb.AppendLine("====================================");
            sb.AppendLine();
            int issues = 0;
            Action<string> warn = delegate(string text) { issues++; sb.AppendLine("WARN - " + text); };
            Action<string> ok = delegate(string text) { sb.AppendLine("OK   - " + text); };

            string baseRoot = Path.Combine(AppRoot, "base_files");
            if (Directory.Exists(Path.Combine(baseRoot, "Scene"))) warn("base_files/Scene exists. Do not publish extracted Nintendo UI/model files."); else ok("base_files/Scene not present.");
            if (Directory.Exists(Path.Combine(baseRoot, "Race"))) warn("base_files/Race exists. Do not publish extracted Nintendo course/kart files."); else ok("base_files/Race not present.");
            if (Directory.Exists(Path.Combine(baseRoot, "Sound")) || Directory.Exists(Path.Combine(baseRoot, "sound"))) warn("base_files/Sound exists. Do not publish extracted Nintendo sound files."); else ok("base_files/Sound not present.");

            string[] riskyExt = new string[] { "*.wbfs", "*.iso", "*.szs", "*.brsar", "*.brres" };
            foreach (string pattern in riskyExt)
            {
                int count = 0;
                try { count = Directory.Exists(AppRoot) ? Directory.GetFiles(AppRoot, pattern, SearchOption.AllDirectories).Count(f => !f.Contains("\\bin\\") && !f.Contains("/bin/") && !f.Contains("\\obj\\") && !f.Contains("/obj/")) : 0; } catch { }
                if ((pattern == "*.szs" || pattern == "*.brsar" || pattern == "*.brres") && count > 0) warn("Found " + count + " " + pattern + " file(s). Verify they are tools/test-safe and not Nintendo base files.");
                else if ((pattern == "*.wbfs" || pattern == "*.iso") && count > 0) warn("Found " + count + " disc image file(s). Never publish WBFS/ISO files.");
                else ok("No " + pattern + " files found under app folder.");
            }

            if (Directory.Exists(Path.Combine(AppRoot, "Data"))) warn("Data folder exists. Clean it before committing source."); else ok("Data folder not present.");
            if (Directory.Exists(Path.Combine(AppRoot, "release")) || Directory.Exists(Path.Combine(AppRoot, "release_portable"))) warn("release/release_portable folder exists. Upload built ZIPs to GitHub Releases, not source."); else ok("release folders not present.");
            if (Directory.Exists(Path.Combine(AppRoot, "build_publish"))) warn("build_publish folder exists. Remove old compiled output from source."); else ok("build_publish not present.");

            string iconDir = Path.Combine(AppRoot, "assets", "character_icons");
            int iconCount = 0;
            try { if (Directory.Exists(iconDir)) iconCount = Directory.GetFiles(iconDir, "*.png", SearchOption.TopDirectoryOnly).Length; } catch { }
            if (iconCount > 0) warn("assets/character_icons contains " + iconCount + " PNG icon(s). Only publish artwork you have rights to share."); else ok("No local character icon PNGs found.");

            sb.AppendLine();
            sb.AppendLine(issues == 0 ? "READY - No obvious public-release problems were found." : "CHECK - Fix or intentionally review " + issues + " item(s) before making the repo public.");
            ShowLargeTextDialog("Release Safety Check", sb.ToString());
        }

        private void OpenBaseFilesFolder()
        {
            try
            {
                string folder = Path.Combine(AppRoot, "base_files");
                Directory.CreateDirectory(folder);
                Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true });
            }
            catch { }
        }

        private async void ExtractRequiredBaseFilesFromDiscImage()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Choose your Mario Kart Wii WBFS or ISO";
            ofd.Filter = "Wii disc images (*.wbfs;*.iso;*.wdf;*.wia;*.ciso)|*.wbfs;*.iso;*.wdf;*.wia;*.ciso|All files (*.*)|*.*";
            if (ofd.ShowDialog(this) != DialogResult.OK) return;

            string imagePath = ofd.FileName;
            string wit = FindToolOnPath("wit.exe");
            if (string.IsNullOrWhiteSpace(wit) || !File.Exists(wit))
            {
                MessageBox.Show(this,
                    "tools\\wit.exe was not found. Put wit.exe beside the other Wiimms tools in the tools folder, then try again.",
                    "wit.exe missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                WritePatchLog("Base extractor stopped: tools/wit.exe not found.");
                return;
            }

            string baseRoot = Path.Combine(AppRoot, "base_files");
            Directory.CreateDirectory(baseRoot);
            if (DirectoryHasUserBaseFiles(baseRoot))
            {
                DialogResult replace = MessageBox.Show(this,
                    "base_files already contains files. Extracting will overwrite matching required folders only:\n\n" +
                    "Scene/UI, Scene/Model, Race/Kart, Race/Course, Sound\n\nContinue?",
                    "Overwrite required base folders?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (replace != DialogResult.Yes) return;
            }

            string tempRoot = Path.Combine(AppRoot, "Data", "DiscExtract", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            string output = "";
            bool ok = false;
            Cursor oldCursor = Cursor.Current;
            OperationProgressDialog progress = null;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                progress = new OperationProgressDialog("Extracting MKWii base files");
                progress.Show(this);
                progress.SetProgress("Preparing WIT extraction...", 5, false);
                progress.AddLine("Selected image: " + imagePath);
                WritePatchLog("Starting base file extraction from: " + imagePath);
                WritePatchLog("Using wit.exe: " + wit);
                WritePatchLog("Temporary extraction folder: " + tempRoot);
                Directory.CreateDirectory(tempRoot);
                progress.SetProgress("Extracting required folders with wit.exe. This may take a few minutes...", 15, true);
                progress.AddLine("Extracting only Scene/UI, Scene/Model, Race/Kart, Race/Course, and Sound.");
                Application.DoEvents();

                ok = await Task.Run(delegate
                {
                    string arguments = BuildRequiredBaseWitArguments(imagePath, tempRoot);
                    string toolOutput;
                    bool result = RunTool(wit, arguments, out toolOutput);
                    output = toolOutput;
                    return result;
                });
                progress.SetProgress("WIT finished. Checking extracted folders...", 65, false);
                progress.AddLine("WIT finished. Verifying required folder layout.");
                Application.DoEvents();

                if (!ok)
                {
                    WritePatchLog("wit.exe failed. Output: " + output);
                    MessageBox.Show(this,
                        "wit.exe could not extract the selected image. Make sure it is a valid Mario Kart Wii WBFS/ISO dump.\n\n" + output,
                        "Extraction failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string extractedFilesRoot = FindExtractedFilesRoot(tempRoot);
                if (string.IsNullOrWhiteSpace(extractedFilesRoot))
                {
                    WritePatchLog("wit.exe finished, but the required MKWii folders were not found. Output: " + output);
                    MessageBox.Show(this,
                        "Extraction finished, but the required Mario Kart Wii folders were not found. This may not be a valid MKWii disc image, or the file filter did not match this image layout.",
                        "Required folders not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                progress.SetProgress("Copying required folders into base_files...", 78, false);
                progress.AddLine("Copying required folders into base_files.");
                Application.DoEvents();
                CopyRequiredBaseFolders(extractedFilesRoot, baseRoot);
                progress.SetProgress("Refreshing setup checks...", 92, false);
                if (SetupStatusBox != null && !SetupStatusBox.IsDisposed) FillBaseFileStatusBox(SetupStatusBox);
                WritePatchLog("Base file extraction complete. Required folders copied into: " + baseRoot);
                progress.SetProgress("Base extraction complete.", 100, false);
                progress.AddLine("Done. Required folders were copied into: " + baseRoot);
                Application.DoEvents();
                MessageBox.Show(this,
                    "Required base files were extracted and copied into base_files.\n\nThe app is ready to build packs if all checks are green.",
                    "Base files ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                WritePatchLog("Base extractor error: " + ex.Message);
                MessageBox.Show(this, "Base extractor error:\n\n" + ex.Message, "Extraction error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                try
                {
                    if (progress != null && !progress.IsDisposed)
                    {
                        progress.SetProgress("Finished.", 100, false);
                        progress.Close();
                    }
                }
                catch { }
                Cursor.Current = oldCursor;
            }
        }

        private string BuildRequiredBaseWitArguments(string imagePath, string tempRoot)
        {
            string[] rules = new string[]
            {
                "+/files/Scene/UI/*",
                "+/files/Scene/Model/*",
                "+/files/Scene/Model/Kart/*",
                "+/files/Race/Kart/*",
                "+/files/Race/Course/*",
                "+/files/Sound/*",
                "+/files/Sound/strm/*",
                "+/files/sound/*",
                "+/files/sound/strm/*"
            };

            StringBuilder sb = new StringBuilder();
            sb.Append("EXTRACT --psel DATA --pmode AUTO --overwrite ");
            foreach (string rule in rules)
            {
                sb.Append("--files ");
                sb.Append(Quote(rule));
                sb.Append(" ");
            }
            sb.Append(Quote(imagePath));
            sb.Append(" ");
            sb.Append(Quote(tempRoot));
            return sb.ToString();
        }

        private bool DirectoryHasUserBaseFiles(string baseRoot)
        {
            try
            {
                if (!Directory.Exists(baseRoot)) return false;
                string[] entries = Directory.GetFileSystemEntries(baseRoot);
                foreach (string entry in entries)
                {
                    string name = Path.GetFileName(entry);
                    if (name.Equals("README.txt", StringComparison.OrdinalIgnoreCase)) continue;
                    if (name.Equals("PUT_YOUR_BASE_FILES_HERE.txt", StringComparison.OrdinalIgnoreCase)) continue;
                    return true;
                }
            }
            catch { }
            return false;
        }

        private string FindExtractedFilesRoot(string tempRoot)
        {
            try
            {
                List<string> candidates = new List<string>();
                candidates.Add(Path.Combine(tempRoot, "files"));
                candidates.Add(tempRoot);
                if (Directory.Exists(tempRoot))
                {
                    candidates.AddRange(Directory.GetDirectories(tempRoot, "files", SearchOption.AllDirectories));
                    candidates.AddRange(Directory.GetDirectories(tempRoot, "RMCE*", SearchOption.TopDirectoryOnly).Select(x => Path.Combine(x, "files")));
                    candidates.AddRange(Directory.GetDirectories(tempRoot, "RMCP*", SearchOption.TopDirectoryOnly).Select(x => Path.Combine(x, "files")));
                    candidates.AddRange(Directory.GetDirectories(tempRoot, "RMCJ*", SearchOption.TopDirectoryOnly).Select(x => Path.Combine(x, "files")));
                    candidates.AddRange(Directory.GetDirectories(tempRoot, "RMCK*", SearchOption.TopDirectoryOnly).Select(x => Path.Combine(x, "files")));
                }

                foreach (string candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(candidate) || !Directory.Exists(candidate)) continue;
                    bool hasUi = Directory.Exists(Path.Combine(candidate, "Scene", "UI"));
                    bool hasModel = Directory.Exists(Path.Combine(candidate, "Scene", "Model"));
                    bool hasKart = Directory.Exists(Path.Combine(candidate, "Race", "Kart"));
                    bool hasCourse = Directory.Exists(Path.Combine(candidate, "Race", "Course"));
                    bool hasSound = Directory.Exists(Path.Combine(candidate, "Sound")) || Directory.Exists(Path.Combine(candidate, "sound"));
                    if (hasUi || hasModel || hasKart || hasCourse || hasSound) return candidate;
                }
            }
            catch { }
            return "";
        }

        private void CopyRequiredBaseFolders(string extractedFilesRoot, string baseRoot)
        {
            CopyRequiredFolder(extractedFilesRoot, baseRoot, Path.Combine("Scene", "UI"));
            CopyRequiredFolder(extractedFilesRoot, baseRoot, Path.Combine("Scene", "Model"));
            CopyRequiredFolder(extractedFilesRoot, baseRoot, Path.Combine("Race", "Kart"));
            CopyRequiredFolder(extractedFilesRoot, baseRoot, Path.Combine("Race", "Course"));
            CopyRequiredFolder(extractedFilesRoot, baseRoot, "Sound");
        }

        private void CopyRequiredFolder(string sourceRoot, string destRoot, string relativeFolder)
        {
            string source = Path.Combine(sourceRoot, relativeFolder);
            string dest = Path.Combine(destRoot, relativeFolder);
            if (!Directory.Exists(source)) source = FindDirectoryIgnoreCase(sourceRoot, relativeFolder);
            if (string.IsNullOrWhiteSpace(source) || !Directory.Exists(source))
            {
                WritePatchLog("Missing after extraction: " + relativeFolder);
                return;
            }
            CopyDirectoryRecursive(source, dest);
            WritePatchLog("Copied: " + relativeFolder);
        }


        private string FindDirectoryIgnoreCase(string root, string relativeFolder)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return "";
                string current = root;
                string[] parts = relativeFolder.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    string next = Directory.GetDirectories(current).FirstOrDefault(d => string.Equals(Path.GetFileName(d), part, StringComparison.OrdinalIgnoreCase));
                    if (string.IsNullOrWhiteSpace(next)) return "";
                    current = next;
                }
                return current;
            }
            catch { return ""; }
        }

        private void CopyDirectoryRecursive(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string dest = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, dest, true);
            }
            foreach (string dir in Directory.GetDirectories(sourceDir))
            {
                CopyDirectoryRecursive(dir, Path.Combine(destDir, Path.GetFileName(dir)));
            }
        }

        private void FillBaseFileStatusBox(RichTextBox box)
        {
            box.Clear();
            string baseRoot = Path.Combine(AppRoot, "base_files");
            AppendStatusLine(box, Directory.Exists(baseRoot), "base_files folder", baseRoot);
            AppendStatusLine(box, SafeTopFileCount(Path.Combine(baseRoot, "Scene", "UI"), "*.szs") > 0, "Scene/UI .szs files", "Needed for cup icons, character icons, and UI.");
            AppendStatusLine(box, File.Exists(Path.Combine(baseRoot, "Scene", "Model", "Driver.szs")), "Scene/Model/Driver.szs", "Needed for character select models.");
            AppendStatusLine(box, SafeTopFileCount(Path.Combine(baseRoot, "Scene", "Model", "Kart"), "*.szs") > 0, "Scene/Model/Kart .szs files", "Needed for vehicle select models.");
            AppendStatusLine(box, SafeTopFileCount(Path.Combine(baseRoot, "Race", "Kart"), "*.szs") > 0, "Race/Kart .szs files", "Needed for in-race character/kart files.");
            AppendStatusLine(box, SafeTopFileCount(Path.Combine(baseRoot, "Race", "Course"), "*.szs") > 0, "Race/Course .szs files", "Optional fallback/reference tracks.");
            AppendStatusLine(box, Directory.Exists(Path.Combine(baseRoot, "Sound")), "Sound folder", "Needed if you use custom voices/sound.");

            bool ready = IsBaseFilesReady();
            box.AppendText("\n");
            box.SelectionColor = ready ? AccentGreen : Warn;
            box.SelectionFont = new Font("Consolas", 10f, FontStyle.Bold);
            box.AppendText(ready ? "READY: Required base files detected." : "NOT READY: Some required base folders/files are missing.");
            box.SelectionFont = new Font("Consolas", 9.4f, FontStyle.Regular);
            box.SelectionColor = TextMain;
        }

        private int SafeTopFileCount(string folder, string pattern)
        {
            try
            {
                if (!Directory.Exists(folder)) return 0;
                return Directory.GetFiles(folder, pattern, SearchOption.TopDirectoryOnly).Length;
            }
            catch { return 0; }
        }

        private void AppendStatusLine(RichTextBox box, bool ok, string name, string note)
        {
            box.SelectionColor = ok ? AccentGreen : Danger;
            box.SelectionFont = new Font("Consolas", 9.4f, FontStyle.Bold);
            box.AppendText(ok ? "[OK]   " : "[MISS] ");
            box.SelectionColor = TextMain;
            box.AppendText(name + "\n");
            box.SelectionColor = TextMuted;
            box.SelectionFont = new Font("Consolas", 8.7f, FontStyle.Regular);
            box.AppendText("       " + note + "\n\n");
        }

        private bool IsBaseFilesReady()
        {
            try
            {
                string baseRoot = Path.Combine(AppRoot, "base_files");
                if (!Directory.Exists(baseRoot)) return false;
                if (SafeTopFileCount(Path.Combine(baseRoot, "Scene", "UI"), "*.szs") == 0) return false;
                if (!File.Exists(Path.Combine(baseRoot, "Scene", "Model", "Driver.szs"))) return false;
                if (SafeTopFileCount(Path.Combine(baseRoot, "Scene", "Model", "Kart"), "*.szs") == 0) return false;
                if (SafeTopFileCount(Path.Combine(baseRoot, "Race", "Kart"), "*.szs") == 0) return false;
                return true;
            }
            catch { return false; }
        }

        private void ShowDashboard()
        {
            SetActiveNav("Dashboard");
            SetHeader("Dashboard", "Set the Riivolution option name, setup base files, drop tracks/characters/music/icons, then export.");
            ClearPage();

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.BackColor = Bg;
            layout.ColumnCount = 2;
            layout.RowCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            PageHost.Controls.Add(layout);

            RoundedPanel metaCard = CreateCard();
            metaCard.Padding = new Padding(26);
            layout.Controls.Add(metaCard, 0, 0);

            TableLayoutPanel meta = new TableLayoutPanel();
            meta.Dock = DockStyle.Fill;
            meta.BackColor = Card;
            meta.ColumnCount = 2;
            meta.RowCount = 7;
            meta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            meta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            meta.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            meta.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            meta.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            meta.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            meta.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            meta.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
            meta.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            metaCard.Controls.Add(meta);

            Label cardTitle = CreateSectionTitle("Pack Info");
            meta.Controls.Add(cardTitle, 0, 0);
            meta.SetColumnSpan(cardTitle, 2);

            TxtPackName = CreateTextBox(PackName); AddLabeled(meta, "Riivolution Option Name", TxtPackName, 0, 1);
            TxtPackId = CreateTextBox(PackId); AddLabeled(meta, "Pack ID / Folder Name", TxtPackId, 1, 1);
            TxtAuthor = CreateTextBox(Author); AddLabeled(meta, "Author", TxtAuthor, 0, 2);
            TxtPackVersion = CreateTextBox(PackVersion); AddLabeled(meta, "Pack Version", TxtPackVersion, 1, 2);
            TxtDescription = CreateMultiTextBox(Description); AddLabeled(meta, "Description / Pack Rules", TxtDescription, 0, 3, 2, 1);

            FlowLayoutPanel actions = new FlowLayoutPanel();
            actions.Dock = DockStyle.Fill;
            actions.FlowDirection = FlowDirection.LeftToRight;
            actions.BackColor = Card;
            actions.Padding = new Padding(0, 12, 0, 0);
            meta.Controls.Add(actions, 0, 4);
            meta.SetColumnSpan(actions, 2);
            actions.Controls.Add(CreateActionButton("Save", Accent, delegate { SaveProject(false); }));
            actions.Controls.Add(CreateActionButton("Save As", AccentPurple, delegate { SaveProject(true); }));
            actions.Controls.Add(CreateActionButton("Load", Color.FromArgb(76, 86, 108), delegate { LoadProjectDialog(); }));
            actions.Controls.Add(CreateActionButton("Import HNS Pack", AccentGreen, delegate { ImportExistingPackDialog(); }));
            actions.Controls.Add(CreateActionButton("Setup Tutorial", Warn, delegate { ShowGettingStartedTutorial(false); }));

            TableLayoutPanel right = new TableLayoutPanel();
            right.Dock = DockStyle.Fill;
            right.BackColor = Bg;
            right.RowCount = 4;
            right.ColumnCount = 1;
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 145));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 145));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 260));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(right, 1, 0);

            RoundedPanel progressCard = CreateCard();
            progressCard.Padding = new Padding(26);
            right.Controls.Add(progressCard, 0, 0);
            DashboardReadyLabel = new Label();
            DashboardReadyLabel.Dock = DockStyle.Top;
            DashboardReadyLabel.Height = 58;
            DashboardReadyLabel.ForeColor = AccentGreen;
            DashboardReadyLabel.Font = new Font("Segoe UI Semibold", 29f, FontStyle.Bold);
            progressCard.Controls.Add(DashboardReadyLabel);
            Label status = new Label();
            status.Dock = DockStyle.Top;
            status.Height = 36;
            status.ForeColor = TextMuted;
            status.Font = new Font("Segoe UI", 10.5f);
            status.Text = "Add race tracks, characters, music, and optional cup icons.";
            progressCard.Controls.Add(status);

            RoundedPanel assetCard = CreateCard();
            assetCard.Padding = new Padding(26);
            right.Controls.Add(assetCard, 0, 1);
            DashboardAssetLabel = new Label();
            DashboardAssetLabel.Dock = DockStyle.Fill;
            DashboardAssetLabel.ForeColor = TextMain;
            DashboardAssetLabel.Font = new Font("Segoe UI Semibold", 15f, FontStyle.Bold);
            assetCard.Controls.Add(DashboardAssetLabel);

            RoundedPanel sampleCard = CreateCard();
            sampleCard.Padding = new Padding(26);
            right.Controls.Add(sampleCard, 0, 2);
            Label sampleTitle = CreateSectionTitle("HNS Pack Layout");
            sampleTitle.Dock = DockStyle.Top;
            sampleCard.Controls.Add(sampleTitle);
            Label sampleText = new Label();
            sampleText.Dock = DockStyle.Fill;
            sampleText.ForeColor = TextMuted;
            sampleText.Font = new Font("Segoe UI", 10.1f);
            sampleText.Text = "Before exporting, every user needs their own MKWii base files in ./base_files. Do not publish Nintendo base files with your release.\n\nNeeded base folders:\nbase_files/Scene/UI\nbase_files/Scene/Model\nbase_files/Scene/Model/Kart\nbase_files/Race/Kart\nbase_files/Race/Course\nbase_files/Sound\n\nThe app uses ./tools automatically for SZS/BMG/image patching. Users should not need to touch Wiimms tools manually.";
            sampleCard.Controls.Add(sampleText);

            RoundedPanel workflow = CreateCard();
            workflow.Padding = new Padding(26);
            right.Controls.Add(workflow, 0, 3);
            Label flowTitle = CreateSectionTitle("Workflow");
            flowTitle.Dock = DockStyle.Top;
            workflow.Controls.Add(flowTitle);
            Label flow = new Label();
            flow.Dock = DockStyle.Fill;
            flow.ForeColor = TextMain;
            flow.Font = new Font("Segoe UI", 10.3f);
            flow.Text = "1. Setup: copy your own MKWii base files into ./base_files once.\n\n2. Tracks: click Source File to add .szs tracks, then type Custom Name.\n\n3. Characters/Music: drop complete character packs and .brstm music.\n\n4. Cup Icons: click a cup, then click an icon. Reset keeps original.\n\n5. Export: one click builds the Riivolution/HNS pack.";
            workflow.Controls.Add(flow);

            UpdateReadyLabels();
        }

        private void ShowTrackSlots()
        {
            SetActiveNav("Tracks");
            SetHeader("Tracks", "Assign custom race SZS files to the original Mario Kart Wii filenames.");
            ClearPage();

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.BackColor = Bg;
            layout.ColumnCount = 1;
            layout.RowCount = 2;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            PageHost.Controls.Add(layout);

            FlowLayoutPanel toolbar = new FlowLayoutPanel();
            toolbar.Dock = DockStyle.Fill;
            toolbar.FlowDirection = FlowDirection.LeftToRight;
            toolbar.BackColor = Bg;
            toolbar.Padding = new Padding(0, 0, 0, 10);
            layout.Controls.Add(toolbar, 0, 0);
            toolbar.Controls.Add(CreateActionButton("Add / Replace", Accent, delegate { AddTrackToSelectedSlot(); }));
            toolbar.Controls.Add(CreateActionButton("Track Import Wizard", AccentPurple, delegate { ShowTrackImportWizardDialog(); }));
            toolbar.Controls.Add(CreateActionButton("Auto Fill Folder", AccentGreen, delegate { AutoFillFolder(); }));
            toolbar.Controls.Add(CreateActionButton("Import Existing HNS Pack", AccentPurple, delegate { ImportExistingPackDialog(); }));
            toolbar.Controls.Add(CreateActionButton("Clear Slot", Color.FromArgb(76, 86, 108), delegate { ClearSelectedSlot(); }));
            toolbar.Controls.Add(CreateActionButton("Validate", Warn, delegate { ValidatePack(true); }));
            toolbar.Controls.Add(CreateActionButton("Validate Track SZS", AccentPurple, delegate { ShowTrackSzsValidatorDialog(); }));

            SlotGrid = CreateGrid();
            SlotGrid.CellClick += SlotGrid_CellClick;
            SlotGrid.CellEndEdit += delegate { ApplyGridEdits(); };
            layout.Controls.Add(SlotGrid, 0, 1);

            SlotGrid.Columns.Add("Type", "Type");
            SlotGrid.Columns.Add("Cup", "Cup / Group");
            SlotGrid.Columns.Add("Track", "Original Slot");
            SlotGrid.Columns.Add("GameFile", "Game Filename");
            SlotGrid.Columns.Add("CustomName", "Custom Name");
            SlotGrid.Columns.Add("Source", "Source File");
            SlotGrid.Columns.Add("Status", "Status");
            for (int i = 0; i < SlotGrid.Columns.Count; i++) SlotGrid.Columns[i].ReadOnly = true;
            SlotGrid.Columns[4].ReadOnly = false;
            SlotGrid.Columns[0].FillWeight = 70;
            SlotGrid.Columns[1].FillWeight = 110;
            SlotGrid.Columns[2].FillWeight = 140;
            SlotGrid.Columns[3].FillWeight = 130;
            SlotGrid.Columns[4].FillWeight = 170;
            SlotGrid.Columns[5].FillWeight = 160;
            SlotGrid.Columns[6].FillWeight = 80;
            RefreshSlotGrid();
        }

        private void ShowAssetPage(string category)
        {
            SetActiveNav(category == "UI / REL Assets" ? "Advanced Files" : category);
            string subtitle = category == "Music" ? "Add BRSTM music replacements and fanfares."
                : category == "Characters" ? "Add driver, kart, bike, and BRRES replacements."
                : "Add StaticR REL files, UI SZS files, BRSAR, menus, titles, and other HNS assets.";
            SetHeader(category, subtitle);
            ClearPage();

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.BackColor = Bg;
            layout.RowCount = 3;
            layout.ColumnCount = 1;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            PageHost.Controls.Add(layout);

            Label help = new Label();
            help.Dock = DockStyle.Fill;
            help.BackColor = Card;
            help.ForeColor = TextMuted;
            help.Font = new Font("Segoe UI", 10f);
            help.Padding = new Padding(18, 0, 18, 0);
            help.TextAlign = ContentAlignment.MiddleLeft;
            if (category == "Music") help.Text = "Music made simple: add .brstm files. Use Slot Helper when you do not know the internal MKWii music filename. _n = normal lap, _f = final lap.";
            else if (category == "UI / REL Assets") help.Text = "Advanced Files are optional. Use them only for UI SZS, StaticR.rel, revo_kart.brsar, or exact files named by a mod readme.";
            else help.Text = "Characters: import a complete character pack when possible. Use Check Conflicts before exporting.";
            layout.Controls.Add(help, 0, 0);

            FlowLayoutPanel toolbar = new FlowLayoutPanel();
            toolbar.Dock = DockStyle.Fill;
            toolbar.BackColor = Bg;
            toolbar.FlowDirection = FlowDirection.LeftToRight;
            toolbar.Padding = new Padding(0, 8, 0, 8);
            layout.Controls.Add(toolbar, 0, 1);
            toolbar.Controls.Add(CreateActionButton(category == "Music" ? "Add Music" : category == "UI / REL Assets" ? "Add Advanced" : "Add Files", Accent, delegate { AddAssetFiles(category); }));
            if (category == "Music")
            {
                toolbar.Controls.Add(CreateActionButton("Slot Helper", AccentGreen, delegate { ShowMusicSlotHelperDialog(); }));
                toolbar.Controls.Add(CreateActionButton("Auto Pair", AccentPurple, delegate { AutoPairMusicAssets(); }));
                toolbar.Controls.Add(CreateActionButton("Explain", Warn, delegate { ShowSelectedAssetExplanation("Music"); }));
            }
            if (category == "Characters")
            {
                toolbar.Controls.Add(CreateActionButton("Import Pack", AccentPurple, delegate { ImportCharacterPackDialog(); }));
                toolbar.Controls.Add(CreateActionButton("Check Conflicts", Warn, delegate { ShowCharacterConflictDialog(); }));
                toolbar.Controls.Add(CreateActionButton("Summary", AccentGreen, delegate { ShowCharacterSummaryDialog(); }));
            }
            if (category == "UI / REL Assets")
            {
                toolbar.Controls.Add(CreateActionButton("Identify", AccentPurple, delegate { ShowAdvancedFileReportDialog(); }));
                toolbar.Controls.Add(CreateActionButton("Explain", Warn, delegate { ShowSelectedAssetExplanation("UI / REL Assets"); }));
            }
            toolbar.Controls.Add(CreateActionButton("Scan Folder", AccentGreen, delegate { ScanAssetFolder(category); }));
            toolbar.Controls.Add(CreateActionButton("Remove", Color.FromArgb(76, 86, 108), delegate { RemoveSelectedAsset(category); }));

            DataGridView grid = CreateGrid();
            grid.CellEndEdit += delegate { ApplyAssetGridEdits(category); };
            layout.Controls.Add(grid, 0, 2);
            if (category == "Characters")
            {
                grid.RowTemplate.Height = 42;
                DataGridViewImageColumn iconColumn = new DataGridViewImageColumn();
                iconColumn.Name = "Icon";
                iconColumn.HeaderText = "";
                iconColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
                grid.Columns.Add(iconColumn);
                grid.Columns.Add("Character", "Character");
                grid.Columns.Add("FileType", "File Type");
                grid.Columns.Add("OutputName", "Export Name");
                grid.Columns.Add("DiscPath", "Replaces In Game");
                grid.Columns.Add("Source", "Source File");
                grid.Columns.Add("Notes", "Notes / Warnings");
                grid.Columns.Add("Status", "Status");
                grid.Columns[0].ReadOnly = true;
                grid.Columns[1].ReadOnly = true;
                grid.Columns[2].ReadOnly = true;
                grid.Columns[3].ReadOnly = false;
                grid.Columns[4].ReadOnly = false;
                grid.Columns[5].ReadOnly = true;
                grid.Columns[6].ReadOnly = false;
                grid.Columns[7].ReadOnly = true;
                grid.Columns[0].FillWeight = 38;
                grid.Columns[1].FillWeight = 120;
                grid.Columns[2].FillWeight = 145;
                grid.Columns[3].FillWeight = 145;
                grid.Columns[4].FillWeight = 220;
                grid.Columns[5].FillWeight = 175;
                grid.Columns[6].FillWeight = 175;
                grid.Columns[7].FillWeight = 75;
            }
            else
            {
                grid.Columns.Add("Kind", "Type");
                grid.Columns.Add("Usage", "Purpose");
                grid.Columns.Add("Folder", "Game Folder");
                grid.Columns.Add("OutputName", "Export Name");
                grid.Columns.Add("DiscPath", "Replaces In Game");
                grid.Columns.Add("Source", "Source File");
                grid.Columns.Add("Notes", "Hint");
                grid.Columns.Add("Size", "Size");
                grid.Columns.Add("Status", "Status");
                grid.Columns[0].ReadOnly = true;
                grid.Columns[1].ReadOnly = true;
                grid.Columns[2].ReadOnly = true;
                grid.Columns[3].ReadOnly = false;
                grid.Columns[4].ReadOnly = false;
                grid.Columns[5].ReadOnly = true;
                grid.Columns[6].ReadOnly = false;
                grid.Columns[7].ReadOnly = true;
                grid.Columns[8].ReadOnly = true;
                grid.Columns[0].FillWeight = 120;
                grid.Columns[1].FillWeight = 170;
                grid.Columns[2].FillWeight = 110;
                grid.Columns[3].FillWeight = 135;
                grid.Columns[4].FillWeight = 210;
                grid.Columns[5].FillWeight = 150;
                grid.Columns[6].FillWeight = 180;
                grid.Columns[7].FillWeight = 65;
                grid.Columns[8].FillWeight = 65;
            }

            if (category == "Music") MusicGrid = grid;
            else if (category == "Characters") CharacterGrid = grid;
            else UiRelGrid = grid;
            RefreshAssetGrid(category);
        }

        private void ShowAssetRules(string category)
        {
            string text;
            if (category == "Music") text = "Music is streamed BRSTM audio. Normal race music usually uses *_n.brstm and final-lap music usually uses *_f.brstm. The game folder is normally /sound/strm. Use Music Slot Helper for track slots, Auto Pair Music for missing final-lap entries, and Explain Selected if you are unsure what a row does.";
            else if (category == "Characters") text = "Use complete character packs or loose .szs/.brres/.brsar/.tpl/.png files. The Characters table now detects the target character from filenames like la_bike-fk.szs, fk-allkart.szs, or allkart-fk.szs. Kart files go to /Race/Kart, Driver.szs goes to /Scene/Model, *-allkart.szs goes to /Scene/Model/Kart, and menu/text SZS files go to /Scene/UI. Inject PNG/TPL files are source files that are patched into real UI SZS files during export. Use Check Conflicts to find duplicate replacements, and Character Summary to check incomplete packs.";
            else text = "Advanced Files is for game system files: UI SZS archives go to /Scene/UI, StaticR REL files go to /rel, revo_kart.brsar goes to /sound, and BRRES resources are usually model/texture resources inside archives. Use Identify Files or Explain Selected before exporting if you are unsure.";
            MessageBox.Show(text, category + " Rules", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowCupIcons()
        {
            SetActiveNav("Cup Icons");
            SetHeader("Cup Icons", "Assign cup icons. When an icon is assigned, export also renames that cup from the icon filename.");
            ClearPage();

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.BackColor = Bg;
            layout.ColumnCount = 2;
            layout.RowCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            PageHost.Controls.Add(layout);

            RoundedPanel left = CreateCard();
            left.Padding = new Padding(22);
            layout.Controls.Add(left, 0, 0);

            TableLayoutPanel leftLayout = new TableLayoutPanel();
            leftLayout.Dock = DockStyle.Fill;
            leftLayout.BackColor = Card;
            leftLayout.RowCount = 5;
            leftLayout.ColumnCount = 1;
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 230));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 145));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            left.Controls.Add(leftLayout);

            leftLayout.Controls.Add(CreateSectionTitle("Cups / Groups"), 0, 0);
            CupList = new ListBox();
            CupList.Dock = DockStyle.Fill;
            CupList.BackColor = Color.FromArgb(16, 20, 29);
            CupList.ForeColor = TextMain;
            CupList.BorderStyle = BorderStyle.FixedSingle;
            CupList.Font = new Font("Segoe UI", 10f);
            foreach (string cup in CupNames) CupList.Items.Add(cup);
            CupList.SelectedIndexChanged += delegate { RefreshCupPreview(); };
            leftLayout.Controls.Add(CupList, 0, 1);

            Panel previewPanel = new Panel();
            previewPanel.Dock = DockStyle.Fill;
            previewPanel.BackColor = Card;
            leftLayout.Controls.Add(previewPanel, 0, 2);
            CupPreview = new PictureBox();
            CupPreview.Size = new Size(100, 100);
            CupPreview.Location = new Point(92, 6);
            CupPreview.SizeMode = PictureBoxSizeMode.Zoom;
            CupPreview.BackColor = Color.FromArgb(16, 20, 29);
            previewPanel.Controls.Add(CupPreview);
            CupPreviewTitle = new Label();
            CupPreviewTitle.Location = new Point(0, 112);
            CupPreviewTitle.Size = new Size(285, 54);
            CupPreviewTitle.TextAlign = ContentAlignment.MiddleCenter;
            CupPreviewTitle.ForeColor = TextMuted;
            CupPreviewTitle.Font = new Font("Segoe UI", 9f);
            previewPanel.Controls.Add(CupPreviewTitle);

            FlowLayoutPanel iconActions = new FlowLayoutPanel();
            iconActions.Dock = DockStyle.Fill;
            iconActions.BackColor = Card;
            leftLayout.Controls.Add(iconActions, 0, 3);
            // Simple mode: click a cup, then click an icon to assign it.
            // Reset Cup / Reset All are intentionally visible here; empty cup slots keep original MKWii icons.
            iconActions.Controls.Add(CreateActionButton("Upload PNG", Accent, delegate { UploadCustomCupIcon(); }));
            iconActions.Controls.Add(CreateActionButton("Import Folder", AccentGreen, delegate { ImportIconFolder(); }));
            iconActions.Controls.Add(CreateActionButton("Reset Cup", Color.FromArgb(76, 86, 108), delegate { ClearCupIcon(); }));
            iconActions.Controls.Add(CreateActionButton("Reset All", Danger, delegate { ClearAllCupIcons(); }));

            RoundedPanel right = CreateCard();
            right.Padding = new Padding(22);
            layout.Controls.Add(right, 1, 0);
            TableLayoutPanel rightLayout = new TableLayoutPanel();
            rightLayout.Dock = DockStyle.Fill;
            rightLayout.BackColor = Card;
            rightLayout.RowCount = 2;
            rightLayout.ColumnCount = 1;
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            right.Controls.Add(rightLayout);
            rightLayout.Controls.Add(CreateSectionTitle("Icon Library / User PNGs"), 0, 0);

            IconImageList = new ImageList();
            IconImageList.ImageSize = new Size(54, 54);
            IconImageList.ColorDepth = ColorDepth.Depth32Bit;
            IconList = new ListView();
            IconList.Dock = DockStyle.Fill;
            IconList.BackColor = Color.FromArgb(16, 20, 29);
            IconList.ForeColor = TextMain;
            IconList.BorderStyle = BorderStyle.FixedSingle;
            IconList.View = View.LargeIcon;
            IconList.LargeImageList = IconImageList;
            IconList.MultiSelect = false;
            IconList.Font = new Font("Segoe UI", 9f);
            // Simple behavior: click a cup, then click an icon to assign it.
            // Nothing is pre-picked. Reset Cup/Reset All returns cups to original game icons.
            IconList.SelectedIndexChanged += delegate { AssignSelectedLibraryIcon(); };
            IconList.DoubleClick += delegate { AssignSelectedLibraryIcon(); };
            rightLayout.Controls.Add(IconList, 0, 1);
            LoadIconLibrary();
            CupList.SelectedIndex = 0;
        }

        private void ShowUiPatcher()
        {
            SetActiveNav("Setup");
            SetHeader("Setup", "Extract the required base files from your own MKWii WBFS/ISO, then build packs normally.");
            ClearPage();

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.BackColor = Bg;
            layout.ColumnCount = 2;
            layout.RowCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));
            PageHost.Controls.Add(layout);

            RoundedPanel left = CreateCard();
            left.Padding = new Padding(26);
            layout.Controls.Add(left, 0, 0);

            TableLayoutPanel form = new TableLayoutPanel();
            form.Dock = DockStyle.Top;
            form.Height = 320;
            form.BackColor = Card;
            form.ColumnCount = 3;
            form.RowCount = 6;
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            for (int i = 0; i < 6; i++) form.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            left.Controls.Add(form);

            Label title = CreateSectionTitle("Required Setup Folder");
            form.Controls.Add(title, 0, 0); form.SetColumnSpan(title, 3);

            PatchBaseUiFolderText = CreateTextBox(Path.Combine(AppRoot, "base_files"));
            PatchBaseUiFolderText.ReadOnly = true;
            AddPatcherRow(form, "Base Files Folder", PatchBaseUiFolderText, 1, delegate { try { Directory.CreateDirectory(Path.Combine(AppRoot, "base_files")); Process.Start(new ProcessStartInfo { FileName = Path.Combine(AppRoot, "base_files"), UseShellExecute = true }); } catch { } });

            PatchOutputFolderText = CreateTextBox(Path.Combine(AppRoot, "Data", "WorkingSZS", SafeId(PackId)));
            PatchOutputFolderText.ReadOnly = true;
            AddPatcherRow(form, "Working Copies", PatchOutputFolderText, 2, delegate { try { Directory.CreateDirectory(Path.Combine(AppRoot, "Data", "WorkingSZS", SafeId(PackId))); Process.Start(new ProcessStartInfo { FileName = Path.Combine(AppRoot, "Data", "WorkingSZS", SafeId(PackId)), UseShellExecute = true }); } catch { } });

            PatchLanguageCombo = CreateCombo(new string[] { "U - USA English", "E - PAL English", "J - Japanese", "K - Korean", "M - USA Spanish", "Q - USA French", "F - PAL French", "G - PAL German", "I - PAL Italian", "S - PAL Spanish" }, string.IsNullOrWhiteSpace(PatchLanguage) ? "U - USA English" : PatchLanguage);
            Label langLabel = new Label(); langLabel.Text = "Language"; langLabel.Dock = DockStyle.Fill; langLabel.ForeColor = TextMuted; langLabel.Font = new Font("Segoe UI", 10f); langLabel.TextAlign = ContentAlignment.MiddleLeft;
            form.Controls.Add(langLabel, 0, 3); form.Controls.Add(PatchLanguageCombo, 1, 3); form.SetColumnSpan(PatchLanguageCombo, 2);

            PatchBrawlCrateText = null;
            PatchWbmgtText = null;
            PatchWszstText = null;

            PatchAwardCheck = new CheckBox();
            PatchAwardCheck.Text = "Include Award.szs / ceremony files in patch plan (can crash if bad)";
            PatchAwardCheck.Checked = PatchIncludeAward;
            PatchAwardCheck.Dock = DockStyle.Fill;
            PatchAwardCheck.ForeColor = TextMain;
            PatchAwardCheck.BackColor = Card;
            PatchAwardCheck.Font = new Font("Segoe UI", 9.5f);
            Label awardLabel = new Label(); awardLabel.Text = "Safety"; awardLabel.Dock = DockStyle.Fill; awardLabel.ForeColor = TextMuted; awardLabel.Font = new Font("Segoe UI", 10f); awardLabel.TextAlign = ContentAlignment.MiddleLeft;
            form.Controls.Add(awardLabel, 0, 4); form.Controls.Add(PatchAwardCheck, 1, 4); form.SetColumnSpan(PatchAwardCheck, 2);

            Button extractHero = CreateActionButton("Extract base files from WBFS/ISO", AccentGreen, delegate { ExtractRequiredBaseFilesFromDiscImage(); });
            extractHero.Dock = DockStyle.Fill;
            extractHero.Height = 52;
            extractHero.Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold);
            Label extractLabel = new Label(); extractLabel.Text = "Auto Setup"; extractLabel.Dock = DockStyle.Fill; extractLabel.ForeColor = TextMuted; extractLabel.Font = new Font("Segoe UI", 10f); extractLabel.TextAlign = ContentAlignment.MiddleLeft;
            form.Controls.Add(extractLabel, 0, 5); form.Controls.Add(extractHero, 1, 5); form.SetColumnSpan(extractHero, 2);

            FlowLayoutPanel buttons = new FlowLayoutPanel();
            buttons.Dock = DockStyle.Bottom;
            buttons.Height = 205;
            buttons.BackColor = Card;
            buttons.Padding = new Padding(0, 16, 0, 0);
            left.Controls.Add(buttons);
            buttons.Controls.Add(CreateActionButton("Open Base Files", Accent, delegate { try { Directory.CreateDirectory(Path.Combine(AppRoot, "base_files")); Process.Start(new ProcessStartInfo { FileName = Path.Combine(AppRoot, "base_files"), UseShellExecute = true }); } catch { } }));
            buttons.Controls.Add(CreateActionButton("Setup Tutorial", Warn, delegate { ShowGettingStartedTutorial(false); }));
            buttons.Controls.Add(CreateActionButton("Open Logs", Color.FromArgb(76, 86, 108), delegate { try { Directory.CreateDirectory(Path.Combine(AppRoot, "logs")); Process.Start(new ProcessStartInfo { FileName = Path.Combine(AppRoot, "logs"), UseShellExecute = true }); } catch { } }));
            buttons.Controls.Add(CreateActionButton("Clean Cache", Danger, delegate { CleanWorkingCacheNow(); }));
            buttons.Controls.Add(CreateActionButton("Project Cleanup", Color.FromArgb(76, 86, 108), delegate { ProjectCleanupNow(); }));
            buttons.Controls.Add(CreateActionButton("Release Safety Check", AccentPurple, delegate { ShowReleaseSafetyCheck(); }));
            buttons.Controls.Add(CreateActionButton("Check Sources", Warn, delegate { ShowSourceHealthReportDialog(); }));

            RoundedPanel right = CreateCard();
            right.Padding = new Padding(24);
            layout.Controls.Add(right, 1, 0);

            TableLayoutPanel rightLayout = new TableLayoutPanel();
            rightLayout.Dock = DockStyle.Fill;
            rightLayout.BackColor = Card;
            rightLayout.ColumnCount = 1;
            rightLayout.RowCount = 5;
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 270));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            right.Controls.Add(rightLayout);

            Label info = new Label();
            info.Dock = DockStyle.Fill;
            info.ForeColor = TextMuted;
            info.Font = new Font("Segoe UI", 10f);
            info.Text = "Quick setup: click Extract base files from WBFS/ISO and choose your own legally dumped MKWii disc image. The app copies only the required folders into base_files.";
            rightLayout.Controls.Add(info, 0, 0);

            Label statusHeader = CreateSectionTitle("Base file status");
            rightLayout.Controls.Add(statusHeader, 0, 1);

            RichTextBox setupStatus = new RichTextBox();
            setupStatus.Dock = DockStyle.Fill;
            setupStatus.ReadOnly = true;
            setupStatus.BorderStyle = BorderStyle.None;
            setupStatus.BackColor = Color.FromArgb(16, 20, 29);
            setupStatus.ForeColor = TextMain;
            setupStatus.Font = new Font("Consolas", 9.2f);
            rightLayout.Controls.Add(setupStatus, 0, 2);
            SetupStatusBox = setupStatus;
            FillBaseFileStatusBox(setupStatus);

            Label logHeader = CreateSectionTitle("Activity log");
            rightLayout.Controls.Add(logHeader, 0, 3);

            PatchLogBox = new RichTextBox();
            PatchLogBox.Dock = DockStyle.Fill;
            PatchLogBox.BackColor = Color.FromArgb(12, 15, 22);
            PatchLogBox.ForeColor = TextMain;
            PatchLogBox.BorderStyle = BorderStyle.None;
            PatchLogBox.Font = new Font("Consolas", 9.2f);
            rightLayout.Controls.Add(PatchLogBox, 0, 4);
            WritePatchLog("Ready. Put extracted MKWii files in ./base_files, or click Extract WBFS/ISO to extract them from your own disc image.");
            WritePatchLog("Expected base folders: Scene/UI, Scene/Model, Scene/Model/Kart, Race/Kart, Race/Course, Sound.");
            WritePatchLog("The extractor uses tools/wit.exe and copies only the required folders into base_files.");
            WritePatchLog("Use Check Sources before exporting to catch missing or broken imported files.");
        }

        private void AddPatcherRow(TableLayoutPanel form, string label, TextBox box, int row, Action browseAction)
        {
            Label l = new Label();
            l.Text = label;
            l.Dock = DockStyle.Fill;
            l.ForeColor = TextMuted;
            l.Font = new Font("Segoe UI", 10f);
            l.TextAlign = ContentAlignment.MiddleLeft;
            form.Controls.Add(l, 0, row);
            form.Controls.Add(box, 1, row);
            Button b = CreateActionButton("Browse", Accent, browseAction);
            b.Width = 100;
            form.Controls.Add(b, 2, row);
        }

        private void BrowseFolderInto(TextBox box, string description)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = description;
            if (Directory.Exists(box.Text)) fbd.SelectedPath = box.Text;
            if (fbd.ShowDialog(this) == DialogResult.OK) { box.Text = fbd.SelectedPath; SaveMetadataFromFields(); }
        }

        private void BrowseExeInto(TextBox box, string exeName)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Choose " + exeName;
            ofd.Filter = exeName + "|" + exeName + "|Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            if (ofd.ShowDialog(this) == DialogResult.OK) { box.Text = ofd.FileName; SaveMetadataFromFields(); }
        }

        private string FindToolOnPath(string exeName)
        {
            try
            {
                string[] localCandidates = new string[]
                {
                    Path.Combine(AppRoot, "tools", exeName),
                    Path.Combine(AppRoot, "tools", "bin", exeName),
                    Path.Combine(AppRoot, "tools", "wiimms", exeName),
                    Path.Combine(AppRoot, "tools", "wiimms", "bin", exeName),
                    Path.Combine(AppRoot, "bin", exeName)
                };
                foreach (string local in localCandidates) if (File.Exists(local)) return local;

                string path = Environment.GetEnvironmentVariable("PATH") ?? "";
                foreach (string part in path.Split(Path.PathSeparator))
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(part)) continue;
                        string p = Path.Combine(part.Trim(), exeName);
                        if (File.Exists(p)) return p;
                    }
                    catch { }
                }
            }
            catch { }
            return "";
        }

        private string PatchLanguageSuffix()
        {
            if (PatchLanguageCombo == null || PatchLanguageCombo.SelectedItem == null) return "U";
            string s = PatchLanguageCombo.SelectedItem.ToString();
            return string.IsNullOrEmpty(s) ? "U" : s.Substring(0, 1);
        }

        private string[] UiIconBaseFiles(bool includeAward)
        {
            List<string> files = new List<string>();
            if (includeAward) files.Add("Award.szs");
            files.AddRange(new string[] { "Channel.szs", "Event.szs", "Globe.szs", "MenuMulti.szs", "MenuSingle.szs", "MenuOther.szs", "Present.szs", "Race.szs" });
            return files.ToArray();
        }

        private string[] UiLanguageFiles(string suffix, bool includeAward)
        {
            return UiIconBaseFiles(includeAward).Select(x => Path.GetFileNameWithoutExtension(x) + "_" + suffix + ".szs").ToArray();
        }

        private void WritePatchLog(string text)
        {
            if (PatchLogBox != null && !PatchLogBox.IsDisposed) PatchLogBox.AppendText(text + Environment.NewLine);
            AddLog("UI Patcher: " + text);
        }

        private void BuildUiPatchWorkspace()
        {
            SaveMetadataFromFields();
            ApplyGridEdits();
            string baseDir = PatchBaseUiFolderText == null ? "" : PatchBaseUiFolderText.Text.Trim();
            string outDir = PatchOutputFolderText == null ? "" : PatchOutputFolderText.Text.Trim();
            if (string.IsNullOrWhiteSpace(outDir)) outDir = Path.Combine(AppRoot, "patch_workspace", SafeId(PackId));
            PatchBaseUiFolder = baseDir;
            PatchWorkspaceFolder = outDir;
            PatchBrawlCratePath = PatchBrawlCrateText == null ? PatchBrawlCratePath : PatchBrawlCrateText.Text.Trim();
            PatchWbmgtPath = PatchWbmgtText == null ? PatchWbmgtPath : PatchWbmgtText.Text.Trim();
            PatchWszstPath = PatchWszstText == null ? PatchWszstPath : PatchWszstText.Text.Trim();
            PatchLanguage = PatchLanguageCombo == null || PatchLanguageCombo.SelectedItem == null ? PatchLanguage : PatchLanguageCombo.SelectedItem.ToString();
            PatchIncludeAward = PatchAwardCheck != null && PatchAwardCheck.Checked;
            Directory.CreateDirectory(outDir);
            string baseCopyDir = Path.Combine(outDir, "01_base_ui_copy");
            string patchedDir = Path.Combine(outDir, "02_put_patched_szs_here");
            string sourceDir = Path.Combine(outDir, "03_source_inputs");
            string scriptsDir = Path.Combine(outDir, "04_scripts");
            Directory.CreateDirectory(baseCopyDir);
            Directory.CreateDirectory(patchedDir);
            Directory.CreateDirectory(sourceDir);
            Directory.CreateDirectory(scriptsDir);

            string suffix = PatchLanguageSuffix();
            bool includeAward = PatchAwardCheck != null && PatchAwardCheck.Checked;
            int copied = 0, missing = 0;
            foreach (string name in UiIconBaseFiles(includeAward).Concat(UiLanguageFiles(suffix, includeAward)))
            {
                string src = Path.Combine(baseDir, name);
                if (File.Exists(src)) { File.Copy(src, Path.Combine(baseCopyDir, name), true); copied++; }
                else missing++;
            }

            Directory.CreateDirectory(Path.Combine(sourceDir, "cup_icons_64_menu"));
            Directory.CreateDirectory(Path.Combine(sourceDir, "cup_icons_32_race"));
            Directory.CreateDirectory(Path.Combine(sourceDir, "character_icons_64_menu"));
            Directory.CreateDirectory(Path.Combine(sourceDir, "character_icons_32_race"));
            Directory.CreateDirectory(Path.Combine(sourceDir, "bmg_text"));
            ExportSelectedCupIconsTo(Path.Combine(sourceDir, "cup_icons_64_menu"));
            ExportSelectedCupIconsTo(Path.Combine(sourceDir, "cup_icons_32_race"));
            WriteNamePatchSource(Path.Combine(sourceDir, "bmg_text"), suffix);
            WriteUiPatchReadme(outDir, suffix, includeAward, copied, missing);
            WritePatcherBatchScripts(outDir, suffix);
            WritePatchLog("Workspace built: " + outDir);
            WritePatchLog("Copied base UI files: " + copied + "; missing from base folder: " + missing + ".");
            WritePatchLog("After patching in BrawlCrate/Wiimms, put final SZS files in 02_put_patched_szs_here, then click Import Patched UI.");
        }

        private void WriteUiPatcherScriptsOnly()
        {
            string outDir = PatchOutputFolderText == null ? "" : PatchOutputFolderText.Text.Trim();
            if (string.IsNullOrWhiteSpace(outDir)) outDir = Path.Combine(AppRoot, "patch_workspace", SafeId(PackId));
            Directory.CreateDirectory(outDir);
            WritePatcherBatchScripts(outDir, PatchLanguageSuffix());
            WritePatchLog("Scripts written to " + Path.Combine(outDir, "04_scripts"));
        }

        private void ExportSelectedCupIconsTo(string destDir)
        {
            Directory.CreateDirectory(destDir);
            int targetSize = destDir.ToLowerInvariant().Contains("32") ? 32 : 64;
            int i = 1;
            foreach (string cup in CupNames)
            {
                string path = CupIconPaths.ContainsKey(cup) ? CupIconPaths[cup] : "";
                // Empty cup icon means keep the original game icon; do not export a default replacement.
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) { i++; continue; }
                string dest = Path.Combine(destDir, SafeFileName(cup) + ".png");
                SaveResizedPngIcon(path, dest, targetSize);
                i++;
            }
        }

        private void SaveResizedPngIcon(string sourcePath, string destPath, int size)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                using (Image src = Image.FromFile(sourcePath))
                using (Bitmap canvas = new Bitmap(size, size))
                using (Graphics g = Graphics.FromImage(canvas))
                {
                    g.Clear(Color.Transparent);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    float scale = Math.Min((float)size / Math.Max(1, src.Width), (float)size / Math.Max(1, src.Height));
                    int w = Math.Max(1, (int)Math.Round(src.Width * scale));
                    int h = Math.Max(1, (int)Math.Round(src.Height * scale));
                    int x = (size - w) / 2;
                    int y = (size - h) / 2;
                    g.DrawImage(src, new Rectangle(x, y, w, h));
                    canvas.Save(destPath, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            catch (Exception ex)
            {
                AddLog("Icon resize failed for " + Path.GetFileName(sourcePath) + ": " + ex.Message);
                try { File.Copy(sourcePath, destPath, true); } catch { }
            }
        }

        private void WriteNamePatchSource(string dir, string suffix)
        {
            Directory.CreateDirectory(dir);
            StringBuilder list = new StringBuilder();
            list.AppendLine("# Track and character name source generated by MKWii Pack Maker");
            list.AppendLine("# Language suffix: _" + suffix);
            list.AppendLine("# This is a patch source/checklist. Use your existing Common.txt/Common.bmg as the base so custom distributions do not lose their message IDs.");
            list.AppendLine();
            list.AppendLine("[Tracks]");
            foreach (TrackSlotInfo s in Slots)
            {
                if (GetSlotStatus(s) != "Ready") continue;
                string custom = string.IsNullOrWhiteSpace(s.CustomName) ? Path.GetFileNameWithoutExtension(s.SourcePath) : s.CustomName;
                list.AppendLine(s.TrackName + " | " + s.GameFile + " => " + custom);
            }
            list.AppendLine();
            list.AppendLine("[Characters]");
            list.AppendLine("Add character name replacements here, then apply them to Common.txt before encoding Common.bmg.");
            File.WriteAllText(Path.Combine(dir, "name_patch_list.txt"), list.ToString(), Encoding.UTF8);

            string common = "# Put your extracted Common.txt here, edit the names, then run 04_scripts\\01_encode_common_bmg.cmd.\r\n" +
                            "# Do not use this placeholder as a full Common.txt; extract it from your own *_" + suffix + ".szs so IDs stay correct.\r\n";
            File.WriteAllText(Path.Combine(dir, "Common.txt"), common, Encoding.UTF8);
        }

        private void WriteUiPatchReadme(string outDir, string suffix, bool includeAward, int copied, int missing)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("MKWii Pack Maker UI Patcher Workspace");
            sb.AppendLine("====================================");
            sb.AppendLine();
            sb.AppendLine("Goal: create real SZS files that MKWii can load for cup icons, character UI icons, minimap/race icons, and names.");
            sb.AppendLine();
            sb.AppendLine("Folders:");
            sb.AppendLine("01_base_ui_copy          Base UI files copied from your game/CTGP/distribution.");
            sb.AppendLine("02_put_patched_szs_here  Put finished patched SZS files here, then click Import Patched UI.");
            sb.AppendLine("03_source_inputs         PNG icons and text-source/checklist files.");
            sb.AppendLine("04_scripts               Helper scripts for Wiimms SZS Tools/BrawlCrate workflow.");
            sb.AppendLine();
            sb.AppendLine("Needed icon SZS files:");
            foreach (string f in UiIconBaseFiles(includeAward)) sb.AppendLine("- " + f);
            sb.AppendLine();
            sb.AppendLine("Needed name SZS files for language _" + suffix + ":");
            foreach (string f in UiLanguageFiles(suffix, includeAward)) sb.AppendLine("- " + f);
            sb.AppendLine();
            sb.AppendLine("Notes:");
            sb.AppendLine("- Race.szs contains 32x32 race/minimap-style icons and also some repeated icon textures.");
            sb.AppendLine("- MenuSingle/MenuMulti/MenuOther/Globe/Channel/Event/Present use 64x64 menu/leaderboard-style icons.");
            sb.AppendLine("- Track and character names live in Common.bmg inside the language _X.szs files.");
            sb.AppendLine("- BrawlCrate is still needed for texture replacement if you want visual patching, because BrawlCrate is the tool that edits the internal TPL/BRRES textures cleanly.");
            sb.AppendLine("- Award.szs is optional because bad Award replacements can crash ceremony scenes.");
            sb.AppendLine();
            sb.AppendLine("Base UI files copied: " + copied);
            sb.AppendLine("Base UI files missing: " + missing);
            File.WriteAllText(Path.Combine(outDir, "README_UI_PATCHER.txt"), sb.ToString(), Encoding.UTF8);
        }

        private void WritePatcherBatchScripts(string outDir, string suffix)
        {
            string scriptsDir = Path.Combine(outDir, "04_scripts");
            Directory.CreateDirectory(scriptsDir);
            string wbmgt = PatchWbmgtText == null ? "" : PatchWbmgtText.Text.Trim();
            string wszst = PatchWszstText == null ? "" : PatchWszstText.Text.Trim();
            string brawl = PatchBrawlCrateText == null ? "" : PatchBrawlCrateText.Text.Trim();
            if (string.IsNullOrWhiteSpace(wbmgt)) wbmgt = "wbmgt.exe";
            if (string.IsNullOrWhiteSpace(wszst)) wszst = "wszst.exe";
            if (string.IsNullOrWhiteSpace(brawl)) brawl = "BrawlCrate.exe";

            string encode = "@echo off\r\n" +
                            "cd /d \"%~dp0..\\03_source_inputs\\bmg_text\"\r\n" +
                            "echo Encoding Common.txt to Common.bmg...\r\n" +
                            "\"" + wbmgt + "\" encode Common.txt --overwrite\r\n" +
                            "echo If successful, Common.bmg is in 03_source_inputs\\bmg_text. Replace it inside each *_" + suffix + ".szs /message folder.\r\n" +
                            "pause\r\n";
            File.WriteAllText(Path.Combine(scriptsDir, "01_encode_common_bmg.cmd"), encode, Encoding.ASCII);

            string open = "@echo off\r\n" +
                          "echo Opening base UI files in BrawlCrate. Save patched files into 02_put_patched_szs_here.\r\n";
            foreach (string f in UiIconBaseFiles(PatchAwardCheck != null && PatchAwardCheck.Checked).Concat(UiLanguageFiles(suffix, PatchAwardCheck != null && PatchAwardCheck.Checked)))
            {
                open += "if exist \"%~dp0..\\01_base_ui_copy\\" + f + "\" start \"\" \"" + brawl + "\" \"%~dp0..\\01_base_ui_copy\\" + f + "\"\r\n";
            }
            File.WriteAllText(Path.Combine(scriptsDir, "02_open_ui_files_in_brawlcrate.cmd"), open, Encoding.ASCII);

            string extract = "@echo off\r\n" +
                             "echo Optional: extract SZS with Wiimms SZS Tools for inspection.\r\n" +
                             "mkdir \"%~dp0..\\extracted_ui\" 2>nul\r\n";
            foreach (string f in UiIconBaseFiles(PatchAwardCheck != null && PatchAwardCheck.Checked).Concat(UiLanguageFiles(suffix, PatchAwardCheck != null && PatchAwardCheck.Checked)))
            {
                extract += "if exist \"%~dp0..\\01_base_ui_copy\\" + f + "\" \"" + wszst + "\" extract \"%~dp0..\\01_base_ui_copy\\" + f + "\" --dest \"%~dp0..\\extracted_ui\\" + Path.GetFileNameWithoutExtension(f) + "\" --overwrite\r\n";
            }
            extract += "pause\r\n";
            File.WriteAllText(Path.Combine(scriptsDir, "03_optional_extract_ui_with_wszst.cmd"), extract, Encoding.ASCII);
        }

        private void ImportPatchedUiIntoProject()
        {
            string outDir = PatchOutputFolderText == null ? "" : PatchOutputFolderText.Text.Trim();
            if (string.IsNullOrWhiteSpace(outDir)) outDir = Path.Combine(AppRoot, "patch_workspace", SafeId(PackId));
            PatchWorkspaceFolder = outDir;
            string patchedDir = Path.Combine(outDir, "02_put_patched_szs_here");
            if (!Directory.Exists(patchedDir))
            {
                MessageBox.Show("Patched UI folder was not found. Build the workspace first or choose the correct workspace folder.", "Import Patched UI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int added = 0;
            foreach (string file in Directory.GetFiles(patchedDir, "*.szs", SearchOption.AllDirectories))
            {
                string name = Path.GetFileName(file);
                AddAsset("UI / REL Assets", file, "Menu/" + name);
                added++;
            }
            RefreshAssetGrid("UI / REL Assets");
            WritePatchLog("Imported " + added + " patched UI SZS file(s) into UI / REL Assets.");
            MessageBox.Show("Imported " + added + " patched UI SZS file(s). They will export to hns/<pack id>/Menu and map to /Scene/UI.", "Import Patched UI", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OpenBrawlCrateForPatching()
        {
            string brawl = PatchBrawlCrateText == null ? "" : PatchBrawlCrateText.Text.Trim();
            if (string.IsNullOrWhiteSpace(brawl) || !File.Exists(brawl))
            {
                MessageBox.Show("Select BrawlCrate.exe first.", "BrawlCrate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = brawl, UseShellExecute = true });
                WritePatchLog("Opened BrawlCrate.");
            }
            catch (Exception ex) { MessageBox.Show("Could not open BrawlCrate.\n\n" + ex.Message, "BrawlCrate", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void ShowExportBuilder()
        {
            SetActiveNav("Export");
            SetHeader("Export", "Drop files, then build the pack. The app uses ./base_files and ./tools automatically.");
            ClearPage();

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.BackColor = Bg;
            layout.ColumnCount = 2;
            layout.RowCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
            PageHost.Controls.Add(layout);

            RoundedPanel settings = CreateCard();
            settings.Padding = new Padding(26);
            layout.Controls.Add(settings, 0, 0);

            TableLayoutPanel form = new TableLayoutPanel();
            form.Dock = DockStyle.Top;
            form.Height = 380;
            form.BackColor = Card;
            form.ColumnCount = 2;
            form.RowCount = 7;
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 7; i++) form.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            settings.Controls.Add(form);
            Label title = CreateSectionTitle("Export Settings");
            form.Controls.Add(title, 0, 0); form.SetColumnSpan(title, 2);

            ExportModeCombo = CreateCombo(new string[] { "HNS / Riivolution", "CTGP My Stuff", "ISO Patch Staging" }, "HNS / Riivolution");
            AddFormRow(form, "Export Mode", ExportModeCombo, 1);
            OutputFolderText = CreateTextBox(string.IsNullOrWhiteSpace(ExportOutputParentFolder) ? Path.Combine(AppRoot, "output") : ExportOutputParentFolder);
            AddFormRow(form, "Output Parent Folder", OutputFolderText, 2);
            Button browse = CreateActionButton("Browse", Accent, delegate { BrowseOutputFolder(); });
            form.Controls.Add(browse, 1, 3);
            browse.Text = "Browse Parent Folder";
            CopyMultiplayerCheck = new CheckBox();
            CopyMultiplayerCheck.Text = "Also copy track _d.szs multiplayer variants";
            CopyMultiplayerCheck.Checked = CopyMultiplayerCourseCopies;
            CopyMultiplayerCheck.Dock = DockStyle.Fill;
            CopyMultiplayerCheck.ForeColor = TextMain;
            CopyMultiplayerCheck.BackColor = Card;
            CopyMultiplayerCheck.Font = new Font("Segoe UI", 10f);
            AddFormRow(form, "Compatibility", CopyMultiplayerCheck, 4);
            CommunityFallbackCheck = new CheckBox();
            CommunityFallbackCheck.Text = "Legacy broad folder patch (unsafe for standalone character packs)";
            CommunityFallbackCheck.Checked = IncludeCommunityFallbackFolderPatch;
            CommunityFallbackCheck.Dock = DockStyle.Fill;
            CommunityFallbackCheck.ForeColor = TextMain;
            CommunityFallbackCheck.BackColor = Card;
            CommunityFallbackCheck.Font = new Font("Segoe UI", 10f);
            AddFormRow(form, "XML Style", CommunityFallbackCheck, 5);
            ExportEstimateLabel = new Label();
            ExportEstimateLabel.Text = EstimateExportSizeSummary();
            ExportEstimateLabel.Dock = DockStyle.Fill;
            ExportEstimateLabel.ForeColor = TextMain;
            ExportEstimateLabel.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
            ExportEstimateLabel.TextAlign = ContentAlignment.MiddleLeft;
            AddFormRow(form, "Estimated Size", ExportEstimateLabel, 6);

            TableLayoutPanel buttons = new TableLayoutPanel();
            buttons.Dock = DockStyle.Bottom;
            buttons.Height = 104;
            buttons.BackColor = Card;
            buttons.Padding = new Padding(0, 10, 0, 0);
            buttons.ColumnCount = 3;
            buttons.RowCount = 2;
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));
            buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            settings.Controls.Add(buttons);
            AddButtonCell(buttons, CreateActionButton("Build Export", AccentGreen, delegate { BuildExport(); }), 0, 0);
            AddButtonCell(buttons, CreateActionButton("Preview", AccentPurple, delegate { ShowExportPreviewDialog(); }), 1, 0);
            AddButtonCell(buttons, CreateActionButton("Check Sources", Color.FromArgb(76, 86, 108), delegate { ShowSourceHealthReportDialog(); }), 2, 0);
            AddButtonCell(buttons, CreateActionButton("Estimate Size", Warn, delegate { UpdateExportSizeEstimate(true); }), 0, 1);
            AddButtonCell(buttons, CreateActionButton("XML Preview", Color.FromArgb(76, 86, 108), delegate { ShowRiivolutionXmlPreviewDialog(); }), 1, 1);
            AddButtonCell(buttons, CreateActionButton("Open Output", Accent, delegate { OpenLastExportFolder(); }), 2, 1);

            RoundedPanel guide = CreateCard();
            guide.Padding = new Padding(26);
            layout.Controls.Add(guide, 1, 0);
            Label guideTitle = CreateSectionTitle("What the Export Creates");
            guideTitle.Dock = DockStyle.Top;
            guide.Controls.Add(guideTitle);
            Label guideText = new Label();
            guideText.Dock = DockStyle.Fill;
            guideText.ForeColor = TextMuted;
            guideText.Font = new Font("Segoe UI", 10.3f);
            guideText.Text = "Simple export workflow:\n  1. Click Check Sources.\n  2. Click Preview to review replacement paths.\n  3. Click Estimate Size if needed.\n  4. Click Build Export.\n\nHNS / Riivolution output:\n  hns/<pack id>/Tracks, Music, StaticR, Menu, Characters\n  riivolution/<pack id>.xml\n\nNotes:\n  Loose PNG/TXT files are source inputs only. Export patches them into base UI SZS files where possible.\n\nCTGP My Stuff creates a flat My Stuff style folder.\nISO Patch Staging creates files/Race, files/sound, files/Scene, and files/rel.";
            guide.Controls.Add(guideText);
        }

        private void ShowLogs()
        {
            SetActiveNav("Logs");
            SetHeader("Logs", "Build messages and validation details.");
            ClearPage();
            LogBox = new RichTextBox();
            LogBox.Dock = DockStyle.Fill;
            LogBox.BackColor = Color.FromArgb(12, 15, 22);
            LogBox.ForeColor = TextMain;
            LogBox.BorderStyle = BorderStyle.None;
            LogBox.Font = new Font("Consolas", 10f);
            PageHost.Controls.Add(LogBox);
            AddLog("Logs opened.");
        }

        private DataGridView CreateGrid()
        {
            DataGridView grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.BackgroundColor = Bg;
            grid.BorderStyle = BorderStyle.None;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(25, 30, 42);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = TextMain;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
            grid.ColumnHeadersHeight = 38;
            grid.DefaultCellStyle.BackColor = Card;
            grid.DefaultCellStyle.ForeColor = TextMain;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(65, 105, 176);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            grid.GridColor = Border;
            grid.RowTemplate.Height = 34;
            return grid;
        }

        private RoundedPanel CreateCard()
        {
            RoundedPanel p = new RoundedPanel();
            p.Dock = DockStyle.Fill;
            p.BackColor = Card;
            p.BorderColor = Border;
            p.Radius = 18;
            p.Margin = new Padding(0, 0, 22, 0);
            return p;
        }

        private Label CreateSectionTitle(string text)
        {
            Label label = new Label();
            label.Text = text;
            label.ForeColor = TextMain;
            label.Font = new Font("Segoe UI Semibold", 13f, FontStyle.Bold);
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
            return label;
        }

        private TextBox CreateTextBox(string text)
        {
            TextBox tb = new TextBox();
            tb.Text = text;
            tb.Dock = DockStyle.Fill;
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.BackColor = Color.FromArgb(16, 20, 29);
            tb.ForeColor = TextMain;
            tb.Font = new Font("Segoe UI", 10f);
            return tb;
        }

        private TextBox CreateMultiTextBox(string text)
        {
            TextBox tb = CreateTextBox(text);
            tb.Multiline = true;
            tb.ScrollBars = ScrollBars.Vertical;
            return tb;
        }

        private ComboBox CreateCombo(string[] items, string selected)
        {
            ComboBox c = new ComboBox();
            c.DropDownStyle = ComboBoxStyle.DropDownList;
            c.Dock = DockStyle.Fill;
            c.BackColor = Color.FromArgb(16, 20, 29);
            c.ForeColor = TextMain;
            c.FlatStyle = FlatStyle.Flat;
            c.Font = new Font("Segoe UI", 10f);
            c.Items.AddRange(items);
            int idx = Array.IndexOf(items, selected);
            c.SelectedIndex = idx >= 0 ? idx : 0;
            return c;
        }

        private Button CreateActionButton(string text, Color color, Action action)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Width = 142;
            btn.Height = 40;
            btn.Margin = new Padding(0, 0, 10, 0);
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.Click += delegate { action(); };
            return btn;
        }

        private void AddButtonCell(TableLayoutPanel table, Button button, int col, int row)
        {
            button.Dock = DockStyle.Fill;
            button.Margin = new Padding(0, 0, 10, 8);
            table.Controls.Add(button, col, row);
        }

        private void AddLabeled(TableLayoutPanel table, string label, Control control, int col, int row, int colspan = 1, int rowspan = 1)
        {
            Panel wrap = new Panel();
            wrap.Dock = DockStyle.Fill;
            wrap.BackColor = Card;
            wrap.Padding = new Padding(0, 0, 14, 6);
            Label l = new Label();
            l.Text = label;
            l.Dock = DockStyle.Top;
            l.Height = 22;
            l.ForeColor = TextMuted;
            l.Font = new Font("Segoe UI", 9f);
            control.Dock = DockStyle.Fill;
            wrap.Controls.Add(control);
            wrap.Controls.Add(l);
            table.Controls.Add(wrap, col, row);
            if (colspan > 1) table.SetColumnSpan(wrap, colspan);
            if (rowspan > 1) table.SetRowSpan(wrap, rowspan);
        }

        private void AddFormRow(TableLayoutPanel table, string label, Control control, int row)
        {
            Label l = new Label();
            l.Text = label;
            l.Dock = DockStyle.Fill;
            l.ForeColor = TextMuted;
            l.Font = new Font("Segoe UI", 10f);
            l.TextAlign = ContentAlignment.MiddleLeft;
            table.Controls.Add(l, 0, row);
            table.Controls.Add(control, 1, row);
        }

        private void RefreshSlotGrid()
        {
            if (SlotGrid == null) return;
            SlotGrid.Rows.Clear();
            for (int i = 0; i < Slots.Count; i++)
            {
                TrackSlotInfo s = Slots[i];
                string source = string.IsNullOrEmpty(s.SourcePath) ? "" : Path.GetFileName(s.SourcePath);
                string status = GetSlotStatus(s);
                int row = SlotGrid.Rows.Add(s.SlotType, s.Cup, s.TrackName, s.GameFile, s.CustomName, source, status);
                SlotGrid.Rows[row].Tag = i;
                SlotGrid.Rows[row].Cells[6].Style.ForeColor = StatusColor(status);
            }
            UpdateReadyLabels();
        }

        private void SlotGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (SlotGrid == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            SlotGrid.Rows[e.RowIndex].Selected = true;
            object tag = SlotGrid.Rows[e.RowIndex].Tag;
            int slotIndex = tag is int ? (int)tag : e.RowIndex;
            if (slotIndex < 0 || slotIndex >= Slots.Count) return;

            string columnName = SlotGrid.Columns[e.ColumnIndex].Name;
            if (columnName == "Source")
            {
                AddTrackToSlot(slotIndex);
                return;
            }

            if (columnName == "CustomName")
            {
                string current = Slots[slotIndex].CustomName;
                string value = PromptForText("Custom Track Name", "Enter the in-game custom track name:", current);
                if (value != null)
                {
                    Slots[slotIndex].CustomName = value.Trim();
                    RefreshSlotGrid();
                }
            }
        }

        private string PromptForText(string title, string prompt, string current)
        {
            using (Form dlg = new Form())
            using (Label label = new Label())
            using (TextBox box = new TextBox())
            using (Button ok = new Button())
            using (Button cancel = new Button())
            {
                dlg.Text = title;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MinimizeBox = false;
                dlg.MaximizeBox = false;
                dlg.ClientSize = new Size(520, 150);
                dlg.BackColor = Card;
                label.Text = prompt;
                label.ForeColor = TextMain;
                label.Font = new Font("Segoe UI", 9.5f);
                label.Location = new Point(16, 14);
                label.Size = new Size(488, 42);
                box.Text = current ?? "";
                box.Location = new Point(18, 60);
                box.Size = new Size(486, 28);
                box.BackColor = Color.FromArgb(16, 20, 29);
                box.ForeColor = TextMain;
                box.BorderStyle = BorderStyle.FixedSingle;
                ok.Text = "OK";
                ok.DialogResult = DialogResult.OK;
                ok.Location = new Point(328, 104);
                ok.Size = new Size(82, 30);
                ok.BackColor = Accent;
                ok.ForeColor = Color.White;
                ok.FlatStyle = FlatStyle.Flat;
                cancel.Text = "Cancel";
                cancel.DialogResult = DialogResult.Cancel;
                cancel.Location = new Point(422, 104);
                cancel.Size = new Size(82, 30);
                cancel.BackColor = Color.FromArgb(76, 86, 108);
                cancel.ForeColor = Color.White;
                cancel.FlatStyle = FlatStyle.Flat;
                dlg.Controls.Add(label);
                dlg.Controls.Add(box);
                dlg.Controls.Add(ok);
                dlg.Controls.Add(cancel);
                dlg.AcceptButton = ok;
                dlg.CancelButton = cancel;
                return dlg.ShowDialog(this) == DialogResult.OK ? box.Text : null;
            }
        }

        private Color StatusColor(string status)
        {
            if (status == "Ready") return AccentGreen;
            if (status == "Invalid") return Danger;
            if (status == "Missing") return TextMuted;
            return Warn;
        }

        private void ApplyGridEdits()
        {
            if (SlotGrid == null) return;
            SlotGrid.EndEdit();
            for (int i = 0; i < SlotGrid.Rows.Count && i < Slots.Count; i++)
            {
                object v = SlotGrid.Rows[i].Cells[4].Value;
                Slots[i].CustomName = v == null ? "" : v.ToString();
            }
        }

        private string GetSlotStatus(TrackSlotInfo s)
        {
            if (string.IsNullOrWhiteSpace(s.SourcePath)) return "Missing";
            if (!File.Exists(s.SourcePath)) return "Invalid";
            return Path.GetExtension(s.SourcePath).Equals(".szs", StringComparison.OrdinalIgnoreCase) ? "Ready" : "Invalid";
        }

        private int SelectedSlotIndex()
        {
            if (SlotGrid == null || SlotGrid.SelectedRows.Count == 0) return -1;
            object tag = SlotGrid.SelectedRows[0].Tag;
            if (tag is int) return (int)tag;
            return SlotGrid.SelectedRows[0].Index;
        }

        private void AddTrackToSelectedSlot()
        {
            int idx = SelectedSlotIndex();
            if (idx < 0) { MessageBox.Show("Select a slot first.", "MKWii Pack Maker", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            AddTrackToSlot(idx);
        }

        private void AddTrackToSlot(int idx)
        {
            if (idx < 0 || idx >= Slots.Count) return;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Choose a custom track .szs file";
            ofd.Filter = "MKWii SZS track (*.szs)|*.szs|All files (*.*)|*.*";
            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                Slots[idx].SourcePath = ofd.FileName;
                if (string.IsNullOrWhiteSpace(Slots[idx].CustomName)) Slots[idx].CustomName = GuessTrackNameFromFile(ofd.FileName);
                AddLog("Assigned " + Path.GetFileName(ofd.FileName) + " to " + Slots[idx].TrackName + ".");
                RefreshSlotGrid();
            }
        }

        private void AutoFillFolder()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select a folder containing .szs custom tracks";
            if (fbd.ShowDialog(this) != DialogResult.OK) return;
            string[] files = Directory.GetFiles(fbd.SelectedPath, "*.szs", SearchOption.TopDirectoryOnly);
            int assigned = AssignTrackFiles(files);
            AddLog("Auto-filled " + assigned + " track slot(s) from " + fbd.SelectedPath + ".");
            RefreshSlotGrid();
        }

        private int AssignTrackFiles(IEnumerable<string> files)
        {
            int assigned = 0;
            foreach (string file in files)
            {
                string name = Path.GetFileName(file).ToLowerInvariant();
                TrackSlotInfo direct = Slots.FirstOrDefault(x => x.GameFile.ToLowerInvariant() == name);
                if (direct != null)
                {
                    direct.SourcePath = file;
                    if (string.IsNullOrWhiteSpace(direct.CustomName)) direct.CustomName = GuessTrackNameFromFile(file);
                    assigned++;
                }
            }
            foreach (string file in files)
            {
                if (Slots.Any(x => string.Equals(x.SourcePath, file, StringComparison.OrdinalIgnoreCase))) continue;
                TrackSlotInfo empty = Slots.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.SourcePath));
                if (empty == null) break;
                empty.SourcePath = file;
                if (string.IsNullOrWhiteSpace(empty.CustomName)) empty.CustomName = GuessTrackNameFromFile(file);
                assigned++;
            }
            return assigned;
        }

        private void ClearSelectedSlot()
        {
            int idx = SelectedSlotIndex();
            if (idx < 0) return;
            Slots[idx].SourcePath = "";
            Slots[idx].CustomName = "";
            RefreshSlotGrid();
        }


        private void ShowTrackSzsValidatorDialog()
        {
            ApplyGridEdits();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Track SZS validation");
            sb.AppendLine("====================");
            sb.AppendLine();
            List<TrackSlotInfo> selected = Slots.Where(x => !string.IsNullOrWhiteSpace(x.SourcePath)).ToList();
            if (selected.Count == 0)
            {
                sb.AppendLine("No custom tracks have been assigned yet.");
                MessageBox.Show(this, sb.ToString(), "Track SZS Validator", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string wszst = !string.IsNullOrWhiteSpace(PatchWszstPath) && File.Exists(PatchWszstPath) ? PatchWszstPath : FindToolOnPath("wszst.exe");
            if (string.IsNullOrWhiteSpace(wszst) || !File.Exists(wszst))
            {
                sb.AppendLine("wszst.exe was not found. Basic checks only:");
                sb.AppendLine();
            }

            int ok = 0, warn = 0;
            foreach (TrackSlotInfo slot in selected)
            {
                TrackValidationResult result = ValidateTrackSzsFile(slot, wszst);
                if (result.Ok) ok++; else warn++;
                sb.AppendLine((result.Ok ? "OK" : "CHECK") + " - " + slot.Cup + " / " + slot.TrackName + " -> " + slot.GameFile);
                sb.AppendLine("  Source: " + Path.GetFileName(slot.SourcePath) + "  " + FormatBytes(FileSizeSafe(slot.SourcePath)));
                foreach (string line in result.Lines) sb.AppendLine("  " + line);
                sb.AppendLine();
            }
            sb.AppendLine("Summary: " + ok + " OK, " + warn + " need checking.");
            AddLog("Track SZS validator finished. OK=" + ok + ", Check=" + warn + ".");
            MessageBox.Show(this, sb.ToString(), "Track SZS Validator", MessageBoxButtons.OK, warn > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        private class TrackValidationResult
        {
            public bool Ok = false;
            public List<string> Lines = new List<string>();
        }

        private TrackValidationResult ValidateTrackSzsFile(TrackSlotInfo slot, string wszst)
        {
            TrackValidationResult r = new TrackValidationResult();
            string path = slot.SourcePath ?? "";
            if (!File.Exists(path))
            {
                r.Lines.Add("Missing source file.");
                return r;
            }
            if (!Path.GetExtension(path).Equals(".szs", StringComparison.OrdinalIgnoreCase))
            {
                r.Lines.Add("Not an .szs file.");
                return r;
            }
            long size = FileSizeSafe(path);
            if (size < 100 * 1024) r.Lines.Add("Very small file size; this may not be a real course archive.");
            if (string.IsNullOrWhiteSpace(wszst) || !File.Exists(wszst))
            {
                r.Ok = size >= 100 * 1024;
                r.Lines.Add("Deep archive check skipped because wszst.exe is missing.");
                return r;
            }

            string temp = Path.Combine(AppRoot, "Data", "TrackValidation", SafeId(Path.GetFileNameWithoutExtension(path)) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            try
            {
                if (Directory.Exists(temp)) Directory.Delete(temp, true);
                Directory.CreateDirectory(temp);
                string toolOutput;
                if (!RunTool(wszst, "EXTRACT " + Quote(path) + " --dest " + Quote(temp) + " --overwrite", out toolOutput))
                {
                    r.Lines.Add("wszst could not extract the archive. " + toolOutput);
                    return r;
                }
                string[] files = Directory.GetFiles(temp, "*.*", SearchOption.AllDirectories);
                Func<string, bool> has = delegate(string leaf) { return files.Any(f => Path.GetFileName(f).Equals(leaf, StringComparison.OrdinalIgnoreCase)); };
                bool hasKmp = has("course.kmp");
                bool hasKcl = has("course.kcl");
                bool hasModel = has("course_model.brres") || files.Any(f => Path.GetFileName(f).ToLowerInvariant().Contains("course") && Path.GetExtension(f).Equals(".brres", StringComparison.OrdinalIgnoreCase));
                bool hasMap = has("map_model.brres") || files.Any(f => Path.GetFileName(f).ToLowerInvariant().Contains("map") && Path.GetExtension(f).Equals(".brres", StringComparison.OrdinalIgnoreCase));
                r.Lines.Add((hasKmp ? "OK" : "MISSING") + " course.kmp gameplay data");
                r.Lines.Add((hasKcl ? "OK" : "MISSING") + " course.kcl collision");
                r.Lines.Add((hasModel ? "OK" : "MISSING") + " course model BRRES");
                r.Lines.Add((hasMap ? "OK" : "INFO") + " minimap/map model");
                r.Ok = hasKmp && hasKcl && hasModel;
            }
            catch (Exception ex)
            {
                r.Lines.Add("Validation error: " + ex.Message);
            }
            finally
            {
                try { if (Directory.Exists(temp)) Directory.Delete(temp, true); } catch { }
            }
            return r;
        }

        private List<AssetFile> AssetsFor(string category)
        {
            if (category == "Music") return MusicAssets;
            if (category == "Characters") return CharacterAssets;
            return UiRelAssets;
        }

        private DataGridView GridFor(string category)
        {
            if (category == "Music") return MusicGrid;
            if (category == "Characters") return CharacterGrid;
            return UiRelGrid;
        }


        private void ShowTrackImportWizardDialog()
        {
            try { ApplyGridEdits(); } catch { }
            string sourceRoot;
            List<string> files;
            if (!ChooseTrackImportSource(out sourceRoot, out files)) return;

            string track = FindLikelyTrackSzs(files);
            string normalMusic = FindLikelyMusic(files, false);
            string finalMusic = FindLikelyMusic(files, true);

            Form dialog = new Form();
            dialog.Text = "Track Import Wizard";
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.Size = new Size(720, 430);
            dialog.MinimumSize = new Size(620, 390);
            dialog.BackColor = Bg;

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.BackColor = Bg;
            layout.Padding = new Padding(18);
            layout.ColumnCount = 2;
            layout.RowCount = 7;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 7; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 0 ? 54 : 44));
            dialog.Controls.Add(layout);

            Label title = CreateSectionTitle("Import a track package");
            title.Text = "Import a track package";
            layout.Controls.Add(title, 0, 0); layout.SetColumnSpan(title, 2);

            ComboBox slotCombo = CreateCombo(Slots.Select((x, i) => (i + 1).ToString("00") + " - " + x.Cup + " - " + x.TrackName + " (" + x.GameFile + ")").ToArray(), "");
            if (Slots.Count > 0) slotCombo.SelectedIndex = 0;
            TextBox trackBox = CreateTextBox(track);
            TextBox nameBox = CreateTextBox(CleanDisplayNameFromFile(track));
            TextBox normalBox = CreateTextBox(normalMusic);
            TextBox finalBox = CreateTextBox(finalMusic);
            CheckBox addMusic = new CheckBox();
            addMusic.Text = "Also add detected BRSTM music to this slot";
            addMusic.Checked = File.Exists(normalMusic) || File.Exists(finalMusic);
            addMusic.ForeColor = TextMain;
            addMusic.BackColor = Bg;
            addMusic.Dock = DockStyle.Fill;

            AddWizardRow(layout, "Track Slot", slotCombo, 1);
            AddWizardRow(layout, "Track SZS", trackBox, 2);
            AddWizardRow(layout, "Custom Name", nameBox, 3);
            AddWizardRow(layout, "Normal Music", normalBox, 4);
            AddWizardRow(layout, "Final Lap Music", finalBox, 5);
            layout.Controls.Add(new Label { Text = "Music", ForeColor = TextMuted, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10f) }, 0, 6);
            layout.Controls.Add(addMusic, 1, 6);

            FlowLayoutPanel bottom = new FlowLayoutPanel();
            bottom.Dock = DockStyle.Bottom;
            bottom.Height = 62;
            bottom.FlowDirection = FlowDirection.RightToLeft;
            bottom.Padding = new Padding(0, 10, 12, 8);
            bottom.BackColor = Card;
            Button cancel = CreateActionButton("Cancel", Color.FromArgb(76, 86, 108), delegate { dialog.Close(); });
            Button import = CreateActionButton("Import", AccentGreen, delegate
            {
                int idx = slotCombo.SelectedIndex;
                if (idx < 0 || idx >= Slots.Count)
                {
                    MessageBox.Show(this, "Choose a valid track slot.", "Track Import Wizard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!File.Exists(trackBox.Text) || !trackBox.Text.EndsWith(".szs", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(this, "The detected track SZS is missing or invalid.", "Track Import Wizard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                Slots[idx].SourcePath = trackBox.Text.Trim();
                if (!string.IsNullOrWhiteSpace(nameBox.Text)) Slots[idx].CustomName = nameBox.Text.Trim();
                if (addMusic.Checked)
                {
                    string normalTarget = SuggestMusicNormalForSlot(idx);
                    string finalTarget = SuggestMusicFinalFromNormal(normalTarget);
                    if (File.Exists(normalBox.Text)) AddOrReplaceMusicAsset(normalBox.Text.Trim(), normalTarget);
                    if (File.Exists(finalBox.Text) && !string.IsNullOrWhiteSpace(finalTarget)) AddOrReplaceMusicAsset(finalBox.Text.Trim(), finalTarget);
                }
                RefreshSlotGrid();
                RefreshAssetGrid("Music");
                AddLog("Track Import Wizard imported " + Path.GetFileName(trackBox.Text) + " into " + Slots[idx].TrackName + ".");
                dialog.Close();
            });
            bottom.Controls.Add(import);
            bottom.Controls.Add(cancel);
            dialog.Controls.Add(bottom);
            dialog.ShowDialog(this);
        }

        private void AddWizardRow(TableLayoutPanel layout, string label, Control control, int row)
        {
            Label l = new Label();
            l.Text = label;
            l.Dock = DockStyle.Fill;
            l.ForeColor = TextMuted;
            l.Font = new Font("Segoe UI", 10f);
            l.TextAlign = ContentAlignment.MiddleLeft;
            layout.Controls.Add(l, 0, row);
            control.Dock = DockStyle.Fill;
            layout.Controls.Add(control, 1, row);
        }

        private bool ChooseTrackImportSource(out string sourceRoot, out List<string> files)
        {
            sourceRoot = "";
            files = new List<string>();
            DialogResult mode = MessageBox.Show(this, "Choose Yes for a .szs/.zip file.\nChoose No for a folder containing a track package.", "Track Import Wizard", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (mode == DialogResult.Cancel) return false;
            if (mode == DialogResult.No)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Choose the track package folder";
                if (fbd.ShowDialog(this) != DialogResult.OK) return false;
                sourceRoot = fbd.SelectedPath;
                files = Directory.GetFiles(sourceRoot, "*.*", SearchOption.AllDirectories).ToList();
                return true;
            }

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Choose track .szs or package .zip";
            ofd.Filter = "Track or package (*.szs;*.zip)|*.szs;*.zip|All files (*.*)|*.*";
            if (ofd.ShowDialog(this) != DialogResult.OK) return false;
            if (ofd.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                string extract = Path.Combine(AppRoot, "Data", "TrackImports", SafeId(Path.GetFileNameWithoutExtension(ofd.FileName)) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                Directory.CreateDirectory(extract);
                ZipFile.ExtractToDirectory(ofd.FileName, extract, true);
                sourceRoot = extract;
                files = Directory.GetFiles(sourceRoot, "*.*", SearchOption.AllDirectories).ToList();
                return true;
            }
            sourceRoot = Path.GetDirectoryName(ofd.FileName) ?? AppRoot;
            files = Directory.GetFiles(sourceRoot, "*.*", SearchOption.AllDirectories).ToList();
            if (!files.Contains(ofd.FileName, StringComparer.OrdinalIgnoreCase)) files.Add(ofd.FileName);
            return true;
        }

        private string FindLikelyTrackSzs(List<string> files)
        {
            return files.Where(f => f.EndsWith(".szs", StringComparison.OrdinalIgnoreCase))
                .Where(f => !Path.GetFileName(f).EndsWith("_d.szs", StringComparison.OrdinalIgnoreCase))
                .Where(f => !IsKnownUiSzs(Path.GetFileName(f)))
                .OrderByDescending(f => FileSizeSafe(f))
                .FirstOrDefault() ?? files.FirstOrDefault(f => f.EndsWith(".szs", StringComparison.OrdinalIgnoreCase)) ?? "";
        }

        private string FindLikelyMusic(List<string> files, bool finalLap)
        {
            IEnumerable<string> brstms = files.Where(f => f.EndsWith(".brstm", StringComparison.OrdinalIgnoreCase));
            string suffix = finalLap ? "_f.brstm" : "_n.brstm";
            string found = brstms.FirstOrDefault(f => Path.GetFileName(f).EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(found)) return found;
            if (!finalLap) return brstms.FirstOrDefault() ?? "";
            return "";
        }

        private string CleanDisplayNameFromFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "";
            string name = Path.GetFileNameWithoutExtension(path).Replace('_', ' ').Replace('-', ' ').Trim();
            return CultureTitleCase(name);
        }

        private string CultureTitleCase(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            string[] words = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++) words[i] = words[i].Length <= 1 ? words[i].ToUpperInvariant() : char.ToUpperInvariant(words[i][0]) + words[i].Substring(1).ToLowerInvariant();
            return string.Join(" ", words);
        }

        private string ParentDiscFolder(string disc)
        {
            disc = NormalizeDiscPath(disc);
            if (string.IsNullOrWhiteSpace(disc)) return "";
            int pos = disc.LastIndexOf('/');
            if (pos <= 0) return disc;
            return disc.Substring(0, pos);
        }

        private void ShowSelectedAssetExplanation(string category)
        {
            ApplyAssetGridEdits(category);
            DataGridView grid = GridFor(category);
            List<AssetFile> list = AssetsFor(category);
            if (grid == null || list.Count == 0)
            {
                MessageBox.Show(this, "No files are listed yet.", "Explain Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int idx = grid.SelectedRows.Count > 0 ? grid.SelectedRows[0].Index : grid.CurrentCell != null ? grid.CurrentCell.RowIndex : 0;
            if (idx < 0 || idx >= list.Count) idx = 0;
            MessageBox.Show(this, BuildAssetExplanation(list[idx], category), "Explain Selected File", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshAssetGrid(category);
        }

        private string BuildAssetExplanation(AssetFile a, string category)
        {
            GeneralAssetInfo info = AnalyzeGeneralAsset(a, category);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("File: " + Path.GetFileName(a.SourcePath));
            sb.AppendLine("Type: " + info.Kind);
            sb.AppendLine("What it changes: " + info.Usage);
            sb.AppendLine("MKWii folder: " + info.Folder);
            sb.AppendLine("Output filename: " + a.OutputName);
            sb.AppendLine("Disc/Riivolution target: " + (string.IsNullOrWhiteSpace(a.DiscPath) ? DefaultDiscPath(category, a.OutputName) : a.DiscPath));
            if (!string.IsNullOrWhiteSpace(info.Details)) sb.AppendLine(info.Details);
            if (!string.IsNullOrWhiteSpace(info.Warning)) sb.AppendLine("Warning: " + info.Warning);
            sb.AppendLine();
            sb.AppendLine("Suggested action:");
            sb.AppendLine(info.SuggestedAction);
            return sb.ToString();
        }

        private void ShowSourceHealthReportDialog()
        {
            try
            {
                ApplyGridEdits();
                ApplyAssetGridEdits("Music");
                ApplyAssetGridEdits("Characters");
                ApplyAssetGridEdits("UI / REL Assets");
            }
            catch { }
            ShowLargeTextDialog("Missing / Broken Source Checker", BuildSourceHealthReport());
        }

        private string BuildSourceHealthReport()
        {
            StringBuilder sb = new StringBuilder();
            List<string> problems = new List<string>();
            List<string> warnings = new List<string>();
            sb.AppendLine("Missing / broken source checker");
            sb.AppendLine("===============================");
            sb.AppendLine();
            foreach (TrackSlotInfo slot in Slots.Where(x => !string.IsNullOrWhiteSpace(x.SourcePath)))
            {
                if (!File.Exists(slot.SourcePath)) problems.Add("Track missing source: " + slot.TrackName + " -> " + slot.SourcePath);
                else if (!slot.SourcePath.EndsWith(".szs", StringComparison.OrdinalIgnoreCase)) problems.Add("Track is not .szs: " + slot.TrackName + " -> " + slot.SourcePath);
                else if (FileSizeSafe(slot.SourcePath) < 100 * 1024) warnings.Add("Track looks very small: " + slot.TrackName + " -> " + FormatBytes(FileSizeSafe(slot.SourcePath)));
            }
            CheckAssetHealth("Music", MusicAssets, problems, warnings);
            CheckAssetHealth("Characters", CharacterAssets, problems, warnings);
            CheckAssetHealth("Advanced", UiRelAssets, problems, warnings);

            var duplicateTargets = MusicAssets.Concat(CharacterAssets).Concat(UiRelAssets)
                .Where(a => !string.IsNullOrWhiteSpace(a.DiscPath))
                .GroupBy(a => NormalizeDiscPath(a.DiscPath), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .ToList();
            foreach (var g in duplicateTargets) warnings.Add("Duplicate target: " + g.Key + " used by " + g.Count() + " files.");

            sb.AppendLine("Problems: " + problems.Count);
            sb.AppendLine("Warnings: " + warnings.Count);
            sb.AppendLine();
            if (problems.Count == 0 && warnings.Count == 0)
            {
                sb.AppendLine("No missing sources, invalid extensions, or duplicate targets were found.");
            }
            else
            {
                if (problems.Count > 0)
                {
                    sb.AppendLine("Problems to fix before export");
                    sb.AppendLine("-----------------------------");
                    foreach (string p in problems) sb.AppendLine("- " + p);
                    sb.AppendLine();
                }
                if (warnings.Count > 0)
                {
                    sb.AppendLine("Warnings to review");
                    sb.AppendLine("------------------");
                    foreach (string w in warnings) sb.AppendLine("- " + w);
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        private void CheckAssetHealth(string label, List<AssetFile> list, List<string> problems, List<string> warnings)
        {
            foreach (AssetFile a in list)
            {
                string source = a.SourcePath ?? "";
                string outName = a.OutputName ?? "";
                string ext = Path.GetExtension(source).ToLowerInvariant();
                if (!File.Exists(source)) problems.Add(label + " missing source: " + source);
                if (string.IsNullOrWhiteSpace(outName)) problems.Add(label + " has empty output filename: " + Path.GetFileName(source));
                if (label == "Music" && ext != ".brstm" && ext != ".brsar") problems.Add("Music file has wrong extension: " + Path.GetFileName(source));
                if (label == "Advanced" && ext != ".szs" && ext != ".rel" && ext != ".brsar" && ext != ".brres") warnings.Add("Advanced file has unusual extension: " + Path.GetFileName(source));
                GeneralAssetInfo info = AnalyzeGeneralAsset(a, label == "Advanced" ? "UI / REL Assets" : label);
                if (!string.IsNullOrWhiteSpace(info.Warning)) warnings.Add(label + " check: " + Path.GetFileName(source) + " - " + info.Warning);
            }
        }

        private void ShowExportErrorHelpDialog()
        {
            ShowLargeTextDialog("Export Error Help", BuildExportErrorHelpText());
        }

        private string BuildExportErrorHelpText()
        {
            return "Export error helper\n" +
                   "===================\n\n" +
                   "If export fails, read the first real error line in Logs, then use this map:\n\n" +
                   "wszst failed -> an SZS archive may be corrupt, locked, or not a real MKWii SZS.\n" +
                   "wbmgt failed -> BMG text patching failed; check base UI files and language/region.\n" +
                   "wimgt failed -> image conversion failed; check cup icon format, size, or transparency.\n" +
                   "wit failed -> WBFS/ISO extraction failed; check the disc image path and free disk space.\n" +
                   "UnauthorizedAccess -> close Dolphin/Explorer preview or choose a writable output folder.\n" +
                   "File not found -> run Check Sources and confirm base_files are extracted.\n" +
                   "Path too long -> use a shorter project path like C:\\MKWiiPackMaker.\n\n" +
                   "Best fix order:\n" +
                   "2. Setup/Export > Check Sources.\n" +
                   "3. Tracks > Validate Track SZS.\n" +
                   "4. Export > Preview Export.\n" +
                   "5. Build again.\n";
        }

        private string ExplainExportError(string message)
        {
            string m = (message ?? "").ToLowerInvariant();
            if (m.Contains("wszst")) return "Likely cause: SZS archive extraction/build failed. Check that your track/UI SZS files are valid and not locked.";
            if (m.Contains("wbmgt") || m.Contains("bmg")) return "Likely cause: BMG text patch failed. Check Setup language/region and base Scene/UI files.";
            if (m.Contains("wimgt") || m.Contains("image")) return "Likely cause: cup/icon image conversion failed. Use PNG files with normal dimensions and transparency.";
            if (m.Contains("wit") || m.Contains("wbfs") || m.Contains("iso")) return "Likely cause: disc extraction failed. Check the WBFS/ISO file and free disk space.";
            if (m.Contains("unauthorized") || m.Contains("access")) return "Likely cause: Windows blocked file access. Close programs using the folder or choose another output folder.";
            if (m.Contains("not find") || m.Contains("not found") || m.Contains("missing")) return "Likely cause: a source or base file is missing. Run Check Sources and verify base_files.";
            if (m.Contains("path") && m.Contains("long")) return "Likely cause: Windows path length issue. Move the app/project to a shorter folder path.";
            return "Tip: run Check Sources, Preview Export, and Track SZS Validator to locate the likely bad file.";
        }


        private void AddAssetFiles(string category)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Title = "Add " + (category == "UI / REL Assets" ? "Advanced Files" : category);

            if (category == "Music")
            {
                ofd.Filter = "MKWii music (*.brstm)|*.brstm|All files (*.*)|*.*";
            }
            else if (category == "Characters")
            {
                ofd.Filter = "Character / vehicle files (*.szs;*.brres;*.brsar;*.tpl;*.png)|*.szs;*.brres;*.brsar;*.tpl;*.png|All files (*.*)|*.*";
            }
            else
            {
                ofd.Filter = "Advanced MKWii files (*.szs;*.rel;*.brsar;*.brres)|*.szs;*.rel;*.brsar;*.brres|All files (*.*)|*.*";
            }

            if (ofd.ShowDialog(this) != DialogResult.OK) return;

            int added = 0;
            foreach (string file in ofd.FileNames)
            {
                if (string.IsNullOrWhiteSpace(file) || !File.Exists(file)) continue;
                int before = AssetsFor(category).Count;
                AddAsset(category, file, null);
                if (AssetsFor(category).Count > before) added++;
            }

            RefreshAssetGrid(category);
            AddLog("Added " + added + " " + category + " file(s).");
        }

        private void ScanAssetFolder(string category)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select a folder to scan for " + category + " files";
            if (fbd.ShowDialog(this) != DialogResult.OK) return;
            string pattern = category == "Music" ? "*.brstm" : "*.*";
            string[] files = Directory.GetFiles(fbd.SelectedPath, pattern, SearchOption.AllDirectories);
            int added = 0;
            foreach (string file in files)
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (category == "Characters" && ext != ".szs" && ext != ".brres" && ext != ".brsar" && ext != ".tpl" && ext != ".png") continue;
                if (category == "UI / REL Assets" && ext != ".szs" && ext != ".rel" && ext != ".brsar" && ext != ".brres") continue;
                AddAsset(category, file, Path.GetRelativePath(fbd.SelectedPath, file)); added++;
            }
            AddLog("Scanned " + category + " folder. Added " + added + " asset(s).");
            RefreshAssetGrid(category);
        }

        private void AddAsset(string category, string file)
        {
            AddAsset(category, file, null);
        }

        private void AddAsset(string category, string file, string relativePath)
        {
            List<AssetFile> list = AssetsFor(category);
            if (list.Any(x => string.Equals(x.SourcePath, file, StringComparison.OrdinalIgnoreCase))) return;
            string output = DefaultOutputName(category, file, relativePath);
            list.Add(new AssetFile
            {
                Category = category,
                SourcePath = file,
                OutputName = output,
                DiscPath = DefaultDiscPath(category, output),
                Notes = ""
            });
        }

        private string DefaultOutputName(string category, string file, string relativePath)
        {
            string rel = string.IsNullOrWhiteSpace(relativePath) ? Path.GetFileName(file) : relativePath.Replace('\\', '/');
            rel = rel.Trim('/');
            string name = Path.GetFileName(file);
            string lowerName = name.ToLowerInvariant();

            if (category == "Music")
            {
                if (rel.ToLowerInvariant().StartsWith("music/")) rel = rel.Substring(6);
                return rel;
            }

            if (category == "Characters")
            {
                string lowerRel = rel.ToLowerInvariant();
                string[] strip = new string[] { "ctgp/karts/", "ctgp/menu/", "ctgp/text/", "ctgp/vocies/", "ctgp/voices/", "inject/" };
                foreach (string prefix in strip)
                {
                    int pos = lowerRel.IndexOf(prefix);
                    if (pos >= 0)
                    {
                        string tail = rel.Substring(pos + prefix.Length);
                        if (prefix.Contains("karts")) return "Karts/" + tail;
                        if (prefix.Contains("menu")) return "Menu/" + tail;
                        if (prefix.Contains("text")) return "Text/" + tail;
                        if (prefix.Contains("vocies") || prefix.Contains("voices")) return "Vocies/" + tail;
                        if (prefix.Contains("inject")) return "Inject/" + tail;
                    }
                }
                if (lowerName.EndsWith(".brsar")) return "Vocies/" + name;
                if (lowerName.EndsWith(".brres") || lowerName.EndsWith(".tpl") || lowerName.EndsWith(".png")) return "Inject/" + name;
                return name;
            }

            // UI / REL assets keep the structured HNS layout.
            string lowerRelUi = rel.ToLowerInvariant();
            if (lowerName.EndsWith(".rel") && IsStaticRelName(name)) return "StaticR/" + CanonicalStaticRelName(name);
            if (lowerRelUi.StartsWith("menu/")) return rel;
            if (lowerRelUi.StartsWith("language_x/")) return "Menu/Language/" + Path.GetFileName(rel);
            if (lowerRelUi.StartsWith("staticr/")) return "StaticR/" + CanonicalStaticRelName(name);
            if (lowerRelUi.StartsWith("music/")) return rel;
            if (lowerName.EndsWith(".brsar")) return "Music/" + name;
            return "Menu/" + rel;
        }

        private string DefaultDiscPath(string category, string filename)
        {
            string normalized = filename.Replace('\\', '/');
            string leaf = Path.GetFileName(normalized);
            string lower = leaf.ToLowerInvariant();
            string lowerFull = normalized.ToLowerInvariant();

            // MKWii uses /sound/strm for streamed BRSTM music, but revo_kart.brsar is the global sound archive.
            if (category == "Music")
            {
                if (lower == "revo_kart.brsar" || lower.EndsWith(".brsar")) return "/sound/revo_kart.brsar";
                return "/sound/strm/" + leaf;
            }

            if (category == "Characters")
            {
                if (lowerFull.Contains("/inject/") || lowerFull.StartsWith("inject/")) return "";
                if (lower == "revo_kart.brsar" || lowerFull.Contains("/vocies/") || lowerFull.Contains("/voices/")) return "/sound/revo_kart.brsar";

                // Driver.szs is the character selection screen driver archive and belongs to /Scene/Model.
                if (lower == "driver.szs") return "/Scene/Model/" + leaf;

                // allkart files belong to /Scene/Model/Kart, not /Race/Kart. Check before Karts/ because CTGP packs often store them under a Karts folder.
                if (lower.Contains("allkart")) return "/Scene/Model/Kart/" + leaf;

                // CTGP character packs usually split in-game kart/bike files into Karts.
                if (lowerFull.Contains("/karts/") || lowerFull.StartsWith("karts/")) return "/Race/Kart/" + leaf;

                // Text/*.szs and normal menu SZS files are /Scene/UI files. Use exact filenames only.
                if (IsKnownUiSzs(leaf) || lowerFull.Contains("/menu/") || lowerFull.StartsWith("menu/") || lowerFull.Contains("/text/") || lowerFull.StartsWith("text/")) return "/Scene/UI/" + leaf;

                if (lower.EndsWith(".brres")) return "/Scene/Model/Kart/" + leaf;
                return "/Race/Kart/" + leaf;
            }

            if (lower.EndsWith(".rel")) return IsStaticRelName(leaf) ? "/rel/StaticR.rel" : "/rel/" + leaf;
            if (lower == "revo_kart.brsar") return "/sound/revo_kart.brsar";
            if (lowerFull.StartsWith("music/")) return "/sound/strm/" + leaf;
            if (lowerFull.StartsWith("menu/") || lower.Contains("title") || lower.Contains("menu") || lower.Contains("globe") || lower.Contains("race") || lower.Contains("award") || lower == "patch.szs" || lower == "backmodel.szs") return "/Scene/UI/" + leaf;
            return "/" + leaf;
        }



        private void ShowMusicSlotHelperDialog()
        {
            try { ApplyAssetGridEdits("Music"); } catch { }

            Form dialog = new Form();
            dialog.Text = "Music Slot Helper - MKWii Pack Maker";
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.Size = new Size(820, 500);
            dialog.MinimumSize = new Size(760, 450);
            dialog.BackColor = Bg;
            dialog.ForeColor = TextMain;
            dialog.Font = new Font("Segoe UI", 10f);
            try { dialog.Icon = this.Icon; } catch { }

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.Padding = new Padding(24);
            root.BackColor = Bg;
            root.ColumnCount = 1;
            root.RowCount = 5;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 126));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            dialog.Controls.Add(root);

            Label title = CreateSectionTitle("Music Slot Helper");
            title.Text = "Music Slot Helper";
            title.Font = new Font("Segoe UI Semibold", 19f, FontStyle.Bold);
            root.Controls.Add(title, 0, 0);

            Label info = new Label();
            info.Dock = DockStyle.Fill;
            info.ForeColor = TextMuted;
            info.Text = "Pick a track slot, choose one .brstm, then add it as the normal and final-lap replacement. The target names can be edited before adding.";
            info.Font = new Font("Segoe UI", 10.3f);
            root.Controls.Add(info, 0, 1);

            TableLayoutPanel form = new TableLayoutPanel();
            form.Dock = DockStyle.Fill;
            form.BackColor = Bg;
            form.ColumnCount = 3;
            form.RowCount = 3;
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            for (int i = 0; i < 3; i++) form.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            root.Controls.Add(form, 0, 2);

            ComboBox slotCombo = CreateCombo(Slots.Select(x => x.Cup + " - " + x.TrackName + "  [" + x.GameFile + "]").ToArray(), Slots.Count > 0 ? Slots[0].Cup + " - " + Slots[0].TrackName + "  [" + Slots[0].GameFile + "]" : "");
            TextBox sourceText = CreateTextBox(SelectedMusicSourcePath());
            ComboBox normalCombo = CreateCombo(GetMusicTargetCandidates(), "");
            normalCombo.DropDownStyle = ComboBoxStyle.DropDown;
            ComboBox finalCombo = CreateCombo(GetMusicTargetCandidates(), "");
            finalCombo.DropDownStyle = ComboBoxStyle.DropDown;

            AddMusicHelperRow(form, "Track Slot", slotCombo, null, 0);
            Button browse = CreateActionButton("Browse .brstm", Accent, delegate
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Choose BRSTM music source";
                ofd.Filter = "BRSTM music (*.brstm)|*.brstm|All files (*.*)|*.*";
                if (ofd.ShowDialog(dialog) == DialogResult.OK) sourceText.Text = ofd.FileName;
            });
            AddMusicHelperRow(form, "Source BRSTM", sourceText, browse, 1);

            TableLayoutPanel targets = new TableLayoutPanel();
            targets.Dock = DockStyle.Fill;
            targets.BackColor = Bg;
            targets.ColumnCount = 2;
            targets.RowCount = 2;
            targets.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            targets.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            targets.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            targets.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Label normalLabel = new Label(); normalLabel.Text = "Normal lap target"; normalLabel.Dock = DockStyle.Fill; normalLabel.ForeColor = TextMuted; normalLabel.Font = new Font("Segoe UI", 9f);
            Label finalLabel = new Label(); finalLabel.Text = "Final lap target"; finalLabel.Dock = DockStyle.Fill; finalLabel.ForeColor = TextMuted; finalLabel.Font = new Font("Segoe UI", 9f);
            targets.Controls.Add(normalLabel, 0, 0);
            targets.Controls.Add(finalLabel, 1, 0);
            targets.Controls.Add(normalCombo, 0, 1);
            targets.Controls.Add(finalCombo, 1, 1);
            form.Controls.Add(new Label() { Text = "Targets", Dock = DockStyle.Fill, ForeColor = TextMuted, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10f) }, 0, 2);
            form.Controls.Add(targets, 1, 2);
            form.SetColumnSpan(targets, 2);

            RichTextBox preview = new RichTextBox();
            preview.Dock = DockStyle.Fill;
            preview.ReadOnly = true;
            preview.BorderStyle = BorderStyle.None;
            preview.BackColor = Color.FromArgb(16, 20, 29);
            preview.ForeColor = TextMain;
            preview.Font = new Font("Consolas", 9.4f);
            root.Controls.Add(preview, 0, 3);

            Action refreshTargets = delegate
            {
                int idx = slotCombo.SelectedIndex;
                if (idx < 0 || idx >= Slots.Count) idx = 0;
                string normal = SuggestMusicNormalForSlot(idx);
                string final = SuggestMusicFinalFromNormal(normal);
                normalCombo.Text = normal;
                finalCombo.Text = final;
                preview.Text = "Suggested replacements:\n" +
                               "  /sound/strm/" + normalCombo.Text + "\n" +
                               "  /sound/strm/" + finalCombo.Text + "\n\n" +
                               "Tip: if the suggested target is wrong for your pack, choose the exact BRSTM filename from your base Sound/strm folder.";
            };
            slotCombo.SelectedIndexChanged += delegate { refreshTargets(); };
            normalCombo.TextChanged += delegate
            {
                string final = SuggestMusicFinalFromNormal(normalCombo.Text);
                if (!string.IsNullOrWhiteSpace(final)) finalCombo.Text = final;
            };
            refreshTargets();

            FlowLayoutPanel buttons = new FlowLayoutPanel();
            buttons.Dock = DockStyle.Fill;
            buttons.FlowDirection = FlowDirection.RightToLeft;
            buttons.BackColor = Bg;
            buttons.Padding = new Padding(0, 12, 0, 0);
            root.Controls.Add(buttons, 0, 4);
            buttons.Controls.Add(CreateActionButton("Close", Color.FromArgb(76, 86, 108), delegate { dialog.Close(); }));
            buttons.Controls.Add(CreateActionButton("Add Normal + Final", AccentGreen, delegate
            {
                if (AddMusicReplacementPair(sourceText.Text, normalCombo.Text, finalCombo.Text)) dialog.Close();
            }));
            buttons.Controls.Add(CreateActionButton("Add Normal Only", Accent, delegate
            {
                if (AddMusicReplacementPair(sourceText.Text, normalCombo.Text, "")) dialog.Close();
            }));

            dialog.ShowDialog(this);
        }

        private void AddMusicHelperRow(TableLayoutPanel table, string label, Control control, Control button, int row)
        {
            Label l = new Label();
            l.Text = label;
            l.Dock = DockStyle.Fill;
            l.ForeColor = TextMuted;
            l.Font = new Font("Segoe UI", 10f);
            l.TextAlign = ContentAlignment.MiddleLeft;
            table.Controls.Add(l, 0, row);
            table.Controls.Add(control, 1, row);
            if (button != null) table.Controls.Add(button, 2, row);
            else table.SetColumnSpan(control, 2);
        }

        private string SelectedMusicSourcePath()
        {
            try
            {
                if (MusicGrid != null && MusicGrid.SelectedRows.Count > 0)
                {
                    int idx = MusicGrid.SelectedRows[0].Index;
                    if (idx >= 0 && idx < MusicAssets.Count) return MusicAssets[idx].SourcePath;
                }
            }
            catch { }
            return MusicAssets.FirstOrDefault(a => File.Exists(a.SourcePath))?.SourcePath ?? "";
        }

        private string[] GetMusicTargetCandidates()
        {
            SortedSet<string> names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            string[] strmFolders = new string[]
            {
                Path.Combine(AppRoot, "base_files", "Sound", "strm"),
                Path.Combine(AppRoot, "base_files", "sound", "strm")
            };
            foreach (string dir in strmFolders)
            {
                try
                {
                    if (!Directory.Exists(dir)) continue;
                    foreach (string file in Directory.GetFiles(dir, "*.brstm", SearchOption.TopDirectoryOnly)) names.Add(Path.GetFileName(file));
                }
                catch { }
            }
            foreach (string n in KnownMusicFallbackNames()) names.Add(n);
            return names.ToArray();
        }

        private string[] KnownMusicFallbackNames()
        {
            List<string> list = new List<string>();
            foreach (string normal in DefaultMusicNormalByGameFile().Values.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                list.Add(normal);
                string final = SuggestMusicFinalFromNormal(normal);
                if (!string.IsNullOrWhiteSpace(final)) list.Add(final);
            }
            return list.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToArray();
        }

        private Dictionary<string, string> DefaultMusicNormalByGameFile()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "beginner_course.szs", "n_Circuit32_n.brstm" },
                { "farm_course.szs", "n_Farm_n.brstm" },
                { "kinoko_course.szs", "n_Kinoko_n.brstm" },
                { "factory_course.szs", "n_Factory_n.brstm" },
                { "castle_course.szs", "n_Circuit32_n.brstm" },
                { "shopping_course.szs", "n_Shopping_n.brstm" },
                { "boardcross_course.szs", "n_Snowboard_n.brstm" },
                { "truck_course.szs", "n_Truck_n.brstm" },
                { "senior_course.szs", "n_Senior_n.brstm" },
                { "water_course.szs", "n_Water_n.brstm" },
                { "treehouse_course.szs", "n_Treehouse_n.brstm" },
                { "volcano_course.szs", "n_Volcano_n.brstm" },
                { "desert_course.szs", "n_Desert_n.brstm" },
                { "ridgehighway_course.szs", "n_RidgeHighway_n.brstm" },
                { "koopa_course.szs", "n_Koopa_n.brstm" },
                { "rainbow_course.szs", "n_Rainbow_n.brstm" },
                { "old_peach_gc.szs", "r_GC_Beach_n.brstm" },
                { "old_falls_ds.szs", "r_DS_Falls_n.brstm" },
                { "old_obake_sfc.szs", "r_SFC_Obake_n.brstm" },
                { "old_mario_64.szs", "r_64_Mario_n.brstm" },
                { "old_sherbet_64.szs", "r_64_Sherbet_n.brstm" },
                { "old_heyho_gba.szs", "r_GBA_Heyho_n.brstm" },
                { "old_town_ds.szs", "r_DS_Town_n.brstm" },
                { "old_waluigi_gc.szs", "r_GC_Waluigi_n.brstm" },
                { "old_desert_ds.szs", "r_DS_Desert_n.brstm" },
                { "old_koopa_gba.szs", "r_GBA_Koopa_n.brstm" },
                { "old_donkey_64.szs", "r_64_Donkey_n.brstm" },
                { "old_mario_gc.szs", "r_GC_Mario_n.brstm" },
                { "old_mario_sfc.szs", "r_SFC_Mario_n.brstm" },
                { "old_garden_ds.szs", "r_DS_Garden_n.brstm" },
                { "old_donkey_gc.szs", "r_GC_Donkey_n.brstm" },
                { "old_koopa_64.szs", "r_64_Koopa_n.brstm" }
            };
        }

        private string SuggestMusicNormalForSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return "";
            string gameFile = Slots[slotIndex].GameFile ?? "";
            Dictionary<string, string> map = DefaultMusicNormalByGameFile();
            return map.ContainsKey(gameFile) ? map[gameFile] : "";
        }

        private string SuggestMusicFinalFromNormal(string normal)
        {
            if (string.IsNullOrWhiteSpace(normal)) return "";
            string n = Path.GetFileName(normal.Trim());
            if (n.EndsWith("_n.brstm", StringComparison.OrdinalIgnoreCase)) return n.Substring(0, n.Length - "_n.brstm".Length) + "_f.brstm";
            if (n.EndsWith("_N.brstm", StringComparison.OrdinalIgnoreCase)) return n.Substring(0, n.Length - "_N.brstm".Length) + "_F.brstm";
            if (n.EndsWith(".brstm", StringComparison.OrdinalIgnoreCase)) return Path.GetFileNameWithoutExtension(n) + "_f.brstm";
            return n;
        }

        private bool AddMusicReplacementPair(string sourcePath, string normalTarget, string finalTarget)
        {
            sourcePath = (sourcePath ?? "").Trim();
            normalTarget = SafeMusicTargetName(normalTarget);
            finalTarget = SafeMusicTargetName(finalTarget);
            if (!File.Exists(sourcePath) || !sourcePath.EndsWith(".brstm", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "Choose a valid .brstm source file first.", "Music Slot Helper", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(normalTarget) && string.IsNullOrWhiteSpace(finalTarget))
            {
                MessageBox.Show(this, "Choose at least one BRSTM target filename.", "Music Slot Helper", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            int added = 0;
            if (!string.IsNullOrWhiteSpace(normalTarget)) { AddOrReplaceMusicAsset(sourcePath, normalTarget); added++; }
            if (!string.IsNullOrWhiteSpace(finalTarget) && !string.Equals(finalTarget, normalTarget, StringComparison.OrdinalIgnoreCase)) { AddOrReplaceMusicAsset(sourcePath, finalTarget); added++; }
            RefreshAssetGrid("Music");
            AddLog("Music Slot Helper added/updated " + added + " BRSTM replacement(s).");
            return true;
        }

        private string SafeMusicTargetName(string target)
        {
            string name = Path.GetFileName((target ?? "").Trim().Replace('\\', '/'));
            if (string.IsNullOrWhiteSpace(name)) return "";
            if (!name.EndsWith(".brstm", StringComparison.OrdinalIgnoreCase)) name += ".brstm";
            return name;
        }

        private void AddOrReplaceMusicAsset(string sourcePath, string targetName)
        {
            AssetFile existing = MusicAssets.FirstOrDefault(a => string.Equals(Path.GetFileName(a.OutputName ?? ""), targetName, StringComparison.OrdinalIgnoreCase) || string.Equals(Path.GetFileName(a.DiscPath ?? ""), targetName, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                MusicAssets.Add(new AssetFile
                {
                    Category = "Music",
                    SourcePath = sourcePath,
                    OutputName = targetName,
                    DiscPath = "/sound/strm/" + targetName,
                    Notes = "Added by Music Slot Helper"
                });
            }
            else
            {
                existing.SourcePath = sourcePath;
                existing.OutputName = targetName;
                existing.DiscPath = "/sound/strm/" + targetName;
                existing.Notes = "Updated by Music Slot Helper";
            }
        }

        private void OpenLastExportFolder()
        {
            string folder = LastExportFolder;
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                string parent = OutputFolderText != null && !string.IsNullOrWhiteSpace(OutputFolderText.Text) ? OutputFolderText.Text : (string.IsNullOrWhiteSpace(ExportOutputParentFolder) ? Path.Combine(AppRoot, "output") : ExportOutputParentFolder);
                if (Directory.Exists(parent)) folder = parent;
            }
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                MessageBox.Show(this, "No exported pack folder was found yet. Build an export first.", "Open Last Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            OpenFolder(folder);
        }

        private void OpenFolder(string folder)
        {
            try
            {
                if (!Directory.Exists(folder)) return;
                Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not open folder:\n" + ex.Message, "Open Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RemoveSelectedAsset(string category)
        {
            DataGridView grid = GridFor(category);
            if (grid == null || grid.SelectedRows.Count == 0) return;
            int idx = grid.SelectedRows[0].Index;
            List<AssetFile> list = AssetsFor(category);
            if (idx >= 0 && idx < list.Count) list.RemoveAt(idx);
            RefreshAssetGrid(category);
        }

        private void ApplyAssetGridEdits(string category)
        {
            DataGridView grid = GridFor(category);
            if (grid == null) return;
            List<AssetFile> list = AssetsFor(category);
            for (int i = 0; i < grid.Rows.Count && i < list.Count; i++)
            {
                if (category == "Characters")
                {
                    list[i].OutputName = SafeRelativePathForProject(Cell(grid, i, 3));
                    list[i].DiscPath = NormalizeDiscPath(Cell(grid, i, 4));
                    string notes = Cell(grid, i, 6);
                    CharacterAssetInfo info = AnalyzeCharacterAsset(list[i]);
                    if (notes == info.Warning) notes = "";
                    list[i].Notes = notes;
                }
                else
                {
                    list[i].OutputName = SafeRelativePathForProject(Cell(grid, i, 3));
                    list[i].DiscPath = NormalizeDiscPath(Cell(grid, i, 4));
                    list[i].Notes = Cell(grid, i, 6);
                }
            }
        }

        private string Cell(DataGridView grid, int row, int col)
        {
            object v = grid.Rows[row].Cells[col].Value;
            return v == null ? "" : v.ToString();
        }

        private void RefreshAssetGrid(string category)
        {
            DataGridView grid = GridFor(category);
            if (grid == null) return;
            grid.Rows.Clear();
            List<AssetFile> list = AssetsFor(category);
            List<string> characterConflicts = category == "Characters" ? BuildCharacterConflictWarningsForGrid(list) : new List<string>();
            for (int ai = 0; ai < list.Count; ai++)
            {
                AssetFile a = list[ai];
                if (category == "Characters")
                {
                    CharacterAssetInfo info = AnalyzeCharacterAsset(a);
                    string conflict = ai < characterConflicts.Count ? characterConflicts[ai] : "";
                    string combinedWarning = CombineWarnings(info.Warning, conflict);
                    string status = File.Exists(a.SourcePath) ? (string.IsNullOrWhiteSpace(combinedWarning) ? "Ready" : "Check") : "Invalid";
                    string notes = string.IsNullOrWhiteSpace(a.Notes) ? combinedWarning : CombineWarnings(a.Notes, conflict);
                    int row = grid.Rows.Add(GetCharacterGridIcon(info.Code, info.CharacterName), info.CharacterName, info.FileType, a.OutputName, a.DiscPath, Path.GetFileName(a.SourcePath), notes, status);
                    grid.Rows[row].Cells[7].Style.ForeColor = StatusColor(status);
                    if (!string.IsNullOrWhiteSpace(combinedWarning)) grid.Rows[row].Cells[6].Style.ForeColor = Warn;
                    if (info.CharacterName == "Unknown") grid.Rows[row].Cells[1].Style.ForeColor = Warn;
                }
                else
                {
                    GeneralAssetInfo info = AnalyzeGeneralAsset(a, category);
                    string status = File.Exists(a.SourcePath) ? (string.IsNullOrWhiteSpace(info.Warning) ? "Ready" : "Check") : "Invalid";
                    string notes = string.IsNullOrWhiteSpace(a.Notes) ? info.Warning : CombineWarnings(a.Notes, info.Warning);
                    int row = grid.Rows.Add(info.Kind, info.Usage, info.Folder, a.OutputName, a.DiscPath, Path.GetFileName(a.SourcePath), notes, FormatBytes(FileSizeSafe(a.SourcePath)), status);
                    grid.Rows[row].Cells[8].Style.ForeColor = StatusColor(status);
                    if (!string.IsNullOrWhiteSpace(info.Warning)) grid.Rows[row].Cells[6].Style.ForeColor = Warn;
                }
            }
            UpdateReadyLabels();
        }


        private class GeneralAssetInfo
        {
            public string Kind = "Asset";
            public string Usage = "Unknown replacement";
            public string Folder = "Unknown";
            public string Warning = "";
            public string Details = "";
            public string SuggestedAction = "Check the target path before exporting.";
        }

        private GeneralAssetInfo AnalyzeGeneralAsset(AssetFile a, string category)
        {
            GeneralAssetInfo info = new GeneralAssetInfo();
            string output = (a.OutputName ?? "").Replace('\\', '/');
            string leaf = Path.GetFileName(string.IsNullOrWhiteSpace(output) ? (a.SourcePath ?? "") : output);
            string lower = (leaf ?? "").ToLowerInvariant();
            string disc = NormalizeDiscPath(string.IsNullOrWhiteSpace(a.DiscPath) ? DefaultDiscPath(category, output) : a.DiscPath);

            if (category == "Music")
            {
                info.Folder = disc.StartsWith("/sound/strm", StringComparison.OrdinalIgnoreCase) ? "/sound/strm" : "/sound";
                if (lower.EndsWith(".brsar"))
                {
                    info.Kind = "Sound archive";
                    info.Usage = "Global sounds / voices";
                    info.SuggestedAction = "Only replace revo_kart.brsar when the mod specifically includes one.";
                }
                else if (lower.EndsWith("_f.brstm"))
                {
                    info.Kind = "Final-lap BRSTM";
                    info.Usage = "Final lap music";
                    info.SuggestedAction = "Keep it paired with the matching *_n.brstm normal-lap file.";
                }
                else if (lower.EndsWith("_n.brstm"))
                {
                    info.Kind = "Normal BRSTM";
                    info.Usage = "Normal race music";
                    info.SuggestedAction = "Add or auto-pair the matching *_f.brstm final-lap file.";
                }
                else if (lower.EndsWith(".brstm"))
                {
                    info.Kind = "BRSTM music";
                    info.Usage = "Music replacement";
                    info.SuggestedAction = "Use Music Slot Helper if this should replace a specific track slot.";
                }
                else
                {
                    info.Kind = "Music asset";
                    info.Usage = "Unusual music item";
                    info.SuggestedAction = "Move non-music files to Advanced Files unless you know this target is correct.";
                }

                if (!lower.EndsWith(".brstm") && !lower.EndsWith(".brsar")) info.Warning = "Music tab normally expects .brstm music or revo_kart.brsar.";
                else if (lower.EndsWith(".brstm") && !disc.StartsWith("/sound/strm", StringComparison.OrdinalIgnoreCase)) info.Warning = "BRSTM usually targets /sound/strm.";
                else if (lower.EndsWith(".brsar") && !disc.Equals("/sound/revo_kart.brsar", StringComparison.OrdinalIgnoreCase)) info.Warning = "BRSAR usually targets /sound/revo_kart.brsar.";

                string pair = MusicPairName(leaf);
                if (!string.IsNullOrWhiteSpace(pair)) info.Details = "Expected pair: " + pair;
                return info;
            }

            if (lower.EndsWith(".rel"))
            {
                info.Kind = IsStaticRelName(leaf) ? "StaticR REL" : "REL file";
                info.Usage = IsStaticRelName(leaf) ? "Game code / region behavior" : "Advanced code module";
                info.Folder = "/rel";
                info.SuggestedAction = "Use only when the mod author supplies a matching region REL.";
            }
            else if (lower.Equals("revo_kart.brsar"))
            {
                info.Kind = "Sound archive";
                info.Usage = "Global sound effects / voices";
                info.Folder = "/sound";
                info.SuggestedAction = "Only replace this when the pack intentionally changes voices or sound effects.";
            }
            else if (IsKnownUiSzs(leaf))
            {
                info.Kind = "UI SZS archive";
                info.Usage = "Menu, text, icons, UI screens";
                info.Folder = "/Scene/UI";
                info.SuggestedAction = "Good for menu/text/icon replacements. Keep the original MKWii filename.";
            }
            else if (lower.EndsWith(".szs"))
            {
                info.Kind = "SZS archive";
                info.Usage = disc.StartsWith("/Scene/UI", StringComparison.OrdinalIgnoreCase) ? "UI/archive replacement" : "Generic archive replacement";
                info.Folder = string.IsNullOrWhiteSpace(disc) ? "Unknown" : ParentDiscFolder(disc);
                info.SuggestedAction = "Check whether this belongs in Tracks, Characters, or Advanced Files.";
            }
            else if (lower.EndsWith(".brres"))
            {
                info.Kind = "BRRES model/resource";
                info.Usage = "Model/texture resource";
                info.Folder = string.IsNullOrWhiteSpace(disc) ? "Usually inside SZS" : ParentDiscFolder(disc);
                info.SuggestedAction = "BRRES is often source material inside an SZS; direct replacement is advanced.";
            }
            else
            {
                info.Kind = "Advanced asset";
                info.Usage = "Unknown advanced file";
                info.Folder = string.IsNullOrWhiteSpace(disc) ? "Unknown" : ParentDiscFolder(disc);
                info.SuggestedAction = "Check the source mod readme for the exact target path.";
            }

            if (lower.EndsWith(".rel") && !disc.StartsWith("/rel", StringComparison.OrdinalIgnoreCase)) info.Warning = "REL files usually target /rel.";
            else if (IsKnownUiSzs(leaf) && !disc.StartsWith("/Scene/UI", StringComparison.OrdinalIgnoreCase)) info.Warning = "Known UI SZS usually targets /Scene/UI.";
            else if (lower.Equals("revo_kart.brsar") && !disc.Equals("/sound/revo_kart.brsar", StringComparison.OrdinalIgnoreCase)) info.Warning = "revo_kart.brsar should target /sound/revo_kart.brsar.";
            else if (string.IsNullOrWhiteSpace(disc)) info.Warning = "No direct disc target. This may be source-only or incomplete.";
            return info;
        }

        private long FileSizeSafe(string path)
        {
            try { if (!string.IsNullOrWhiteSpace(path) && File.Exists(path)) return new FileInfo(path).Length; }
            catch { }
            return 0;
        }

        private string MusicPairName(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename)) return "";
            string name = Path.GetFileName(filename);
            if (name.EndsWith("_n.brstm", StringComparison.OrdinalIgnoreCase)) return name.Substring(0, name.Length - 8) + "_f.brstm";
            if (name.EndsWith("_f.brstm", StringComparison.OrdinalIgnoreCase)) return name.Substring(0, name.Length - 8) + "_n.brstm";
            return "";
        }

        private void ShowMusicReportDialog()
        {
            ApplyAssetGridEdits("Music");
            MessageBox.Show(this, BuildMusicReportText(), "Music File Identification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshAssetGrid("Music");
        }

        private string BuildMusicReportText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Music file report");
            sb.AppendLine("=================");
            sb.AppendLine();
            if (MusicAssets.Count == 0)
            {
                sb.AppendLine("No music files have been added yet.");
                return sb.ToString();
            }
            int normal = 0, final = 0, other = 0, brsar = 0, missing = 0;
            foreach (AssetFile a in MusicAssets)
            {
                GeneralAssetInfo info = AnalyzeGeneralAsset(a, "Music");
                if (!File.Exists(a.SourcePath)) missing++;
                if (info.Kind.StartsWith("Normal")) normal++;
                else if (info.Kind.StartsWith("Final")) final++;
                else if (info.Kind.Contains("archive")) brsar++;
                else other++;
                sb.AppendLine(info.Kind + " - " + Path.GetFileName(a.OutputName));
                sb.AppendLine("  What it changes: " + info.Usage);
                sb.AppendLine("  MKWii folder: " + info.Folder);
                sb.AppendLine("  Target: " + (string.IsNullOrWhiteSpace(a.DiscPath) ? DefaultDiscPath("Music", a.OutputName) : a.DiscPath));
                sb.AppendLine("  Source: " + Path.GetFileName(a.SourcePath) + "  " + FormatBytes(FileSizeSafe(a.SourcePath)));
                sb.AppendLine("  Suggested action: " + info.SuggestedAction);
                string pair = MusicPairName(Path.GetFileName(a.OutputName));
                if (!string.IsNullOrWhiteSpace(pair))
                {
                    bool hasPair = MusicAssets.Any(x => Path.GetFileName(x.OutputName).Equals(pair, StringComparison.OrdinalIgnoreCase));
                    sb.AppendLine("  Pair: " + pair + "  " + (hasPair ? "OK" : "MISSING"));
                }
                if (!string.IsNullOrWhiteSpace(info.Warning)) sb.AppendLine("  Check: " + info.Warning);
                sb.AppendLine();
            }
            sb.AppendLine("Summary");
            sb.AppendLine("-------");
            sb.AppendLine("Normal lap files: " + normal);
            sb.AppendLine("Final lap files: " + final);
            sb.AppendLine("Sound archives: " + brsar);
            sb.AppendLine("Other music entries: " + other);
            sb.AppendLine("Missing source files: " + missing);
            return sb.ToString();
        }

        private void AutoPairMusicAssets()
        {
            ApplyAssetGridEdits("Music");
            int added = 0;
            List<AssetFile> snapshot = MusicAssets.ToList();
            foreach (AssetFile a in snapshot)
            {
                string leaf = Path.GetFileName(a.OutputName);
                if (string.IsNullOrWhiteSpace(leaf) || !leaf.EndsWith("_n.brstm", StringComparison.OrdinalIgnoreCase)) continue;
                string final = SuggestMusicFinalFromNormal(leaf);
                if (string.IsNullOrWhiteSpace(final)) continue;
                if (MusicAssets.Any(x => Path.GetFileName(x.OutputName).Equals(final, StringComparison.OrdinalIgnoreCase))) continue;
                AddOrReplaceMusicAsset(a.SourcePath, final);
                AssetFile created = MusicAssets.FirstOrDefault(x => Path.GetFileName(x.OutputName).Equals(final, StringComparison.OrdinalIgnoreCase));
                if (created != null) created.Notes = "Auto-paired from " + leaf + "; replace source with a true final-lap BRSTM if you have one.";
                added++;
            }
            RefreshAssetGrid("Music");
            MessageBox.Show(this, added == 0 ? "No missing final-lap pairs were found from *_n.brstm entries." : "Added " + added + " final-lap pair placeholder(s).", "Auto Pair Music", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowAdvancedFileReportDialog()
        {
            ApplyAssetGridEdits("UI / REL Assets");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Advanced file identification");
            sb.AppendLine("============================");
            sb.AppendLine();
            if (UiRelAssets.Count == 0)
            {
                sb.AppendLine("No advanced files have been added yet.");
            }
            foreach (AssetFile a in UiRelAssets)
            {
                GeneralAssetInfo info = AnalyzeGeneralAsset(a, "UI / REL Assets");
                sb.AppendLine(info.Kind + " - " + Path.GetFileName(a.OutputName));
                sb.AppendLine("  What it changes: " + info.Usage);
                sb.AppendLine("  MKWii folder: " + info.Folder);
                sb.AppendLine("  Target: " + (string.IsNullOrWhiteSpace(a.DiscPath) ? DefaultDiscPath("UI / REL Assets", a.OutputName) : a.DiscPath));
                sb.AppendLine("  Source: " + Path.GetFileName(a.SourcePath) + "  " + FormatBytes(FileSizeSafe(a.SourcePath)));
                sb.AppendLine("  Suggested action: " + info.SuggestedAction);
                if (!string.IsNullOrWhiteSpace(info.Warning)) sb.AppendLine("  Check: " + info.Warning);
                sb.AppendLine();
            }
            MessageBox.Show(this, sb.ToString(), "Advanced File Identification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshAssetGrid("UI / REL Assets");
        }

        private class CharacterAssetInfo
        {
            public string Code = "";
            public string CharacterName = "Unknown";
            public string FileType = "Character asset";
            public string Warning = "";
            public string SortKey = "zz_unknown";
        }

        private readonly Dictionary<string, string> CharacterCodeNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "mr", "Mario" }, { "lg", "Luigi" }, { "pc", "Peach" }, { "ds", "Daisy" },
            { "ys", "Yoshi" }, { "ca", "Birdo" }, { "br", "Birdo" }, { "wr", "Wario" },
            { "wl", "Waluigi" }, { "dk", "Donkey Kong" }, { "dd", "Diddy Kong" },
            { "kp", "Bowser" }, { "jr", "Bowser Jr." }, { "ko", "Koopa Troopa" }, { "nk", "Koopa Troopa" },
            { "ka", "Dry Bones" }, { "kb", "King Boo" }, { "rs", "Rosalina" }, { "fk", "Funky Kong" },
            { "bk", "Dry Bowser" }, { "kt", "Toad" }, { "kk", "Toadette" },
            { "bm", "Baby Mario" }, { "bl", "Baby Luigi" }, { "bp", "Baby Peach" }, { "bd", "Baby Daisy" },
            { "mi", "Mii" }, { "mii", "Mii" }, { "mii_a", "Mii Outfit A" }, { "mii_b", "Mii Outfit B" }
        };

        private CharacterAssetInfo AnalyzeCharacterAsset(AssetFile a)
        {
            CharacterAssetInfo info = new CharacterAssetInfo();
            string output = (a.OutputName ?? "").Replace('\\', '/');
            string sourceLeaf = Path.GetFileName(a.SourcePath ?? "");
            string leaf = Path.GetFileName(string.IsNullOrWhiteSpace(output) ? sourceLeaf : output);
            string lowerLeaf = (leaf ?? "").ToLowerInvariant();
            string combined = (output + "/" + sourceLeaf + "/" + (a.DiscPath ?? "")).Replace('\\', '/').ToLowerInvariant();
            string ext = Path.GetExtension(lowerLeaf);

            info.Code = DetectCharacterCodeFromText(combined);
            info.CharacterName = CharacterNameFromCode(info.Code);
            if (info.CharacterName == "Unknown" && combined.Contains("driver.szs")) info.CharacterName = "All Characters";
            if (info.CharacterName == "Unknown" && (combined.Contains("menusingle") || combined.Contains("menumulti") || combined.Contains("race.szs") || combined.Contains("menu/") || combined.Contains("text/"))) info.CharacterName = "Menu UI";
            if (info.CharacterName == "Unknown" && (combined.Contains("revo_kart.brsar") || combined.Contains("vocies/") || combined.Contains("voices/"))) info.CharacterName = "All Characters";

            if (combined.Contains("revo_kart.brsar") || combined.Contains("vocies/") || combined.Contains("voices/")) info.FileType = "Character voices";
            else if (lowerLeaf == "driver.szs") info.FileType = "Character select model";
            else if (combined.Contains("/inject/driver/") || (combined.Contains("inject/driver/") && ext == ".brres")) info.FileType = "Driver source model";
            else if (lowerLeaf.Contains("allkart")) info.FileType = "Vehicle select allkart";
            else if (combined.Contains("/race/kart/") || combined.Contains("/karts/") || combined.StartsWith("karts/") || IsKartBikeModelName(lowerLeaf)) info.FileType = lowerLeaf.Contains("bike") ? "In-race bike model" : "In-race kart model";
            else if (IsKnownUiSzs(leaf) || combined.Contains("/menu/") || combined.Contains("/text/")) info.FileType = "Menu/Race UI archive";
            else if (combined.Contains("/inject/img/") || ext == ".png" || ext == ".tpl") info.FileType = "UI icon source";
            else if (ext == ".brres") info.FileType = "BRRES model source";
            else info.FileType = "Character asset";

            if (info.CharacterName == "Unknown" && (info.FileType.Contains("In-race") || info.FileType.Contains("allkart")))
                info.Warning = "Could not detect character code from filename.";
            else if (string.IsNullOrWhiteSpace(a.DiscPath) && (info.FileType.Contains("source") || info.FileType.Contains("icon")))
                info.Warning = "Source-only file: patched into UI/Driver during export.";
            else if (info.FileType == "Menu/Race UI archive" && (combined.Contains("/characters/") || combined.Contains("/menu/") || combined.Contains("/text/")))
                info.Warning = "Full UI archive is merged safely; not exported directly over cup icons.";

            info.SortKey = (info.CharacterName == "Unknown" ? "zz_unknown" : info.CharacterName) + "|" + info.FileType + "|" + leaf;
            return info;
        }

        private string DetectCharacterCodeFromText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            string lower = text.Replace('\\', '/').ToLowerInvariant();
            Match m;
            m = Regex.Match(lower, @"(?:^|/)([a-z0-9_]{2,6})-allkart(?:[_-]battle)?\.szs$");
            if (m.Success) return NormalizeCharacterCode(m.Groups[1].Value);
            m = Regex.Match(lower, @"allkart[_-]([a-z0-9_]{2,6})(?:[_-]battle)?\.szs$");
            if (m.Success) return NormalizeCharacterCode(m.Groups[1].Value);
            m = Regex.Match(lower, @"-([a-z0-9_]{2,6})(?:_[24])?\.(?:szs|brres|tpl|png)$");
            if (m.Success) return NormalizeCharacterCode(m.Groups[1].Value);
            m = Regex.Match(lower, @"(?:^|/)([a-z0-9_]{2,6})(?:_[a-z0-9]+)?\.(?:brres|tpl|png)$");
            if (m.Success && CharacterCodeNames.ContainsKey(NormalizeCharacterCode(m.Groups[1].Value))) return NormalizeCharacterCode(m.Groups[1].Value);
            foreach (string code in CharacterCodeNames.Keys.OrderByDescending(k => k.Length))
            {
                if (Regex.IsMatch(lower, @"(^|[-_/])" + Regex.Escape(code.ToLowerInvariant()) + @"($|[-_/.])")) return code;
            }
            return "";
        }

        private string NormalizeCharacterCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return "";
            code = code.Trim().Trim('_', '-').ToLowerInvariant();
            if (code.EndsWith("_4") || code.EndsWith("_2")) code = code.Substring(0, code.Length - 2);
            return code;
        }

        private string CharacterNameFromCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return "Unknown";
            string name;
            if (CharacterCodeNames.TryGetValue(code, out name)) return name;
            return "Unknown";
        }

        private bool IsKartBikeModelName(string lowerLeaf)
        {
            return Regex.IsMatch(lowerLeaf ?? "", @"^[sml][a-z0-9_]*_(kart|bike)-[a-z0-9_]+(?:_[24])?\.szs$", RegexOptions.IgnoreCase)
                || Regex.IsMatch(lowerLeaf ?? "", @"^[a-z]{1,4}_(kart|bike)[a-z0-9_]*-[a-z0-9_]+(?:_[24])?\.szs$", RegexOptions.IgnoreCase);
        }

        private Image GetCharacterGridIcon(string code, string name)
        {
            string key = string.IsNullOrWhiteSpace(code) ? name : code;
            if (string.IsNullOrWhiteSpace(key)) key = "unknown";
            Image cached;
            if (CharacterIconCache.TryGetValue(key, out cached)) return cached;

            string iconPath = FindCharacterIconFile(code, name);
            if (!string.IsNullOrWhiteSpace(iconPath) && File.Exists(iconPath))
            {
                try
                {
                    using (Image img = Image.FromFile(iconPath))
                    {
                        Bitmap loaded = new Bitmap(img, new Size(32, 32));
                        CharacterIconCache[key] = loaded;
                        return loaded;
                    }
                }
                catch { }
            }

            Bitmap generated = GenerateCharacterPlaceholderIcon(code, name);
            CharacterIconCache[key] = generated;
            return generated;
        }

        private string FindCharacterIconFile(string code, string name)
        {
            try
            {
                string dir = Path.Combine(AppRoot, "assets", "character_icons");
                if (!Directory.Exists(dir)) return "";
                List<string> candidates = new List<string>();
                if (!string.IsNullOrWhiteSpace(code)) candidates.Add(Path.Combine(dir, code + ".png"));
                if (!string.IsNullOrWhiteSpace(name)) candidates.Add(Path.Combine(dir, SafeId(name) + ".png"));
                if (!string.IsNullOrWhiteSpace(name)) candidates.Add(Path.Combine(dir, name + ".png"));
                return candidates.FirstOrDefault(File.Exists) ?? "";
            }
            catch { return ""; }
        }

        private Bitmap GenerateCharacterPlaceholderIcon(string code, string name)
        {
            Bitmap bmp = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                string seed = string.IsNullOrWhiteSpace(code) ? (name ?? "?") : code;
                int hash = Math.Abs(seed.GetHashCode());
                Color bg = Color.FromArgb(255, 70 + (hash % 95), 85 + ((hash / 7) % 95), 115 + ((hash / 13) % 95));
                using (SolidBrush b = new SolidBrush(bg)) g.FillEllipse(b, 1, 1, 30, 30);
                using (Pen p = new Pen(Color.FromArgb(220, 255, 255, 255), 2)) g.DrawEllipse(p, 1, 1, 30, 30);
                string initials = CharacterInitials(name, code);
                using (Font f = new Font("Segoe UI Semibold", initials.Length > 2 ? 8.0f : 9.5f, FontStyle.Bold))
                using (SolidBrush tb = new SolidBrush(Color.White))
                using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.DrawString(initials, f, tb, new RectangleF(0, 0, 32, 31), sf);
                }
            }
            return bmp;
        }

        private string CharacterInitials(string name, string code)
        {
            if (!string.IsNullOrWhiteSpace(name) && name != "Unknown" && name != "All Characters" && name != "Menu UI")
            {
                string[] parts = name.Split(new char[] { ' ', '.', '-' }, StringSplitOptions.RemoveEmptyEntries);
                string init = string.Concat(parts.Take(2).Select(p => p.Substring(0, 1).ToUpperInvariant()));
                return init.Length == 0 ? "?" : init;
            }
            if (!string.IsNullOrWhiteSpace(code)) return code.ToUpperInvariant();
            if (name == "All Characters") return "ALL";
            if (name == "Menu UI") return "UI";
            return "?";
        }

        private void SortCharacterAssetsByDetectedCharacter()
        {
            ApplyAssetGridEdits("Characters");
            CharacterAssets.Sort(delegate(AssetFile a, AssetFile b)
            {
                CharacterAssetInfo ia = AnalyzeCharacterAsset(a);
                CharacterAssetInfo ib = AnalyzeCharacterAsset(b);
                return string.Compare(ia.SortKey, ib.SortKey, StringComparison.OrdinalIgnoreCase);
            });
            RefreshAssetGrid("Characters");
            AddLog("Sorted character assets by detected character and file type.");
        }

        private void ShowCharacterConflictDialog()
        {
            ApplyAssetGridEdits("Characters");
            string report = BuildCharacterConflictReport();
            RefreshAssetGrid("Characters");
            AddLog("Character conflict check completed.");
            MessageBox.Show(this, report, "Character Duplicate Conflict Check", MessageBoxButtons.OK, report.Contains("CONFLICT") ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        private string BuildCharacterConflictReport()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Duplicate-file conflict check");
            sb.AppendLine("=============================");
            sb.AppendLine();
            if (CharacterAssets.Count == 0)
            {
                sb.AppendLine("No character files have been added yet.");
                return sb.ToString();
            }

            var rows = CharacterAssets.Select((a, i) => new
            {
                Index = i,
                Asset = a,
                Info = AnalyzeCharacterAsset(a),
                Disc = CharacterConflictDiscKey(a),
                Output = CharacterConflictOutputKey(a)
            }).ToList();

            var discConflicts = rows.Where(x => !string.IsNullOrWhiteSpace(x.Disc))
                .GroupBy(x => x.Disc, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .OrderBy(g => g.Key)
                .ToList();
            var outputConflicts = rows.Where(x => !string.IsNullOrWhiteSpace(x.Output))
                .GroupBy(x => x.Output, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .OrderBy(g => g.Key)
                .ToList();

            if (discConflicts.Count == 0 && outputConflicts.Count == 0)
            {
                sb.AppendLine("OK - No duplicate character replacement targets were found.");
                sb.AppendLine();
                sb.AppendLine("Checked files: " + CharacterAssets.Count);
                return sb.ToString();
            }

            sb.AppendLine("CONFLICT - One or more files target the same replacement path.");
            sb.AppendLine("When this happens, the later file can overwrite the earlier one during export.");
            sb.AppendLine();

            if (discConflicts.Count > 0)
            {
                sb.AppendLine("Same disc/Riivolution target");
                sb.AppendLine("----------------------------");
                foreach (var group in discConflicts)
                {
                    sb.AppendLine(group.Key + "  (" + group.Count() + " files)");
                    foreach (var item in group)
                        sb.AppendLine("  - " + item.Info.CharacterName + " / " + item.Info.FileType + " / " + Path.GetFileName(item.Asset.SourcePath));
                    sb.AppendLine();
                }
            }

            if (outputConflicts.Count > 0)
            {
                sb.AppendLine("Same output filename/path");
                sb.AppendLine("-------------------------");
                foreach (var group in outputConflicts)
                {
                    sb.AppendLine(group.Key + "  (" + group.Count() + " files)");
                    foreach (var item in group)
                        sb.AppendLine("  - " + item.Info.CharacterName + " / " + item.Info.FileType + " / " + Path.GetFileName(item.Asset.SourcePath));
                    sb.AppendLine();
                }
            }

            sb.AppendLine("Fix: remove the duplicate, rename the output file, or change its target path if you intentionally want both files.");
            return sb.ToString();
        }

        private List<string> BuildCharacterConflictWarningsForGrid(List<AssetFile> list)
        {
            List<string> warnings = Enumerable.Repeat("", list.Count).ToList();
            var rows = list.Select((a, i) => new
            {
                Index = i,
                Asset = a,
                Disc = CharacterConflictDiscKey(a),
                Output = CharacterConflictOutputKey(a)
            }).ToList();

            foreach (var group in rows.Where(x => !string.IsNullOrWhiteSpace(x.Disc)).GroupBy(x => x.Disc, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
            {
                foreach (var item in group)
                    warnings[item.Index] = CombineWarnings(warnings[item.Index], "Conflict: " + group.Count() + " files target " + group.Key + ".");
            }
            foreach (var group in rows.Where(x => !string.IsNullOrWhiteSpace(x.Output)).GroupBy(x => x.Output, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
            {
                foreach (var item in group)
                    warnings[item.Index] = CombineWarnings(warnings[item.Index], "Conflict: duplicate output path " + group.Key + ".");
            }
            return warnings;
        }

        private string CharacterConflictDiscKey(AssetFile a)
        {
            string output = a == null ? "" : (a.OutputName ?? "");
            string disc = a == null ? "" : (a.DiscPath ?? "");
            if (string.IsNullOrWhiteSpace(disc)) disc = DefaultDiscPath("Characters", output);
            disc = NormalizeDiscPath(disc);
            if (string.IsNullOrWhiteSpace(disc)) return ""; // source-only Inject PNG/TPL/BRRES entries are not direct conflicts
            return disc.ToLowerInvariant();
        }

        private string CharacterConflictOutputKey(AssetFile a)
        {
            if (a == null) return "";
            string output = SafeRelativePathForProject(a.OutputName ?? "").Replace('\\', '/').Trim('/');
            if (string.IsNullOrWhiteSpace(output)) return "";
            string lower = output.ToLowerInvariant();
            if (lower.StartsWith("inject/")) return ""; // source files can share patch-input style folders without direct export conflict
            return lower;
        }

        private string CombineWarnings(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a)) return b ?? "";
            if (string.IsNullOrWhiteSpace(b)) return a ?? "";
            if (a.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0) return a;
            return a.TrimEnd() + "  " + b.Trim();
        }

        private void ShowCharacterSummaryDialog()
        {
            MessageBox.Show(this, BuildCharacterSummaryText(true), "Character Pack Summary", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string ShortCharacterSummaryForLog()
        {
            var infos = CharacterAssets.Select(a => AnalyzeCharacterAsset(a)).ToList();
            int known = infos.Count(i => i.CharacterName != "Unknown");
            int unknown = infos.Count(i => i.CharacterName == "Unknown");
            int checks = infos.Count(i => !string.IsNullOrWhiteSpace(i.Warning));
            return "Character summary: " + known + " detected, " + unknown + " unknown, " + checks + " check item(s).";
        }

        private string BuildCharacterSummaryText(bool detailed)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Detected character pack contents");
            sb.AppendLine("================================");
            sb.AppendLine();
            if (CharacterAssets.Count == 0)
            {
                sb.AppendLine("No character files have been added yet.");
                return sb.ToString();
            }

            var infos = CharacterAssets.Select(a => new { Asset = a, Info = AnalyzeCharacterAsset(a) }).ToList();
            var byCharacter = infos.GroupBy(x => x.Info.CharacterName).OrderBy(g => g.Key == "Unknown" ? "zz_unknown" : g.Key).ToList();
            sb.AppendLine("Files: " + CharacterAssets.Count);
            sb.AppendLine("Detected groups: " + byCharacter.Count);
            sb.AppendLine();
            foreach (var group in byCharacter)
            {
                sb.AppendLine(group.Key + "  (" + group.Count() + " file" + (group.Count() == 1 ? "" : "s") + ")");
                var typeCounts = group.GroupBy(x => x.Info.FileType).OrderBy(g => g.Key);
                foreach (var t in typeCounts) sb.AppendLine("  - " + t.Key + ": " + t.Count());
                if (detailed)
                {
                    foreach (var item in group.Take(8))
                    {
                        string leaf = Path.GetFileName(item.Asset.OutputName);
                        sb.AppendLine("      " + leaf);
                    }
                    if (group.Count() > 8) sb.AppendLine("      ..." + (group.Count() - 8) + " more");
                }
                sb.AppendLine();
            }

            int driverCount = infos.Count(x => x.Info.FileType.Contains("Driver"));
            int allkartCount = infos.Count(x => x.Info.FileType.Contains("allkart"));
            int raceKartCount = infos.Count(x => x.Info.FileType.Contains("In-race"));
            int voiceCount = infos.Count(x => x.Info.FileType.Contains("voices"));
            int uiCount = infos.Count(x => x.Info.FileType.Contains("UI"));
            int conflictCount = BuildCharacterConflictWarningsForGrid(CharacterAssets).Count(x => !string.IsNullOrWhiteSpace(x));
            sb.AppendLine("Health check");
            sb.AppendLine("------------");
            sb.AppendLine((raceKartCount > 0 ? "OK" : "WARN") + " - In-race kart/bike files: " + raceKartCount);
            sb.AppendLine((allkartCount > 0 ? "OK" : "INFO") + " - Vehicle select allkart files: " + allkartCount);
            sb.AppendLine((driverCount > 0 ? "OK" : "INFO") + " - Driver/select model files: " + driverCount);
            sb.AppendLine((voiceCount > 0 ? "OK" : "INFO") + " - Voice/sound files: " + voiceCount);
            sb.AppendLine((uiCount > 0 ? "OK" : "INFO") + " - UI/icon source files: " + uiCount);
            sb.AppendLine((conflictCount == 0 ? "OK" : "WARN") + " - Duplicate target conflicts: " + conflictCount);
            sb.AppendLine();

            var warnings = infos.Where(x => !string.IsNullOrWhiteSpace(x.Info.Warning)).Take(12).ToList();
            if (warnings.Count > 0)
            {
                sb.AppendLine("Warnings / checks");
                sb.AppendLine("-----------------");
                foreach (var w in warnings) sb.AppendLine("- " + Path.GetFileName(w.Asset.OutputName) + ": " + w.Info.Warning);
                if (infos.Count(x => !string.IsNullOrWhiteSpace(x.Info.Warning)) > warnings.Count) sb.AppendLine("- More warnings are visible in the Characters table.");
            }
            else sb.AppendLine("No character-pack warnings detected.");

            sb.AppendLine();
            sb.AppendLine("Icon note: this build can use PNG files in assets/character_icons using character codes such as fk.png, ds.png, mr.png, etc. If no icon is found, the app falls back to generated initials. For public GitHub releases, only distribute character artwork you have rights to share.");
            return sb.ToString();
        }

        private void ImportExistingPackDialog()
        {
            DialogResult ask = MessageBox.Show("Import from an archive file?\n\nChoose Yes for ZIP, No for an extracted folder.", "Import Existing HNS Pack", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (ask == DialogResult.Cancel) return;
            string importRoot = "";
            if (ask == DialogResult.Yes)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Choose HNS pack archive";
                ofd.Filter = "ZIP archive (*.zip)|*.zip";
                if (ofd.ShowDialog(this) != DialogResult.OK) return;
                string imports = Path.Combine(AppRoot, "imports");
                Directory.CreateDirectory(imports);
                string folder = Path.Combine(imports, SafeId(Path.GetFileNameWithoutExtension(ofd.FileName)) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                Directory.CreateDirectory(folder);
                ExtractArchive(ofd.FileName, folder);
                importRoot = folder;
            }
            else
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Select extracted HNS pack folder";
                if (fbd.ShowDialog(this) != DialogResult.OK) return;
                importRoot = fbd.SelectedPath;
            }
            ImportExistingPack(importRoot);
        }

        private void ImportCharacterPackDialog()
        {
            DialogResult ask = MessageBox.Show("Import a custom character pack from an archive?\n\nChoose Yes for ZIP, No for an extracted folder.", "Import Character Pack", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (ask == DialogResult.Cancel) return;
            string importRoot = "";
            if (ask == DialogResult.Yes)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Choose custom character pack archive";
                ofd.Filter = "ZIP archive (*.zip)|*.zip";
                if (ofd.ShowDialog(this) != DialogResult.OK) return;
                string imports = Path.Combine(AppRoot, "imports");
                Directory.CreateDirectory(imports);
                string folder = Path.Combine(imports, SafeId(Path.GetFileNameWithoutExtension(ofd.FileName)) + "_characters_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                Directory.CreateDirectory(folder);
                ExtractArchive(ofd.FileName, folder);
                importRoot = folder;
            }
            else
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Select extracted custom character pack folder";
                if (fbd.ShowDialog(this) != DialogResult.OK) return;
                importRoot = fbd.SelectedPath;
            }
            int added = ImportCharacterPack(importRoot);
            RefreshAssetGrid("Characters");
            AddLog("Imported custom character pack. Files added: " + added + ".");
            AddLog(ShortCharacterSummaryForLog());
            MessageBox.Show("Imported " + added + " custom character file(s).\n\n" + BuildCharacterSummaryText(false), "Import Character Pack Summary", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExtractArchive(string archivePath, string destination)
        {
            string ext = Path.GetExtension(archivePath).ToLowerInvariant();
            if (ext != ".zip")
            {
                throw new NotSupportedException("Only ZIP archives are supported directly. For RAR/7z packs, extract them first and choose the extracted folder option.");
            }

            ZipFile.ExtractToDirectory(archivePath, destination, true);
        }

        private void ImportExistingPack(string root)
        {
            if (!Directory.Exists(root)) return;
            SaveMetadataFromFields();

            string packContentDir = FindBestPackContentFolder(root);
            if (!string.IsNullOrWhiteSpace(packContentDir))
            {
                string leaf = Path.GetFileName(packContentDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                if (!string.IsNullOrWhiteSpace(leaf) && !leaf.Equals("Customtrackfile", StringComparison.OrdinalIgnoreCase)) PackId = SafeId(leaf);
            }
            if (string.IsNullOrWhiteSpace(PackName) || PackName.StartsWith("New MKWii")) PackName = HumanName(PackId);

            int tracks = 0, music = 0, ui = 0, chars = 0;

            string tracksDir = FindChildDirectory(packContentDir, "Tracks");
            IEnumerable<string> trackCandidates;
            if (!string.IsNullOrEmpty(tracksDir)) trackCandidates = Directory.GetFiles(tracksDir, "*.szs", SearchOption.AllDirectories);
            else trackCandidates = Directory.GetFiles(packContentDir, "*.szs", SearchOption.TopDirectoryOnly).Where(f => Slots.Any(s => s.GameFile.Equals(Path.GetFileName(f), StringComparison.OrdinalIgnoreCase)));
            tracks = AssignTrackFiles(trackCandidates);

            string musicDir = FindChildDirectory(packContentDir, "Music");
            if (!string.IsNullOrEmpty(musicDir))
            {
                foreach (string file in Directory.GetFiles(musicDir, "*.*", SearchOption.AllDirectories))
                {
                    string ext = Path.GetExtension(file).ToLowerInvariant();
                    if (ext == ".brstm" || ext == ".brsar") { AddAsset("Music", file, Path.GetRelativePath(musicDir, file)); music++; }
                }
            }
            else
            {
                foreach (string file in Directory.GetFiles(packContentDir, "*.brstm", SearchOption.TopDirectoryOnly)) { AddAsset("Music", file); music++; }
            }

            string menuDir = FindChildDirectory(packContentDir, "Menu");
            if (!string.IsNullOrEmpty(menuDir))
            {
                foreach (string file in Directory.GetFiles(menuDir, "*.*", SearchOption.AllDirectories))
                {
                    string ext = Path.GetExtension(file).ToLowerInvariant();
                    if (ext == ".szs" || ext == ".brres") { AddAsset("UI / REL Assets", file, "Menu/" + Path.GetRelativePath(menuDir, file)); ui++; }
                }
            }

            string staticDir = FindChildDirectory(packContentDir, "StaticR");
            if (!string.IsNullOrEmpty(staticDir))
            {
                foreach (string file in Directory.GetFiles(staticDir, "*.rel", SearchOption.AllDirectories)) { AddAsset("UI / REL Assets", file, "StaticR/" + Path.GetFileName(file)); ui++; }
            }
            else
            {
                foreach (string file in Directory.GetFiles(packContentDir, "*.rel", SearchOption.TopDirectoryOnly)) { AddAsset("UI / REL Assets", file); ui++; }
            }

            string charactersDir = FindChildDirectory(packContentDir, "Characters");
            if (string.IsNullOrEmpty(charactersDir)) charactersDir = Directory.GetDirectories(root, "Characters", SearchOption.AllDirectories).FirstOrDefault();
            if (!string.IsNullOrEmpty(charactersDir)) chars += ImportCharacterFolder(charactersDir, charactersDir);

            // Also catch loose community files from flat hns/<pack> folders.
            foreach (string file in Directory.GetFiles(packContentDir, "*.*", SearchOption.TopDirectoryOnly))
            {
                string name = Path.GetFileName(file).ToLowerInvariant();
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (Slots.Any(s => s.GameFile.ToLowerInvariant() == name)) continue;
                if (ext == ".brstm") continue;
                if (ext == ".rel" || ext == ".brsar" || ext == ".brres" || (ext == ".szs" && !Slots.Any(s => s.SourcePath == file))) { AddAsset("UI / REL Assets", file); ui++; }
            }

            string tracklist = Directory.GetFiles(root, "*tracklist*.*", SearchOption.AllDirectories).FirstOrDefault();
            if (!string.IsNullOrEmpty(tracklist))
            {
                try
                {
                    string raw = File.ReadAllText(tracklist, Encoding.UTF8);
                    string plain = RtfToPlain(raw);
                    if (!string.IsNullOrWhiteSpace(plain)) Description = "Imported tracklist:\r\n" + plain.Trim();
                }
                catch { }
            }

            AddLog("Imported pack from " + root + ". Tracks: " + tracks + ", Music: " + music + ", UI/REL: " + ui + ", Characters: " + chars + ".");
            ShowDashboard();
        }

        private string FindBestPackContentFolder(string root)
        {
            string custom = Directory.GetDirectories(root, "Customtrackfile", SearchOption.AllDirectories).FirstOrDefault();
            if (!string.IsNullOrEmpty(custom)) return custom;
            string hnsDir = Directory.GetDirectories(root, "hns", SearchOption.AllDirectories).FirstOrDefault();
            if (!string.IsNullOrEmpty(hnsDir))
            {
                string[] children = Directory.GetDirectories(hnsDir);
                if (children.Length > 0) return children.OrderByDescending(d => Directory.GetFiles(d, "*.*", SearchOption.AllDirectories).Length).First();
            }
            string structured = Directory.GetDirectories(root, "*", SearchOption.AllDirectories)
                .FirstOrDefault(d => Directory.Exists(Path.Combine(d, "Tracks")) || Directory.Exists(Path.Combine(d, "Music")) || Directory.Exists(Path.Combine(d, "StaticR")) || Directory.Exists(Path.Combine(d, "Menu")));
            if (!string.IsNullOrEmpty(structured)) return structured;
            return Directory.GetDirectories(root, "*", SearchOption.AllDirectories)
                .FirstOrDefault(d => Directory.GetFiles(d, "*.szs", SearchOption.TopDirectoryOnly).Length > 10) ?? root;
        }

        private string FindChildDirectory(string root, string name)
        {
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) return "";
            string direct = Path.Combine(root, name);
            if (Directory.Exists(direct)) return direct;
            return Directory.GetDirectories(root, name, SearchOption.TopDirectoryOnly).FirstOrDefault() ?? "";
        }

        private int ImportCharacterPack(string root)
        {
            if (!Directory.Exists(root)) return 0;
            string best = Directory.GetDirectories(root, "CTGP", SearchOption.AllDirectories).FirstOrDefault();
            if (string.IsNullOrEmpty(best)) best = root;
            return ImportCharacterFolder(best, best);
        }

        private int ImportCharacterFolder(string folder, string relativeRoot)
        {
            int added = 0;
            foreach (string file in Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories))
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (ext != ".szs" && ext != ".brres" && ext != ".brsar" && ext != ".tpl" && ext != ".png") continue;
                string rel = Path.GetRelativePath(relativeRoot, file).Replace('\\', '/');
                AddAsset("Characters", file, rel);
                added++;
            }
            return added;
        }

        private string RtfToPlain(string raw)
        {
            if (!raw.TrimStart().StartsWith("{\\rtf")) return raw;
            string s = Regex.Replace(raw, @"\\'[0-9a-fA-F]{2}", m => ((char)Convert.ToInt32(m.Value.Substring(2), 16)).ToString());
            s = Regex.Replace(s, @"\\par[d]?", "\n");
            s = Regex.Replace(s, @"\\[a-zA-Z]+-?\d* ?", "");
            s = s.Replace("{", "").Replace("}", "").Replace("\\", "");
            return s;
        }

        private void LoadIconLibrary()
        {
            IconList.Items.Clear();
            IconImageList.Images.Clear();
            List<string> files = new List<string>();
            string stockDir = Path.Combine(AppRoot, "assets", "powerups");
            string userDir = Path.Combine(AppRoot, "assets", "user_icons");
            if (Directory.Exists(stockDir)) files.AddRange(Directory.GetFiles(stockDir, "*.png", SearchOption.TopDirectoryOnly));
            if (Directory.Exists(userDir))
            {
                files.AddRange(Directory.GetFiles(userDir, "*.png", SearchOption.AllDirectories));
                files.AddRange(Directory.GetFiles(userDir, "*.jpg", SearchOption.AllDirectories));
                files.AddRange(Directory.GetFiles(userDir, "*.jpeg", SearchOption.AllDirectories));
            }
            files = files.OrderBy(x => x).ToList();
            int index = 0;
            foreach (string file in files)
            {
                try
                {
                    using (Image img = Image.FromFile(file)) IconImageList.Images.Add(new Bitmap(img));
                    string name = Path.GetFileNameWithoutExtension(file);
                    ListViewItem item = new ListViewItem(name, index);
                    item.Tag = file;
                    IconList.Items.Add(item);
                    index++;
                }
                catch { }
            }
        }

        private string SelectedCup()
        {
            if (CupList == null || CupList.SelectedItem == null) return "";
            return CupList.SelectedItem.ToString();
        }

        private void AssignSelectedLibraryIcon()
        {
            if (IconList.SelectedItems.Count == 0) return;
            string cup = SelectedCup();
            if (string.IsNullOrEmpty(cup)) return;
            string path = IconList.SelectedItems[0].Tag as string;
            if (!string.IsNullOrEmpty(path)) CupIconPaths[cup] = path;
            RefreshCupPreview();
        }

        private void ImportOriginalCupIcons()
        {
            MessageBox.Show("Choose a folder containing cup icon PNG/JPG files that you extracted or created yourself. Official Nintendo assets are not bundled with this software.", "Original Cup Icons", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ImportIconFolder();
        }

        private void ImportIconFolder()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Choose a folder of PNG/JPG cup icons to add to the app library";
            if (fbd.ShowDialog(this) != DialogResult.OK) return;
            string userDir = Path.Combine(AppRoot, "assets", "user_icons");
            Directory.CreateDirectory(userDir);
            int copied = 0;
            foreach (string file in Directory.GetFiles(fbd.SelectedPath, "*.*", SearchOption.AllDirectories))
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (ext != ".png" && ext != ".jpg" && ext != ".jpeg") continue;
                string name = SafeFileName(Path.GetFileName(file));
                string dest = Path.Combine(userDir, name);
                int n = 2;
                while (File.Exists(dest))
                {
                    dest = Path.Combine(userDir, Path.GetFileNameWithoutExtension(name) + "_" + n + Path.GetExtension(name));
                    n++;
                }
                File.Copy(file, dest);
                copied++;
            }
            LoadIconLibrary();
            AddLog("Imported " + copied + " cup icon file(s) into the local icon library.");
        }

        private void UploadCustomCupIcon()
        {
            string cup = SelectedCup();
            if (string.IsNullOrEmpty(cup)) return;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Choose custom cup icon";
            ofd.Filter = "PNG image (*.png)|*.png|Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*";
            if (ofd.ShowDialog(this) == DialogResult.OK) { CupIconPaths[cup] = ofd.FileName; RefreshCupPreview(); }
        }

        private void ClearCupIcon()
        {
            string cup = SelectedCup();
            if (string.IsNullOrEmpty(cup)) return;
            CupIconPaths[cup] = "";
            AddLog("Cup icon reset to original: " + cup);
            RefreshCupPreview();
        }

        private void ClearAllCupIcons()
        {
            foreach (string cup in CupNames) CupIconPaths[cup] = "";
            AddLog("All cup icons reset to original MKWii icons.");
            RefreshCupPreview();
        }

        private void RefreshCupPreview()
        {
            if (CupPreview == null || CupPreviewTitle == null) return;
            string cup = SelectedCup();
            string path = CupIconPaths.ContainsKey(cup) ? CupIconPaths[cup] : "";
            if (CupPreview.Image != null) { Image old = CupPreview.Image; CupPreview.Image = null; old.Dispose(); }
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                try { using (Image img = Image.FromFile(path)) CupPreview.Image = new Bitmap(img); CupPreviewTitle.Text = cup + "\nIcon: " + Path.GetFileName(path) + "\nGame name: " + CupIconPathToCupName(path); }
                catch { CupPreviewTitle.Text = cup + "\nInvalid image"; }
            }
            else
            {
                CupPreviewTitle.Text = cup + "\nOriginal game icon will be kept";
            }
        }

        private void BrowseOutputFolder()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Choose a parent output location. The app will create a new export subfolder inside it.";
            if (fbd.ShowDialog(this) == DialogResult.OK) { OutputFolderText.Text = fbd.SelectedPath; ExportOutputParentFolder = fbd.SelectedPath; }
        }


        private int RemoveStaleAutoGeneratedUiAssets()
        {
            int removed = 0;
            string dataRoot = Path.Combine(AppRoot, "Data", "WorkingSZS");
            List<AssetFile> remove = new List<AssetFile>();
            foreach (AssetFile a in UiRelAssets)
            {
                try
                {
                    string p = Path.GetFullPath(a.SourcePath ?? "");
                    bool underWorking = Directory.Exists(dataRoot) && p.StartsWith(Path.GetFullPath(dataRoot), StringComparison.OrdinalIgnoreCase);
                    bool autoPatch = p.IndexOf("AutoPatchedCupIcons", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     p.IndexOf("AutoPatchedTrackNames", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     p.IndexOf("AutoPatchedCharacterUI", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     p.IndexOf("AutoPatchedCharacterModel", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (underWorking || autoPatch) remove.Add(a);
                }
                catch { }
            }
            foreach (AssetFile a in remove)
            {
                if (UiRelAssets.Remove(a)) removed++;
            }
            return removed;
        }


        private void CleanWorkingCacheNow()
        {
            try
            {
                string dataRoot = Path.Combine(AppRoot, "Data", "WorkingSZS");
                if (!Directory.Exists(dataRoot))
                {
                    MessageBox.Show("No working cache exists yet.", "Clean Cache", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                long before = DirectorySize(dataRoot);
                Directory.Delete(dataRoot, true);
                Directory.CreateDirectory(dataRoot);
                AddLog("Cleaned working cache: " + FormatBytes(before) + " removed.");
                MessageBox.Show("Working cache cleaned. Removed about " + FormatBytes(before) + ".", "Clean Cache", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AddLog("Clean cache failed: " + ex.Message);
                MessageBox.Show("Could not clean cache:\n" + ex.Message, "Clean Cache", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }



        private void ProjectCleanupNow()
        {
            DialogResult confirm = MessageBox.Show(this,
                "Clean safe temporary project files now?\n\nThis removes old working SZS folders, temporary WBFS/ISO extraction folders, stale auto-generated UI entries, and imported asset rows whose source files no longer exist.\n\nIt will not delete base_files, tools, source files, or exported packs.",
                "Project Cleanup",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            long removedBytes = 0;
            int removedFolders = 0;
            int removedAssets = 0;
            List<string> details = new List<string>();

            try
            {
                string data = Path.Combine(AppRoot, "Data");
                string[] safeFolders = new string[]
                {
                    Path.Combine(data, "WorkingSZS"),
                    Path.Combine(data, "DiscExtractTemp"),
                    Path.Combine(data, "DiscExtract"),
                    Path.Combine(data, "WitExtract"),
                    Path.Combine(data, "BaseExtract"),
                    Path.Combine(data, "BaseExtractTemp"),
                    Path.Combine(data, "RequiredExtract"),
                    Path.Combine(data, "RequiredExtractTemp")
                };

                foreach (string folder in safeFolders)
                {
                    if (!Directory.Exists(folder)) continue;
                    try
                    {
                        long size = DirectorySize(folder);
                        Directory.Delete(folder, true);
                        removedBytes += size;
                        removedFolders++;
                        details.Add("Removed folder: " + folder);
                    }
                    catch (Exception ex)
                    {
                        details.Add("Could not remove " + folder + ": " + ex.Message);
                    }
                }

                Directory.CreateDirectory(Path.Combine(data, "WorkingSZS"));

                removedAssets += RemoveMissingAssetRows(MusicAssets, "Music", details);
                removedAssets += RemoveMissingAssetRows(CharacterAssets, "Characters", details);
                removedAssets += RemoveMissingAssetRows(UiRelAssets, "UI/REL", details);
                int staleUi = RemoveStaleAutoGeneratedUiAssets();
                if (staleUi > 0)
                {
                    removedAssets += staleUi;
                    details.Add("Removed stale auto-generated UI asset rows: " + staleUi);
                }

                RefreshAssetGrid("Music");
                RefreshAssetGrid("Characters");
                RefreshAssetGrid("UI / REL Assets");
                UpdateReadyLabels();

                AddLog("Project cleanup complete. Folders removed: " + removedFolders + ", asset rows removed: " + removedAssets + ", space freed: " + FormatBytes(removedBytes) + ".");
                MessageBox.Show(this,
                    "Project cleanup complete.\n\nFolders removed: " + removedFolders + "\nStale asset rows removed: " + removedAssets + "\nSpace freed: " + FormatBytes(removedBytes) + (details.Count > 0 ? "\n\nDetails are in the log." : ""),
                    "Project Cleanup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                foreach (string d in details) AddLog("Cleanup: " + d);
            }
            catch (Exception ex)
            {
                AddLog("Project cleanup failed: " + ex.Message);
                MessageBox.Show(this, "Project cleanup failed:\n" + ex.Message, "Project Cleanup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private int RemoveMissingAssetRows(List<AssetFile> assets, string label, List<string> details)
        {
            int before = assets.Count;
            List<AssetFile> missing = assets.Where(a => string.IsNullOrWhiteSpace(a.SourcePath) || !File.Exists(a.SourcePath)).ToList();
            foreach (AssetFile a in missing)
            {
                assets.Remove(a);
                if (details != null) details.Add("Removed missing " + label + " asset row: " + (a.OutputName ?? ""));
            }
            return before - assets.Count;
        }

        private void CleanupOldWorkingFolders(int keepNewest)
        {
            try
            {
                string packRoot = Path.Combine(AppRoot, "Data", "WorkingSZS", SafeId(PackId));
                if (!Directory.Exists(packRoot)) return;
                DirectoryInfo di = new DirectoryInfo(packRoot);
                DirectoryInfo[] dirs = di.GetDirectories().OrderByDescending(d => d.LastWriteTimeUtc).ToArray();
                int removed = 0;
                long bytes = 0;
                foreach (DirectoryInfo d in dirs.Skip(Math.Max(0, keepNewest)))
                {
                    try
                    {
                        bytes += DirectorySize(d.FullName);
                        d.Delete(true);
                        removed++;
                    }
                    catch { }
                }
                if (removed > 0) AddLog("Cleaned " + removed + " old working cache folder(s): " + FormatBytes(bytes) + " removed.");
            }
            catch { }
        }

        private long DirectorySize(string path)
        {
            try
            {
                if (!Directory.Exists(path)) return 0;
                long total = 0;
                foreach (string f in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                {
                    try { total += new FileInfo(f).Length; } catch { }
                }
                return total;
            }
            catch { return 0; }
        }

        private string FormatBytes(long bytes)
        {
            double v = bytes;
            string[] units = new string[] { "B", "KB", "MB", "GB", "TB" };
            int i = 0;
            while (v >= 1024 && i < units.Length - 1) { v /= 1024; i++; }
            return v.ToString(i == 0 ? "0" : "0.0") + " " + units[i];
        }


        private void UpdateExportSizeEstimate(bool showMessage)
        {
            try
            {
                SaveMetadataFromFields();
                ApplyGridEdits();
                ApplyAssetGridEdits("Music");
                ApplyAssetGridEdits("Characters");
                ApplyAssetGridEdits("UI / REL Assets");
                string summary = EstimateExportSizeSummary();
                if (ExportEstimateLabel != null) ExportEstimateLabel.Text = summary;
                if (showMessage) MessageBox.Show(this, BuildExportSizeReport(), "Estimated Final Pack Size", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (showMessage) MessageBox.Show(this, "Could not estimate size:\n\n" + ex.Message, "Estimate Size", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private string EstimateExportSizeSummary()
        {
            long bytes = EstimateFinalPackSizeBytes();
            return bytes <= 0 ? "No export files selected yet" : FormatBytes(bytes) + " estimated folder size";
        }

        private long EstimateFinalPackSizeBytes()
        {
            long total = 0;
            bool copyD = CopyMultiplayerCheck != null ? CopyMultiplayerCheck.Checked : CopyMultiplayerCourseCopies;
            foreach (TrackSlotInfo slot in Slots)
            {
                if (GetSlotStatus(slot) != "Ready") continue;
                long size = FileSizeSafe(slot.SourcePath);
                total += size;
                if (copyD) total += size;
            }
            foreach (AssetFile a in MusicAssets) if (File.Exists(a.SourcePath)) total += FileSizeSafe(a.SourcePath);
            foreach (AssetFile a in UiRelAssets) if (File.Exists(a.SourcePath)) total += FileSizeSafe(a.SourcePath);
            foreach (AssetFile a in CharacterAssets)
            {
                if (!File.Exists(a.SourcePath)) continue;
                if (ShouldSkipFullCharacterUiAsset(a)) continue;
                string disc = NormalizeDiscPath(string.IsNullOrWhiteSpace(a.DiscPath) ? DefaultDiscPath("Characters", a.OutputName) : a.DiscPath);
                if (string.IsNullOrWhiteSpace(disc) && (a.OutputName ?? "").ToLowerInvariant().StartsWith("inject/")) continue;
                total += FileSizeSafe(a.SourcePath);
            }
            total += EstimateAutoPatchedUiBytes();
            total += 256 * 1024; // XML, readme, manifests, reports, and filesystem overhead.
            return total;
        }

        private long EstimateAutoPatchedUiBytes()
        {
            long total = 0;
            bool needsCup = CupIconPaths.Values.Any(x => !string.IsNullOrWhiteSpace(x) && File.Exists(x));
            bool needsNames = Slots.Any(x => !string.IsNullOrWhiteSpace(x.CustomName) && !string.Equals(x.CustomName.Trim(), x.TrackName, StringComparison.OrdinalIgnoreCase));
            bool needsCharacterUi = CharacterAssets.Any(a => (a.OutputName ?? "").Replace('\\','/').ToLowerInvariant().Contains("inject/img") || Path.GetExtension(a.SourcePath ?? "").Equals(".png", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(a.SourcePath ?? "").Equals(".tpl", StringComparison.OrdinalIgnoreCase));
            if (!needsCup && !needsNames && !needsCharacterUi) return 0;

            string baseRoot = Path.Combine(AppRoot, "base_files");
            List<string> names = new List<string>();
            if (needsCup || needsCharacterUi) names.AddRange(UiIconBaseFiles(PatchIncludeAward));
            if (needsNames)
            {
                string suffix = string.IsNullOrWhiteSpace(PatchLanguage) ? "U" : PatchLanguage.Substring(0, 1);
                names.AddRange(UiLanguageFiles(suffix, PatchIncludeAward));
                names.AddRange(UiIconBaseFiles(PatchIncludeAward));
            }
            foreach (string name in names.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                string file = FindBaseFileByName(baseRoot, name);
                if (File.Exists(file)) total += FileSizeSafe(file);
            }
            return total;
        }


        private string FindBaseFileByName(string baseRoot, string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(baseRoot) || string.IsNullOrWhiteSpace(fileName) || !Directory.Exists(baseRoot)) return "";
                string directUi = Path.Combine(baseRoot, "Scene", "UI", fileName);
                if (File.Exists(directUi)) return directUi;
                string lang = Path.Combine(baseRoot, "Scene", "UI", "Language", fileName);
                if (File.Exists(lang)) return lang;
                string found = Directory.GetFiles(baseRoot, fileName, SearchOption.AllDirectories).FirstOrDefault();
                return found ?? "";
            }
            catch { return ""; }
        }

        private string BuildExportSizeReport()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Estimated final pack size");
            sb.AppendLine("=========================");
            sb.AppendLine();
            long tracks = Slots.Where(x => GetSlotStatus(x) == "Ready").Sum(x => FileSizeSafe(x.SourcePath));
            long music = MusicAssets.Where(x => File.Exists(x.SourcePath)).Sum(x => FileSizeSafe(x.SourcePath));
            long chars = CharacterAssets.Where(x => File.Exists(x.SourcePath) && !ShouldSkipFullCharacterUiAsset(x)).Sum(x => FileSizeSafe(x.SourcePath));
            long ui = UiRelAssets.Where(x => File.Exists(x.SourcePath)).Sum(x => FileSizeSafe(x.SourcePath));
            long autoUi = EstimateAutoPatchedUiBytes();
            bool copyD = CopyMultiplayerCheck != null ? CopyMultiplayerCheck.Checked : CopyMultiplayerCourseCopies;
            if (copyD) tracks *= 2;
            sb.AppendLine("Tracks: " + FormatBytes(tracks));
            sb.AppendLine("Music: " + FormatBytes(music));
            sb.AppendLine("Characters: " + FormatBytes(chars));
            sb.AppendLine("Advanced/UI/REL assets: " + FormatBytes(ui));
            sb.AppendLine("Auto-patched UI estimate: " + FormatBytes(autoUi));
            sb.AppendLine("Reports/XML overhead: about 0.3 MB");
            sb.AppendLine();
            sb.AppendLine("Total estimate: " + FormatBytes(EstimateFinalPackSizeBytes()));
            sb.AppendLine();
            sb.AppendLine("Note: this is a folder-size estimate before compression. The final ZIP release may be smaller.");
            return sb.ToString();
        }

        private void ShowExportPreviewDialog()
        {
            SaveMetadataFromFields();
            ApplyGridEdits();
            ApplyAssetGridEdits("Music");
            ApplyAssetGridEdits("Characters");
            ApplyAssetGridEdits("UI / REL Assets");
            ShowLargeTextDialog("Export Preview / Dry Run", BuildExportPreviewText());
        }

        private string BuildExportPreviewText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Export preview / dry run");
            sb.AppendLine("========================");
            sb.AppendLine();
            sb.AppendLine("Pack option name: " + (string.IsNullOrWhiteSpace(PackName) ? HumanName(PackId) : PackName));
            sb.AppendLine("Pack folder ID: " + SafeId(PackId));
            sb.AppendLine("Estimated size: " + FormatBytes(EstimateFinalPackSizeBytes()));
            sb.AppendLine();
            sb.AppendLine("Tracks");
            sb.AppendLine("------");
            foreach (TrackSlotInfo slot in Slots.Where(x => GetSlotStatus(x) == "Ready"))
                sb.AppendLine("/Race/Course/" + slot.GameFile + "  <=  " + Path.GetFileName(slot.SourcePath) + (string.IsNullOrWhiteSpace(slot.CustomName) ? "" : "  [name: " + slot.CustomName + "]"));
            sb.AppendLine();
            sb.AppendLine("Music");
            sb.AppendLine("-----");
            foreach (AssetFile a in MusicAssets.Where(x => File.Exists(x.SourcePath)))
                sb.AppendLine((string.IsNullOrWhiteSpace(a.DiscPath) ? DefaultDiscPath("Music", a.OutputName) : a.DiscPath) + "  <=  " + Path.GetFileName(a.SourcePath));
            sb.AppendLine();
            sb.AppendLine("Characters");
            sb.AppendLine("----------");
            foreach (AssetFile a in CharacterAssets.Where(x => File.Exists(x.SourcePath)))
            {
                if (ShouldSkipFullCharacterUiAsset(a)) continue;
                string disc = CorrectKnownCharacterDiscPath(NormalizeDiscPath(string.IsNullOrWhiteSpace(a.DiscPath) ? DefaultDiscPath("Characters", a.OutputName) : a.DiscPath), a.OutputName);
                if (string.IsNullOrWhiteSpace(disc)) continue;
                CharacterAssetInfo info = AnalyzeCharacterAsset(a);
                sb.AppendLine(disc + "  <=  " + Path.GetFileName(a.SourcePath) + "  [" + info.CharacterName + " / " + info.FileType + "]");
            }
            sb.AppendLine();
            sb.AppendLine("Advanced UI / REL");
            sb.AppendLine("-----------------");
            foreach (AssetFile a in UiRelAssets.Where(x => File.Exists(x.SourcePath)))
                sb.AppendLine((string.IsNullOrWhiteSpace(a.DiscPath) ? DefaultDiscPath("UI / REL Assets", a.OutputName) : a.DiscPath) + "  <=  " + Path.GetFileName(a.SourcePath));
            sb.AppendLine();
            sb.AppendLine("Automatic patching that may happen during export");
            sb.AppendLine("-----------------------------------------------");
            sb.AppendLine("Cup icons selected: " + CupIconPaths.Values.Count(x => !string.IsNullOrWhiteSpace(x) && File.Exists(x)));
            sb.AppendLine("Custom track/cup names: " + Slots.Count(x => !string.IsNullOrWhiteSpace(x.CustomName) && !string.Equals(x.CustomName.Trim(), x.TrackName, StringComparison.OrdinalIgnoreCase)));
            sb.AppendLine("Character UI/Driver source files: " + CharacterAssets.Count(x => (x.OutputName ?? "").ToLowerInvariant().StartsWith("inject/")));
            return sb.ToString();
        }

        private void ShowRiivolutionXmlPreviewDialog()
        {
            SaveMetadataFromFields();
            ApplyGridEdits();
            ApplyAssetGridEdits("Music");
            ApplyAssetGridEdits("Characters");
            ApplyAssetGridEdits("UI / REL Assets");
            ShowLargeTextDialog("Riivolution XML Preview", BuildRiivolutionXmlText(Path.Combine(AppRoot, "Data", "XmlPreview")));
        }

        private void ShowLargeTextDialog(string title, string text)
        {
            Form dialog = new Form();
            dialog.Text = title;
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.Size = new Size(900, 650);
            dialog.MinimumSize = new Size(650, 420);
            dialog.BackColor = Bg;
            RichTextBox box = new RichTextBox();
            box.Dock = DockStyle.Fill;
            box.ReadOnly = true;
            box.BorderStyle = BorderStyle.None;
            box.BackColor = Color.FromArgb(10, 13, 20);
            box.ForeColor = TextMain;
            box.Font = new Font("Consolas", 10f);
            box.Text = text;
            dialog.Controls.Add(box);
            FlowLayoutPanel bottom = new FlowLayoutPanel();
            bottom.Dock = DockStyle.Bottom;
            bottom.Height = 58;
            bottom.FlowDirection = FlowDirection.RightToLeft;
            bottom.Padding = new Padding(0, 10, 10, 8);
            bottom.BackColor = Card;
            Button close = CreateActionButton("Close", Accent, delegate { dialog.Close(); });
            Button copy = CreateActionButton("Copy", AccentGreen, delegate { try { Clipboard.SetText(box.Text); } catch { } });
            bottom.Controls.Add(close);
            bottom.Controls.Add(copy);
            dialog.Controls.Add(bottom);
            dialog.ShowDialog(this);
        }

        private bool ValidatePack(bool showMessage)
        {
            ApplyGridEdits();
            ApplyAssetGridEdits("Music");
            ApplyAssetGridEdits("Characters");
            ApplyAssetGridEdits("UI / REL Assets");
            List<string> problems = new List<string>();
            foreach (TrackSlotInfo s in Slots)
            {
                if (string.IsNullOrWhiteSpace(s.SourcePath)) continue;
                if (!File.Exists(s.SourcePath)) problems.Add(s.GameFile + ": file missing");
                else if (!Path.GetExtension(s.SourcePath).Equals(".szs", StringComparison.OrdinalIgnoreCase)) problems.Add(s.GameFile + ": must be .szs");
            }
            foreach (AssetFile a in MusicAssets.Concat(CharacterAssets).Concat(UiRelAssets))
            {
                if (!File.Exists(a.SourcePath)) problems.Add(a.OutputName + ": asset file missing");
                if (string.IsNullOrWhiteSpace(a.OutputName)) problems.Add(a.SourcePath + ": missing output name");
            }
            if (showMessage)
            {
                if (problems.Count == 0) MessageBox.Show("Validation passed.", "MKWii Pack Maker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else MessageBox.Show("Validation found issues:\n\n" + string.Join("\n", problems.Take(25)) + (problems.Count > 25 ? "\n..." : ""), "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            AddLog(problems.Count == 0 ? "Validation passed." : "Validation found " + problems.Count + " issue(s).");
            return problems.Count == 0;
        }

        private void BuildExport()
        {
            OperationProgressDialog progress = null;
            try
            {
                progress = new OperationProgressDialog("Building MKWii pack export");
                progress.Show(this);
                progress.SetProgress("Reading export settings...", 2, false);
                progress.AddLine("Starting pack export.");
                Application.DoEvents();

                SaveMetadataFromFields();
                CopyMultiplayerCourseCopies = CopyMultiplayerCheck != null && CopyMultiplayerCheck.Checked;
                IncludeCommunityFallbackFolderPatch = CommunityFallbackCheck != null && CommunityFallbackCheck.Checked;
                string mode = ExportModeCombo == null || ExportModeCombo.SelectedItem == null ? "HNS / Riivolution" : ExportModeCombo.SelectedItem.ToString();
                string parent = OutputFolderText == null ? "" : OutputFolderText.Text.Trim();
                if (string.IsNullOrWhiteSpace(parent)) parent = Path.Combine(AppRoot, "output");
                ExportOutputParentFolder = parent;
                Directory.CreateDirectory(parent);
                string root = CreateUniqueExportFolder(parent, mode);
                Directory.CreateDirectory(root);
                progress.SetProgress("Preparing export folder...", 6, false);
                progress.AddLine("Output folder: " + root);
                Application.DoEvents();

                // Do not let UI SZS files generated by older exports stay in the project and override
                // the newly patched files. This fixes stale Race.szs/MenuSingle.szs issues after testing
                // multiple builds in the same folder.
                int removedStaleAutoAssets = RemoveStaleAutoGeneratedUiAssets();
                if (removedStaleAutoAssets > 0) AddLog("Removed " + removedStaleAutoAssets + " stale auto-generated UI asset(s) before rebuilding.");

                string baseWorkingRoot;
                progress.SetProgress("Copying clean base SZS files to working folder...", 12, false);
                progress.AddLine("Copying base SZS working files.");
                Application.DoEvents();
                int stagedBaseFiles = CopyBaseSzsToWorkingFolder(out baseWorkingRoot);

                progress.SetProgress("Patching character UI icons...", 24, true);
                progress.AddLine("Merging character icon sources into UI SZS files.");
                Application.DoEvents();
                LastAutoPatchedCharacterUi = AutoPatchCharacterInjectIconsFromWorkingBase(baseWorkingRoot);

                progress.SetProgress("Patching Driver.szs when needed...", 34, true);
                progress.AddLine("Merging driver/select model sources.");
                Application.DoEvents();
                LastAutoPatchedCharacterDriver = AutoPatchCharacterDriverFromWorkingBase(baseWorkingRoot);

                progress.SetProgress("Patching cup icons...", 44, true);
                progress.AddLine("Patching selected cup icons into UI archives.");
                Application.DoEvents();
                int autoCupIconUi = AutoPatchCupIconsFromWorkingBase(baseWorkingRoot);

                progress.SetProgress("Patching track and cup names...", 54, true);
                progress.AddLine("Patching BMG text for track and cup names.");
                Application.DoEvents();
                int autoTrackNameUi = AutoPatchTrackNamesFromWorkingBase(baseWorkingRoot);

                progress.SetProgress("Importing auto-patched UI assets...", 62, false);
                Application.DoEvents();
                int autoUi = AutoImportPatchedUiAssetsFromWorkspace();
                int autoReadyUi = AutoImportBaseReadyUiAssets();
                int tracks = 0, assets = 0;

                progress.SetProgress("Copying tracks and custom assets...", 70, false);
                progress.AddLine("Copying tracks, music, characters, and UI assets.");
                Application.DoEvents();

                if (mode == "CTGP My Stuff")
                {
                    string myStuff = Path.Combine(root, "My Stuff");
                    Directory.CreateDirectory(myStuff);
                    tracks += CopyTracksTo(myStuff, true);
                    assets += CopyAssetList(MusicAssets, myStuff, false);
                    assets += CopyAssetList(CharacterAssets, myStuff, false);
                    assets += CopyAssetList(UiRelAssets, myStuff, false);
                }
                else if (mode == "ISO Patch Staging")
                {
                    tracks += CopyTracksTo(Path.Combine(root, "files", "Race", "Course"), true);
                    assets += CopyAssetsByDiscPath(root, "files");
                }
                else
                {
                    string packContentDir = CommunityContentDir(root);
                    Directory.CreateDirectory(packContentDir);
                    Directory.CreateDirectory(Path.Combine(packContentDir, "Tracks"));
                    Directory.CreateDirectory(Path.Combine(packContentDir, "Music"));
                    Directory.CreateDirectory(Path.Combine(packContentDir, "Music", "Tracks"));
                    Directory.CreateDirectory(Path.Combine(packContentDir, "StaticR"));
                    Directory.CreateDirectory(Path.Combine(packContentDir, "Menu"));
                    Directory.CreateDirectory(Path.Combine(packContentDir, "Menu", "Language"));
                    Directory.CreateDirectory(Path.Combine(packContentDir, "Characters"));
                    tracks += CopyTracksTo(Path.Combine(packContentDir, "Tracks"), true);
                    assets += CopyMusicCommunityAssets(packContentDir);
                    assets += CopyUiRelCommunityAssets(packContentDir);
                    assets += CopyCharacterCommunityAssets(packContentDir);
                    progress.SetProgress("Writing Riivolution XML...", 82, false);
                    Application.DoEvents();
                    WriteRiivolutionXml(root, mode);
                    if (mode == "Dolphin Riivolution Install") WriteDolphinNotes(root);
                }

                progress.SetProgress("Writing reports and manifest...", 90, false);
                progress.AddLine("Writing reports, readme, manifest, and tracklist.");
                Application.DoEvents();
                WriteGameReadyUiReport(root);
                WriteAutomaticPatchStatus(root, autoUi, autoReadyUi);
                WriteTracklist(root);
                WriteReadme(root, mode, tracks, assets);
                WriteManifest(root, mode, tracks, assets);
                CleanupOldWorkingFolders(2);
                progress.SetProgress("Export complete.", 100, false);
                progress.AddLine("Done: " + root);
                Application.DoEvents();

                AddLog("Export built: " + root);
                LastExportFolder = root;
                try { if (progress != null && !progress.IsDisposed) progress.Close(); } catch { }
                string buildSummary = "Export built successfully.\n\nTracks copied: " + tracks + "\nExtra assets copied: " + assets + (stagedBaseFiles > 0 ? "\nBase SZS working copies created: " + stagedBaseFiles : "") + (LastAutoPatchedCharacterUi > 0 ? "\nCharacter icon UI files patched: " + LastAutoPatchedCharacterUi : "") + (LastAutoPatchedCharacterDriver > 0 ? "\nDriver.szs patched from Inject/Driver: " + LastAutoPatchedCharacterDriver : "") + (autoCupIconUi > 0 ? "\nCup-icon UI files patched: " + autoCupIconUi : "") + (autoTrackNameUi > 0 ? "\nTrack/cup-name UI files patched: " + autoTrackNameUi : "") + (autoUi > 0 ? "\nPatched UI files auto-added: " + autoUi : "") + (autoReadyUi > 0 ? "\nGame-ready UI files auto-added: " + autoReadyUi : "") + "\n\nFolder: " + root + "\n\nOpen the exported pack folder now?";
                DialogResult openResult = MessageBox.Show(buildSummary, "Build Complete", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (openResult == DialogResult.Yes) OpenFolder(root);
            }
            catch (Exception ex)
            {
                AddLog("Export failed: " + ex.Message);
                try
                {
                    if (progress != null && !progress.IsDisposed)
                    {
                        progress.SetProgress("Export failed.", 100, false);
                        progress.AddLine("ERROR: " + ex.Message);
                    }
                }
                catch { }
                MessageBox.Show(this, "Export failed:\n\n" + ex.Message + "\n\n" + ExplainExportError(ex.Message), "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                try { if (progress != null && !progress.IsDisposed) progress.Close(); } catch { }
            }
        }

        private string CommunityPackFolderName()
        {
            return SafeId(PackId);
        }

        private string CommunityExternalBase()
        {
            return "/hns/" + CommunityPackFolderName();
        }

        private string CommunityContentDir(string root)
        {
            return Path.Combine(root, "hns", CommunityPackFolderName());
        }

        private string CreateUniqueExportFolder(string parent, string mode)
        {
            string modeId = SafeId(mode.Replace("/", "_").Replace(" ", "_"));
            string baseName = SafeId(PackId) + "_" + modeId;
            string candidate = Path.Combine(parent, baseName);
            if (!Directory.Exists(candidate) && !File.Exists(candidate)) return candidate;
            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            candidate = Path.Combine(parent, baseName + "_" + stamp);
            int n = 2;
            while (Directory.Exists(candidate) || File.Exists(candidate))
            {
                candidate = Path.Combine(parent, baseName + "_" + stamp + "_" + n);
                n++;
            }
            return candidate;
        }

        private int CopyTracksTo(string dir, bool renameToGameFile)
        {
            Directory.CreateDirectory(dir);
            int copied = 0;
            foreach (TrackSlotInfo s in Slots)
            {
                if (GetSlotStatus(s) != "Ready") continue;
                string destName = renameToGameFile ? s.GameFile : Path.GetFileName(s.SourcePath);
                File.Copy(s.SourcePath, Path.Combine(dir, destName), true);
                copied++;
                if (CopyMultiplayerCourseCopies && s.SlotType == "Race")
                {
                    string baseName = Path.GetFileNameWithoutExtension(s.GameFile);
                    File.Copy(s.SourcePath, Path.Combine(dir, baseName + "_d.szs"), true);
                }
            }
            return copied;
        }

        private int CopyAssetList(List<AssetFile> list, string dir, bool preserveSubfolders)
        {
            Directory.CreateDirectory(dir);
            int copied = 0;
            foreach (AssetFile a in list)
            {
                if (!File.Exists(a.SourcePath)) continue;
                string output = string.IsNullOrWhiteSpace(a.OutputName) ? Path.GetFileName(a.SourcePath) : a.OutputName;
                string rel = preserveSubfolders ? SafeRelativePathForProject(output) : SafeFileName(Path.GetFileName(output.Replace('\\', '/')));
                string dest = Path.Combine(dir, rel.Replace('/', Path.DirectorySeparatorChar));
                string folder = Path.GetDirectoryName(dest);
                if (!string.IsNullOrWhiteSpace(folder)) Directory.CreateDirectory(folder);
                File.Copy(a.SourcePath, dest, true);
                copied++;
            }
            return copied;
        }

        private int CopyMusicCommunityAssets(string packContentDir)
        {
            string musicDir = Path.Combine(packContentDir, "Music");
            Directory.CreateDirectory(musicDir);
            int copied = 0;
            foreach (AssetFile a in MusicAssets)
            {
                if (!File.Exists(a.SourcePath)) continue;
                string output = string.IsNullOrWhiteSpace(a.OutputName) ? Path.GetFileName(a.SourcePath) : a.OutputName;
                output = SafeRelativePathForProject(output);
                string lower = output.ToLowerInvariant();
                if (lower.StartsWith("music/")) output = output.Substring(6).TrimStart('/', '\\');
                string dest = Path.Combine(musicDir, output.Replace('/', Path.DirectorySeparatorChar));
                string folder = Path.GetDirectoryName(dest);
                if (!string.IsNullOrWhiteSpace(folder)) Directory.CreateDirectory(folder);
                File.Copy(a.SourcePath, dest, true);
                copied++;
            }
            return copied;
        }

        private int CopyUiRelCommunityAssets(string packContentDir)
        {
            Directory.CreateDirectory(packContentDir);
            int copied = 0;
            foreach (AssetFile a in UiRelAssets)
            {
                if (!File.Exists(a.SourcePath)) continue;
                string output = string.IsNullOrWhiteSpace(a.OutputName) ? Path.GetFileName(a.SourcePath) : a.OutputName;
                output = SafeRelativePathForProject(output);
                string leaf = Path.GetFileName(output.Replace('\\', '/'));
                string lower = output.ToLowerInvariant();
                string lowerLeaf = leaf.ToLowerInvariant();
                string rel;
                if (lower.StartsWith("menu/")) rel = output;
                else if (lower.StartsWith("language_x/")) rel = "Menu/Language/" + leaf;
                else if (lower.StartsWith("staticr/")) rel = "StaticR/" + CanonicalStaticRelName(leaf);
                else if (lower.StartsWith("music/")) rel = output;
                else if (lowerLeaf.EndsWith(".rel") && IsStaticRelName(leaf)) rel = "StaticR/" + CanonicalStaticRelName(leaf);
                else if (lowerLeaf == "revo_kart.brsar" || lowerLeaf.EndsWith(".brsar")) rel = "Music/" + leaf;
                else rel = "Menu/" + leaf;
                string dest = Path.Combine(packContentDir, rel.Replace('/', Path.DirectorySeparatorChar));
                string folder = Path.GetDirectoryName(dest);
                if (!string.IsNullOrWhiteSpace(folder)) Directory.CreateDirectory(folder);
                File.Copy(a.SourcePath, dest, true);
                copied++;
            }
            return copied;
        }

        private int CopyCharacterCommunityAssets(string packContentDir)
        {
            Directory.CreateDirectory(packContentDir);
            int copied = 0;
            foreach (AssetFile a in CharacterAssets)
            {
                if (!File.Exists(a.SourcePath)) continue;
                if (ShouldSkipFullCharacterUiAsset(a)) continue;
                string output = string.IsNullOrWhiteSpace(a.OutputName) ? Path.GetFileName(a.SourcePath) : a.OutputName;
                output = SafeRelativePathForProject(output);
                string leaf = Path.GetFileName(output.Replace('\\', '/'));
                string lowerLeaf = leaf.ToLowerInvariant();
                string disc = NormalizeDiscPath(string.IsNullOrWhiteSpace(a.DiscPath) ? DefaultDiscPath("Characters", output) : a.DiscPath);
                disc = CorrectKnownCharacterDiscPath(disc, output);

                string rel;
                if (disc.StartsWith("/Scene/UI/", StringComparison.OrdinalIgnoreCase))
                    rel = IsLanguageUiFile(leaf) ? "Menu/Language/" + leaf : "Menu/" + leaf;
                else if (disc.StartsWith("/Scene/Model/Kart/", StringComparison.OrdinalIgnoreCase))
                    rel = "Characters/" + leaf;
                else if (disc.StartsWith("/Scene/Model/", StringComparison.OrdinalIgnoreCase))
                    rel = "Characters/" + leaf;
                else if (disc.Equals("/sound/revo_kart.brsar", StringComparison.OrdinalIgnoreCase))
                    rel = "Music/" + leaf;
                else if (disc.StartsWith("/Race/Kart/", StringComparison.OrdinalIgnoreCase))
                    rel = "Characters/" + leaf;
                else
                    rel = "Characters/" + output;

                string dest = Path.Combine(packContentDir, rel.Replace('/', Path.DirectorySeparatorChar));
                string folder = Path.GetDirectoryName(dest);
                if (!string.IsNullOrWhiteSpace(folder)) Directory.CreateDirectory(folder);
                File.Copy(a.SourcePath, dest, true);
                copied++;
            }
            return copied;
        }

        private int CopyAssetsByDiscPath(string root, string prefix)
        {
            int copied = 0;
            foreach (AssetFile a in MusicAssets.Concat(CharacterAssets).Concat(UiRelAssets))
            {
                if (!File.Exists(a.SourcePath)) continue;
                string disc = NormalizeDiscPath(string.IsNullOrWhiteSpace(a.DiscPath) ? DefaultDiscPath(a.Category, a.OutputName) : a.DiscPath);
                if (string.IsNullOrWhiteSpace(disc)) continue;
                string rel = disc.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                string dest = Path.Combine(root, prefix, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
                File.Copy(a.SourcePath, dest, true);
                copied++;
            }
            return copied;
        }

        private void ExportCupIcons(string root)
        {
            string iconOut = Path.Combine(CommunityContentDir(root), "cup_icons");
            Directory.CreateDirectory(iconOut);
            int fallbackIndex = 0;
            foreach (string cup in CupNames)
            {
                string path = CupIconPaths.ContainsKey(cup) ? CupIconPaths[cup] : "";
                fallbackIndex++;
                // Empty cup icon means keep the original game icon; do not export a default replacement.
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) continue;
                string fileName = SafeFileName(cup) + ".png";
                try { SaveResizedPngIcon(path, Path.Combine(iconOut, fileName), 64); }
                catch { }
            }
        }

        private string DefaultIconPathForCup(string cup, int index)
        {
            string stockDir = Path.Combine(AppRoot, "assets", "powerups");
            if (!Directory.Exists(stockDir)) return "";
            string[] files = Directory.GetFiles(stockDir, "*.png", SearchOption.TopDirectoryOnly).OrderBy(x => x).ToArray();
            if (files.Length == 0) return "";
            int cupIndex = Array.IndexOf(CupNames, cup);
            if (cupIndex < 0) cupIndex = index;
            return files[cupIndex % files.Length];
        }

        private void WriteRiivolutionXml(string root, string mode)
        {
            string xmlDir = Path.Combine(root, "riivolution");
            Directory.CreateDirectory(xmlDir);
            string patchId = SafeId(PackId);
            File.WriteAllText(Path.Combine(xmlDir, patchId + ".xml"), BuildRiivolutionXmlText(root), Encoding.UTF8);
        }

        private string BuildRiivolutionXmlText(string root)
        {
            string id = RegionId();
            string patchId = SafeId(PackId);
            string externalBase = CommunityExternalBase();
            string optionName = string.IsNullOrWhiteSpace(PackName) ? HumanName(PackId) : PackName.Trim();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<wiidisc version=\"1\">");
            sb.AppendLine("  <id game=\"" + id + "\" />");
            sb.AppendLine("  <options>");
            sb.AppendLine("    <section name=\"Hide and Seek\">");
            sb.AppendLine("      <option name=\"My Stuff\">");
            sb.AppendLine("        <choice name=\"" + EscapeXml(optionName) + "\">");
            sb.AppendLine("          <patch id=\"" + patchId + "\" />");
            sb.AppendLine("        </choice>");
            sb.AppendLine("      </option>");
            sb.AppendLine("    </section>");
            sb.AppendLine("  </options>");
            sb.AppendLine("  <patch id=\"" + patchId + "\">");
            sb.AppendLine("    <memory offset=\"0x8000400E\" value=\"01\" />");

            foreach (TrackSlotInfo slot in Slots)
            {
                if (GetSlotStatus(slot) == "Ready")
                    sb.AppendLine("    <file disc=\"/Race/Course/" + EscapeXml(slot.GameFile) + "\" external=\"" + EscapeXml(externalBase) + "/Tracks/" + EscapeXml(slot.GameFile) + "\" />");
            }

            foreach (AssetFile a in MusicAssets)
            {
                if (!File.Exists(a.SourcePath)) continue;
                string outName = SafeRelativePathForProject(string.IsNullOrWhiteSpace(a.OutputName) ? Path.GetFileName(a.SourcePath) : a.OutputName);
                if (outName.ToLowerInvariant().StartsWith("music/")) outName = outName.Substring(6).TrimStart('/', '\\');
                string disc = NormalizeDiscPath(string.IsNullOrWhiteSpace(a.DiscPath) ? DefaultDiscPath("Music", outName) : a.DiscPath);
                if (!string.IsNullOrWhiteSpace(disc))
                    sb.AppendLine("    <file disc=\"" + EscapeXml(disc) + "\" external=\"" + EscapeXml(externalBase) + "/Music/" + EscapeXml(outName.Replace('\\', '/')) + "\" />");
            }

            bool wroteRegionRel = false;
            foreach (AssetFile a in UiRelAssets)
            {
                if (!File.Exists(a.SourcePath)) continue;
                string outName = SafeRelativePathForProject(a.OutputName);
                string leaf = Path.GetFileName(outName.Replace('\\', '/'));
                if (IsStaticRelName(leaf))
                {
                    if (!wroteRegionRel)
                    {
                        sb.AppendLine("    <file disc=\"/rel/StaticR.rel\" external=\"" + EscapeXml(externalBase) + "/StaticR/StaticR{$__region}.rel\" />");
                        wroteRegionRel = true;
                    }
                    continue;
                }
                string disc = NormalizeDiscPath(string.IsNullOrWhiteSpace(a.DiscPath) ? DefaultDiscPath("UI / REL Assets", outName) : a.DiscPath);
                string uiExternal = CommunityExternalForUiRel(outName);
                AppendUiFileMappings(sb, disc, uiExternal);
            }

            foreach (AssetFile a in CharacterAssets)
            {
                if (!File.Exists(a.SourcePath)) continue;
                if (ShouldSkipFullCharacterUiAsset(a)) continue;
                string outName = SafeRelativePathForProject(a.OutputName);
                string disc = NormalizeDiscPath(string.IsNullOrWhiteSpace(a.DiscPath) ? DefaultDiscPath("Characters", outName) : a.DiscPath);
                disc = CorrectKnownCharacterDiscPath(disc, outName);
                string charExternal = CommunityExternalForCharacter(disc, outName);
                AppendCharacterFileMappings(sb, disc, charExternal, outName);
            }

            AppendLanguageXMappings(sb, externalBase, root);

            if (IncludeCommunityFallbackFolderPatch)
            {
                sb.AppendLine("    <folder external=\"" + EscapeXml(externalBase) + "/Tracks\" disc=\"/Race/Course\" />");
                sb.AppendLine("    <folder external=\"" + EscapeXml(externalBase) + "/Music\" disc=\"/sound/strm\" />");
                sb.AppendLine("    <folder external=\"" + EscapeXml(externalBase) + "/Menu\" disc=\"/Scene/UI\" />");
            }
            sb.AppendLine("  </patch>");
            sb.AppendLine("</wiidisc>");
            return sb.ToString();
        }

        private string CommunityExternalForUiRel(string outName)
        {
            string output = SafeRelativePathForProject(outName).Replace('\\', '/');
            string lower = output.ToLowerInvariant();
            string leaf = Path.GetFileName(output);
            if (lower.StartsWith("language_x/")) return CommunityExternalBase() + "/Menu/Language/" + leaf;
            if (lower.StartsWith("menu/")) return CommunityExternalBase() + "/" + output;
            if (lower.StartsWith("staticr/")) return CommunityExternalBase() + "/StaticR/" + CanonicalStaticRelName(leaf);
            if (lower.StartsWith("music/")) return CommunityExternalBase() + "/" + output;
            if (leaf.ToLowerInvariant().EndsWith(".rel") && IsStaticRelName(leaf)) return CommunityExternalBase() + "/StaticR/" + CanonicalStaticRelName(leaf);
            if (leaf.Equals("revo_kart.brsar", StringComparison.OrdinalIgnoreCase) || leaf.ToLowerInvariant().EndsWith(".brsar")) return CommunityExternalBase() + "/Music/" + leaf;
            return CommunityExternalBase() + "/Menu/" + leaf;
        }

        private void AppendUiFileMappings(StringBuilder sb, string disc, string external)
        {
            if (string.IsNullOrWhiteSpace(disc) || string.IsNullOrWhiteSpace(external)) return;
            sb.AppendLine("    <file disc=\"" + EscapeXml(disc) + "\" external=\"" + EscapeXml(external) + "\" />");
        }

        private string CommunityExternalForCharacter(string disc, string outName)
        {
            string output = SafeRelativePathForProject(outName).Replace('\\', '/');
            string leaf = Path.GetFileName(output);
            if (!string.IsNullOrWhiteSpace(disc))
            {
                if (disc.StartsWith("/Scene/UI/", StringComparison.OrdinalIgnoreCase))
                    return CommunityExternalBase() + (IsLanguageUiFile(leaf) ? "/Menu/Language/" : "/Menu/") + leaf;
                if (disc.Equals("/sound/revo_kart.brsar", StringComparison.OrdinalIgnoreCase))
                    return CommunityExternalBase() + "/Music/" + leaf;
            }
            return CommunityExternalBase() + "/Characters/" + leaf;
        }

        private string CorrectKnownCharacterDiscPath(string disc, string outName)
        {
            string output = (outName ?? "").Replace('\\', '/');
            string leaf = Path.GetFileName(output);
            string lower = leaf.ToLowerInvariant();
            if (lower == "driver.szs") return "/Scene/Model/" + leaf;
            if (lower.Contains("allkart")) return "/Scene/Model/Kart/" + leaf;
            if (lower == "revo_kart.brsar") return "/sound/revo_kart.brsar";
            return disc;
        }

        private void AppendCharacterFileMappings(StringBuilder sb, string disc, string external, string outName)
        {
            if (string.IsNullOrWhiteSpace(disc) || string.IsNullOrWhiteSpace(external)) return;
            string lower = outName.Replace('\\', '/').ToLowerInvariant();
            if (lower.StartsWith("inject/") || lower.Contains("/inject/")) return;
            sb.AppendLine("    <file disc=\"" + EscapeXml(disc) + "\" external=\"" + EscapeXml(external) + "\" />");
        }

        private void AppendLanguageVariantMappings(StringBuilder sb, string disc, string external)
        {
            string leaf = Path.GetFileName(disc.Replace('\\', '/'));
            string lower = leaf.ToLowerInvariant();
            string[] variants = new string[] { "U", "E", "J", "F", "G", "S", "I", "K", "Q", "M" };
            string baseName = "";
            if (lower == "menusingle.szs") baseName = "MenuSingle";
            else if (lower == "menumulti.szs") baseName = "MenuMulti";
            else if (lower == "race.szs") baseName = "Race";
            else if (lower == "globe.szs") baseName = "Globe";
            else if (lower == "title.szs") baseName = "Title";
            if (string.IsNullOrEmpty(baseName)) return;
            foreach (string v in variants)
            {
                string variantDisc = "/Scene/UI/" + baseName + "_" + v + ".szs";
                if (!string.Equals(variantDisc, disc, StringComparison.OrdinalIgnoreCase))
                    sb.AppendLine("    <file disc=\"" + EscapeXml(variantDisc) + "\" external=\"" + EscapeXml(external) + "\" />");
            }
        }

        private void AppendLanguageXMappings(StringBuilder sb, string externalBase, string exportRoot)
        {
            string menuLanguageDir = externalBase + "/Menu/Language";
            string oldLanguageDir = externalBase + "/Language_X";
            string contentDir = CommunityContentDir(exportRoot);
            string menuLangLocal = Path.Combine(contentDir, "Menu", "Language");
            if (Directory.Exists(menuLangLocal) && Directory.GetFiles(menuLangLocal, "*.szs", SearchOption.TopDirectoryOnly).Length > 0)
            {
                sb.AppendLine("    <folder disc=\"/Scene/UI\" external=\"" + EscapeXml(menuLanguageDir) + "\" />");
                foreach (string langFile in Directory.GetFiles(menuLangLocal, "*.szs", SearchOption.TopDirectoryOnly))
                {
                    string leaf = Path.GetFileName(langFile);
                    string externalFile = EscapeXml(menuLanguageDir) + "/" + EscapeXml(leaf);
                    sb.AppendLine("    <file disc=\"/Scene/UI/" + EscapeXml(leaf) + "\" external=\"" + externalFile + "\" />");

                    // Some Dolphin/Riivolution setups use a different language suffix than expected
                    // even with an RMCE disc. If we patched MenuSingle_U.szs, also allow that same
                    // patched file to satisfy MenuSingle_E.szs, MenuSingle_F.szs, etc.
                    string noExt = Path.GetFileNameWithoutExtension(leaf);
                    Match langMatch = Regex.Match(noExt, @"^(?<base>.+)_(?<lang>[A-Z])$", RegexOptions.IgnoreCase);
                    if (langMatch.Success)
                    {
                        string baseName = langMatch.Groups["base"].Value;
                        string[] languageVariants = new string[] { "U", "E", "J", "F", "G", "S", "I", "K", "Q", "M" };
                        foreach (string v in languageVariants)
                        {
                            string variantLeaf = baseName + "_" + v + ".szs";
                            if (!string.Equals(variantLeaf, leaf, StringComparison.OrdinalIgnoreCase))
                                sb.AppendLine("    <file disc=\"/Scene/UI/" + EscapeXml(variantLeaf) + "\" external=\"" + externalFile + "\" />");
                        }
                    }
                }
            }

            string oldLangLocal = Path.Combine(contentDir, "Language_X");
            bool hasOldLanguageX = Directory.Exists(oldLangLocal) && (File.Exists(Path.Combine(oldLangLocal, "Language_X.szs")) || File.Exists(Path.Combine(oldLangLocal, "Title_X.szs")));
            if (!hasOldLanguageX) return;

            sb.AppendLine("    <folder disc=\"/Scene/UI\" external=\"" + EscapeXml(oldLanguageDir) + "\" />");
            string[] oldLanguageVariants = new string[] { "I", "S", "K", "J", "F", "G", "Q", "M", "U", "E" };
            foreach (string v in oldLanguageVariants)
            {
                if (File.Exists(Path.Combine(oldLangLocal, "Language_X.szs")))
                {
                    sb.AppendLine("    <file disc=\"/Scene/UI/Globe_" + v + ".szs\" external=\"" + EscapeXml(oldLanguageDir) + "/Language_X.szs\" />");
                    sb.AppendLine("    <file disc=\"/Scene/UI/MenuSingle_" + v + ".szs\" external=\"" + EscapeXml(oldLanguageDir) + "/Language_X.szs\" />");
                }
                if (File.Exists(Path.Combine(oldLangLocal, "Title_X.szs")))
                    sb.AppendLine("    <file disc=\"/Scene/UI/Title_" + v + ".szs\" external=\"" + EscapeXml(oldLanguageDir) + "/Title_X.szs\" />");
            }
            if (File.Exists(Path.Combine(oldLangLocal, "Language_X.szs")))
            {
                sb.AppendLine("    <file disc=\"/Scene/UI/Race_U.szs\" external=\"" + EscapeXml(oldLanguageDir) + "/Language_X.szs\" />");
                sb.AppendLine("    <file disc=\"/Scene/UI/Race_E.szs\" external=\"" + EscapeXml(oldLanguageDir) + "/Language_X.szs\" />");
            }
        }

        private string RegionId()
        {
            return "RMC";
        }



        private string EffectiveBaseSzsFolder()
        {
            // Simple public workflow: the software always uses the base_files folder beside the EXE.
            // No import button and no risky external folder mirroring.
            string local = Path.Combine(AppRoot, "base_files");
            if (Directory.Exists(local)) return local;
            return "";
        }

        private int CopyBaseSzsToWorkingFolder(out string workingRoot)
        {
            workingRoot = "";
            try
            {
                string baseFolder = EffectiveBaseSzsFolder();
                if (string.IsNullOrWhiteSpace(baseFolder) || !Directory.Exists(baseFolder))
                {
                    AddLog("Base SZS folder not found. Create ./base_files or set Base Files Folder.");
                    return 0;
                }

                string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                workingRoot = Path.Combine(AppRoot, "Data", "WorkingSZS", SafeId(PackId), stamp);
                Directory.CreateDirectory(workingRoot);

                int copied = 0;
                foreach (string file in Directory.GetFiles(baseFolder, "*.szs", SearchOption.AllDirectories))
                {
                    string rel = BaseSzsCanonicalRelativePath(baseFolder, file);
                    string dest = Path.Combine(workingRoot, rel.Replace('/', Path.DirectorySeparatorChar));
                    string folder = Path.GetDirectoryName(dest);
                    if (!string.IsNullOrWhiteSpace(folder)) Directory.CreateDirectory(folder);
                    File.Copy(file, dest, true);
                    copied++;
                }

                if (copied > 0)
                {
                    AddLog("Copied " + copied + " base SZS file(s) to safe working folder: " + workingRoot);
                    WriteBaseWorkingReadme(workingRoot, baseFolder, copied);
                }
                else
                {
                    AddLog("No .szs files found in base folder: " + baseFolder);
                }
                return copied;
            }
            catch (Exception ex)
            {
                AddLog("Could not copy base SZS files to working folder: " + ex.Message);
                return 0;
            }
        }

        private string BaseSzsCanonicalRelativePath(string baseFolder, string file)
        {
            string rel = SafeRelativePathForProject(Path.GetRelativePath(baseFolder, file));
            string leaf = Path.GetFileName(file);
            string lowerRel = rel.ToLowerInvariant().Replace('\\', '/');
            string lowerLeaf = leaf.ToLowerInvariant();

            if (lowerRel.Contains("scene/ui/")) return "Scene/UI/" + leaf;
            if (lowerRel.Contains("scene/model/kart/")) return "Scene/Model/Kart/" + leaf;
            if (lowerRel.Contains("scene/model/")) return "Scene/Model/" + leaf;
            if (lowerRel.Contains("race/kart/")) return "Race/Kart/" + leaf;
            if (lowerRel.Contains("race/course/")) return "Race/Course/" + leaf;
            if (lowerRel.Contains("sound/") || lowerRel.Contains("audio/")) return "Sound/" + leaf;

            if (IsKnownUiSzs(leaf)) return IsLanguageUiFile(leaf) ? "Scene/UI/Language/" + leaf : "Scene/UI/" + leaf;
            if (lowerLeaf == "driver.szs") return "Scene/Model/Driver.szs";
            if (lowerLeaf.Contains("allkart")) return "Scene/Model/Kart/" + leaf;
            if (LooksLikeKartOrBikeFile(lowerLeaf)) return "Race/Kart/" + leaf;
            if (lowerLeaf.EndsWith("_course.szs") || lowerLeaf.EndsWith("_battle.szs") || lowerLeaf.Contains("course")) return "Race/Course/" + leaf;
            return "Other/" + leaf;
        }

        private bool IsKnownUiSzs(string name)
        {
            string n = Path.GetFileNameWithoutExtension(name ?? "").ToLowerInvariant();
            string[] bases = new string[] { "award", "channel", "event", "globe", "menumulti", "menuother", "menusingle", "present", "race", "title", "language_x", "title_x" };
            foreach (string b in bases)
            {
                if (n == b || n.StartsWith(b + "_")) return true;
            }
            return false;
        }

        private bool LooksLikeKartOrBikeFile(string lowerLeaf)
        {
            if (string.IsNullOrWhiteSpace(lowerLeaf) || !lowerLeaf.EndsWith(".szs")) return false;
            if (lowerLeaf.Contains("_kart-") || lowerLeaf.Contains("_bike-")) return true;
            if (Regex.IsMatch(lowerLeaf, @"^[a-z0-9]+_(kart|bike)_[a-z0-9]+(_[24])?\.szs$")) return true;
            return false;
        }

        private void WriteBaseWorkingReadme(string workingRoot, string baseFolder, int copied)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("MKWii Pack Maker Base SZS Working Copy");
            sb.AppendLine("=====================================");
            sb.AppendLine();
            sb.AppendLine("Source base folder:");
            sb.AppendLine(baseFolder);
            sb.AppendLine();
            sb.AppendLine("Files copied: " + copied);
            sb.AppendLine();
            sb.AppendLine("This folder is a safe copy. The app must patch copies, never your original SZS files.");
            sb.AppendLine("Put your extracted base MKWii files in the app's base_files folder or select a different Base Files Folder.");
            sb.AppendLine();
            sb.AppendLine("Expected canonical folders:");
            sb.AppendLine("- Scene/UI/ for MenuSingle.szs, MenuMulti.szs, Race.szs, *_U.szs, etc.");
            sb.AppendLine("- Scene/Model/ for Driver.szs.");
            sb.AppendLine("- Scene/Model/Kart/ for allkart files.");
            sb.AppendLine("- Race/Kart/ for in-game character/kart/bike SZS files.");
            sb.AppendLine("- Race/Course/ for track slot files.");
            File.WriteAllText(Path.Combine(workingRoot, "README_WORKING_COPY.txt"), sb.ToString(), Encoding.UTF8);
        }



        private string FindWorkingUiFile(string workingRoot, string fileName)
        {
            if (string.IsNullOrWhiteSpace(workingRoot) || string.IsNullOrWhiteSpace(fileName)) return "";
            string uiRoot = Path.Combine(workingRoot, "Scene", "UI");
            string direct = Path.Combine(uiRoot, fileName);
            if (File.Exists(direct)) return direct;
            string language = Path.Combine(uiRoot, "Language", fileName);
            if (File.Exists(language)) return language;
            try
            {
                if (Directory.Exists(uiRoot))
                {
                    string found = Directory.GetFiles(uiRoot, fileName, SearchOption.AllDirectories).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(found) && File.Exists(found)) return found;
                }
            }
            catch { }
            return direct;
        }


        private bool ShouldSkipFullCharacterUiAsset(AssetFile a)
        {
            try
            {
                string output = string.IsNullOrWhiteSpace(a.OutputName) ? Path.GetFileName(a.SourcePath) : a.OutputName;
                string leaf = Path.GetFileName(output.Replace('\\', '/'));
                string disc = NormalizeDiscPath(string.IsNullOrWhiteSpace(a.DiscPath) ? DefaultDiscPath("Characters", output) : a.DiscPath);
                disc = CorrectKnownCharacterDiscPath(disc, output);
                // Full UI SZS files from character packs are merge/patch sources only.
                // Exporting them directly after our auto-patched UI can overwrite cup icons and track-name BMG patches.
                // This intentionally includes language UI files like MenuSingle_U.szs; their useful character-name text
                // is pulled in by FindBestUiSourceForPatching() before the final track-name patch runs.
                return disc.StartsWith("/Scene/UI/", StringComparison.OrdinalIgnoreCase) && IsKnownUiSzs(leaf);
            }
            catch { return false; }
        }

        private List<string> CharacterInjectFiles(string subFolder, string ext)
        {
            string wanted = (subFolder ?? "").Replace('\\', '/').Trim('/').ToLowerInvariant();
            return CharacterAssets
                .Where(a => File.Exists(a.SourcePath))
                .Where(a => Path.GetExtension(a.SourcePath).Equals(ext, StringComparison.OrdinalIgnoreCase))
                .Where(a => SafeRelativePathForProject(a.OutputName).Replace('\\', '/').ToLowerInvariant().Contains("inject/" + wanted + "/"))
                .Select(a => a.SourcePath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private int ReplaceFilesByLeafName(string extractedRoot, IEnumerable<string> replacements)
        {
            int replaced = 0;
            foreach (string replacement in replacements)
            {
                string leaf = Path.GetFileName(replacement);
                if (string.IsNullOrWhiteSpace(leaf)) continue;
                foreach (string target in Directory.GetFiles(extractedRoot, leaf, SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Copy(replacement, target, true);
                        replaced++;
                    }
                    catch { }
                }
            }
            return replaced;
        }

        private int AutoPatchCharacterInjectIconsFromWorkingBase(string workingRoot)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workingRoot) || !Directory.Exists(workingRoot)) return 0;
                List<string> tplIcons = CharacterInjectFiles("Img", ".tpl");
                List<AssetFile> fullUiSources = CharacterFullUiSources();
                if (tplIcons.Count == 0 && fullUiSources.Count == 0) return 0;

                string wszst = !string.IsNullOrWhiteSpace(PatchWszstPath) && File.Exists(PatchWszstPath) ? PatchWszstPath : FindToolOnPath("wszst.exe");
                if (string.IsNullOrWhiteSpace(wszst))
                {
                    AddLog("Character UI merge skipped: wszst.exe was not found. Put Wiimms tools in ./tools or ./tools/bin.");
                    return 0;
                }

                HashSet<string> charIds = InferCharacterIdsForUiMerge();
                string patchedRoot = Path.Combine(workingRoot, "AutoPatchedCharacterUI");
                Directory.CreateDirectory(patchedRoot);
                int patchedFiles = 0;
                int totalReplaced = 0;
                string[] uiFiles = UiIconBaseFiles(PatchIncludeAward);
                foreach (string name in uiFiles)
                {
                    string src = FindBestUiSourceForPatching(workingRoot, name);
                    if (!File.Exists(src)) continue;

                    string temp = Path.Combine(workingRoot, "TempPatchCharacterIcons", Path.GetFileNameWithoutExtension(name));
                    if (Directory.Exists(temp)) Directory.Delete(temp, true);
                    Directory.CreateDirectory(temp);
                    if (!RunTool(wszst, "EXTRACT " + Quote(src) + " --dest " + Quote(temp) + " --overwrite", out string ex1))
                    {
                        AddLog("Could not extract " + name + " for character icon merge. " + ex1);
                        continue;
                    }

                    int replaced = 0;
                    if (tplIcons.Count > 0) replaced += ReplaceFilesByLeafName(temp, tplIcons);

                    AssetFile uiSource = FindMatchingCharacterUiSource(fullUiSources, name);
                    if (uiSource != null && charIds.Count > 0)
                    {
                        string tempSource = Path.Combine(workingRoot, "TempCharacterUiSource", Path.GetFileNameWithoutExtension(name));
                        if (Directory.Exists(tempSource)) Directory.Delete(tempSource, true);
                        Directory.CreateDirectory(tempSource);
                        if (RunTool(wszst, "EXTRACT " + Quote(uiSource.SourcePath) + " --dest " + Quote(tempSource) + " --overwrite", out string exSrc))
                            replaced += ReplaceCharacterIconFilesFromExtractedUi(temp, tempSource, charIds);
                        else
                            AddLog("Could not extract character-pack UI source " + Path.GetFileName(uiSource.SourcePath) + ". " + exSrc);
                    }

                    if (replaced <= 0) continue;
                    string dest = Path.Combine(patchedRoot, name);
                    if (!RunTool(wszst, "CREATE " + Quote(temp) + " --dest " + Quote(dest) + " --overwrite", out string ex2))
                    {
                        AddLog("Could not rebuild " + name + " after character icon merge. " + ex2);
                        continue;
                    }
                    AddAsset("UI / REL Assets", dest, "Menu/" + name);
                    patchedFiles++;
                    totalReplaced += replaced;
                }
                if (patchedFiles > 0)
                    AddLog("Merged only custom character UI images into " + patchedFiles + " base UI SZS file(s), preserving cup icons. Replaced image files: " + totalReplaced + ". Race.szs is handled here for minimap icons, not by the cup-icon patcher.");
                else if (fullUiSources.Count > 0)
                    AddLog("Character pack had full UI SZS files, but no matching character-id image files were found to merge. Full UI files were skipped to avoid blanking cup icons.");
                return patchedFiles;
            }
            catch (Exception ex)
            {
                AddLog("Automatic character icon merge failed: " + ex.Message);
                return 0;
            }
        }

        private List<AssetFile> CharacterFullUiSources()
        {
            List<AssetFile> result = new List<AssetFile>();
            foreach (AssetFile a in CharacterAssets)
            {
                try
                {
                    if (!File.Exists(a.SourcePath)) continue;
                    string output = string.IsNullOrWhiteSpace(a.OutputName) ? Path.GetFileName(a.SourcePath) : a.OutputName;
                    string leaf = Path.GetFileName(output.Replace('\\', '/'));
                    if (IsLanguageUiFile(leaf)) continue;
                    string disc = NormalizeDiscPath(string.IsNullOrWhiteSpace(a.DiscPath) ? DefaultDiscPath("Characters", output) : a.DiscPath);
                    disc = CorrectKnownCharacterDiscPath(disc, output);
                    if (disc.StartsWith("/Scene/UI/", StringComparison.OrdinalIgnoreCase) && IsKnownUiSzs(leaf)) result.Add(a);
                }
                catch { }
            }
            return result;
        }

        private AssetFile FindMatchingCharacterUiSource(List<AssetFile> sources, string uiFileName)
        {
            foreach (AssetFile a in sources)
            {
                string output = string.IsNullOrWhiteSpace(a.OutputName) ? Path.GetFileName(a.SourcePath) : a.OutputName;
                string leaf = Path.GetFileName(output.Replace('\\', '/'));
                if (string.Equals(leaf, uiFileName, StringComparison.OrdinalIgnoreCase)) return a;
            }
            return null;
        }

        private HashSet<string> InferCharacterIdsForUiMerge()
        {
            HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (AssetFile a in CharacterAssets)
            {
                string name = Path.GetFileName((string.IsNullOrWhiteSpace(a.OutputName) ? a.SourcePath : a.OutputName).Replace('\\', '/')).ToLowerInvariant();
                Match m1 = Regex.Match(name, @"-([a-z]{2,3})(?:_[24])?\.szs$");
                if (m1.Success) ids.Add(m1.Groups[1].Value);
                Match m2 = Regex.Match(name, @"^([a-z]{2,3})-allkart");
                if (m2.Success) ids.Add(m2.Groups[1].Value);
                Match m3 = Regex.Match(name, @"allkart[_-]([a-z]{2,3})");
                if (m3.Success) ids.Add(m3.Groups[1].Value);
            }
            return ids;
        }

        private int ReplaceCharacterIconFilesFromExtractedUi(string targetRoot, string sourceRoot, HashSet<string> characterIds)
        {
            int replaced = 0;
            int directMatches = 0;
            foreach (string src in Directory.GetFiles(sourceRoot, "*.*", SearchOption.AllDirectories))
            {
                string ext = Path.GetExtension(src).ToLowerInvariant();
                if (ext != ".tpl" && ext != ".png" && ext != ".tex0" && ext != ".tex") continue;
                string rel = Path.GetRelativePath(sourceRoot, src).Replace('\\', '/');
                if (!IsLikelyCharacterUiImage(rel, characterIds)) continue;
                string dest = Path.Combine(targetRoot, rel.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(dest)) continue;
                try
                {
                    File.Copy(src, dest, true);
                    replaced++;
                    directMatches++;
                }
                catch { }
            }

            if (directMatches > 0) return replaced;

            // Many MKWii character packs use generic texture names inside MenuSingle/Race SZS files,
            // so there is no "fk"/"mr"/character-id in the internal texture path. In that case we diff
            // the character-pack UI SZS against the user's base UI SZS and copy only changed, non-cup,
            // non-course images. This preserves cup icons while still importing the custom character
            // portrait/minimap icon.
            int diffMatches = 0;
            foreach (string src in Directory.GetFiles(sourceRoot, "*.*", SearchOption.AllDirectories))
            {
                string ext = Path.GetExtension(src).ToLowerInvariant();
                if (ext != ".tpl" && ext != ".png" && ext != ".tex0" && ext != ".tex") continue;
                string rel = Path.GetRelativePath(sourceRoot, src).Replace('\\', '/');
                if (!IsSafeCharacterUiDiffCandidate(rel)) continue;
                string dest = Path.Combine(targetRoot, rel.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(dest)) continue;
                try
                {
                    string srcHash = SafeFileHash(src);
                    string dstHash = SafeFileHash(dest);
                    if (string.IsNullOrWhiteSpace(srcHash) || string.Equals(srcHash, dstHash, StringComparison.OrdinalIgnoreCase)) continue;
                    File.Copy(src, dest, true);
                    replaced++;
                    diffMatches++;
                }
                catch { }
            }
            if (diffMatches > 0) AddLog("Character UI merge used image-diff fallback and copied " + diffMatches + " changed character UI image(s).");
            return replaced;
        }

        private bool IsLikelyCharacterUiImage(string relativePath, HashSet<string> characterIds)
        {
            if (characterIds == null || characterIds.Count == 0) return false;
            string lower = (relativePath ?? "").Replace('\\', '/').ToLowerInvariant();
            if (!(lower.Contains("/timg/") || lower.EndsWith(".tpl") || lower.EndsWith(".png") || lower.EndsWith(".tex0"))) return false;
            string[] blockWords = new string[] { "cup", "course", "track", "battle", "kinoko", "mushroomcup", "flowercup", "starcup", "specialcup", "shellcup", "bananacup", "leafcup", "lightningcup" };
            foreach (string b in blockWords) if (lower.Contains(b)) return false;
            foreach (string id in characterIds)
            {
                if (Regex.IsMatch(lower, @"(^|[^a-z0-9])" + Regex.Escape(id.ToLowerInvariant()) + @"([^a-z0-9]|$)")) return true;
            }
            return false;
        }

        private bool IsSafeCharacterUiDiffCandidate(string relativePath)
        {
            string lower = (relativePath ?? "").Replace('\\', '/').ToLowerInvariant();
            if (!(lower.Contains("/timg/") || lower.EndsWith(".tpl") || lower.EndsWith(".png") || lower.EndsWith(".tex0") || lower.EndsWith(".tex"))) return false;

            // Do not copy cup/course-selection textures from the character pack. Cup icons are patched separately.
            string[] block = new string[]
            {
                "cup", "course", "track", "battle", "kinoko", "mushroomcup", "flowercup", "starcup", "specialcup",
                "shellcup", "bananacup", "leafcup", "lightningcup", "thunder", "trophy", "preview", "map_model", "button_course"
            };
            foreach (string b in block) if (lower.Contains(b)) return false;

            // Prefer UI image areas where character portraits/minimap icons normally live.
            if (lower.Contains("button/timg") || lower.Contains("control/timg") || lower.Contains("result/timg") || lower.Contains("rank/timg")) return true;

            // Generic fallback: allow small TIMG images, but avoid huge background/layout images.
            if (lower.Contains("/timg/")) return true;
            return false;
        }

        private int AutoPatchCharacterDriverFromWorkingBase(string workingRoot)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workingRoot) || !Directory.Exists(workingRoot)) return 0;
                List<string> brresFiles = CharacterInjectFiles("Driver", ".brres");
                if (brresFiles.Count == 0) return 0;
                string wszst = !string.IsNullOrWhiteSpace(PatchWszstPath) && File.Exists(PatchWszstPath) ? PatchWszstPath : FindToolOnPath("wszst.exe");
                if (string.IsNullOrWhiteSpace(wszst)) return 0;
                string src = Path.Combine(workingRoot, "Scene", "Model", "Driver.szs");
                if (!File.Exists(src)) return 0;
                string temp = Path.Combine(workingRoot, "TempPatchDriver");
                if (Directory.Exists(temp)) Directory.Delete(temp, true);
                Directory.CreateDirectory(temp);
                if (!RunTool(wszst, "EXTRACT " + Quote(src) + " --dest " + Quote(temp) + " --overwrite", out string ex1))
                {
                    AddLog("Could not extract Driver.szs for Inject/Driver merge. " + ex1);
                    return 0;
                }
                int replaced = ReplaceFilesByLeafName(temp, brresFiles);
                if (replaced <= 0) return 0;
                string patchedRoot = Path.Combine(workingRoot, "AutoPatchedCharacterModel");
                Directory.CreateDirectory(patchedRoot);
                string dest = Path.Combine(patchedRoot, "Driver.szs");
                if (!RunTool(wszst, "CREATE " + Quote(temp) + " --dest " + Quote(dest) + " --overwrite", out string ex2))
                {
                    AddLog("Could not rebuild Driver.szs after Inject/Driver merge. " + ex2);
                    return 0;
                }
                AddAsset("Characters", dest, "Driver.szs");
                AddLog("Merged Inject/Driver BRRES into base Driver.szs.");
                return 1;
            }
            catch (Exception ex)
            {
                AddLog("Automatic Driver.szs merge failed: " + ex.Message);
                return 0;
            }
        }


        private int AutoPatchCupIconsFromWorkingBase(string workingRoot)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workingRoot) || !Directory.Exists(workingRoot)) return 0;
                string wimgt = FindToolOnPath("wimgt.exe");
                string wszst = !string.IsNullOrWhiteSpace(PatchWszstPath) && File.Exists(PatchWszstPath) ? PatchWszstPath : FindToolOnPath("wszst.exe");
                if (string.IsNullOrWhiteSpace(wimgt) || string.IsNullOrWhiteSpace(wszst))
                {
                    AddLog("Cup-icon patch skipped: wimgt.exe/wszst.exe were not found. Put Wiimms tools in ./tools or ./tools/bin.");
                    return 0;
                }

                string patchedRoot = Path.Combine(workingRoot, "AutoPatchedCupIcons");
                Directory.CreateDirectory(patchedRoot);
                string iconTemp = Path.Combine(workingRoot, "TempCupIcons64");
                if (Directory.Exists(iconTemp)) Directory.Delete(iconTemp, true);
                Directory.CreateDirectory(iconTemp);

                Dictionary<string, string> preparedIcons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < CupNames.Length; i++)
                {
                    string cup = CupNames[i];
                    string srcIcon = CupIconPaths.ContainsKey(cup) ? CupIconPaths[cup] : "";
                    // Empty cup icon slot means keep the original game icon. Never auto-pick a default.
                    if (string.IsNullOrWhiteSpace(srcIcon) || !File.Exists(srcIcon)) continue;
                    string destPng = Path.Combine(iconTemp, SafeFileName(cup) + ".png");
                    try
                    {
                        SaveResizedPngIcon(srcIcon, destPng, 64);
                        preparedIcons[cup] = destPng;
                        AddLog("Cup-icon patch prepared selected icon: " + cup + " -> " + Path.GetFileName(srcIcon));
                    }
                    catch (Exception ex)
                    {
                        AddLog("Cup-icon patch: could not prepare " + cup + " icon: " + ex.Message);
                    }
                }
                if (preparedIcons.Count == 0)
                {
                    AddLog("Cup-icon patch skipped: no cups have a selected custom icon. Empty cup icon slots keep the original game icons.");
                    return 0;
                }

                int patchedFiles = 0;
                int totalTargets = 0;

                // Fix: patch only the course-select menu archive for cup icons.
                // Earlier broad/slot-ordered patches could hit page arrows, cup-title art, or Race.szs HUD/item icons.
                // MenuSingle.szs is the file Dolphin showed for this user's course-select cup icons.
                List<string> cupUiFiles = new List<string> { "MenuSingle.szs" };
                AddLog("Cup-icon patch mode: selected cups only; MenuSingle.szs only; Race.szs skipped; empty cups keep original icons.");

                foreach (string name in cupUiFiles)
                {
                    string src = FindBestUiSourceForPatching(workingRoot, name);
                    if (!File.Exists(src))
                    {
                        AddLog("Cup-icon patch: base UI file missing: " + name);
                        continue;
                    }
                    string temp = Path.Combine(workingRoot, "TempPatchCupIcons", Path.GetFileNameWithoutExtension(name));
                    if (Directory.Exists(temp)) Directory.Delete(temp, true);
                    Directory.CreateDirectory(temp);
                    if (!RunTool(wszst, "EXTRACT " + Quote(src) + " --dest " + Quote(temp) + " --overwrite", out string ex1))
                    {
                        AddLog("Cup-icon patch: could not extract " + name + ". " + ex1);
                        continue;
                    }

                    int replacedThisFile = 0;
                    foreach (var kv in preparedIcons)
                    {
                        string cup = kv.Key;
                        string png = kv.Value;

                        // Restore the known-working name/path-based search. Do NOT use the slot-ordered square
                        // texture search here; it selected the wrong UI art in 5.6.6.
                        List<string> targets = FindCupIconTargets(temp, cup);
                        if (targets.Count == 0)
                        {
                            targets = FindLikelyCupIconFallbackTargets(temp, cup);
                        }

                        if (targets.Count == 0)
                        {
                            AddLog("Cup-icon patch: no target image found for " + cup + " in " + name + ".");
                            continue;
                        }

                        int changedForCup = 0;
                        foreach (string target in targets)
                        {
                            if (!File.Exists(target)) continue;
                            string relTarget = Path.GetRelativePath(temp, target).Replace('\\', '/');
                            string beforeHash = SafeFileHash(target);
                            if (RunTool(wimgt, "COPY " + Quote(png) + " " + Quote(target) + " --overwrite", out string exImg))
                            {
                                string afterHash = SafeFileHash(target);
                                if (!string.Equals(beforeHash, afterHash, StringComparison.OrdinalIgnoreCase))
                                {
                                    replacedThisFile++;
                                    changedForCup++;
                                    AddLog("Cup-icon patch target changed: " + cup + " -> " + relTarget);
                                }
                            }
                            else
                            {
                                AddLog("Cup-icon patch: wimgt failed for " + relTarget + " in " + name + ". " + exImg);
                            }
                        }
                        AddLog("Cup-icon patch: " + cup + " changed " + changedForCup + " target(s) in " + name + ".");
                    }

                    if (replacedThisFile <= 0)
                    {
                        AddLog("Cup-icon patch: no matching cup icon textures were changed inside " + name + ".");
                        continue;
                    }

                    string dest = Path.Combine(patchedRoot, name);
                    if (!RunTool(wszst, "CREATE " + Quote(temp) + " --dest " + Quote(dest) + " --overwrite", out string ex2))
                    {
                        AddLog("Cup-icon patch: could not rebuild " + name + ". " + ex2);
                        continue;
                    }

                    AddAsset("UI / REL Assets", dest, "Menu/" + name);
                    patchedFiles++;
                    totalTargets += replacedThisFile;
                    AddLog("Cup-icon patch: " + name + " rebuilt with " + replacedThisFile + " image replacement(s).");
                }

                if (patchedFiles > 0) AddLog("Cup-icon patch complete: patched " + patchedFiles + " UI SZS file(s), changed image targets: " + totalTargets + ".");
                else AddLog("Cup-icon patch produced no SZS files. Send latest.log; target-level logging is enabled in 10.2.4.");
                return patchedFiles;
            }
            catch (Exception ex)
            {
                AddLog("Automatic cup-icon patch failed: " + ex.Message);
                return 0;
            }
        }

        private string FindBestUiSourceForPatching(string workingRoot, string uiFileName)
        {
            string leaf = Path.GetFileName(uiFileName ?? "");
            // 1) Prefer already auto-patched UI assets. This preserves character-icon merges when cup icons
            // and track names are patched later in the export pipeline.
            for (int i = UiRelAssets.Count - 1; i >= 0; i--)
            {
                try
                {
                    AssetFile a = UiRelAssets[i];
                    string outName = string.IsNullOrWhiteSpace(a.OutputName) ? Path.GetFileName(a.SourcePath) : a.OutputName;
                    string outLeaf = Path.GetFileName(outName.Replace('\\', '/'));
                    if (string.Equals(outLeaf, leaf, StringComparison.OrdinalIgnoreCase) && File.Exists(a.SourcePath)) return a.SourcePath;
                }
                catch { }
            }

            // 2) Do NOT use full UI SZS files from character packs here.
            // Character-pack MenuSingle/MenuMulti/Race files can contain modified course/cup UI and can
            // overwrite the clean base menu. They are handled only by the character-icon merge step, where
            // selected character icon images are copied into the base UI. Track names and cup icons must
            // always patch the latest auto-patched UI or the clean working base.
            return FindWorkingUiFile(workingRoot, leaf);
        }

        private List<string> FindSlotOrderedCupIconTargets(string extractedUiRoot, string cup)
        {
            List<string> candidates = new List<string>();
            foreach (string file in Directory.GetFiles(extractedUiRoot, "*.*", SearchOption.AllDirectories))
            {
                if (!IsInternalImageFile(file)) continue;
                string rel = Path.GetRelativePath(extractedUiRoot, file).Replace('\\', '/').ToLowerInvariant();
                if (IsBlockedCupPatchTarget(rel)) continue;
                if (!IsSlotCupCandidatePath(rel)) continue;
                if (!IsPlausibleSquareIcon(file)) continue;
                candidates.Add(file);
            }

            candidates = candidates
                .OrderBy(x => NaturalSortKey(Path.GetRelativePath(extractedUiRoot, x).Replace('\\', '/')))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            int cupIndex = Array.IndexOf(CupNames, cup);
            if (cupIndex < 0 || candidates.Count < CupNames.Length) return new List<string>();

            List<string> targets = new List<string>();
            for (int i = cupIndex; i < candidates.Count; i += CupNames.Length)
            {
                targets.Add(candidates[i]);
            }
            return targets;
        }

        private bool IsSlotCupCandidatePath(string lowerRel)
        {
            if (string.IsNullOrWhiteSpace(lowerRel)) return false;
            if (IsBlockedCupPatchTarget(lowerRel)) return false;

            // Strong hints used by MKWii/custom distributions for cup icon textures.
            if (lowerRel.Contains("cup") || lowerRel.Contains("trophy") || lowerRel.Contains("cupicon")) return true;
            if (lowerRel.Contains("selectcup") || lowerRel.Contains("cup_select")) return true;

            // Common short numbered texture patterns found inside menu/timg folders. These are only accepted
            // if the image itself is square/icon-like by IsPlausibleSquareIcon().
            string file = Path.GetFileNameWithoutExtension(lowerRel);
            if ((lowerRel.Contains("/timg/") || lowerRel.Contains("\\timg\\")) && Regex.IsMatch(file, @"^(c|cup|cupicon|kc|icon|i)[_\-]?[0-9]{1,2}$")) return true;
            if ((lowerRel.Contains("/button/") || lowerRel.Contains("/control/")) && Regex.IsMatch(file, @"^(c|cup|cupicon|kc)[_\-]?[0-9]{1,2}$")) return true;

            return false;
        }

        private bool IsPlausibleSquareIcon(string file)
        {
            try
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp")
                {
                    using (Image img = Image.FromFile(file))
                    {
                        if (img.Width != img.Height) return false;
                        if (img.Width < 24 || img.Width > 128) return false;
                        return true;
                    }
                }
            }
            catch { return false; }

            // For TPL/TEX files that System.Drawing cannot open, rely on name/path filtering.
            string rel = file.ToLowerInvariant();
            if (rel.Contains("arrow") || rel.Contains("scroll") || rel.Contains("page")) return false;
            return true;
        }

        private string NaturalSortKey(string text)
        {
            if (text == null) return "";
            return Regex.Replace(text.ToLowerInvariant(), @"\d+", m => m.Value.PadLeft(8, '0'));
        }

        private List<string> FindCupIconTargets(string extractedUiRoot, string cup)
        {
            List<string> targets = new List<string>();
            string[] words = CupIconKeywords(cup);
            foreach (string file in Directory.GetFiles(extractedUiRoot, "*.*", SearchOption.AllDirectories))
            {
                if (!IsInternalImageFile(file)) continue;
                string rel = Path.GetRelativePath(extractedUiRoot, file).Replace('\\', '/').ToLowerInvariant();
                if (!IsLikelyCupTexturePath(rel)) continue;
                foreach (string word in words)
                {
                    if (!string.IsNullOrWhiteSpace(word) && rel.Contains(word))
                    {
                        targets.Add(file);
                        break;
                    }
                }
            }
            return targets.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private List<string> FindLikelyCupIconFallbackTargets(string extractedUiRoot, string cup)
        {
            // Known-working fallback from 5.6.1, limited to MenuSingle.szs by the caller.
            // It avoids arbitrary character/map/icon files but allows the cup/trophy/kc/c_ image names
            // used by MKWii menu archives.
            List<string> all = new List<string>();
            foreach (string file in Directory.GetFiles(extractedUiRoot, "*.*", SearchOption.AllDirectories))
            {
                if (!IsInternalImageFile(file)) continue;
                string rel = Path.GetRelativePath(extractedUiRoot, file).Replace('\\', '/').ToLowerInvariant();
                if (IsBlockedCupPatchTarget(rel)) continue;
                bool cupish = rel.Contains("cup") || rel.Contains("trophy") || rel.Contains("timg/kc") || rel.Contains("timg/c_") || rel.Contains("cupicon");
                if (!cupish) continue;
                all.Add(file);
            }
            all = all.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            int cupIndex = Array.IndexOf(CupNames, cup);
            if (cupIndex < 0 || all.Count == 0) return new List<string>();
            if (cupIndex < all.Count) return new List<string> { all[cupIndex] };
            return new List<string>();
        }

        private bool IsBlockedCupPatchTarget(string lowerRel)
        {
            if (string.IsNullOrWhiteSpace(lowerRel)) return true;
            string[] block = new string[]
            {
                "arrow", "cursor", "next", "prev", "previous", "page", "scroll", "slide",
                "driver", "mii", "rank", "character", "chara", "kart", "license",
                "button_player", "mapicon", "item", "hud", "result"
            };
            foreach (string b in block) if (lowerRel.Contains(b)) return true;
            return false;
        }

        private bool IsInternalImageFile(string file)
        {
            string ext = Path.GetExtension(file).ToLowerInvariant();
            return ext == ".tpl" || ext == ".tex" || ext == ".tex0" || ext == ".bti" || ext == ".png";
        }

        private bool IsLikelyCupTexturePath(string lowerRel)
        {
            if (string.IsNullOrWhiteSpace(lowerRel)) return false;
            string[] mustHints = new string[] { "cup", "trophy", "kinoko", "flower", "star", "special", "shell", "banana", "leaf", "thunder", "lightning" };
            bool hasHint = mustHints.Any(h => lowerRel.Contains(h));
            if (!hasHint) return false;
            if (IsBlockedCupPatchTarget(lowerRel)) return false;
            return true;
        }

        private string[] CupIconKeywords(string cup)
        {
            string c = (cup ?? "").ToLowerInvariant();
            if (c.Contains("mushroom")) return new string[] { "mushroom", "kinoko", "kinoco", "fungus", "mush" };
            if (c.Contains("flower")) return new string[] { "flower", "hana" };
            if (c.Contains("star")) return new string[] { "star" };
            if (c.Contains("special")) return new string[] { "special", "sp_cup", "specialcup" };
            if (c.Contains("shell")) return new string[] { "shell", "koura", "koula" };
            if (c.Contains("banana")) return new string[] { "banana" };
            if (c.Contains("leaf")) return new string[] { "leaf", "konoha" };
            if (c.Contains("lightning")) return new string[] { "lightning", "thunder", "bolt", "sandaa" };
            return new string[] { SafeId(c) };
        }

        private string SafeFileHash(string file)
        {
            try
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                using (var fs = File.OpenRead(file))
                    return BitConverter.ToString(md5.ComputeHash(fs)).Replace("-", "");
            }
            catch { return ""; }
        }

        private int AutoPatchTrackNamesFromWorkingBase(string workingRoot)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workingRoot) || !Directory.Exists(workingRoot)) return 0;

                List<TrackSlotInfo> namedTracks = Slots
                    .Where(x => GetSlotStatus(x) == "Ready" && !string.IsNullOrWhiteSpace(x.CustomName) && !string.Equals(x.CustomName.Trim(), x.TrackName.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();
                Dictionary<string, string> cupNamePatches = SelectedCupNamePatches();
                if (namedTracks.Count == 0 && cupNamePatches.Count == 0)
                {
                    AddLog("BMG name patch skipped: no custom track names or custom cup-icon names need patching.");
                    return 0;
                }

                string wszst = !string.IsNullOrWhiteSpace(PatchWszstPath) && File.Exists(PatchWszstPath) ? PatchWszstPath : FindToolOnPath("wszst.exe");
                string wbmgt = !string.IsNullOrWhiteSpace(PatchWbmgtPath) && File.Exists(PatchWbmgtPath) ? PatchWbmgtPath : FindToolOnPath("wbmgt.exe");
                if (string.IsNullOrWhiteSpace(wszst) || string.IsNullOrWhiteSpace(wbmgt))
                {
                    AddLog("Track-name patch failed: wszst.exe/wbmgt.exe were not found. Put Wiimms tools in ./tools or ./tools/bin.");
                    return 0;
                }

                string suffix = string.IsNullOrWhiteSpace(PatchLanguage) ? "U" : PatchLanguage.Substring(0, 1);
                string patchedRoot = Path.Combine(workingRoot, "AutoPatchedTrackNames");
                Directory.CreateDirectory(patchedRoot);

                List<string> filesToPatch = new List<string>();
                filesToPatch.AddRange(UiLanguageFiles(suffix, PatchIncludeAward));
                // Some bases/custom packs store Common.bmg in regular UI SZS files too. Try them safely; files without Common.bmg are skipped.
                filesToPatch.AddRange(UiIconBaseFiles(PatchIncludeAward));
                filesToPatch = filesToPatch.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                List<string> targetNotes = new List<string>();
                targetNotes.AddRange(namedTracks.Select(t => t.TrackName + " -> " + t.CustomName.Trim()));
                targetNotes.AddRange(cupNamePatches.Select(kv => kv.Key + " -> " + kv.Value));
                AddLog("BMG name patch using precision mode: " + Path.GetFileName(wszst) + " + " + Path.GetFileName(wbmgt) + ". Targets: " + string.Join(", ", targetNotes));
                AddLog("Name patch safety: track internal filenames are ignored; cup names are derived from selected cup icon filenames.");

                int patchedCount = 0;
                int totalReplacements = 0;
                foreach (string name in filesToPatch)
                {
                    string src = FindBestUiSourceForPatching(workingRoot, name);
                    if (!File.Exists(src))
                    {
                        if (IsLanguageUiFile(name)) AddLog("Base UI file missing for track-name patch: " + name);
                        continue;
                    }

                    string temp = Path.Combine(workingRoot, "TempPatchTrackNames", Path.GetFileNameWithoutExtension(name));
                    if (Directory.Exists(temp)) Directory.Delete(temp, true);
                    Directory.CreateDirectory(temp);

                    if (!RunTool(wszst, "EXTRACT " + Quote(src) + " --dest " + Quote(temp) + " --overwrite", out string ex1))
                    {
                        AddLog("Could not extract " + name + " for track-name patch. " + ex1);
                        continue;
                    }

                    string[] bmgFiles = Directory.GetFiles(temp, "*.bmg", SearchOption.AllDirectories);
                    if (bmgFiles.Length == 0) continue;

                    int replacementsThisFile = 0;
                    int patchedBmgFiles = 0;
                    foreach (string bmg in bmgFiles)
                    {
                        string bmgBase = Path.GetFileNameWithoutExtension(bmg);
                        string txt = Path.Combine(temp, bmgBase + "_decoded.txt");
                        if (!RunTool(wbmgt, "DECODE " + Quote(bmg) + " --dest " + Quote(txt) + " --overwrite", out string ex2))
                        {
                            AddLog("Could not decode " + Path.GetFileName(bmg) + " in " + name + ". " + ex2);
                            continue;
                        }
                        if (!File.Exists(txt)) continue;

                        string body = File.ReadAllText(txt, Encoding.UTF8);
                        string updated = body;
                        int replacementsThisBmg = 0;
                        foreach (TrackSlotInfo slot in namedTracks)
                        {
                            string newName = slot.CustomName.Trim();
                            foreach (string oldName in TrackNameSearchTerms(slot))
                            {
                                if (string.IsNullOrWhiteSpace(oldName) || string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase)) continue;
                                updated = ReplaceInsensitiveFirstWithCount(updated, oldName, newName, ref replacementsThisBmg);
                            }
                        }

                        foreach (KeyValuePair<string, string> cupPatch in cupNamePatches)
                        {
                            if (string.IsNullOrWhiteSpace(cupPatch.Key) || string.IsNullOrWhiteSpace(cupPatch.Value)) continue;
                            if (string.Equals(cupPatch.Key.Trim(), cupPatch.Value.Trim(), StringComparison.OrdinalIgnoreCase)) continue;
                            updated = ReplaceInsensitiveWithCount(updated, cupPatch.Key.Trim(), cupPatch.Value.Trim(), ref replacementsThisBmg);
                        }

                        if (updated == body || replacementsThisBmg <= 0) continue;
                        File.WriteAllText(txt, updated, Encoding.UTF8);
                        if (!RunTool(wbmgt, "ENCODE " + Quote(txt) + " --dest " + Quote(bmg) + " --overwrite", out string ex3))
                        {
                            AddLog("Could not encode " + Path.GetFileName(bmg) + " for " + name + ". " + ex3);
                            continue;
                        }
                        patchedBmgFiles++;
                        replacementsThisFile += replacementsThisBmg;
                        AddLog("BMG name patch: " + name + " / " + Path.GetFileName(bmg) + " changed " + replacementsThisBmg + " text occurrence(s).");
                    }

                    if (replacementsThisFile <= 0)
                    {
                        AddLog("BMG name patch: no matching old track/cup names found inside any BMG in " + name + ".");
                        continue;
                    }

                    string dest = Path.Combine(patchedRoot, name);
                    if (!RunTool(wszst, "CREATE " + Quote(temp) + " --dest " + Quote(dest) + " --overwrite", out string ex4))
                    {
                        AddLog("Could not rebuild " + name + " after BMG name patch. " + ex4);
                        continue;
                    }

                    bool verified = VerifyPatchedNamesInSzs(dest, namedTracks, wszst, wbmgt, workingRoot);
                    AddAsset("UI / REL Assets", dest, IsLanguageUiFile(name) ? "Menu/Language/" + name : "Menu/" + name);
                    patchedCount++;
                    totalReplacements += replacementsThisFile;
                    AddLog("BMG name patch: " + name + " updated with " + replacementsThisFile + " text replacement(s). Verify=" + (verified ? "PASS" : "UNKNOWN/FAIL"));
                }

                if (patchedCount > 0) AddLog("Auto-patched track/cup names into " + patchedCount + " UI SZS file(s), total text replacements: " + totalReplacements + ".");
                else AddLog("BMG name patch produced no patched UI files. This usually means the selected base UI files do not contain the original track/cup names in BMG text.");
                return patchedCount;
            }
            catch (Exception ex)
            {
                AddLog("Automatic BMG name patch failed: " + ex.Message);
                return 0;
            }
        }


        private bool VerifyPatchedNamesInSzs(string szsFile, List<TrackSlotInfo> namedTracks, string wszst, string wbmgt, string workingRoot)
        {
            try
            {
                if (!File.Exists(szsFile)) return false;
                string temp = Path.Combine(workingRoot, "TempVerifyNames", Path.GetFileNameWithoutExtension(szsFile));
                if (Directory.Exists(temp)) Directory.Delete(temp, true);
                Directory.CreateDirectory(temp);
                if (!RunTool(wszst, "EXTRACT " + Quote(szsFile) + " --dest " + Quote(temp) + " --overwrite", out string ex1))
                {
                    AddLog("Track-name verify: could not extract final " + Path.GetFileName(szsFile) + ". " + ex1);
                    return false;
                }
                foreach (string bmg in Directory.GetFiles(temp, "*.bmg", SearchOption.AllDirectories))
                {
                    string txt = Path.Combine(temp, Path.GetFileNameWithoutExtension(bmg) + "_verify.txt");
                    if (!RunTool(wbmgt, "DECODE " + Quote(bmg) + " --dest " + Quote(txt) + " --overwrite", out string ex2)) continue;
                    if (!File.Exists(txt)) continue;
                    string body = File.ReadAllText(txt, Encoding.UTF8);
                    bool allFound = true;
                    foreach (TrackSlotInfo slot in namedTracks)
                    {
                        string newName = slot.CustomName.Trim();
                        if (!string.IsNullOrWhiteSpace(newName) && body.IndexOf(newName, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            allFound = false;
                            break;
                        }
                    }
                    if (allFound)
                    {
                        AddLog("Track-name verify: PASS for " + Path.GetFileName(szsFile) + " using " + Path.GetFileName(bmg) + ".");
                        return true;
                    }
                }
                AddLog("Track-name verify: custom names were not found when decoding final " + Path.GetFileName(szsFile) + ".");
                return false;
            }
            catch (Exception ex)
            {
                AddLog("Track-name verify failed: " + ex.Message);
                return false;
            }
        }

        private Dictionary<string, string> SelectedCupNamePatches()
        {
            Dictionary<string, string> patches = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string cup in CupNames)
            {
                string path = CupIconPaths.ContainsKey(cup) ? CupIconPaths[cup] : "";
                string customCupName = CupIconPathToCupName(path);
                if (string.IsNullOrWhiteSpace(customCupName)) continue;
                if (string.Equals(customCupName.Trim(), cup.Trim(), StringComparison.OrdinalIgnoreCase)) continue;
                patches[cup] = customCupName.Trim();
            }
            return patches;
        }

        private string CupIconPathToCupName(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return "";
            string name = Path.GetFileNameWithoutExtension(path);
            if (string.IsNullOrWhiteSpace(name)) return "";
            name = Regex.Replace(name, @"\([^)]*\)", " ");
            name = Regex.Replace(name, @"\[[^\]]*\]", " ");
            name = Regex.Replace(name, @"\bv\d+(\.\d+)*\b", " ", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, @"[_\-]+", " ");
            name = Regex.Replace(name, @"\s+", " ").Trim();
            if (string.IsNullOrWhiteSpace(name)) return "";
            if (!Regex.IsMatch(name, @"\bCup\b", RegexOptions.IgnoreCase)) name += " Cup";
            return name;
        }

        private string GuessTrackNameFromFile(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path ?? "");
            if (string.IsNullOrWhiteSpace(name)) return "Custom Track";
            name = Regex.Replace(name, @"\([^)]*\)", " ");
            name = Regex.Replace(name, @"\[[^\]]*\]", " ");
            name = Regex.Replace(name, @"\bv\d+(\.\d+)*\b", " ", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, @"[_\-]+", " ");
            name = Regex.Replace(name, @"\s+", " ").Trim();
            return string.IsNullOrWhiteSpace(name) ? Path.GetFileNameWithoutExtension(path ?? "") : name;
        }

        private IEnumerable<string> TrackNameSearchTerms(TrackSlotInfo slot)
        {
            // Precision mode: only replace the visible original slot name.
            // Do NOT use internal filenames like castle_course or old_mario_64, because those broad terms
            // can corrupt other course names such as GBA Bowser Castle 3 or GCN Mario Circuit.
            HashSet<string> terms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            void AddTerm(string value)
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                value = value.Trim();
                if (value.Length == 0) return;
                terms.Add(value);
                terms.Add(value.Replace("'", "’"));
                terms.Add(value.Replace("’", "'"));
            }
            AddTerm(slot.TrackName);
            return terms;
        }

        private string ReplaceInsensitiveFirstWithCount(string input, string search, string replacement, ref int count)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(search)) return input;
            string pattern = Regex.Escape(search);
            Match m = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
            if (!m.Success) return input;
            count++;
            return input.Substring(0, m.Index) + replacement + input.Substring(m.Index + m.Length);
        }

        private string ReplaceInsensitiveWithCount(string input, string search, string replacement, ref int count)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(search)) return input;
            int localCount = count;
            string safeReplacement = replacement.Replace("$", "$$");
            string result = Regex.Replace(input, Regex.Escape(search), m =>
            {
                localCount++;
                return safeReplacement;
            }, RegexOptions.IgnoreCase);
            count = localCount;
            return result;
        }

        private string ReplaceInsensitive(string input, string search, string replacement)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(search)) return input;
            return Regex.Replace(input, Regex.Escape(search), m => replacement.Replace("$", "$$"), RegexOptions.IgnoreCase);
        }

        private bool RunTool(string exe, string arguments, out string output)
        {
            output = "";
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = exe;
                psi.Arguments = arguments;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                using (Process p = Process.Start(psi))
                {
                    if (p == null) return false;
                    string stdout = p.StandardOutput.ReadToEnd();
                    string stderr = p.StandardError.ReadToEnd();
                    p.WaitForExit();
                    output = (stdout + " " + stderr).Trim();
                    return p.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                output = ex.Message;
                return false;
            }
        }

        private string Quote(string path)
        {
            return "\"" + (path ?? "").Replace("\"", "\\\"") + "\"";
        }

        private int AutoImportPatchedUiAssetsFromWorkspace()
        {
            SaveMetadataFromFields();
            string workspace = PatchWorkspaceFolder;
            if (string.IsNullOrWhiteSpace(workspace)) return 0;
            string patchedDir = Path.Combine(workspace, "02_put_patched_szs_here");
            if (!Directory.Exists(patchedDir)) return 0;
            int added = 0;
            foreach (string file in Directory.GetFiles(patchedDir, "*.szs", SearchOption.AllDirectories))
            {
                string name = Path.GetFileName(file);
                if (UiRelAssets.Any(x => string.Equals(x.SourcePath, file, StringComparison.OrdinalIgnoreCase))) continue;
                AddAsset("UI / REL Assets", file, IsLanguageUiFile(name) ? "Menu/Language/" + name : "Menu/" + name);
                added++;
            }
            if (added > 0) AddLog("Auto-added " + added + " patched UI SZS file(s) from the saved patch workspace.");
            return added;
        }


        private int AutoImportBaseReadyUiAssets()
        {
            // Build Export should be simple, but it must not pretend loose PNG/TXT files are game-ready.
            // This method only auto-adds real SZS files that the user has already placed in a ready folder.
            // It intentionally does not copy clean base UI files, because that would overwrite menus without applying the user's icons/names.
            SaveMetadataFromFields();
            int added = 0;
            List<string> readyFolders = new List<string>();
            if (!string.IsNullOrWhiteSpace(PatchWorkspaceFolder))
            {
                readyFolders.Add(Path.Combine(PatchWorkspaceFolder, "02_put_patched_szs_here"));
                readyFolders.Add(Path.Combine(PatchWorkspaceFolder, "auto_ready_ui"));
                readyFolders.Add(Path.Combine(PatchWorkspaceFolder, "game_ready_ui"));
            }
            foreach (string folder in readyFolders.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) continue;
                foreach (string file in Directory.GetFiles(folder, "*.szs", SearchOption.AllDirectories))
                {
                    string name = Path.GetFileName(file);
                    if (UiRelAssets.Any(x => string.Equals(x.SourcePath, file, StringComparison.OrdinalIgnoreCase))) continue;
                    AddAsset("UI / REL Assets", file, IsLanguageUiFile(name) ? "Menu/Language/" + name : "Menu/" + name);
                    added++;
                }
            }
            if (added > 0) AddLog("Auto-added " + added + " game-ready UI SZS file(s) from patch workspace ready folders.");
            return added;
        }

        private void WriteAutomaticPatchStatus(string root, int patchedUiCount, int readyUiCount)
        {
            try
            {
                string hnsDir = CommunityContentDir(root);
                Directory.CreateDirectory(hnsDir);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("MKWii Pack Maker Automatic Patch Status");
                sb.AppendLine("======================================");
                sb.AppendLine();
                sb.AppendLine("This export no longer writes loose PNG cup icons or loose TXT name files as if they were game-ready.");
                sb.AppendLine("Mario Kart Wii loads cup icons, character icons, and names from patched SZS files under /Scene/UI.");
                sb.AppendLine();
                sb.AppendLine("Game-ready UI SZS files included from UI / REL Assets or ready folders: " + (patchedUiCount + readyUiCount));
                sb.AppendLine();
                sb.AppendLine("Expected UI files for visual/text changes:");
                sb.AppendLine("- MenuSingle.szs, MenuMulti.szs, MenuOther.szs, Globe.szs, Race.szs for menu/race textures.");
                sb.AppendLine("- MenuSingle_U.szs, MenuMulti_U.szs, MenuOther_U.szs, Globe_U.szs, Race_U.szs for Common.bmg names on USA English.");
                sb.AppendLine("- Use your actual language suffix instead of _U if your game is not USA English.");
                sb.AppendLine();
                sb.AppendLine("Character file rules used by this export:");
                sb.AppendLine("- Driver.szs -> /Scene/Model/Driver.szs");
                sb.AppendLine("- any file containing allkart -> /Scene/Model/Kart/<file>");
                sb.AppendLine("- kart/bike character SZS files -> /Race/Kart/<file>");
                sb.AppendLine("- menu/text SZS files -> /Scene/UI/<file>");
                sb.AppendLine();
                sb.AppendLine("If your icons/names still do not show, the required UI SZS files are not actually patched yet.");
                File.WriteAllText(Path.Combine(hnsDir, "AUTOMATIC_PATCH_STATUS.txt"), sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                AddLog("Could not write automatic patch status: " + ex.Message);
            }
        }

        private bool IsLanguageUiFile(string name)
        {
            return Regex.IsMatch(name ?? "", @"_[A-Za-z]\.szs$", RegexOptions.IgnoreCase);
        }

        private void AutoPrepareUiPatchWorkspaceInsideExport(string exportRoot)
        {
            try
            {
                SaveMetadataFromFields();
                string hnsDir = CommunityContentDir(exportRoot);
                string sourceDir = Path.Combine(hnsDir, "ui_patch_sources");
                Directory.CreateDirectory(sourceDir);
                Directory.CreateDirectory(Path.Combine(sourceDir, "cup_icons_64_menu"));
                Directory.CreateDirectory(Path.Combine(sourceDir, "cup_icons_32_race"));
                Directory.CreateDirectory(Path.Combine(sourceDir, "bmg_text"));
                ExportSelectedCupIconsTo(Path.Combine(sourceDir, "cup_icons_64_menu"));
                ExportSelectedCupIconsTo(Path.Combine(sourceDir, "cup_icons_32_race"));
                WriteNamePatchSource(Path.Combine(sourceDir, "bmg_text"), string.IsNullOrWhiteSpace(PatchLanguage) ? "U" : PatchLanguage.Substring(0, 1));

                string baseUi = PatchBaseUiFolder;
                if (!string.IsNullOrWhiteSpace(baseUi) && Directory.Exists(baseUi))
                {
                    string baseCopy = Path.Combine(sourceDir, "base_ui_copy");
                    Directory.CreateDirectory(baseCopy);
                    string suffix = string.IsNullOrWhiteSpace(PatchLanguage) ? "U" : PatchLanguage.Substring(0, 1);
                    foreach (string f in UiIconBaseFiles(PatchIncludeAward).Concat(UiLanguageFiles(suffix, PatchIncludeAward)))
                    {
                        string src = Path.Combine(baseUi, f);
                        if (File.Exists(src)) File.Copy(src, Path.Combine(baseCopy, f), true);
                    }
                }

                string readme = "This folder is generated automatically when you build an export.\r\n" +
                                "MKWii does not load PNG/TXT files directly. Use these as sources to patch MenuSingle.szs, MenuMulti.szs, Race.szs, Globe.szs, and language *_X.szs files.\r\n" +
                                "If you put finished patched SZS files in your saved patch workspace's 02_put_patched_szs_here folder, the app auto-adds them to UI / REL Assets on export.\r\n";
                File.WriteAllText(Path.Combine(sourceDir, "README_AUTOMATIC_UI_SOURCES.txt"), readme, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                AddLog("Could not prepare automatic UI patch source folder: " + ex.Message);
            }
        }

        private void WriteUiPatchInstructions(string root)
        {
            string hnsDir = CommunityContentDir(root);
            Directory.CreateDirectory(hnsDir);
            string dir = Path.Combine(hnsDir, "ui_patch_inputs");
            Directory.CreateDirectory(dir);

            StringBuilder names = new StringBuilder();
            names.AppendLine("# MKWii Pack Maker generated track-name source");
            names.AppendLine("# This file is not loaded by the game directly.");
            names.AppendLine("# Patch these names into the BMG files inside MenuSingle_*.szs/MenuMulti_*.szs/Race_*.szs.");
            names.AppendLine();
            foreach (TrackSlotInfo s in Slots)
            {
                if (GetSlotStatus(s) != "Ready") continue;
                string custom = string.IsNullOrWhiteSpace(s.CustomName) ? Path.GetFileNameWithoutExtension(s.SourcePath) : s.CustomName;
                names.AppendLine(s.GameFile + " = " + custom);
            }
            File.WriteAllText(Path.Combine(dir, "track_name_patch_source.txt"), names.ToString(), Encoding.UTF8);

            string note = "Game-ready cup icons and names require patched /Scene/UI SZS files.\r\n" +
                          "Loose PNG cup icons and loose TXT track-name lists are source assets only. Mario Kart Wii will not load them by themselves.\r\n\r\n" +
                          "Cup icons: patch the 64x64 menu icon textures into Award.szs, Channel.szs, Event.szs, Globe.szs, MenuMulti.szs, MenuSingle.szs, MenuOther.szs and Race.szs as needed. Race.szs also contains the 32x32 icon used by race UI/minimap-style places.\r\n" +
                          "Track/character names: patch Common.bmg inside language SZS files such as Award_U.szs, Channel_U.szs, Event_U.szs, Globe_U.szs, MenuMulti_U.szs, MenuSingle_U.szs, MenuOther_U.szs and Race_U.szs. Use your actual language suffix.\r\n\r\n" +
                          "Import the already-patched SZS files into the UI / REL Assets tab. This exporter maps exact filenames to /Scene/UI and does not fake PNG/TXT loading.\r\n";
            File.WriteAllText(Path.Combine(dir, "READ_ME_UI_PATCHES.txt"), note, Encoding.UTF8);
        }

        private void WriteGameReadyUiReport(string root)
        {
            string hnsDir = CommunityContentDir(root);
            Directory.CreateDirectory(hnsDir);
            string dir = Path.Combine(hnsDir, "ui_patch_inputs");
            Directory.CreateDirectory(dir);

            HashSet<string> exportedUi = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string menuDir = Path.Combine(hnsDir, "Menu");
            if (Directory.Exists(menuDir))
            {
                foreach (string f in Directory.GetFiles(menuDir, "*.szs", SearchOption.AllDirectories))
                    exportedUi.Add(Path.GetFileName(f));
            }

            string[] iconBaseFiles = new string[] { "Award.szs", "Channel.szs", "Event.szs", "Globe.szs", "MenuMulti.szs", "MenuSingle.szs", "MenuOther.szs", "Race.szs" };
            string[] textFilesU = new string[] { "Award_U.szs", "Channel_U.szs", "Event_U.szs", "Globe_U.szs", "MenuMulti_U.szs", "MenuSingle_U.szs", "MenuOther_U.szs", "Race_U.szs" };

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("MKWii Pack Maker game-ready UI check");
            sb.AppendLine("===================================");
            sb.AppendLine();
            sb.AppendLine("This report checks whether the export contains the SZS files that actually make cup icons, character icons and names appear in-game.");
            sb.AppendLine("Loose PNG/TXT files are source assets only.");
            sb.AppendLine();
            sb.AppendLine("Regular UI SZS files for icon textures:");
            foreach (string name in iconBaseFiles) sb.AppendLine("- " + name + ": " + (exportedUi.Contains(name) ? "FOUND" : "missing"));
            sb.AppendLine();
            sb.AppendLine("US English language SZS files for Common.bmg text:");
            foreach (string name in textFilesU) sb.AppendLine("- " + name + ": " + (exportedUi.Contains(name) ? "FOUND" : "missing"));
            sb.AppendLine();
            sb.AppendLine("If your game language is not US English, use the matching suffix instead of _U. Examples: _E PAL English, _J Japanese, _M NTSC Spanish, _Q NTSC French.");
            sb.AppendLine("For character selection models, Driver.szs must map to /Scene/Model/Driver.szs. This build fixes that mapping.");
            sb.AppendLine("For vehicle menu models, *-allkart.szs must map to /Scene/Model/Kart/.");
            File.WriteAllText(Path.Combine(dir, "GAME_READY_UI_CHECK.txt"), sb.ToString(), Encoding.UTF8);
        }

        private void WriteDolphinNotes(string root)
        {
            string note = "Dolphin Riivolution Install\r\n============================\r\n\r\n1. Open Dolphin.\r\n2. Right-click Mario Kart Wii.\r\n3. Choose Start with Riivolution Patches.\r\n4. Select riivolution/" + SafeId(PackId) + ".xml.\r\n5. Enable the Hide and Seek / My Stuff option.\r\n6. Start the game.\r\n\r\nKeep the hns and riivolution folders together.\r\n";
            File.WriteAllText(Path.Combine(root, "DOLPHIN_INSTALL.txt"), note, Encoding.UTF8);
        }

        private void WriteTracklist(string root)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string cup in CupNames)
            {
                var group = Slots.Where(s => s.Cup == cup && GetSlotStatus(s) == "Ready").ToList();
                if (group.Count == 0) continue;
                sb.AppendLine(cup + ":");
                foreach (TrackSlotInfo s in group)
                {
                    string custom = string.IsNullOrWhiteSpace(s.CustomName) ? Path.GetFileNameWithoutExtension(s.SourcePath) : s.CustomName;
                    sb.AppendLine("- " + custom);
                }
                sb.AppendLine();
            }
            string text = sb.ToString();
            File.WriteAllText(Path.Combine(root, SafeId(PackName) + "_Tracklist.txt"), text, Encoding.UTF8);
            string hnsDir = CommunityContentDir(root);
            Directory.CreateDirectory(hnsDir);
            File.WriteAllText(Path.Combine(hnsDir, SafeId(PackName) + "_Tracklist.txt"), text, Encoding.UTF8);
            File.WriteAllText(Path.Combine(hnsDir, "custom_track_names.txt"), text, Encoding.UTF8);
        }

        private void WriteReadme(string root, string mode, int tracks, int assets)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(PackName);
            sb.AppendLine(new string('=', Math.Max(8, PackName.Length)));
            sb.AppendLine();
            sb.AppendLine("Pack ID: " + PackId);
            sb.AppendLine("Author: " + Author);
            sb.AppendLine("Version: " + PackVersion);
            sb.AppendLine("Created With: MKWii Pack Maker " + AppVersion);
            sb.AppendLine("Export Mode: " + mode);
            sb.AppendLine("Tracks Copied: " + tracks + "/" + Slots.Count);
            sb.AppendLine("Extra Assets Copied: " + assets);
            sb.AppendLine();
            sb.AppendLine(Description);
            sb.AppendLine();
            sb.AppendLine("Folder Notes");
            sb.AppendLine("------------");
            sb.AppendLine("hns/<pack id> contains exported tracks, music, StaticR REL files, UI files, and Language_X.");
            sb.AppendLine("Characters contains custom character/kart/menu/text/voice replacements. The app detects character names from MKWii file codes in the Characters tab.");
            sb.AppendLine("hns/<pack id>/cup_icons contains selected custom cup icons as source PNG assets.");
            sb.AppendLine("Loose PNG cup icons and TXT track-name lists do not change the game by themselves; in-game names/icons require patched /Scene/UI SZS files and Common.bmg edits.");
            sb.AppendLine("riivolution contains the XML patch for Wii/Dolphin.");
            File.WriteAllText(Path.Combine(root, "README.txt"), sb.ToString(), Encoding.UTF8);
        }

        private void WriteManifest(string root, string mode, int tracks, int assets)
        {
            ProjectState st = CurrentState();
            string json = JsonSerializer.Serialize(new
            {
                app = "MKWii Pack Maker",
                app_version = AppVersion,
                export_mode = mode,
                tracks_copied = tracks,
                extra_assets_copied = assets,
                state = st
            }, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(root, "pack_manifest.json"), json, Encoding.UTF8);
        }

        private void SaveProject(bool forceSaveAs)
        {
            SaveMetadataFromFields();
            ApplyGridEdits();
            ApplyAssetGridEdits("Music");
            ApplyAssetGridEdits("Characters");
            ApplyAssetGridEdits("UI / REL Assets");
            if (forceSaveAs || string.IsNullOrWhiteSpace(ProjectFile))
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Save MKWii Pack Maker project";
                sfd.Filter = "MKWii Pack Project (*.mkwpack.json)|*.mkwpack.json|JSON (*.json)|*.json";
                sfd.FileName = SafeId(PackId) + ".mkwpack.json";
                if (sfd.ShowDialog(this) != DialogResult.OK) return;
                ProjectFile = sfd.FileName;
            }
            string json = JsonSerializer.Serialize(CurrentState(), new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ProjectFile, json, Encoding.UTF8);
            AddLog("Saved project: " + ProjectFile);
        }

        private ProjectState CurrentState()
        {
            SaveMetadataFromFields();
            return new ProjectState
            {
                PackName = PackName,
                PackId = PackId,
                Author = Author,
                Version = PackVersion,
                Description = Description,
                CopyMultiplayerCourseCopies = CopyMultiplayerCourseCopies,
                IncludeCommunityFallbackFolderPatch = IncludeCommunityFallbackFolderPatch,
                Tracks = Slots.Select(s => new TrackSlotInfo { SlotType = s.SlotType, Cup = s.Cup, TrackName = s.TrackName, GameFile = s.GameFile, SourcePath = s.SourcePath, CustomName = s.CustomName }).ToList(),
                Music = MusicAssets.ToList(),
                Characters = CharacterAssets.ToList(),
                UiRelAssets = UiRelAssets.ToList(),
                CupIcons = new Dictionary<string, string>(CupIconPaths),
                ExportOutputParentFolder = ExportOutputParentFolder,
                PatchBaseUiFolder = PatchBaseUiFolder,
                PatchWorkspaceFolder = PatchWorkspaceFolder,
                PatchBrawlCratePath = PatchBrawlCratePath,
                PatchWbmgtPath = PatchWbmgtPath,
                PatchWszstPath = PatchWszstPath,
                PatchLanguage = PatchLanguage,
                PatchIncludeAward = PatchIncludeAward
            };
        }

        private void LoadProjectDialog()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Load MKWii Pack Maker project";
            ofd.Filter = "MKWii Pack Project (*.mkwpack.json)|*.mkwpack.json|JSON (*.json)|*.json|All files (*.*)|*.*";
            if (ofd.ShowDialog(this) != DialogResult.OK) return;
            LoadProject(ofd.FileName);
        }

        private void LoadProject(string path)
        {
            try
            {
                ProjectState st = JsonSerializer.Deserialize<ProjectState>(File.ReadAllText(path, Encoding.UTF8));
                if (st == null) return;
                ProjectFile = path;
                PackName = st.PackName;
                PackId = st.PackId;
                Author = st.Author;
                PackVersion = st.Version;
                Description = st.Description;
                CopyMultiplayerCourseCopies = st.CopyMultiplayerCourseCopies;
                IncludeCommunityFallbackFolderPatch = st.IncludeCommunityFallbackFolderPatch;
                ExportOutputParentFolder = st.ExportOutputParentFolder ?? "";
                PatchBaseUiFolder = st.PatchBaseUiFolder ?? "";
                PatchWorkspaceFolder = st.PatchWorkspaceFolder ?? "";
                PatchBrawlCratePath = st.PatchBrawlCratePath ?? "";
                PatchWbmgtPath = st.PatchWbmgtPath ?? "";
                PatchWszstPath = st.PatchWszstPath ?? "";
                PatchLanguage = string.IsNullOrWhiteSpace(st.PatchLanguage) ? "U - USA English" : st.PatchLanguage;
                PatchIncludeAward = st.PatchIncludeAward;
                foreach (TrackSlotInfo s in Slots)
                {
                    TrackSlotInfo loaded = st.Tracks.FirstOrDefault(x => x.GameFile == s.GameFile);
                    if (loaded != null) { s.SourcePath = loaded.SourcePath; s.CustomName = loaded.CustomName; }
                }
                MusicAssets.Clear(); MusicAssets.AddRange(st.Music ?? new List<AssetFile>());
                CharacterAssets.Clear(); CharacterAssets.AddRange(st.Characters ?? new List<AssetFile>());
                UiRelAssets.Clear(); UiRelAssets.AddRange(st.UiRelAssets ?? new List<AssetFile>());
                InitializeCupIcons();
                if (st.CupIcons != null) foreach (var kv in st.CupIcons) CupIconPaths[kv.Key] = kv.Value;
                AddLog("Loaded project: " + path);
                ShowDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load project.\n\n" + ex.Message, "Load Project", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveMetadataFromFields()
        {
            if (TxtPackName != null) PackName = TxtPackName.Text.Trim();
            if (TxtPackId != null) PackId = SafeId(TxtPackId.Text.Trim());
            if (TxtAuthor != null) Author = TxtAuthor.Text.Trim();
            if (TxtPackVersion != null) PackVersion = TxtPackVersion.Text.Trim();
            if (TxtDescription != null) Description = TxtDescription.Text;
            if (OutputFolderText != null) ExportOutputParentFolder = OutputFolderText.Text.Trim();
            if (PatchBaseUiFolderText != null) PatchBaseUiFolder = PatchBaseUiFolderText.Text.Trim();
            if (PatchOutputFolderText != null) PatchWorkspaceFolder = PatchOutputFolderText.Text.Trim();
            if (PatchBrawlCrateText != null) PatchBrawlCratePath = PatchBrawlCrateText.Text.Trim();
            if (PatchWbmgtText != null) PatchWbmgtPath = PatchWbmgtText.Text.Trim();
            if (PatchWszstText != null) PatchWszstPath = PatchWszstText.Text.Trim();
            if (PatchLanguageCombo != null && PatchLanguageCombo.SelectedItem != null) PatchLanguage = PatchLanguageCombo.SelectedItem.ToString();
            if (PatchAwardCheck != null) PatchIncludeAward = PatchAwardCheck.Checked;
            if (string.IsNullOrWhiteSpace(PackId)) PackId = "mkwii_hns_pack";
            if (string.IsNullOrWhiteSpace(PackName)) PackName = HumanName(PackId);
        }

        private void UpdateReadyLabels()
        {
            int ready = Slots.Count(x => GetSlotStatus(x) == "Ready");
            string text = ready + " / " + Slots.Count + " slots ready";
            if (HeaderReadyLabel != null) HeaderReadyLabel.Text = text;
            if (DashboardReadyLabel != null) DashboardReadyLabel.Text = ready + "/" + Slots.Count;
            if (DashboardAssetLabel != null) DashboardAssetLabel.Text = MusicAssets.Count + " music  •  " + CharacterAssets.Count + " characters  •  " + UiRelAssets.Count + " UI/REL assets";
        }

        private void AddLog(string message)
        {
            string line = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + message + Environment.NewLine;
            try
            {
                string logDir = Path.Combine(AppRoot, "logs");
                Directory.CreateDirectory(logDir);
                File.AppendAllText(Path.Combine(logDir, "latest.log"), line, Encoding.UTF8);
                File.AppendAllText(Path.Combine(logDir, "session_" + DateTime.Now.ToString("yyyyMMdd") + ".log"), line, Encoding.UTF8);
            }
            catch { }

            if (LogBox != null && !LogBox.IsDisposed)
            {
                LogBox.AppendText(line);
                LogBox.ScrollToCaret();
            }
        }

        private string SafeId(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "mkwii_hns_pack";
            string s = Regex.Replace(text.Trim().ToLowerInvariant(), @"[^a-z0-9_\-]+", "_");
            s = Regex.Replace(s, @"_+", "_").Trim('_');
            return string.IsNullOrWhiteSpace(s) ? "mkwii_hns_pack" : s;
        }

        private string HumanName(string id)
        {
            return Regex.Replace(id.Replace('_', ' '), @"\b\w", m => m.Value.ToUpperInvariant());
        }

        private string SafeFileName(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "asset.bin";
            foreach (char c in Path.GetInvalidFileNameChars()) text = text.Replace(c, '_');
            return text.Trim();
        }

        private string SafeRelativePathForProject(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "asset.bin";
            text = text.Replace('\\', '/').Trim('/');
            string[] rawParts = text.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> parts = new List<string>();
            foreach (string part in rawParts)
            {
                if (part == "." || part == "..") continue;
                parts.Add(SafeFileName(part));
            }
            return parts.Count == 0 ? "asset.bin" : string.Join("/", parts);
        }

        private bool IsStaticRelName(string name)
        {
            string n = Path.GetFileName(name.Replace('\\', '/')).ToLowerInvariant();
            return n == "staticr.rel" || Regex.IsMatch(n, @"^staticr?[ejkp]\.rel$");
        }

        private string CanonicalStaticRelName(string name)
        {
            string n = Path.GetFileName(name.Replace('\\', '/'));
            string lower = n.ToLowerInvariant();
            if (lower == "staticr.rel") return "StaticR.rel";
            Match m = Regex.Match(lower, @"^staticr?([ejkp])\.rel$");
            if (m.Success) return "StaticR" + m.Groups[1].Value.ToUpperInvariant() + ".rel";
            return SafeFileName(n);
        }

        private string NormalizeDiscPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "/";
            path = path.Replace('\\', '/').Trim();
            if (!path.StartsWith("/")) path = "/" + path;
            return path;
        }

        private string EscapeXml(string s)
        {
            if (s == null) return "";
            return s.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }


    public class OperationProgressDialog : Form
    {
        private readonly ProgressBar progressBar;
        private readonly Label statusLabel;
        private readonly RichTextBox detailBox;

        public OperationProgressDialog(string title)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(620, 300);
            MinimumSize = new Size(520, 260);
            BackColor = Color.FromArgb(14, 17, 24);
            ForeColor = Color.FromArgb(245, 248, 255);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ControlBox = false;
            ShowInTaskbar = false;

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.Padding = new Padding(18);
            root.RowCount = 4;
            root.ColumnCount = 1;
            root.BackColor = BackColor;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            Label titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Dock = DockStyle.Fill;
            titleLabel.ForeColor = ForeColor;
            titleLabel.Font = new Font("Segoe UI Semibold", 13f, FontStyle.Bold);
            titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            root.Controls.Add(titleLabel, 0, 0);

            statusLabel = new Label();
            statusLabel.Dock = DockStyle.Fill;
            statusLabel.ForeColor = Color.FromArgb(165, 177, 199);
            statusLabel.Font = new Font("Segoe UI", 10f);
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            statusLabel.Text = "Starting...";
            root.Controls.Add(statusLabel, 0, 1);

            progressBar = new ProgressBar();
            progressBar.Dock = DockStyle.Fill;
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;
            root.Controls.Add(progressBar, 0, 2);

            detailBox = new RichTextBox();
            detailBox.Dock = DockStyle.Fill;
            detailBox.BackColor = Color.FromArgb(10, 13, 20);
            detailBox.ForeColor = Color.FromArgb(245, 248, 255);
            detailBox.BorderStyle = BorderStyle.None;
            detailBox.ReadOnly = true;
            detailBox.Font = new Font("Consolas", 9.5f);
            root.Controls.Add(detailBox, 0, 3);
        }

        public void SetProgress(string status, int value, bool marquee)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(delegate { SetProgress(status, value, marquee); }));
                return;
            }
            statusLabel.Text = status;
            progressBar.Style = marquee ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
            if (!marquee)
            {
                if (value < progressBar.Minimum) value = progressBar.Minimum;
                if (value > progressBar.Maximum) value = progressBar.Maximum;
                progressBar.Value = value;
            }
            Refresh();
        }

        public void AddLine(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(delegate { AddLine(text); }));
                return;
            }
            detailBox.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text + Environment.NewLine);
            detailBox.SelectionStart = detailBox.TextLength;
            detailBox.ScrollToCaret();
            Refresh();
        }
    }

    public class RoundedPanel : Panel
    {
        public int Radius { get; set; } = 16;
        public Color BorderColor { get; set; } = Color.FromArgb(55, 64, 83);

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = RoundedRect(rect, Radius))
            using (Pen pen = new Pen(BorderColor, 1))
            {
                e.Graphics.DrawPath(pen, path);
            }
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
