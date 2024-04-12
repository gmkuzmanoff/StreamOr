using System;
using System.Collections.Generic;
using System.Text;

namespace STREAMOR
{
    public class RadioStation
    {
        private string title;
        private string genre;
        private string pictureUrl;
        private string description;
        private string bitrate;
        private bool isFavorite = false;

        public RadioStation(string title, string genre, string pictureUrl, string url, string bitrate, string description, bool isFavorite)
        {
            Title = title;
            Genre = genre;
            PictureUrl = pictureUrl;
            Url = url;
            Bitrate = bitrate;
            Description = description;
            IsFavorite = isFavorite;
        }

        public string Title
        {
            get => title;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = Url;
                }
                title = value;
            }
        }
        public string Genre
        {
            get => genre;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = "various";
                }
                genre = value;
            }
        }
        public string PictureUrl
        {
            get => pictureUrl;
            set
            {
                string[] pics = { "https://gmkuzmanoff.free.bg/images/STREAMOR/P1.jpg",
                    "https://gmkuzmanoff.free.bg/images/STREAMOR/P2.jpg",
                    "https://gmkuzmanoff.free.bg/images/STREAMOR/P3.jpg",
                    "https://gmkuzmanoff.free.bg/images/STREAMOR/P4.jpg",
                    "https://gmkuzmanoff.free.bg/images/STREAMOR/P5.jpg",
                    "https://gmkuzmanoff.free.bg/images/STREAMOR/P6.jpg",};
                if (string.IsNullOrWhiteSpace(value))
                {
                    int i = new Random().Next(0, 5);
                    value = pics[i];
                }
                pictureUrl = value;
            }
        }
        public string Url { get; set; }
        public string Bitrate 
        {
            get => bitrate;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = "192";
                }
                bitrate = value;
            }
        }
        public string Description 
        {
            get => description;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = $"The best {genre} music on the Planet!";
                }
                description = value;
            }
        }
        public bool IsFavorite { get => isFavorite; set => isFavorite = value; }
    }
}
