using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NAudio.Wave;

namespace MusicPlayer
{
    public partial class PlaybackControl : UserControl
    {
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        private Timer timer;
        private bool isLooping = false;
        private bool isDragging = false;
        private Label elapsedTimeLabel;
        private Label totalTimeLabel;
        private TrackBar progressBar;
        private Button loopButton;
        private Button backwardButton;
        private Button playPauseButton;
        private Button forwardButton;
        private TrackBar volumeBar;
        private PrivateFontCollection privateFonts;

        public PlaybackControl()
        {
            InitializeComponent();
            InitializeFonts();
            InitializePlaybackControl();
            this.Visible = false;  // Initially hide the playback control
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

        private void InitializePlaybackControl()
        {
            this.Height = 70; // Shrink the playback control height
            this.Dock = DockStyle.Bottom;
            this.BackColor = Color.FromArgb(45, 45, 45); // Slightly lighter background color

            PictureBox thumbnail = new PictureBox
            {
                Name = "Thumbnail",
                SizeMode = PictureBoxSizeMode.Zoom,  // Maintain aspect ratio
                Size = new Size(32, 32),  // Resize to 2/5 of the original size (80, 80)
                Location = new Point(10, 10)
            };

            Label songInfo = new Label
            {
                Name = "SongInfo",
                AutoSize = true,
                ForeColor = Color.White, // Text color
                Location = new Point(60, 40),
                Font = GetCustomFont(12)
            };

            elapsedTimeLabel = new Label
            {
                Text = "0:00",
                AutoSize = true,
                ForeColor = Color.White, // Text color
                Location = new Point(10, 30),
                Font = GetCustomFont(10)
            };

            loopButton = new Button
            {
                Text = "Loop",
                Width = 60,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 60, 60), // Button background color
                Font = GetCustomFont(12, FontStyle.Bold)
            };
            loopButton.FlatAppearance.BorderSize = 0;
            loopButton.Click += LoopButton_Click;

            backwardButton = new Button
            {
                Text = "<<",
                Width = 40,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 60, 60), // Button background color
                Font = GetCustomFont(12, FontStyle.Bold)
            };
            backwardButton.FlatAppearance.BorderSize = 0;
            backwardButton.Click += BackwardButton_Click;

            playPauseButton = new Button
            {
                Name = "PlayPauseButton",
                Text = "Play",
                Width = 60,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 60, 60), // Button background color
                Font = GetCustomFont(12, FontStyle.Bold)
            };
            playPauseButton.FlatAppearance.BorderSize = 0;
            playPauseButton.Click += PlayPauseButton_Click;

