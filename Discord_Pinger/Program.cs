using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace Discord_Pinger
{
    class Program
    {
        /// <summary>
        /// Discord proprietary bot for checking my servers
        /// Fairly simple. Ping discord's main server. Then the router.
        /// Send it off to discord.
        /// </summary>

        // URL format: https://discordapp.com/api/webhooks/CHANNEL_ID/AUTH_TOKEN
        #region Config vars

        private static String channel_id = ""; // Discord channel ID. Not tested to see if bot can talk to another channel.
        private static String auth_token = ""; // Discord key. This is used for authorizing and identifying the bot.

        private static int minutes = 15; // Minutes between messages.

        private static String pinginternal = "192.168.1.1"; // Internal server to ping for link latency check.
        private static String pingexternal = "8.8.8.8"; // External server to ping for internet latency.

        #endregion

        #region static vars

        private static int sleep_time; // Time in ms to wait

        #endregion


        static void Main(string[] args)
        {
            #region Start-Up
            Console.Write("Loaded.");

            to_discord("Alert",true,"The server has been restarted. Time between messages is " + minutes + " minutes.");

            sleep_time = (minutes * 60000) ;

            #endregion

            #region loop code
            while (true)
            {
                #region while session vars
                // Ping times. Set to "Error" if it wasn't moved.
                string pinginttime = "Error";
                string pingexttime = "Error";

                #endregion
                #region ping the servers

               try
                {
                    // Define the function.
                    Ping p = new Ping();

                    // Internal then external
                    PingReply intreply = p.Send(pinginternal, 1000);
                    PingReply extreply = p.Send(pingexternal, 1000);

                    // Collect Data
                    pinginttime = intreply.RoundtripTime.ToString();
                    pingexttime = extreply.RoundtripTime.ToString();

                    // Construct message
                    String message = "External ping time: **" + pingexttime + "ms** \n                                    Internal ping time: **" + pinginttime + "ms**";
                    Console.Write(message);

                    // And ship it out
                    to_discord("Status", false, message);

                } catch(Exception e)
                {
                    to_discord("Error", false, e.ToString());
                }

                #endregion

                // Wait
                Thread.Sleep(sleep_time);

            }
            #endregion
        }

        static void to_discord(String status, Boolean use_tts, String message)
        {

            #region Create new JSON request

            Discord_Webhook json = new Discord_Webhook();
            json.content = message;
            json.tts = use_tts;
            json.username = "Server: " + status;
            string json_bin = JsonConvert.SerializeObject(json);
            #endregion

            #region Fire a POST request

            //Construct the URL
            string URL = "https://discordapp.com/api/webhooks/"+channel_id+"/"+auth_token;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            { 
                //Send the JSON
                streamWriter.Write(json_bin);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }

            #endregion
        
        }
    }
}
