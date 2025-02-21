using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MusicPlayer
{
    public partial class SearchForm : Form
    {
        private List<Song> songs;
        private List<Playlist> playlists;
        private HomeForm homeForm;
        private PrivateFontCollection privateFonts;

        public SearchForm(HomeForm form, List<Song> songs, List<Playlist> playlists)
        {
            InitializeComponent();
            homeForm = form;
            this.songs = songs;
            this.playlists = playlists;

            InitializeFonts();
            InitializeForm();
            this.Load += new EventHandler(this.SearchForm_Load);
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
            this.Text = "Search";
            this.BackColor = Color.FromArgb(30, 30, 30); // Dark background color
        }

        private void SearchForm_Load(object sender, EventArgs e)
        {
            InitializeSearchPage();
        }

        private void InitializeSearchPage()
        {
            Label searchLabel = new Label
            {
                Text = "Search",
                Font = GetCustomFont(16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 10),
                AutoSize = true
            };

            TextBox searchBox = new TextBox
            {
                Name = "SearchBox",
                Location = new Point(10, 40),
                Width = 250,
                Font = GetCustomFont(12)
            };
            searchBox.TextChanged += SearchBox_TextChanged;

            FlowLayoutPanel resultsPanel = new FlowLayoutPanel
            {
                Name = "ResultsPanel",
                Location = new Point(10, 70),
                Width = this.Width - 20,
                Height = this.Height - 80,
                AutoScroll = true,
                BackColor = Color.FromArgb(45, 45, 45) // Slightly lighter background color
            };

            Controls.Add(searchLabel);
            Controls.Add(searchBox);
            Controls.Add(resultsPanel);
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            TextBox searchBox = sender as TextBox;
            FlowLayoutPanel resultsPanel = Controls.Find("ResultsPanel", true).FirstOrDefault() as FlowLayoutPanel;
            resultsPanel.Controls.Clear();

            string query = searchBox.Text.ToLower();

            var filteredSongs = songs.Where(song =>
                (song.Title?.ToLower().Contains(query) ?? false) ||
                (song.Artist?.ToLower().Contains(query) ?? false)).ToList();

            var filteredPlaylists = playlists.Where(playlist =>
                (playlist.Name?.ToLower().Contains(query) ?? false) ||
                (playlist.Description?.ToLower().Contains(query) ?? false)).ToList();

            Label songsLabel = new Label
            {
                Text = "Songs",
                Font = GetCustomFont(14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true
            };
            resultsPanel.Controls.Add(songsLabel);

            foreach (var song in filteredSongs)
            {
                Panel songPanel = CreateSongPanel(song);
                resultsPanel.Controls.Add(songPanel);
            }

            Label playlistsLabel = new Label
            {
                Text = "Playlists",
                Font = GetCustomFont(14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true
            };
            resultsPanel.Controls.Add(playlistsLabel);

            foreach (var playlist in filteredPlaylists)
            {
                Panel playlistPanel = CreatePlaylistPanel(playlist);
                resultsPanel.Controls.Add(playlistPanel);
            }
        }

        private Panel CreateSongPanel(Song song)
        {
            Panel songPanel = new Panel
            {
                Width = 180,
                Height = 100,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(60, 60, 60) // Panel background color
            };
            songPanel.Click += (s, e) => OpenPlaybackWindow(song);
            songPanel.MouseEnter += (s, e) => songPanel.BackColor = Color.FromArgb(80, 80, 80);
            songPanel.MouseLeave += (s, e) => songPanel.BackColor = Color.FromArgb(60, 60, 60);

            PictureBox thumbnail = new PictureBox
            {
                ImageLocation = song.ThumbnailPath,
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Top,
                Height = 60,
                Cursor = Cursors.Hand
            };
            thumbnail.Click += (s, e) => OpenPlaybackWindow(song);

            Label songLabel = new Label
            {
                Text = $"{song.Title} by {song.Artist}",
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = GetCustomFont(10)
            };
            songLabel.Click += (s, e) => OpenPlaybackWindow(song);

            Button removeButton = new Button
            {
                Text = "X",
                Location = new Point(150, 70),
                Width = 20,
                Height = 20,
                ForeColor = Color.Red,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat
            };
            removeButton.Click += (s, e) => RemoveSongFromApp(song);

            songPanel.Controls.Add(thumbnail);
            songPanel.Controls.Add(songLabel);
            songPanel.Controls.Add(removeButton);

            return songPanel;
        }

        private void RemoveSongFromApp(Song song)
        {
            var confirmResult = MessageBox.Show($"Are you sure you want to delete {song.Title}? If you proceed, {song.Title} will permanently be deleted from the app.",
                                                 "Confirm Delete",
                                                 MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Warning);
            if (confirmResult == DialogResult.Yes)
            {
                songs.Remove(song);
                homeForm.SaveSongsToFile("songs.json");
                homeForm.RefreshUI(); // Refresh the UI components in HomeForm

                // Refresh the search results
                SearchBox_TextChanged(Controls.Find("SearchBox", true).FirstOrDefault(), EventArgs.Empty);
            }
        }

        private Panel CreatePlaylistPanel(Playlist playlist)
        {
            Panel playlistPanel = new Panel
            {
                Width = 180,
                Height = 100,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(60, 60, 60) // Panel background color
            };
            playlistPanel.Click += (s, e) => OpenPlaylistPage(playlist);
            playlistPanel.MouseEnter += (s, e) => playlistPanel.BackColor = Color.FromArgb(80, 80, 80);
            playlistPanel.MouseLeave += (s, e) => playlistPanel.BackColor = Color.FromArgb(60, 60, 60);

            PictureBox thumbnail = new PictureBox
            {
                ImageLocation = playlist.ThumbnailPath,
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Top,
                Height = 60,
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
                Font = GetCustomFont(10)
            };
            playlistLabel.Click += (s, e) => OpenPlaylistPage(playlist);

            playlistPanel.Controls.Add(thumbnail);
            playlistPanel.Controls.Add(playlistLabel);

            return playlistPanel;
        }

        private void OpenPlaybackWindow(Song song)
        {
            homeForm.PlaySong(song.FilePath, song.Title, song.Artist, song.ThumbnailPath);
        }

        private void OpenPlaylistPage(Playlist playlist)
        {
            homeForm.OpenForm(new PlaylistForm(homeForm, playlist));
        }
    }
}