            forwardButton = new Button
            {
                Text = ">>",
                Width = 40,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 60, 60), // Button background color
                Font = GetCustomFont(12, FontStyle.Bold)
            };
            forwardButton.FlatAppearance.BorderSize = 0;
            forwardButton.Click += ForwardButton_Click;

            volumeBar = new TrackBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 50,
                TickStyle = TickStyle.None,
                SmallChange = 1,
                LargeChange = 10,
                Width = 100,
                Location = new Point(this.Width - 120, 10),
                ForeColor = Color.White, // Text color
                BackColor = Color.FromArgb(45, 45, 45) // Background color
            };
            volumeBar.Scroll += VolumeBar_Scroll;

            progressBar = new TrackBar
            {
                Name = "ProgressBar",
                Minimum = 0,
                Maximum = 100,
                TickStyle = TickStyle.None,
                SmallChange = 1,
                LargeChange = 10,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                ForeColor = Color.White, // Text color
                BackColor = Color.FromArgb(45, 45, 45) // Background color
            };
            progressBar.MouseDown += ProgressBar_MouseDown;
            progressBar.MouseUp += ProgressBar_MouseUp;

            totalTimeLabel = new Label
            {
                Text = "0:00",
                AutoSize = true,
                ForeColor = Color.White, // Text color
                Location = new Point(this.Width - 120, 30),
                Font = GetCustomFont(10)
            };

            this.Controls.Add(thumbnail);
            this.Controls.Add(songInfo);
            this.Controls.Add(elapsedTimeLabel);
            this.Controls.Add(loopButton);
            this.Controls.Add(backwardButton);
            this.Controls.Add(playPauseButton);
            this.Controls.Add(forwardButton);
            this.Controls.Add(volumeBar);
            this.Controls.Add(totalTimeLabel);
            this.Controls.Add(progressBar);

            this.Resize += ResizePlaybackControl;
            ResizePlaybackControl(this, EventArgs.Empty);
        }

        private void ResizePlaybackControl(object sender, EventArgs e)
        {
            int controlWidth = this.Width;
            int buttonGroupWidth = loopButton.Width + backwardButton.Width + playPauseButton.Width + forwardButton.Width + 20; // 20 for spacing
            int buttonGroupStartX = (controlWidth - buttonGroupWidth) / 2;

            loopButton.Location = new Point(buttonGroupStartX, 10);
            backwardButton.Location = new Point(buttonGroupStartX + loopButton.Width + 5, 10);
            playPauseButton.Location = new Point(buttonGroupStartX + loopButton.Width + backwardButton.Width + 10, 10);
            forwardButton.Location = new Point(buttonGroupStartX + loopButton.Width + backwardButton.Width + playPauseButton.Width + 15, 10);

            volumeBar.Location = new Point(controlWidth - volumeBar.Width - 10, 10);
            totalTimeLabel.Location = new Point(controlWidth - volumeBar.Width - totalTimeLabel.Width - 20, 30);

            int progressBarWidth = (int)(controlWidth * 0.66);
            int progressBarStartX = (controlWidth - progressBarWidth) / 2;

            progressBar.Location = new Point(progressBarStartX, 30);
            progressBar.Width = progressBarWidth;

            elapsedTimeLabel.Location = new Point(progressBarStartX - elapsedTimeLabel.Width - 10, 30);
            totalTimeLabel.Location = new Point(progressBarStartX + progressBarWidth + 10, 30);
        }

        private void PlaybackControl_Load(object sender, EventArgs e)
        {
            // Any additional initialization code can be added here
        }

        public void PlaySong(string filePath, string title, string artist, string thumbnailPath)
        {
            StopPlayback();

            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(title) || string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(thumbnailPath))
            {
                MessageBox.Show("Invalid song data. Please check the song details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.Controls["Thumbnail"].BackgroundImage = Image.FromFile(thumbnailPath);
            this.Controls["SongInfo"].Text = $"Listening to: '{title}' by '{artist}'";

            outputDevice = new WaveOutEvent();
            audioFile = new AudioFileReader(filePath);
            outputDevice.Init(audioFile);
            outputDevice.Play();

            totalTimeLabel.Text = FormatTime(audioFile.TotalTime);

            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            timer.Start();

            this.Controls["PlayPauseButton"].Text = "Pause";
            this.Visible = true;  // Show the playback control when a song is played
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!isDragging)
            {
                progressBar.Value = (int)(audioFile.CurrentTime.TotalSeconds / audioFile.TotalTime.TotalSeconds * 100);
                elapsedTimeLabel.Text = FormatTime(audioFile.CurrentTime);
            }

            if (outputDevice.PlaybackState == PlaybackState.Stopped)
            {
                if (isLooping)
                {
                    audioFile.Position = 0;
                    outputDevice.Play();
                }
                else
                {
                    timer.Stop();
                    this.Visible = false;  // Hide the playback control when the song finishes
                }
            }
        }

        private void LoopButton_Click(object sender, EventArgs e)
        {
            isLooping = !isLooping;
            Button loopButton = sender as Button;
            loopButton.BackColor = isLooping ? Color.Green : SystemColors.Control;
        }

        private void BackwardButton_Click(object sender, EventArgs e)
        {
            if (audioFile != null)
            {
                if (audioFile.CurrentTime.TotalSeconds <= 5)
                {
                    audioFile.CurrentTime = TimeSpan.Zero;
                }
                else
                {
                    audioFile.CurrentTime = audioFile.CurrentTime.Add(TimeSpan.FromSeconds(-5));
                }
                elapsedTimeLabel.Text = FormatTime(audioFile.CurrentTime);
            }
        }

        private void PlayPauseButton_Click(object sender, EventArgs e)
        {
            if (outputDevice == null)
            {
                return;
            }
            else if (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                outputDevice.Pause();
                (sender as Button).Text = "Play";
            }
            else if (outputDevice.PlaybackState == PlaybackState.Paused)
            {
                outputDevice.Play();
                (sender as Button).Text = "Pause";
            }
        }

        private void ForwardButton_Click(object sender, EventArgs e)
        {
            if (audioFile != null)
            {
                audioFile.CurrentTime = audioFile.CurrentTime.Add(TimeSpan.FromSeconds(5));
                elapsedTimeLabel.Text = FormatTime(audioFile.CurrentTime);
            }
        }

        private void VolumeBar_Scroll(object sender, EventArgs e)
        {
            if (audioFile != null)
            {
                audioFile.Volume = (sender as TrackBar).Value / 100f;
            }
        }

        private void ProgressBar_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
        }

        private void ProgressBar_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            if (audioFile != null)
            {
                audioFile.CurrentTime = TimeSpan.FromSeconds(audioFile.TotalTime.TotalSeconds * progressBar.Value / 100);
                elapsedTimeLabel.Text = FormatTime(audioFile.CurrentTime);
            }
        }

        private string FormatTime(TimeSpan time)
        {
            return $"{(int)time.TotalMinutes}:{time.Seconds:D2}";
        }

        private void StopPlayback()
        {
            if (outputDevice != null)
            {
                outputDevice.Stop();
                outputDevice.Dispose();
                outputDevice = null;
            }

            if (audioFile != null)
            {
                audioFile.Dispose();
                audioFile = null;
            }

            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }

            this.Visible = false;  // Hide the playback control when playback stops
        }
    }
}