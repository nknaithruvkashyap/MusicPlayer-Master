//imports
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace MusicPlayer
{
    public partial class HomeForm : Form
    {
        public List<Playlist> playlists; // Changed to public
        public List<Song> songs; // Changed to public
        private PlaybackControl playbackControl;
        private PrivateFontCollection privateFonts;

        public HomeForm()
        {
            InitializeFonts();
            InitializePlaybackControl();
            InitializeForm();
        }

        private void InitializeFonts()
        {
            privateFonts = new PrivateFontCollection();
            string fontResource = "MusicPlayer.Fonts.ReadexPro-Regular.ttf";
            using (Stream fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fontResource))
            {
                if (fontStream != null)
                {
                    byte[] fontData = new byte[fontStream.Length];
                    fontStream.Read(fontData, 0, (int)fontStream.Length);

                    IntPtr fontPtr = Marshal.AllocCoTaskMem(fontData.Length);
                    Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
                    privateFonts.AddMemoryFont(fontPtr, fontData.Length);
                    Marshal.FreeCoTaskMem(fontPtr);
                }
            }
        }

        private Font GetCustomFont(float size, FontStyle style = FontStyle.Regular)
        {
            if (privateFonts.Families.Length > 0)
            {
                return new Font(privateFonts.Families[0], size, style);
            }
            else
            {
                return new Font("Arial", size, style); // Fallback to Arial if custom font fails to load
            }
        }

        private void InitializeForm()
        {
            this.Load += new EventHandler(this.HomeForm_Load);
            this.Text = "Home";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(30, 30, 30); // Dark background color
        }

        private void InitializePlaybackControl()
        {
            this.playbackControl = new PlaybackControl();
            this.Controls.Add(this.playbackControl);
            this.playbackControl.Dock = DockStyle.Bottom;
        }

        private void HomeForm_Load(object sender, EventArgs e)
        {
            LoadData();
            CleanupInvalidSongs();
            InitializeNavigationBar();
            InitializeRecentlyPlayedPlaylistsSection();
            InitializeRecentlyPlayedSongsSection();
        }

        private void LoadData()
        {
            playlists = LoadPlaylistsFromFile("playlists.json");
            songs = LoadSongsFromFile("songs.json");
        }

        private List<Playlist> LoadPlaylistsFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<Playlist>>(json);
            }
            return new List<Playlist>();
        }

        private List<Song> LoadSongsFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<Song>>(json);
            }
            return new List<Song>();
        }

        public void SavePlaylistsToFile(string filePath)
        {
            string json = JsonConvert.SerializeObject(playlists, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public void SaveSongsToFile(string filePath)
        {
            string json = JsonConvert.SerializeObject(songs, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public void OpenForm(Form form)
        {
            form.TopLevel = false;
            this.Controls.Add(form);
            form.Dock = DockStyle.Fill;
            form.BringToFront();
            form.Show();
        }

        private void InitializeNavigationBar()
        {
            Panel navigationBar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 150,
                BackColor = Color.FromArgb(45, 45, 45) // Slightly lighter background color
            };

            string[] buttons = { "Upload", "Library", "Search", "Home" };
            foreach (string buttonText in buttons)
            {
                Button button = new Button
                {
                    Text = buttonText,
                    Dock = DockStyle.Top,
                    Height = 50,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(60, 60, 60), // Button background color
                    Font = GetCustomFont(12, FontStyle.Bold)
                };
                button.FlatAppearance.BorderSize = 0;
                button.Click += NavigationButton_Click;
                navigationBar.Controls.Add(button);
            }

            Controls.Add(navigationBar);
        }

        private void NavigationButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            switch (clickedButton.Text)
            {
                case "Search":
                    OpenForm(new SearchForm(this, songs, playlists));
                    break;
                case "Library":
                    OpenForm(new LibraryForm(this, playlists, songs));
                    break;
                case "Upload":
                    OpenForm(new UploadForm(this));
                    break;
                case "Home":
                    CloseAllChildForms();
                    break;
                default:
                    break;
            }
        }

        private void CloseAllChildForms()
        {
            foreach (Form form in this.MdiChildren)
            {
                form.Close();
            }
            InitializeRecentlyPlayedPlaylistsSection();
            InitializeRecentlyPlayedSongsSection();
        }

        private void InitializeRecentlyPlayedPlaylistsSection()
        {
            // Clear existing controls
            Controls.Clear();
            InitializeNavigationBar();
            InitializePlaybackControl();

            Label playlistsLabel = new Label
            {
                Text = "Recently Played Playlists",
                Font = GetCustomFont(12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(160, 10),
                AutoSize = true
            };

            FlowLayoutPanel playlistsPanel = new FlowLayoutPanel
            {
                Location = new Point(160, 40),
                Width = 800,
                Height = 200,
                AutoScroll = true,
                BackColor = Color.FromArgb(45, 45, 45), // Slightly lighter background color
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            foreach (var playlist in playlists)
            {
                Panel playlistPanel = CreatePlaylistPanel(playlist);
                playlistsPanel.Controls.Add(playlistPanel);
            }

            Controls.Add(playlistsLabel);
            Controls.Add(playlistsPanel);
        }

        private void InitializeRecentlyPlayedSongsSection()
        {
            Label songsLabel = new Label
            {
                Text = "Recently Played Songs",
                Font = GetCustomFont(12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(160, 250),
                AutoSize = true
            };

            FlowLayoutPanel songsPanel = new FlowLayoutPanel
            {
                Location = new Point(160, 280),
                Width = 800,
                Height = 200,
                AutoScroll = true,
                BackColor = Color.FromArgb(45, 45, 45), // Slightly lighter background color
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            foreach (var song in songs)
            {
                Panel songPanel = CreateSongPanel(song);
                songsPanel.Controls.Add(songPanel);
            }

            Controls.Add(songsLabel);
            Controls.Add(songsPanel);
        }

        private Panel CreatePlaylistPanel(Playlist playlist)
        {
            Panel playlistPanel = new Panel
            {
                Width = 220,
                Height = 150,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(60, 60, 60), // Panel background color
                Margin = new Padding(10)
            };
            playlistPanel.Click += (s, e) => OpenPlaylistPage(playlist);
            playlistPanel.MouseEnter += (s, e) => playlistPanel.BackColor = Color.FromArgb(80, 80, 80);
            playlistPanel.MouseLeave += (s, e) => playlistPanel.BackColor = Color.FromArgb(60, 60, 60);

            PictureBox thumbnail = new PictureBox
            {
                ImageLocation = playlist.ThumbnailPath,
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Top,
                Height = 100,
                Cursor = Cursors.Hand
            };
            thumbnail.Click += (s, e) => OpenPlaylistPage(playlist);

            Label playlistLabel = new Label
            {
                Text = playlist.Name,
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = GetCustomFont(12)
            };
            playlistLabel.Click += (s, e) => OpenPlaylistPage(playlist);

            playlistPanel.Controls.Add(thumbnail);
            playlistPanel.Controls.Add(playlistLabel);
            return playlistPanel;
        }

        private Panel CreateSongPanel(Song song)
        {
            Panel songPanel = new Panel
            {
                Width = 220,
                Height = 150,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(60, 60, 60), // Panel background color
                Margin = new Padding(10)
            };
            songPanel.Click += (s, e) => OpenPlaybackWindow(song);
            songPanel.MouseEnter += (s, e) => songPanel.BackColor = Color.FromArgb(80, 80, 80);
            songPanel.MouseLeave += (s, e) => songPanel.BackColor = Color.FromArgb(60, 60, 60);

            PictureBox thumbnail = new PictureBox
            {
                ImageLocation = song.ThumbnailPath,
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Top,
                Height = 100,
                Cursor = Cursors.Hand
            };
            thumbnail.Click += (s, e) => OpenPlaybackWindow(song);

            Label songLabel = new Label
            {
                Text = song.Title,
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = GetCustomFont(12)
            };
            songLabel.Click += (s, e) => OpenPlaybackWindow(song);

            songPanel.Controls.Add(thumbnail);
            songPanel.Controls.Add(songLabel);
            return songPanel;
        }

        private void OpenPlaylistPage(Playlist playlist)
        {
            OpenForm(new PlaylistForm(this, playlist));
        }

        private void OpenPlaybackWindow(Song song)
        {
            try
            {
                playbackControl.PlaySong(song.FilePath, song.Title, song.Artist, song.ThumbnailPath);
            }
            catch (DirectoryNotFoundException ex)
            {
                MessageBox.Show($"The file path '{song.FilePath}' could not be found. Please check the file path and try again.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show($"The file '{song.FilePath}' could not be found. Please check the file path and try again.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void PlaySong(string filePath, string title, string artist, string thumbnailPath)
        {
            try
            {
                playbackControl.PlaySong(filePath, title, artist, thumbnailPath);
            }
            catch (DirectoryNotFoundException ex)
            {
                MessageBox.Show($"The file path '{filePath}' could not be found. Please check the file path and try again.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show($"The file '{filePath}' could not be found. Please check the file path and try again.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CleanupInvalidSongs()
        {
            songs.RemoveAll(song => string.IsNullOrEmpty(song.Title) || string.IsNullOrEmpty(song.Artist) || string.IsNullOrEmpty(song.FilePath));
            SaveSongsToFile("songs.json");
        }

        public void RefreshUI()
        {
            InitializeRecentlyPlayedPlaylistsSection();
            InitializeRecentlyPlayedSongsSection();
        }
    }
}