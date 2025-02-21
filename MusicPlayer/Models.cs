using System.Collections.Generic;

namespace MusicPlayer
{
    public class Song
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string FilePath { get; set; }
        public string ThumbnailPath { get; set; }
    }

    public class Playlist
    {
        public string Name { get; set; }
        public string ThumbnailPath { get; set; }
        public string Description { get; set; }
        public List<Song> Songs { get; set; }
    }
}