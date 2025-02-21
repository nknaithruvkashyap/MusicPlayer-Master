using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace MusicPlayer
{
    public partial class LibraryForm : Form
    {
        private List<Playlist> playlists;
        private List<Song> songs;
        private HomeForm homeForm;
        private PrivateFontCollection privateFonts;

        public LibraryForm(HomeForm form, List<Playlist> playlists, List<Song> songs)
        {
            InitializeComponent();
            homeForm = form;
            this.playlists = playlists;
            this.songs = songs;

            InitializeFonts();
            InitializeForm();
            this.Load += new EventHandler(this.LibraryForm_Load);
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
            this.Text = "Library";
            this.BackColor = Color.FromArgb(30, 30, 30); // Dark background color
        }

        private void LibraryForm_Load(object sender, EventArgs e)
        {
            InitializeLibraryPage();
        }

        private void InitializeLibraryPage()
        {
            Label libraryLabel = new Label
            {
                Text = "Library",
                Font = GetCustomFont(16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 10),
                AutoSize = true
            };

            FlowLayoutPanel playlistsPanel = new FlowLayoutPanel
            {
                Name = "PlaylistsPanel",
                Location = new Point(10, 40),
                Width = 800,
                Height = 200,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.FromArgb(45, 45, 45) // Slightly lighter background color
            };

            foreach (var playlist in playlists)
            {
                Panel playlistPanel = CreatePlaylistPanel(playlist);
                playlistsPanel.Controls.Add(playlistPanel);
            }

            Button createPlaylistButton = new Button
            {
                Text = "Create Playlist",
                Location = new Point(820, 40),
                Width = 150,
                Height = 40,
                Font = GetCustomFont(12),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };
            createPlaylistButton.Click += CreatePlaylistButton_Click;

            FlowLayoutPanel songsPanel = new FlowLayoutPanel
            {
                Location = new Point(10, 250),
                Width = 800,
                Height = 200,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.FromArgb(45, 45, 45) // Slightly lighter background color
            };

            foreach (var song in songs)
            {
                Panel songPanel = CreateSongPanel(song);
                songsPanel.Controls.Add(songPanel);
            }

            Controls.Add(libraryLabel);
            Controls.Add(playlistsPanel);
            Controls.Add(createPlaylistButton);
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
                Text = $"{song.Title} by {song.Artist}",
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
            OpenForm(new PlaylistForm(homeForm, playlist));
        }

        private void OpenPlaybackWindow(Song song)
        {
            homeForm.PlaySong(song.FilePath, song.Title, song.Artist, song.ThumbnailPath);
        }

        private void CreatePlaylistButton_Click(object sender, EventArgs e)
        {
            using (CreatePlaylistForm createPlaylistForm = new CreatePlaylistForm())
            {
                if (createPlaylistForm.ShowDialog() == DialogResult.OK)
                {
                    Playlist newPlaylist = createPlaylistForm.NewPlaylist;
                    playlists.Add(newPlaylist);
                    SavePlaylistsToFile("playlists.json");
                    Panel newPlaylistPanel = CreatePlaylistPanel(newPlaylist);
                    FlowLayoutPanel playlistsPanel = Controls.Find("PlaylistsPanel", true).FirstOrDefault() as FlowLayoutPanel;
                    playlistsPanel.Controls.Add(newPlaylistPanel);
                }
            }
        }

        private void SavePlaylistsToFile(string filePath)
        {
            string json = JsonConvert.SerializeObject(playlists, Formatting.Indented);
            System.IO.File.WriteAllText(filePath, json);
        }

        private void OpenForm(Form form)
        {
            form.TopLevel = false;
            this.Controls.Add(form);
            form.Dock = DockStyle.Fill;
            form.BringToFront();
            form.Show();
        }
    }
}