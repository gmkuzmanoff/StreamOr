using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace STREAMOR
{
    public class Vlc
    {
        //VLC Lib
        public event PropertyChangedEventHandler PropertyChanged;
        private Media media = null;
        private LibVLC _libVLC;
        /// <summary>
        /// Gets the <see cref="LibVLCSharp.Shared.LibVLC"/> instance.
        /// </summary>
        public LibVLC LibVLC
        {
            get => _libVLC;
            private set => Set(nameof(LibVLC), ref _libVLC, value);
        }

        private MediaPlayer _Player;
        /// <summary>
        /// Gets the <see cref="LibVLCSharp.Shared.MediaPlayer"/> instance.
        /// </summary>
        public LibVLCSharp.Shared.MediaPlayer Player
        {
            get => _Player;
            private set => Set(nameof(Player), ref _Player, value);
        }

        private void Set<T>(string propertyName, ref T field, T value)
        {
            if (field == null && value != null || field != null && !field.Equals(value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void LoadMedia(string station)
        {
            Core.Initialize();
            LibVLC = new LibVLC();
            try
            {
                media = new Media(LibVLC, new Uri(station));
            }
            catch
            {
                media = null;
                return;
            }
            
        }

        public void PlayMedia()
        {
            try
            {
                Player.Stop();
            }
            catch { }

            Player = new MediaPlayer(media) { EnableHardwareDecoding = true };
            //media.Dispose();
            Player.Play();
        }

        public void StopMedia()
        {
            try
            {
                Player.Stop();
            }
            catch { }
        }

        public void PauseMedia()
        {
            try
            {
                Player.Pause();
            }
            catch { }
        }

        public bool IsPlayingMedia()
        {
            if (Player.IsPlaying)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
