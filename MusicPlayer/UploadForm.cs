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
    public partial class UploadForm : Form
    {
        private string thumbnailPath = string.Empty;
        private string songFilePath = string.Empty;
        private HomeForm homeForm;
        private PrivateFontCollection privateFonts;

        public UploadForm(HomeForm form)
        {
            InitializeComponent();
            homeForm = form;
            InitializeFonts();
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

        private void UploadForm_Load(object sender, EventArgs e)
        {
            InitializeUploadPage();
        }

        private void InitializeUploadPage()
        {
            this.BackColor = Color.FromArgb(30, 30, 30); // Dark background color

            Label uploadLabel = new Label
            {
                Text = "Upload MP3",
                Font = GetCustomFont(16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 10),
                AutoSize = true
            };

            Button thumbnailButton = new Button
            {
                Text = "Upload Thumbnail",
                Location = new Point(10, 40),
                Width = 150,
                Font = GetCustomFont(12),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };
            thumbnailButton.Click += ThumbnailButton_Click;

            Label thumbnailPathLabel = new Label
            {
                Name = "ThumbnailPathLabel",
                Location = new Point(170, 40),
                ForeColor = Color.White,
                Font = GetCustomFont(12),
                AutoSize = true
            };

            Button uploadButton = new Button
            {
                Text = "Upload MP3",
                Location = new Point(10, 80),
                Width = 100,
                Font = GetCustomFont(12),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };
            uploadButton.Click += UploadButton_Click;

            Label songPathLabel = new Label
            {
                Name = "SongPathLabel",
                Location = new Point(120, 80),
                ForeColor = Color.White,
                Font = GetCustomFont(12),
                AutoSize = true
            };

            TextBox titleBox = new TextBox
            {
                Name = "TitleBox",
                Text = "Song Title",
                ForeColor = Color.Gray,
                Location = new Point(10, 120),
                Width = 200,
                Font = GetCustomFont(12)
            };
            titleBox.GotFocus += RemovePlaceholderText;
            titleBox.LostFocus += AddPlaceholderText;

            TextBox artistBox = new TextBox
            {
                Name = "ArtistBox",
                Text = "Artist Name",
                ForeColor = Color.Gray,
                Location = new Point(10, 160),
                Width = 200,
                Font = GetCustomFont(12)
            };
            artistBox.GotFocus += RemovePlaceholderText;
            artistBox.LostFocus += AddPlaceholderText;

            Button saveButton = new Button
            {
                Text = "Save",
                Location = new Point(10, 200),
                Width = 100,
                Font = GetCustomFont(12),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };
            saveButton.Click += SaveButton_Click;

            Controls.Add(uploadLabel);
            Controls.Add(thumbnailButton);
            Controls.Add(thumbnailPathLabel);
            Controls.Add(uploadButton);
            Controls.Add(songPathLabel);
            Controls.Add(titleBox);
            Controls.Add(artistBox);
            Controls.Add(saveButton);
        }

        private void ThumbnailButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image files (*.png, *.jpg)|*.png;*.jpg";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    thumbnailPath = openFileDialog.FileName;
                    Controls.Find("ThumbnailPathLabel", true)[0].Text = "Thumbnail: " + thumbnailPath;
                }
            }
        }

        private void UploadButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    songFilePath = openFileDialog.FileName;
                    Controls.Find("SongPathLabel", true)[0].Text = "Song: " + songFilePath;
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            string title = Controls.Find("TitleBox", true)[0].Text;
            string artist = Controls.Find("ArtistBox", true)[0].Text;

            if (string.IsNullOrEmpty(songFilePath) || string.IsNullOrEmpty(title) || title == "Song Title" || string.IsNullOrEmpty(artist) || artist == "Artist Name")
            {
                MessageBox.Show("Please fill in all fields and upload an MP3 file.");
                return;
            }

            SaveMusicFile(songFilePath, title, artist, thumbnailPath);
            MessageBox.Show("Song details saved successfully!");

            this.Close();
        }

        private void SaveMusicFile(string filePath, string title, string artist, string thumbnailPath)
        {
            string saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MusicLibrary");
            Directory.CreateDirectory(saveDir);

            string fileName = Path.GetFileName(filePath);
            string destinationPath = Path.Combine(saveDir, fileName);
            File.Copy(filePath, destinationPath, true);

            string metadataFile = Path.Combine(saveDir, fileName + ".meta");
            File.WriteAllText(metadataFile, $"Title={title}\nArtist={artist}\nThumbnail={thumbnailPath}");

            UpdateJsonFiles(title, artist, destinationPath, thumbnailPath);
        }

        private void UpdateJsonFiles(string title, string artist, string filePath, string thumbnailPath)
        {
            string songsJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "songs.json");
            List<Song> songs;

            if (File.Exists(songsJsonPath))
            {
                string json = File.ReadAllText(songsJsonPath);
                songs = JsonConvert.DeserializeObject<List<Song>>(json);
            }
            else
            {
                songs = new List<Song>();
            }

            Song newSong = new Song
            {
                Title = title,
                Artist = artist,
                FilePath = filePath,
                ThumbnailPath = thumbnailPath
            };

            songs.Add(newSong);
            File.WriteAllText(songsJsonPath, JsonConvert.SerializeObject(songs, Formatting.Indented));

            homeForm.songs.Add(newSong); // Update the songs list in HomeForm
            homeForm.RefreshUI(); // Refresh the UI components in HomeForm
        }

        private void RemovePlaceholderText(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.ForeColor == Color.Gray)
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
                if (textBox.Name == "TitleBox")
                {
                    textBox.Text = "Song Title";
                }
                else if (textBox.Name == "ArtistBox")
                {
                    textBox.Text = "Artist Name";
                }
                textBox.ForeColor = Color.Gray;
            }
        }
    }
}