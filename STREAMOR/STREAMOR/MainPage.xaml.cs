using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using System.Xml.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace STREAMOR
{
    public partial class MainPage : ContentPage
    {
        System.Timers.Timer timer = new System.Timers.Timer();
        TimeSpan timeCounter = new TimeSpan(0, 0, 0, 0);
        Vlc vlc = new Vlc();
        RadioStation target = null;
        RadioStation nowPlayingTarget = null;
        string xmlFile = Android.OS.Environment.ExternalStorageDirectory.ToString() + "/STREAMOR/List.xml";
        string xmlDir = Android.OS.Environment.ExternalStorageDirectory.ToString() + "/STREAMOR";
        ObservableCollection<RadioStation> radioList = new ObservableCollection<RadioStation>();
        int counterMenuTapping = 0;
        bool isPlayButtonPressedOnce = false;

        public MainPage()
        {
            PermissionsGrand();
            
            InitializeComponent();

            GetSettings();
            picker_sort.SelectedIndex = 0;
        }

        private void GetSettings()
        {
            XmlDocument document = new XmlDocument();
            document.Load(xmlFile);
            XmlNode settings = document.DocumentElement.FirstChild;
            //Get Theme
            XmlNode theme = settings.ChildNodes[0];
            picker_theme.SelectedIndex = int.Parse(theme.InnerText);
            //Get Vibration
            XmlNode vibration = settings.ChildNodes[1];
            picker_vibration.SelectedIndex = int.Parse(vibration.InnerText);
        }

        //----------------------Create Timer --------------------------
        private void StartTimer()
        {
            if (timer.Enabled)
            {
                timer.Dispose();
            }
            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            if (nowPlayingTarget == target)
            {
                int min = int.Parse(lbl_player_timer.Text.Split(':')[0]);
                int sec = int.Parse(lbl_player_timer.Text.Split(':')[1]);
                timeCounter = new TimeSpan(0, 0, min, sec);
            }
            else
            {
                timeCounter = new TimeSpan(0, 0, 0, 0);
            }
            
            timer.Elapsed += Timer_Tick;
            timer.Start();
            
        }

        private void TimerStop()
        {
            timer.Stop();
            timer.Dispose();
        }

        private void Timer_Tick(object sender, ElapsedEventArgs e)
        {
            timeCounter += TimeSpan.FromSeconds(1);
            Device.BeginInvokeOnMainThread(() =>
            {
                lbl_player_timer.Text = timeCounter.ToString(@"mm\:ss");
                pbar_player_progressBar.Progress = timeCounter.TotalSeconds / 5000;
            });
            
        }
        //--------------------------End---------------------------------

        protected override bool OnBackButtonPressed()
        {
            if (stack_player.TranslationY == 0)
            {
                ClosePlayerPage();
            }
            else if (stack_addStation.TranslationX == 0)
            {
                CloseAddStationPage();
            }
            else if (stack_editStation.TranslationX == 0)
            {
                CloseEditStationPage();
            }
            else if (stack_settings.TranslationX == 0)
            {
                CloseSettingsPage();
            }
            else
            {
                DependencyService.Register<ILaunchActivity>();
                DependencyService.Get<ILaunchActivity>().StartNativeIntentOnBackButtonPressed();
            }

            return true;
        }

        private async void PermissionsGrand()
        {
            var statusWrite = await Permissions.RequestAsync<Permissions.StorageWrite>();

            if (statusWrite == PermissionStatus.Granted)
            {
                CreateDirAndFile();
            }
            else
            { System.Environment.Exit(0); }
        }

        private async Task OpenMenu()
        {
            sl_MenuBackground.IsVisible = true;
            await sl_MenuBackground.FadeTo(0.5, 200);
            await sl_Menu.TranslateTo(0, -270, 200, Easing.SpringOut);
            counterMenuTapping = 1;
        }

        private async Task CloseMenu()
        {
            await sl_Menu.TranslateTo(0, -650, 200, Easing.SpringIn);
            await sl_MenuBackground.FadeTo(0, 200);
            sl_MenuBackground.IsVisible = false;
            counterMenuTapping = 0;
        }

        private async Task CloseAddStationPage()
        {
            await stack_addStation.TranslateTo(-400, 0, 250, Easing.SpringIn);
            entry_stationUrl.Text = "";
            entry_title.Text = "";
            entry_pictureUrl.Text = "";
            entry_genre.Text = "";
            counterMenuTapping = 0;
        }

        private async Task CloseEditStationPage()
        {
            await stack_editStation.TranslateTo(-400, 0, 250, Easing.SpringIn);
            entry_stationUrlOnEdit.Text = "";
            entry_titleOnEdit.Text = "";
            entry_pictureUrlOnEdit.Text = "";
            entry_genreOnEdit.Text = "";
            counterMenuTapping = 0;
        }

        private async Task CloseSettingsPage()
        {
            await stack_settings.TranslateTo(-400, 0, 250, Easing.SpringIn);
            counterMenuTapping = 0;
        }

        private async Task ClosePlayerPage()
        {
            await stack_player.TranslateTo(0, 660, 250, Easing.SpringIn);
        }

        public string GetNowPlayingTitleFromIcecastServer(string uri) // get now played
        {
            string strCreator = "";
            string strTitle = "";
            string result = "";
            XmlDocument document = new XmlDocument();
            try
            {
                document.Load($"{uri}.xspf");
                XmlNodeList creatorNodes = document.GetElementsByTagName("creator");
                XmlNodeList titlesNodes = document.GetElementsByTagName("title");
                foreach (XmlNode item in creatorNodes)
                {
                    if (!string.IsNullOrEmpty(item.InnerXml))
                    {
                        strCreator = item.InnerXml;
                    }
                }
                foreach (XmlNode item in titlesNodes)
                {
                    if (!string.IsNullOrEmpty(item.InnerXml))
                    {
                        strTitle = item.InnerXml;
                    }
                }
                if (string.IsNullOrEmpty(strCreator))
                {
                    result = strTitle;
                }
                else
                {
                    result = $"{strCreator} - {strTitle}";
                }
            }
            catch 
            {
                
            }
            return result;
        }

        public async Task<string> GetMetaDataFromIceCastStream(string uri)
        {
            string result = "";
            HttpClient m_httpClient = new HttpClient();
            HttpResponseMessage response = null;
            m_httpClient.DefaultRequestHeaders.Add("Icy-MetaData", "1");
            try
            {
                response = await m_httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                m_httpClient.DefaultRequestHeaders.Remove("Icy-MetaData");
                if (response.IsSuccessStatusCode)
                {
                    IEnumerable<string> headerValues;
                    if (response.Headers.TryGetValues("icy-metaint", out headerValues))
                    {
                        string metaIntString = headerValues.First();

                        if (!string.IsNullOrEmpty(metaIntString))
                        {
                            int metadataInterval = Convert.ToInt16(metaIntString);
                            byte[] buffer = new byte[metadataInterval];

                            Stream stream = await response.Content.ReadAsStreamAsync();

                            int numBytesRead = 0;
                            int numBytesToRead = metadataInterval;
                            do
                            {
                                int n = await stream.ReadAsync(buffer, numBytesRead, 10);
                                numBytesRead += n;
                                numBytesToRead -= n;
                            } while (numBytesToRead > 0);

                            int lengthOfMetaData = stream.ReadByte();
                            int metaBytesToRead = lengthOfMetaData * 16;
                            byte[] metadataBytes = new byte[metaBytesToRead];
                            var bytesRead = await stream.ReadAsync(metadataBytes, 0, metaBytesToRead);
                            result = System.Text.Encoding.UTF8.GetString(metadataBytes);
                            stream.Dispose();
                        }

                    }
                }
            }
            catch
            {
                
            }
            m_httpClient.Dispose();
            return result;
        }

        public async Task<string> GetNowPlayingTitleFromShoutcastServer(string uri) // get now played
        {
            WebResponse response = null;
            string result = "";
            Uri URL = new Uri(uri);
            string finalURL = $"{URL.Scheme}://{URL.Host}/currentsong?sid=#"; //For next song "/nextsong?sid=1"
            WebRequest request = WebRequest.Create(finalURL);

            try
            {
                response = await request.GetResponseAsync();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                result = reader.ReadLine();
                reader.Close();
            }
            catch
            {
                
            }
            if (result.Contains('�'))
            {
                result = "Unknown title";
            }
            return result;
        }

        public async Task<string> GetFullInfo(string uri) // Get Full metadata popup (icy-metadata)
        {
            HttpClient m_httpClient = new HttpClient();
            HttpResponseMessage response = null;
            string result = "";
            m_httpClient.DefaultRequestHeaders.Add("icy-metadata", "1");
            response = await m_httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            m_httpClient.DefaultRequestHeaders.Remove("icy-metadata");
            if (response.IsSuccessStatusCode)
            {
                result = response.ToString();
            }
            else
            {
                result = null;
            }
            m_httpClient.Dispose();
            return result;
        }

        public async Task<string> GetTitle(string uri) // Get Radio Title (icy-name)
        {
            string result = "";
            HttpClient m_httpClient = new HttpClient();
            HttpResponseMessage response = null;
            m_httpClient.DefaultRequestHeaders.Add("icy-metadata", "1");
            try
            {
                response = await m_httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                m_httpClient.DefaultRequestHeaders.Remove("icy-metadata");

                if (response.IsSuccessStatusCode)
                {
                    IEnumerable<string> headerValues;
                    if (response.Headers.TryGetValues("icy-name", out headerValues))
                    {
                        string metaIntString = headerValues.First();
                        if (!string.IsNullOrEmpty(metaIntString))
                        {
                            result = metaIntString;
                        }

                    }
                }
            }
            catch (Exception)
            {
                result = null;
            }
            m_httpClient.Dispose();
            return result;
        }

        public async Task<string> GetGenre(string uri) // Get Radio Genre (icy-genre)
        {
            string result = "";
            HttpClient m_httpClient = new HttpClient();
            HttpResponseMessage response = null;
            m_httpClient.DefaultRequestHeaders.Add("icy-metadata", "1");
            try
            {
                response = await m_httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                m_httpClient.DefaultRequestHeaders.Remove("icy-metadata");

                if (response.IsSuccessStatusCode)
                {
                    IEnumerable<string> headerValues;
                    if (response.Headers.TryGetValues("icy-genre", out headerValues))
                    {
                        string metaIntString = headerValues.First();
                        if (!string.IsNullOrEmpty(metaIntString))
                        {
                            result = metaIntString;
                        }

                    }
                }
            }
            catch (Exception)
            {
                result = null;
            }
            m_httpClient.Dispose();
            return result;
        }

        public async Task<string> GetBitrate(string uri) // Get Radio Bitrate (icy-bitrate)
        {
            string result = "";
            HttpClient m_httpClient = new HttpClient();
            HttpResponseMessage response = null;
            m_httpClient.DefaultRequestHeaders.Add("icy-metadata", "1");
            try
            {
                response = await m_httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                m_httpClient.DefaultRequestHeaders.Remove("icy-metadata");

                if (response.IsSuccessStatusCode)
                {
                    IEnumerable<string> headerValues;
                    if (response.Headers.TryGetValues("icy-br", out headerValues))
                    {
                        string metaIntString = headerValues.First();
                        if (!string.IsNullOrEmpty(metaIntString))
                        {
                            result = metaIntString;
                        }

                    }
                }
            }
            catch (Exception)
            {
                result = null;
            }
            m_httpClient.Dispose();
            return result;
        }

        public async Task<string> GetDescription(string uri) // Get Radio Description (icy-description)
        {
            string result = "";
            HttpClient m_httpClient = new HttpClient();
            HttpResponseMessage response = null;
            m_httpClient.DefaultRequestHeaders.Add("icy-metadata", "1");
            try
            {
                response = await m_httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                m_httpClient.DefaultRequestHeaders.Remove("icy-metadata");

                if (response.IsSuccessStatusCode)
                {
                    IEnumerable<string> headerValues;
                    if (response.Headers.TryGetValues("icy-description", out headerValues))
                    {
                        string metaIntString = headerValues.First();
                        if (!string.IsNullOrEmpty(metaIntString))
                        {
                            result = metaIntString;
                        }

                    }
                }
            }
            catch (Exception)
            {
                result = null;
            }
            m_httpClient.Dispose();
            return result;
        }

        private async Task TryGetCurrentSong()
        {
            string nowPlayingSong = GetNowPlayingTitleFromIcecastServer(target.Url);
            if (string.IsNullOrEmpty(nowPlayingSong))
            {
                nowPlayingSong = await GetNowPlayingTitleFromShoutcastServer(target.Url);
                if (string.IsNullOrEmpty(nowPlayingSong))
                {
                    string metadata = await GetMetaDataFromIceCastStream(target.Url);
                    try
                    {
                        nowPlayingSong = metadata.Split('=')[1].Replace("'", "").Replace(";", "");
                    }
                    catch
                    {

                    }
                }
            }
            lbl_nowPlayedSong.Text = nowPlayingSong.Trim();
        }

        private void XmlDataProvider(ObservableCollection<RadioStation> radioList)
        {
            XmlDocument document = new XmlDocument();
            document.Load(xmlFile);
            XmlNodeList nodes = document.GetElementsByTagName("Station");
            foreach (XmlNode node in nodes)
            {
                string title = node.ChildNodes[0].InnerText;
                string genre = node.ChildNodes[1].InnerText;
                string pictureUrl = node.ChildNodes[2].InnerText;
                string url = node.ChildNodes[3].InnerText;
                string bitrate = node.ChildNodes[4].InnerText;
                string desc = node.ChildNodes[5].InnerText;
                bool isFavor = bool.Parse(node.ChildNodes[6].InnerText);
                radioList.Add(new RadioStation(title, genre, pictureUrl, url, bitrate, desc, isFavor));
            }
        }

        private void CreateDirAndFile()
        {
            if (!Directory.Exists(xmlDir))
            {
                Directory.CreateDirectory(xmlDir);
                CreateXmlFile();
            }
            else if (!File.Exists(xmlFile))
            {
                CreateXmlFile();
            }
            else
            {
                XmlDataProvider(radioList);
            }
        }

        public void CreateXmlFile()
        {
            Thread.Sleep(2000);
            //Create Root Element
            XDocument document = new XDocument();
            XElement rootElement = new XElement("Radios");
            //Create Settings Node
            XElement settings = new XElement("Settings");
            XElement theme = new XElement("Theme");
            theme.Value = "0";
            XElement vibration = new XElement("Vibration");
            vibration.Value = "0";

            document.Add(rootElement);
            rootElement.Add(settings);
            settings.Add(theme);
            settings.Add(vibration);
            document.Save(xmlFile);
        }

        private async void lv_radios_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            Color[] colors = {Color.GreenYellow, Color.Yellow, Color.Aquamarine, Color.SkyBlue, Color.Beige, Color.Orange, Color.Lime};
            int random1 = new Random().Next(0, 6);
            int random2 = new Random().Next(0, 6);
            frame_onAir.IsVisible = true;
            stack_onAir.IsVisible = true;
            target = (RadioStation)e.Item;

            //Button "Listen now" animation
            await frame_onAir.ScaleTo(1.2, 150, Easing.CubicIn);
             frame_onAir.ScaleTo(1, 100, Easing.CubicOut);

            await lbl_selected_radio_genre.TranslateTo(-400, 0, 100, Easing.SpringIn);
            await lbl_selected_radio_title.TranslateTo(-400, 0, 100, Easing.SpringIn);
            lbl_selected_radio_genre.Text = target.Genre;
            lbl_selected_radio_title.TextColor = colors[random1];
            lbl_selected_radio_title.Text = target.Title;
            lbl_selected_radio_genre.TextColor = colors[random2];
            await lbl_selected_radio_genre.TranslateTo(0, 0, 150, Easing.SpringOut);
            await lbl_selected_radio_title.TranslateTo(0, 0, 150, Easing.SpringOut);

            //Button "Edit" animation
            await btn_edit.RotateYTo(180, 150);
             btn_edit.RotateYTo(0, 100);

            //Button "Delete" animation
            await btn_delete.RotateYTo(90, 150);
             btn_delete.RotateYTo(0, 100);

           
        }

        private void picker_sort_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (picker_sort.SelectedIndex)
            {
                case 1:
                    lv_radios.ItemsSource = radioList.OrderBy(x => x.Title);
                    break;
                case 2:
                    lv_radios.ItemsSource = radioList.OrderByDescending(x => x.Title);
                    break;
                case 3:
                    lv_radios.ItemsSource = radioList.Where(x => x.IsFavorite);
                    break;
                default:
                    lv_radios.ItemsSource = radioList;
                    break;
            }
        }

        private async void btn_ListenNow_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            target = radioList.First(x => x.Title == lbl_selected_radio_title.Text && x.Genre == lbl_selected_radio_genre.Text);
            vlc.LoadMedia(target.Url);
            await stack_player.TranslateTo(0, 0, 250, Easing.SpringOut);
            if (lbl_player_targetTitle.Text != target.Title)
            {
                lbl_player_targetTitle.Text = target.Title;
                img_palyer_targetPicture.Source = target.PictureUrl;
                lbl_player_bitrate.Text = $"{target.Bitrate} Kbps";
                lbl_player_desc.Text = target.Description;
                CheckForFavorite();
                imgbtn_player_play.IsVisible = true;
                imgbtn_player_pause.IsVisible = false;
                lbl_nowPlayedSong.Text = "";
            }
            
        }

        IEnumerable<RadioStation> GetStations(string searchText = null)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                lbl_allRadios.Text = "All Radios";
                switch (picker_sort.SelectedIndex)
                {
                    case 1:
                        return radioList.OrderBy(x => x.Title);
                    case 2:
                        return radioList.OrderByDescending(x => x.Title);
                    default:
                        return radioList;
                }
            }
            else
            {
                lbl_allRadios.Text = $"Found {radioList.Where(p => p.Title.ToLower().Contains(searchText)).Count()}";
                return radioList.Where(p => p.Title.ToLower().Contains(searchText));
            }

        }

        private void sb_searchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            lv_radios.ItemsSource = GetStations(e.NewTextValue);
        }

        private async void lbl_menu_icon_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            if (counterMenuTapping == 0)
            {
                await OpenMenu();
            }
            else
            {
                await CloseMenu();
            }
            
        }

        private async void TapGestureRecognizer_sl_MenuBackground_Tapped(object sender, EventArgs e)
        {
            await CloseMenu();
            
        }

        private async void imgbtn_back_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            await CloseAddStationPage();
        }

        private async void btn_addStation_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            await CloseMenu();
            await stack_addStation.TranslateTo(0, 0, 250, Easing.SpringOut);
            
        }

        private async void btn_cancelAddStation_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            await CloseAddStationPage();
        }

        private async void btn_okAddStation_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            string title = "";
            string genre = "";
            string pictureUrl = "";
            string url = entry_stationUrl.Text;
            string bitrate = "";
            string description = "";

            if (string.IsNullOrWhiteSpace(entry_stationUrl.Text))
            {
                await DisplayAlert("ℹ️ Information", "Empty field (Station URL)!", "Cancel");
                return;
            }
            //Get Title
            if (string.IsNullOrWhiteSpace(entry_title.Text))
            {
                try
                {
                    title = await GetTitle(url);
                }
                catch
                {
                    
                }
            }
            else
            {
                title = entry_title.Text;
            }
            //Get Genre
            if (string.IsNullOrWhiteSpace(entry_genre.Text))
            {
                try
                {
                    genre = await GetGenre(url);
                }
                catch
                {

                }
            }
            else
            {
                genre = entry_genre.Text;
            }
            //Get Picture
            pictureUrl = entry_pictureUrl.Text;
            //Get Bitrate
            try
            {
                bitrate = await GetBitrate(url);
            }
            catch
            {

            }
            //Get Description
            try
            {
                description = await GetDescription(url);
            }
            catch
            {

            }
            //Create XML Node with Station data
            XmlDocument document = new XmlDocument();
            document.Load(xmlFile);
            XmlNodeList nodes = document.GetElementsByTagName("Station");
            XmlNode targetElement = null;
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes["id"].Value == url)
                {
                    targetElement = node;
                }
            }

            if (targetElement != null)
            {
                targetElement.ChildNodes[0].InnerText = title;
                targetElement.ChildNodes[1].InnerText = genre;
                targetElement.ChildNodes[2].InnerText = pictureUrl;
                targetElement.ChildNodes[3].InnerText = url;
                targetElement.ChildNodes[4].InnerText = bitrate;
                targetElement.ChildNodes[5].InnerText = description;
            }
            else
            {
                XmlNode rootElement = document.GetElementsByTagName("Radios")[0];
                XmlElement xStation = document.CreateElement("Station");
                xStation.SetAttribute("id", url);
                XmlElement xTitle = document.CreateElement("Title");
                xTitle.InnerText = title;
                XmlElement xGenre = document.CreateElement("Genre");
                xGenre.InnerText = genre;
                XmlElement xPicture = document.CreateElement("Picture");
                xPicture.InnerText = pictureUrl;
                XmlElement xUrl = document.CreateElement("URL");
                xUrl.InnerText = url;
                XmlElement xBitrate = document.CreateElement("Bitrate");
                xBitrate.InnerText = bitrate;
                XmlElement xDesc = document.CreateElement("Description");
                xDesc.InnerText = description;
                XmlElement xFavor = document.CreateElement("IsFavorite");
                xFavor.InnerText = "false";
                xStation.AppendChild(xTitle);
                xStation.AppendChild(xGenre);
                xStation.AppendChild(xPicture);
                xStation.AppendChild(xUrl);
                xStation.AppendChild(xBitrate);
                xStation.AppendChild(xDesc);
                xStation.AppendChild(xFavor);
                rootElement.AppendChild(xStation);
            }
            document.Save(xmlFile);
            await CloseAddStationPage();
            entry_stationUrl.Text = "";
            entry_title.Text = "";
            entry_genre.Text = "";
            entry_pictureUrl.Text = "";
            radioList.Clear();
            XmlDataProvider(radioList);
            target = radioList.Last();
            lv_radios.ScrollTo(radioList.Last(), ScrollToPosition.MakeVisible, true);
        }

        private async void btn_Exit_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            var isTrue = await DisplayAlert("Do you want to close Streamor?", "", "Close Streamor", "Not now");
            if (isTrue)
            {
                Environment.Exit(0);
            }
            
        }

        private async void btn_delete_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            bool result = await DisplayAlert("ℹ️ Confirm", $"DELETE ({target.Title})!\nAre you sure?", "Delete", "Cancel");
            if (result)
            {
                XmlDocument document = new XmlDocument();
                document.Load(xmlFile);
                XmlNodeList nodes = document.GetElementsByTagName("Station");
                XmlNode targetElement = null;
                foreach (XmlNode node in nodes)
                {
                    if (node.Attributes["id"].Value == target.Url)
                    {
                        targetElement = node;
                    }
                }
                targetElement.ParentNode.RemoveChild(targetElement);
                document.Save(xmlFile);
                radioList.Clear();
                XmlDataProvider(radioList);
                stack_onAir.IsVisible = false;
                lbl_selected_radio_title.Text = "";
                lbl_selected_radio_genre.Text = "";
            }
            
        }

        private async void btn_edit_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            await stack_editStation.TranslateTo(0, 0, 250, Easing.SpringOut);
            entry_stationUrlOnEdit.Text = target.Url;
            entry_titleOnEdit.Text = target.Title;
            entry_genreOnEdit.Text = target.Genre;
            entry_pictureUrlOnEdit.Text = target.PictureUrl;
        }

        private async void btn_editStation_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            int targetIndex = radioList.IndexOf(target);
            string title = "";
            string genre = "";
            string pictureUrl = "";
            string url = entry_stationUrlOnEdit.Text;
            string bitrate = "";
            string description = "";
            if (string.IsNullOrWhiteSpace(entry_stationUrlOnEdit.Text))
            {
                await DisplayAlert("ℹ️ Information", "Empty field (Station URL)!", "Cancel");
                return;
            }
            //Get Title
            if (string.IsNullOrWhiteSpace(entry_titleOnEdit.Text))
            {
                try
                {
                    title = await GetTitle(url);
                }
                catch
                {
                    
                }
            }
            else
            {
                title = entry_titleOnEdit.Text;
            }
            //Get Genre
            if (string.IsNullOrWhiteSpace(entry_genreOnEdit.Text))
            {
                try
                {
                    genre = await GetGenre(url);
                }
                catch
                {

                }
            }
            else
            {
                genre = entry_genreOnEdit.Text;
            }
            //Get Picture
            pictureUrl = entry_pictureUrlOnEdit.Text;
            //Get Bitrate
            try
            {
                bitrate = await GetBitrate(url);
            }
            catch
            {

            }
            //Get Description
            try
            {
                description = await GetDescription(url);
            }
            catch
            {

            }
            XmlDocument document = new XmlDocument();
            document.Load(xmlFile);
            XmlNodeList nodes = document.GetElementsByTagName("Station");
            XmlNode targetElement = null;
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes["id"].Value == target.Url)
                {
                    targetElement = node;
                }
            }
            targetElement.Attributes["id"].Value = url;
            targetElement.ChildNodes[0].InnerText = title;
            targetElement.ChildNodes[1].InnerText = genre;
            targetElement.ChildNodes[2].InnerText = pictureUrl;
            targetElement.ChildNodes[3].InnerText = url;
            targetElement.ChildNodes[4].InnerText = bitrate;
            targetElement.ChildNodes[5].InnerText = description;

            document.Save(xmlFile);
            await CloseEditStationPage();
            entry_stationUrlOnEdit.Text = "";
            entry_titleOnEdit.Text = "";
            entry_genreOnEdit.Text = "";
            entry_pictureUrlOnEdit.Text = "";
            radioList.Clear();
            XmlDataProvider(radioList);
            target = radioList.ElementAt(targetIndex);
        }

        private async void btn_cancelEditStation_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            await CloseEditStationPage();
        }

        private async void imgbtn_backOnEdit_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            await CloseEditStationPage();
        }

        private async void imgbtn_backOnPlayer_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            await ClosePlayerPage();
        }

        private async Task AnimationNowPlayingSong()
        {
            await TryGetCurrentSong();
            await Task.Run(async () =>
            {
                await lbl_nowPlayedSong.FadeTo(1, 250);
                Device.StartTimer(TimeSpan.FromSeconds(20), () =>
                {
                     Device.BeginInvokeOnMainThread(async () =>
                     {
                         await TryGetCurrentSong();
                     });
                    if (string.IsNullOrWhiteSpace(lbl_nowPlayedSong.Text))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                });
            });
        }

        private async void imgbtn_player_play_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            try
            {
                imgbtn_player_play.IsVisible = false;
                imgbtn_player_pause.IsVisible = true;
                vlc.PlayMedia();
                StartTimer();
                nowPlayingTarget = target;
                lbl_now_playing_target.Text = $"<strong>Now listen:</strong> {nowPlayingTarget.Title}";
                await frame_now_playing_target.TranslateTo(0, 0, 300);
                await AnimationNowPlayingSong();
            }
            catch (Exception x)
            {
                await DisplayAlert("ℹ️ Information", x.Message, "OK"); return;
            }
            isPlayButtonPressedOnce = true;
        }

        private async void imgbtn_player_pause_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            try
            {
                vlc.StopMedia();
                TimerStop();
                imgbtn_player_play.IsVisible = true;
                imgbtn_player_pause.IsVisible = false;
                nowPlayingTarget = null;
                await frame_now_playing_target.TranslateTo(0, -30,300);
            }
            catch (Exception x)
            {
                await DisplayAlert("ℹ️ Information", x.Message, "OK"); return;
            }
            isPlayButtonPressedOnce = false;
        }

        private async void imgbtn_player_artist_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            await AnimationNowPlayingSong();
        }

        private void imgbtn_player_back_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            if (radioList.IndexOf(target) != 0)
            {
                lv_radios.SelectedItem = radioList.IndexOf(target) - 1;
                target = radioList.ElementAt(radioList.IndexOf(target) - 1);
                lbl_selected_radio_title.Text = target.Title;
                lbl_selected_radio_genre.Text = target.Genre;
                vlc.LoadMedia(target.Url);
                lbl_player_targetTitle.Text = target.Title;
                img_palyer_targetPicture.Source = target.PictureUrl;
                lbl_player_bitrate.Text = $"{target.Bitrate} Kbps";
                lbl_player_desc.Text = target.Description;
                CheckForFavorite();
                if (nowPlayingTarget != null)
                {
                    if (nowPlayingTarget.Url == target.Url && isPlayButtonPressedOnce)
                    {
                        imgbtn_player_play.IsVisible = false;
                        imgbtn_player_pause.IsVisible = true;
                    }
                    else
                    {
                        imgbtn_player_play.IsVisible = true;
                        imgbtn_player_pause.IsVisible = false;
                    }
                }
                
                lbl_nowPlayedSong.Text = "";
                lv_radios.ScrollTo(target, ScrollToPosition.MakeVisible, true);
            }
        }

        private void imgbtn_player_farw_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            if (radioList.IndexOf(target) == radioList.Count - 1)
            {
                lv_radios.SelectedItem = radioList[0];
                target = radioList[0];
            }
            else
            {
                lv_radios.SelectedItem = radioList.IndexOf(target) + 1;
                target = radioList.ElementAt(radioList.IndexOf(target) + 1);
            }
            lbl_selected_radio_title.Text = target.Title;
            lbl_selected_radio_genre.Text = target.Genre;
            vlc.LoadMedia(target.Url);
            lbl_player_targetTitle.Text = target.Title;
            img_palyer_targetPicture.Source = target.PictureUrl;
            lbl_player_bitrate.Text = $"{target.Bitrate} Kbps";
            lbl_player_desc.Text = target.Description;
            CheckForFavorite();
            if (nowPlayingTarget != null)
            {
                if (nowPlayingTarget.Url == target.Url && isPlayButtonPressedOnce)
                {
                    imgbtn_player_play.IsVisible = false;
                    imgbtn_player_pause.IsVisible = true;
                }
                else
                {
                    imgbtn_player_play.IsVisible = true;
                    imgbtn_player_pause.IsVisible = false;
                }
            }
            
            lbl_nowPlayedSong.Text = "";
            
            lv_radios.ScrollTo(target, ScrollToPosition.MakeVisible, true);
        }

        private void imgbtn_player_fav_false_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            int index = radioList.IndexOf(target);
            XmlDocument document = new XmlDocument();
            document.Load(xmlFile);
            XmlNodeList nodes = document.GetElementsByTagName("Station");
            XmlNode targetElement = null;
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes["id"].Value == target.Url)
                {
                    targetElement = node;
                }
            }
            
            targetElement.ChildNodes[6].InnerText = "true";

            document.Save(xmlFile);
            radioList.Clear();
            XmlDataProvider(radioList);
            target = radioList.ElementAt(index);
            imgbtn_player_fav_false.IsVisible = false;
            imgbtn_player_fav_true.IsVisible = true;
        }

        private void imgbtn_player_fav_true_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            int index = radioList.IndexOf(target);
            XmlDocument document = new XmlDocument();
            document.Load(xmlFile);
            XmlNodeList nodes = document.GetElementsByTagName("Station");
            XmlNode targetElement = null;
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes["id"].Value == target.Url)
                {
                    targetElement = node;
                }
            }

            targetElement.ChildNodes[6].InnerText = "false";

            document.Save(xmlFile);
            radioList.Clear();
            XmlDataProvider(radioList);
            target = radioList.ElementAt(index);
            imgbtn_player_fav_false.IsVisible = true;
            imgbtn_player_fav_true.IsVisible = false;
        }

        private void CheckForFavorite()
        {
            if (target.IsFavorite)
            {
                imgbtn_player_fav_false.IsVisible = false;
                imgbtn_player_fav_true.IsVisible = true;
            }
            else
            {
                imgbtn_player_fav_false.IsVisible = true;
                imgbtn_player_fav_true.IsVisible = false;
            }
        }

        private async void btn_settings_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            await CloseMenu();
            await stack_settings.TranslateTo(0, 0, 250, Easing.SpringOut);
        }

        private async void imgbtn_settings_back_Clicked(object sender, EventArgs e)
        {
            MakeVibration();
            await CloseSettingsPage();
        }

        private void picker_theme_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = 0;
            XmlDocument document = new XmlDocument();
            document.Load(xmlFile);
            XmlNode settingsNode = document.DocumentElement.FirstChild;
            
            switch (picker_theme.SelectedIndex)
            {
                case 0 :
                    theme_Home.Color = Color.Firebrick;
                    theme_addStation.Color = Color.Firebrick;
                    theme_editStation.Color = Color.Firebrick;
                    theme_settings.Color = Color.Firebrick;
                    theme_player.Color = Color.Firebrick;
                    index = 0;
                    break;
                case 1:
                    theme_Home.Color = Color.FromHex("#581845");
                    theme_addStation.Color = Color.FromHex("#581845");
                    theme_editStation.Color = Color.FromHex("#581845");
                    theme_settings.Color = Color.FromHex("#581845");
                    theme_player.Color = Color.FromHex("#581845");
                    index = 1;
                    break;
                case 2:
                    theme_Home.Color = Color.DarkSlateGray;
                    theme_addStation.Color = Color.DarkSlateGray;
                    theme_editStation.Color = Color.DarkSlateGray;
                    theme_settings.Color = Color.DarkSlateGray;
                    theme_player.Color = Color.DarkSlateGray;
                    index = 2;
                    break;
                case 3:
                    theme_Home.Color = Color.FromHex("#313358");
                    theme_addStation.Color = Color.FromHex("#313358");
                    theme_editStation.Color = Color.FromHex("#313358");
                    theme_settings.Color = Color.FromHex("#313358");
                    theme_player.Color = Color.FromHex("#313358");
                    index = 3;
                    break;
            }
            settingsNode.ChildNodes[0].InnerText = index.ToString();
            document.Save(xmlFile);
        }

        private void picker_vibration_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = 0;
            XmlDocument document = new XmlDocument();
            document.Load(xmlFile);
            XmlNode settingsNode = document.DocumentElement.FirstChild;

            switch (picker_vibration.SelectedIndex)
            {
                case 0:
                    index = 0;
                    MakeVibration();
                    break;
                case 1:
                    index = 1;
                    break;
            }
            settingsNode.ChildNodes[1].InnerText = index.ToString();
            document.Save(xmlFile);
        }

        private void MakeVibration()
        {
            if (picker_vibration.SelectedIndex == 0)
            {
                var duration = TimeSpan.FromSeconds(0.03);
                Vibration.Vibrate(duration);
            }
        }

        private async void lbl_now_playing_target_Tapped(object sender, EventArgs e)
        {
            MakeVibration();
            target = nowPlayingTarget;
            await CloseMenu();
            await CloseAddStationPage();
            await CloseEditStationPage();
            await CloseSettingsPage();
            await ClosePlayerPage();
            
            vlc.LoadMedia(target.Url);
            await stack_player.TranslateTo(0, 0, 250, Easing.SpringOut);
            lbl_player_targetTitle.Text = target.Title;
            img_palyer_targetPicture.Source = target.PictureUrl;
            lbl_player_bitrate.Text = $"{target.Bitrate} Kbps";
            lbl_player_desc.Text = target.Description;
            CheckForFavorite();
            imgbtn_player_play.IsVisible = false;
            imgbtn_player_pause.IsVisible = true;
            lbl_nowPlayedSong.Text = "";
        }
    }
}
