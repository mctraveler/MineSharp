using System;
using System.IO;
using System.Text;
using System.Net;
using System.Collections.Generic;
using MineProxy.Commands;
using Newtonsoft.Json;

namespace MineProxy.Chatting
{
    static class Translator
    {
        public static void Init(MainCommands c)
        {
            c.AddCommand(Translate, "translate", "tr", "trans");
        }

        /// <summary>
        /// Google Translate API key
        /// </summary>
        const string ApiKey = "AIzaSyCepjSUIb8cTGqdgfUmMYDcH6xxbr-Uf7g";

        static Dictionary<string, Languages> senderTranslate = new Dictionary<string, Languages>();
        class Languages
        {
            public string From { get; set; }
            public string To { get; set; }

            public override string ToString()
            {
                return From + " -> " + To;
            }

            public Languages(string to)
            {
                to = to.ToLowerInvariant();
                if (to.Length != 2)
                    throw new UsageException("Language code must be 2 characters");
                this.To = to;
            }

            public Languages(string fr, string to)
            {
                fr = fr.ToLowerInvariant();
                to = to.ToLowerInvariant();
                if (fr.Length != 2)
                    throw new UsageException("Language code must be 2 characters");
                if (to.Length != 2)
                    throw new UsageException("Language code must be 2 characters");

                this.From = fr;
                this.To = to;
            }
        }

        static void Translate(Client p, string[] cmd, int offset)
        {
            if (!p.Donor && !p.Admin())
                throw new ErrorException("Only for donors, /donate");

            if (cmd.Length == offset)
            {
                lock (senderTranslate)
                {
                    if (senderTranslate.Remove(p.MinecraftUsername))
                        p.TellSystem(Chat.Aqua, "Removed translation");
                }
                throw new ShowHelpException();
            }

            if (cmd.Length == offset + 1)
            {
                if (cmd [offset].Length == 2)
                    TranslateNext(p, new Languages(cmd [offset]));
                else
                    TranslatePrev(p, new Languages(p.Language), cmd [offset]);
                return;
            }


            Languages lang;
            if (cmd [offset].Length == 2 && cmd [offset + 1].Length == 2)
            {
                lang = new Languages(cmd [offset], cmd [offset + 1]);
                offset += 2;
            } else
            {
                lang = new Languages(cmd [offset]);
                offset += 1;
            }

            if (cmd.Length == offset)
            {
                TranslateNext(p, lang);
                return;
            }
            
            if (cmd.Length == offset + 1)
            {
                TranslatePrev(p, lang, cmd [offset]);
                return;
            }

            TranslateText(p, lang, cmd.JoinFrom(offset));
        }
        /// <summary>
        /// Indicate that next chat message should be translated
        /// </summary>
        static void TranslateNext(Client p, Languages lang)
        {
            lock (senderTranslate)
            {
                if (senderTranslate.ContainsKey(p.MinecraftUsername))
                    senderTranslate.Remove(p.MinecraftUsername);
                senderTranslate.Add(p.MinecraftUsername, lang);
            }
            p.TellSystem(Chat.Aqua, "Next line will be translated: " + lang);
        }

        /// <summary>
        /// only return a message if translation succeeded
        /// </summary>
        public static string TranslateFromPlayer(Client p, string m)
        {
            Languages lang;
            lock (senderTranslate)
            {
                if (senderTranslate.ContainsKey(p.MinecraftUsername) == false)
                    return null;
                lang = senderTranslate [p.MinecraftUsername];
                //senderTranslate.Remove(p.MinecraftUsername);
            }
            p.TellSystem(Chat.Aqua, "Translating...");
            string trans = Translate(lang, m);
            if (trans == null)
            {
                lock (senderTranslate)
                {
                    senderTranslate.Remove(p.MinecraftUsername);
                }
            }
            return trans;
        }

        /// <summary>
        /// Translate what other player just said
        /// </summary>
        static void TranslatePrev(Client p, Languages lang, string opName)
        {
            Client op = PlayerList.GetPlayerByName(opName);
            if (op == null)
                throw new ErrorException("Player not found: " + opName);
            if (op.ChatEntry == null || op.ChatEntry.Channel != null)
                throw new ErrorException(op.Name + " has not said anything");

            //Do the translation
            string locale = op.Locale.Split('_') [0];
            if (locale != lang.From)
                p.TellSystem(Chat.DarkAqua, op.Name + " has language: " + locale);

            string trans = Translate(lang, op.ChatEntry.Message);
            p.TellSystem(Chat.Aqua + op.Name + "[" + lang + "] " + Chat.White, trans);
        }

        static void TranslateText(Client p, Languages lang, string text)
        {
            if (text.Length == 0)
                throw new UsageException("no text");

            string trans = Translate(lang, text);
            if (trans == null)
                throw new ErrorException("No translation");
            p.TellSystem(Chat.Aqua + "Translated: ", trans);
        }

        #region API Call to translate

        /// <summary>
        /// Do the translation
        /// </summary>
        static string Translate(Languages lang, string text)
        {
            if (lang.From == lang.To)
                return null;

            text = text.Trim().Replace("  ", " ");
            string urlText = System.Web.HttpUtility.UrlEncode(text);
            string url = "https://www.googleapis.com/language/translate/v2?key=" + ApiKey + "&" + (lang.From != null ? "source=" + lang.From + "&" : "") + "target=" + lang.To + "&q=" + urlText;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            Translation trans;
            try
            {
                var response = (HttpWebResponse)req.GetResponse();
                if (response == null)
                    throw new ErrorException("No response");
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new ErrorException(response.StatusCode + " " + response.StatusDescription);
                string json;
                using (var rs = response.GetResponseStream())
                using (TextReader tr = new StreamReader(rs))
                    json = tr.ReadToEnd();
                var apiResp = JsonConvert.DeserializeObject<ApiResponse>(json);
                if (apiResp == null || apiResp.Data == null || apiResp.Data.Translations == null)
                    return null;
                if (apiResp.Data.Translations.Count == 0)
                    return null;
                trans = apiResp.Data.Translations [0];
            } catch (Exception e)
            {
                Log.WriteServer("Translating " + lang + ": " + text, e);
                return null;
            }
            if (trans.Language == null)
                return trans.Text;
            else
                return "[" + trans.Language + "] " + trans.Text;
        }

        class ApiResponse
        {
            [JsonProperty("data")]
            public DataClass Data { get; set; }
        }

        class DataClass
        {
            [JsonProperty("translations")]
            public List<Translation> Translations { get; set; }
        }

        class Translation
        {
            [JsonProperty("translatedText")]
            public string Text { get; set; }
            [JsonProperty("detectedSourceLanguage")]
            public string Language { get; set; }
        }

        #endregion
    }
}

