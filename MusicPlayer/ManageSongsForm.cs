using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MusicPlayer
{
    public partial class ManageSongsForm : Form
    {
        private Playlist playlist;
        private List<Song> songs;
        private HomeForm homeForm;

        public ManageSongsForm(HomeForm form, Playlist playlist, List<Song> songs)
        {
            InitializeComponent();
            homeForm = form;
            this.playlist = playlist;
            this.songs = songs;

            this.Load += new EventHandler(this.ManageSongsForm_Load);
        }

        private void ManageSongsForm_Load(object sender, EventArgs e)
        {
            InitializeManageSongsPage();
        }

        private void InitializeManageSongsPage()
        {
            Label manageSongsLabel = new Label
            {
                Text = "Manage Songs",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            TextBox searchBox = new TextBox
            {
                Name = "SearchBox",
                Location = new Point(10, 40),
                Width = 250
            };
            searchBox.TextChanged += SearchBox_TextChanged;

            FlowLayoutPanel resultsPanel = new FlowLayoutPanel
            {
                Name = "ResultsPanel",
                Location = new Point(10, 70),
                Width = this.Width - 20,
                Height = this.Height - 80,
                AutoScroll = true
            };

            Controls.Add(manageSongsLabel);
            Controls.Add(searchBox);
            Controls.Add(resultsPanel);
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            TextBox searchBox = sender as TextBox;
            FlowLayoutPanel resultsPanel = Controls.Find("ResultsPanel", true).FirstOrDefault() as FlowLayoutPanel;
            resultsPanel.Controls.Clear();

            string query = searchBox.Text.ToLower();

            var filteredSongs = songs.Where(song => song.Title.ToLower().Contains(query) || song.Artist.ToLower().Contains(query)).ToList();

            foreach (var song in filteredSongs)
            {
                Panel songPanel = CreateSongPanel(song);
                resultsPanel.Controls.Add(songPanel);
            }
        }

        private Panel CreateSongPanel(Song song)
        {
            Panel songPanel = new Panel
            {
                Width = 180,
                Height = 100,
                BorderStyle = BorderStyle.FixedSingle
            };

            songPanel.MouseEnter += (s, e) => songPanel.BackColor = Color.LightBlue;
            songPanel.MouseLeave += (s, e) => songPanel.BackColor = Color.Transparent;

            PictureBox thumbnail = new PictureBox
            {
                ImageLocation = song.ThumbnailPath,
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Top,
                Height = 42, // Shrink the thumbnail height to 42 pixels
                Width = 42 // Preserve the aspect ratio
            };

            Label songLabel = new Label
            {
                Text = $"{song.Title} by {song.Artist}",
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Button manageButton = new Button
            {
                Text = playlist.Songs.Contains(song) ? "Remove" : "Add",
                Location = new Point(130, 60), // Adjusted location to fit with the shrunken thumbnail
                Width = 50
            };
            manageButton.Click += (s, e) => ManageSong(song, manageButton);

            if (playlist.Songs.Contains(song))
            {
                songLabel.Text = "*" + songLabel.Text;
            }

            songPanel.Controls.Add(thumbnail);
            songPanel.Controls.Add(songLabel);
            songPanel.Controls.Add(manageButton);

            return songPanel;
        }

        private void ManageSong(Song song, Button manageButton)
        {
            if (playlist.Songs.Contains(song))
            {
                playlist.Songs.Remove(song);
                manageButton.Text = "Add";
            }
            else
            {
                playlist.Songs.Add(song);
                manageButton.Text = "Remove";
            }
            SavePlaylistChanges();
        }

        private void SavePlaylistChanges()
        {
            homeForm.SavePlaylistsToFile("playlists.json");
        }

        private void ManageSongsForm_Load_1(object sender, EventArgs e)
        {

        }
    }
}