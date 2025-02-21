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
    public partial class PlaylistForm : Form
    {
        private Playlist playlist;
        private HomeForm homeForm;
        private string searchBoxPlaceholder = "Search in playlist...";
        private PrivateFontCollection privateFonts;

        public PlaylistForm(HomeForm form, Playlist playlist)
        {
            InitializeComponent();
            homeForm = form;
            this.playlist = playlist;

            InitializeFonts();
            InitializeForm();
            this.Load += new EventHandler(this.PlaylistForm_Load);
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
            this.Text = "Playlist";
            this.BackColor = Color.FromArgb(30, 30, 30); // Dark background color
        }

        private void PlaylistForm_Load(object sender, EventArgs e)
        {
            InitializePlaylistPage();
        }

        private void InitializePlaylistPage()
        {
            // Clear existing controls
            Controls.Clear();

            PictureBox thumbnail = new PictureBox
            {
                ImageLocation = playlist.ThumbnailPath,
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(10, 10),
                Height = 100,
                Width = 100
            };

            Label playlistLabel = new Label
            {
                Text = playlist.Name,
                Font = GetCustomFont(16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(120, 10),
                AutoSize = true
            };

            Button editPlaylistButton = new Button
            {
                Text = "Edit Playlist",
                Location = new Point(300, 10),
                Width = 100,
                Font = GetCustomFont(12),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };
            editPlaylistButton.Click += EditPlaylistButton_Click;

            Label descriptionLabel = new Label
            {
                Text = playlist.Description,
                Font = GetCustomFont(12, FontStyle.Italic),
                ForeColor = Color.White,
                Location = new Point(120, 40),
                AutoSize = true
            };

            Button manageSongsButton = new Button
            {
                Text = "Manage Songs",
                Location = new Point(300, 40),
                Width = 100,
                Font = GetCustomFont(12),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };
            manageSongsButton.Click += ManageSongsButton_Click;

            Button deletePlaylistButton = new Button
            {
                Text = "Delete Playlist",
                Location = new Point(300, 70),
                Width = 100,
                Font = GetCustomFont(12),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };
            deletePlaylistButton.Click += DeletePlaylistButton_Click;

            TextBox searchBox = new TextBox
            {
                Name = "SearchBox",
                Text = searchBoxPlaceholder,
                ForeColor = Color.Gray,
                Location = new Point(10, 150),
                Width = 200,
                Font = GetCustomFont(12)
            };
            searchBox.GotFocus += RemovePlaceholderText;
            searchBox.LostFocus += AddPlaceholderText;
            searchBox.TextChanged += SearchBox_TextChanged;

            FlowLayoutPanel songsPanel = new FlowLayoutPanel
            {
                Name = "SongsPanel",
                Location = new Point(10, 180),
                Width = this.Width - 20,
                Height = this.Height - 190,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.FromArgb(45, 45, 45) // Slightly lighter background color
            };

            foreach (var song in playlist.Songs)
            {
                Panel songPanel = CreateSongPanel(song);
                songsPanel.Controls.Add(songPanel);
            }

            Controls.Add(thumbnail);
            Controls.Add(playlistLabel);
            Controls.Add(editPlaylistButton);
            Controls.Add(descriptionLabel);
            Controls.Add(manageSongsButton);
            Controls.Add(deletePlaylistButton);
            Controls.Add(searchBox);
            Controls.Add(songsPanel);
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

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            TextBox searchBox = sender as TextBox;
            FlowLayoutPanel songsPanel = Controls.Find("SongsPanel", true).FirstOrDefault() as FlowLayoutPanel;
            songsPanel.Controls.Clear();

            string query = searchBox.Text.ToLower();
            var filteredSongs = playlist.Songs.Where(song => song.Title.ToLower().Contains(query) || song.Artist.ToLower().Contains(query)).ToList();

            foreach (var song in filteredSongs)
            {
                Panel songPanel = CreateSongPanel(song);
                songsPanel.Controls.Add(songPanel);
            }
        }

        private void OpenPlaybackWindow(Song song)
        {
            homeForm.PlaySong(song.FilePath, song.Title, song.Artist, song.ThumbnailPath);
        }

        private void RemoveSongFromPlaylist(Song song)
        {
            var confirmResult = MessageBox.Show($"Are you sure you want to remove {song.Title} from the playlist?",
                                                 "Confirm Remove",
                                                 MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Warning);
            if (confirmResult == DialogResult.Yes)
            {
                playlist.Songs.Remove(song);
                SavePlaylistChanges();
                FlowLayoutPanel songsPanel = Controls.Find("SongsPanel", true).FirstOrDefault() as FlowLayoutPanel;
                songsPanel.Controls.Remove(songsPanel.Controls.OfType<Panel>().FirstOrDefault(p => p.Controls.OfType<Label>().FirstOrDefault().Text == $"{song.Title} by {song.Artist}"));

                // Save playlist changes to file
                homeForm.SavePlaylistsToFile("playlists.json");
            }
        }

        private void RemovePlaceholderText(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text == searchBoxPlaceholder)
            {
                textBox.Text = "";
                textBox.ForeColor = Color.White;
            }
        }

        private void AddPlaceholderText(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = searchBoxPlaceholder;
                textBox.ForeColor = Color.Gray;
            }
        }

        private void EditPlaylistButton_Click(object sender, EventArgs e)
        {
            using (EditPlaylistForm editPlaylistForm = new EditPlaylistForm(playlist))
            {
                if (editPlaylistForm.ShowDialog() == DialogResult.OK)
                {
                    playlist.Name = editPlaylistForm.EditedPlaylist.Name;
                    playlist.ThumbnailPath = editPlaylistForm.EditedPlaylist.ThumbnailPath;
                    playlist.Description = editPlaylistForm.EditedPlaylist.Description;

                    InitializePlaylistPage();
                    // Save changes to file
                    homeForm.SavePlaylistsToFile("playlists.json");
                }
            }
        }

        private void ManageSongsButton_Click(object sender, EventArgs e)
        {
            using (ManageSongsForm manageSongsForm = new ManageSongsForm(homeForm, playlist, homeForm.songs))
            {
                if (manageSongsForm.ShowDialog() == DialogResult.OK)
                {
                    InitializePlaylistPage();
                    // Save changes to file
                    homeForm.SavePlaylistsToFile("playlists.json");
                }
            }
        }

        private void DeletePlaylistButton_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show($"Are you sure you want to delete {playlist.Name}? If you proceed, your songs in {playlist.Name} will be unassigned.",
                                                 "Confirm Delete",
                                                 MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Warning);
            if (confirmResult == DialogResult.Yes)
            {
                homeForm.playlists.Remove(playlist);
                homeForm.SavePlaylistsToFile("playlists.json");
                this.Close();
            }
        }

        private void SavePlaylistChanges()
        {
            homeForm.SavePlaylistsToFile("playlists.json");
        }
    }
}