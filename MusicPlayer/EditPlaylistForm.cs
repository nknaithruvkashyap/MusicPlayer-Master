using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MusicPlayer
{
    public partial class EditPlaylistForm : Form
    {
        public Playlist EditedPlaylist { get; private set; }

        public EditPlaylistForm(Playlist playlist)
        {
            InitializeComponent();
            InitializeEditPlaylistForm(playlist);
        }

        private void InitializeEditPlaylistForm(Playlist playlist)
        {
            this.Text = "Edit Playlist";
            this.Size = new Size(300, 300);

            Label nameLabel = new Label
            {
                Text = "Playlist Name:",
                Location = new Point(10, 10),
                AutoSize = true
            };

            TextBox nameTextBox = new TextBox
            {
                Name = "NameTextBox",
                Location = new Point(10, 30),
                Width = 250,
                Text = playlist.Name
            };

            Label thumbnailLabel = new Label
            {
                Text = "Thumbnail:",
                Location = new Point(10, 60),
                AutoSize = true
            };

            Button thumbnailButton = new Button
            {
                Text = "Choose Thumbnail",
                Location = new Point(10, 80),
                Width = 150
            };
            thumbnailButton.Click += ThumbnailButton_Click;

            Label thumbnailPathLabel = new Label
            {
                Name = "ThumbnailPathLabel",
                Location = new Point(170, 85),
                AutoSize = true,
                Text = playlist.ThumbnailPath
            };

            Label descriptionLabel = new Label
            {
                Text = "Description:",
                Location = new Point(10, 110),
                AutoSize = true
            };

            TextBox descriptionTextBox = new TextBox
            {
                Name = "DescriptionTextBox",
                Location = new Point(10, 130),
                Width = 250,
                Height = 60,
                Multiline = true,
                Text = playlist.Description
            };

            Button saveButton = new Button
            {
                Text = "Save",
                Location = new Point(10, 200),
                Width = 100
            };
            saveButton.Click += SaveButton_Click;

            Controls.Add(nameLabel);
            Controls.Add(nameTextBox);
            Controls.Add(thumbnailLabel);
            Controls.Add(thumbnailButton);
            Controls.Add(thumbnailPathLabel);
            Controls.Add(descriptionLabel);
            Controls.Add(descriptionTextBox);
            Controls.Add(saveButton);
        }

        private void ThumbnailButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image files (*.png, *.jpg)|*.png;*.jpg";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Controls.Find("ThumbnailPathLabel", true)[0].Text = openFileDialog.FileName;
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            string name = Controls.Find("NameTextBox", true)[0].Text;
            string thumbnailPath = Controls.Find("ThumbnailPathLabel", true)[0].Text;
            string description = Controls.Find("DescriptionTextBox", true)[0].Text;

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a name for the playlist.");
                return;
            }

            EditedPlaylist = new Playlist
            {
                Name = name,
                ThumbnailPath = thumbnailPath,
                Description = description,
                Songs = new List<Song>()
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}