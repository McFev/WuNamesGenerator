using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Telegram.Bot; //v 14.10.0.0
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net.Http;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Security.Application;

namespace WuTangNamesGeneratorBot
{
    static class Program
    {
        public static TelegramBotClient Bot;

        static void Main(string[] args)
        {
            var httpClient = new HttpClient();

            Bot = new TelegramBotClient("*********************", httpClient);

            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            var me = Bot.GetMeAsync().Result;

            Bot.StartReceiving();
            while (true)
            {
                Thread.Sleep(5000);
            }
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Debugger.Break();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            string sOriginalName = null;
            string sWuName = null;
            ChatId cID = new ChatId(message.Chat.Id);

            if (message == null || message.Type != MessageType.Text)
            {
                await Bot.SendChatActionAsync(cID, ChatAction.Typing);
                await Bot.SendTextMessageAsync(cID, Fun.ErrText("<" + message.Type + ">"), parseMode: ParseMode.Markdown, replyMarkup: new ReplyKeyboardRemove());
                return;
            }

            if (message.Type == MessageType.Text)
            {
                sOriginalName = message.Text;
            }

            string result = null;
            if (sOriginalName != "/start")
            {
                sWuName = Fun.GetWuName(sOriginalName);
            }
            else
            {
                result = "Now entah ur full name";
            }

            if(!string.IsNullOrEmpty(sWuName))
            {
                result = string.Format("`{0}` from this day forward you will also be known as\r\n`{1}`", sOriginalName, sWuName);
            }

            if (!string.IsNullOrEmpty(result))
            {
                await Bot.SendChatActionAsync(cID, ChatAction.Typing);
                await Bot.SendTextMessageAsync(cID, result, parseMode: ParseMode.Markdown, replyMarkup: new ReplyKeyboardRemove());
            }
            else
            {
                await Bot.SendChatActionAsync(cID, ChatAction.Typing);
                await Bot.SendTextMessageAsync(cID, Fun.ErrText(sOriginalName), parseMode: ParseMode.Markdown, replyMarkup: new ReplyKeyboardRemove());
                return;
            }
        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await Bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id, string.Format("Notification {0}", callbackQueryEventArgs.CallbackQuery.Data));
        }
    }

    public static class Fun
    {
        public static string ErrText(string Query)
        {
            return string.Format("Error in the text: '`{0}`'", Query);
        }


        public static string GetWuName(string OriginalName)
        {
            string result = "";
            try
            {
                string rez = AntiXss.HtmlEncode(OriginalName);
                rez = AntiXss.UrlEncode(rez);
                rez = GetHtml(rez);
                while (rez.IndexOf("<center>") != -1)
                {
                    rez = rez.Replace("<center>", "");
                }
                while (rez.IndexOf("</center>") != -1)
                {
                    rez = rez.Replace("</center>", "");
                }
                string pattern = @"you will also be known as([^\/]*)";
                Regex rgx = new Regex(pattern);

                foreach (Match match in rgx.Matches(rez))
                {
                    result += match.Value;
                }

                result = result.Replace("you will also be known as", "");
                result = Regex.Replace(result, "<[^>]*>", "");
                result = result.Replace("<", "").Replace(">", "").Replace("\n", " ");//.Replace(" ", " ");
                while (result.IndexOf("  ") != -1)
                {
                    result = result.Replace("  ", " ");
                }
            }
            catch { }
            
            return result.Trim();
        }
        
        private static string GetHtml(string param)
        {
            HttpWebResponse response = null;
            string htmlText = "";

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.mess.be/inickgenwuname.php");

                request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                request.Headers.Set(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
                request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                request.Referer = "http://www.mess.be/inickgenwuname.php";
                request.KeepAlive = true;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Headers.Add("DNT", @"1");

                request.Method = "POST";
                request.ServicePoint.Expect100Continue = false;

                string body = "realname=" + param + @"&Submit=Enter+the+Wu-Tang";
                byte[] postBytes = System.Text.Encoding.UTF8.GetBytes(body);
                request.ContentLength = postBytes.Length;
                Stream stream = request.GetRequestStream();
                stream.Write(postBytes, 0, postBytes.Length);
                stream.Close();

                response = (HttpWebResponse)request.GetResponse();

                htmlText = new StreamReader(response.GetResponseStream(), Encoding.Default).ReadToEnd();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError) response = (HttpWebResponse)e.Response;
                else return "";
            }
            catch (Exception)
            {
                if (response != null) response.Close();
                return "";
            }
            response.Close();

            return htmlText;
        }
    }
}
