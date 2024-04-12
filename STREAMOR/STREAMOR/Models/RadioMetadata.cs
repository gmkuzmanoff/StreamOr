using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace STREAMOR.Models
{
    public class RadioMetadata
    {
        public string GetXspfFromIcecastServer(string uri) // get now played
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
                if (response != null)
                {
                    m_httpClient.DefaultRequestHeaders.Remove("Icy-MetaData");
                    if (response.IsSuccessStatusCode)
                    {
                        IEnumerable<string> headerValues;
                        if (response.Headers.TryGetValues("icy-metaint", out headerValues))
                        {
                            string metaIntString = headerValues.First();
                            if (!string.IsNullOrEmpty(metaIntString))
                            {
                                int metadataInterval = int.Parse(metaIntString);
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
            }
            catch
            {
                return result;
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
    }
}
