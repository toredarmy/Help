using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Help.Main.Telegram
{
    internal sealed class Telegram : Base
    {
        public event Action<Alarm> AlarmEvent;

        private readonly Queue<Alarm> queue = new Queue<Alarm>();

        public void Start()
        {
            Task.Run(async () =>
            {
                Log("Start");

                if (Settings.TelegramToken.Length == 0)
                {
                    Except("[ Token ] not defined");
                    run = false;
                }
                if (Settings.TelegramChatId.Length == 0)
                {
                    Except("[ Chat Id ] not defined");
                    run = false;
                }

                while (run)
                {
                    if (queue.Count > 0)
                    {
                        var client = new HttpClient();
                        var serializer = new JavaScriptSerializer();
                        while (queue.Count > 0)
                        {
                            var alarm = queue.Peek();
                            if (SendAlarm(client, serializer, alarm))
                            {
                                delay = 100;
                                queue.Dequeue();
                                await Task.Delay(3000); // telegram limit: 20 message/min
                            }
                            else
                            {
                                delay += 500;
                                break;
                            }
                        }
                    }

                    await Task.Delay(delay);
                    if (delay > 10000)
                    {
                        delay = 100;
                    }
                }

                Except("Stopped");
            });
        }

        public void SendAlarms(List<Alarm> alarms)
        {
            if (!run)
            {
                return;
            }
            delay = 100;
            foreach (var alarm in alarms)
            {
                queue.Enqueue(alarm);
            }
            Log($"Added [ {alarms.Count} ] alarms to queue");
        }

        private bool SendAlarm(HttpClient client, JavaScriptSerializer serializer, Alarm alarm)
        {
            try
            {
                var url = $"https://api.telegram.org/bot{Settings.TelegramToken}/sendMessage";
                HttpResponseMessage response;
                try
                {
                    response = client.PostAsync(url, AlarmToContent(alarm)).Result;
                }
                catch (Exception ex)
                {
                    // Internet? connection error
                    Except(ex);
                    return false;
                }

                var jsonString = response.Content.ReadAsStringAsync().Result;
                IDictionary<string, object> json;
                try
                {
                    json = (IDictionary<string, object>)serializer.DeserializeObject(jsonString);
                }
                catch (Exception ex)
                {
                    // Not correct json
                    Except(ex);
                    return false;
                }

                if (response.IsSuccessStatusCode)
                {
                    if ((bool)json["ok"])
                    {
                        Log($"[ {response.StatusCode} ] Alarm#{alarm.Id} [ {alarm.Guid} ] - sent");
                        AlarmEvent?.Invoke(alarm);
                        return true;
                    }
                    Except("OOPS something wrong");
                }
                else
                {
                    var errorCode = (int)json["error_code"];
                    switch (errorCode)
                    {
                        case 429:
                            var parameters = (IDictionary<string, object>)json["parameters"];
                            var retryAfter = (int)parameters["retry_after"];
                            Except($"[ {errorCode} ] Many Requests detected, sleeping [ {retryAfter} ] sec");
                            Task.Run(() => Task.Delay(1000 * retryAfter)).Wait();
                            break;
                        case 400 when (string)json["description"] == "Bad Request: chat not found":
                            Except($"[ {errorCode} ] Chat [ {Settings.TelegramChatId} ] not found");
                            run = false;
                            break;
                        case 400 when (string)json["description"] == "Bad Request: chat_id is empty":
                            Except($"[ {errorCode} ] Chat [ {Settings.TelegramChatId} ] is empty");
                            run = false;
                            break;
                        case 400:
                            Except($"[ {errorCode} ] UNKNOWN [ {jsonString} ]");
                            run = false;
                            break;
                        case 401:
                            if ((string)json["description"] == "Unauthorized")
                            {
                                Except($"[ {errorCode} ] Unauthorized with token [ {Settings.TelegramToken} ]");
                                run = false;
                            }
                            break;
                        default:
                            Except($"[ {errorCode} ] UNKNOWN [ {jsonString} ]");
                            run = false;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Except(ex);
            }
            return false;
        }

        private static FormUrlEncodedContent AlarmToContent(Alarm alarm)
        {
            return new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "chat_id", Settings.TelegramChatId },
                { "text", AlarmToHTML(alarm) },
                { "parse_mode", "HTML" }
            });
        }

        private static string AlarmToHTML(Alarm alarm)
        {
            var emoji = "";
            switch (alarm.Event)
            {
                case "Пожар":
                    emoji = "🔥";
                    break;
                case "Пожар 2":
                case "Два пожара":
                    emoji = "🔥🔥";
                    break;
                case "Внимание! Опасность пожара":
                    emoji = "❗️";
                    break;
            }
            if (emoji.Length != 0)
                emoji += " ";

            return $"<b>{alarm.Object}</b>\r\n" +
                   $"{emoji}<b>{alarm.Event}</b>\r\n\r\n" +
                   $"[<b>{alarm.Section}</b>] <b>{alarm.SectionName}</b>\r\n\r\n" +
                   $"{alarm.AddressName}\r\n" +
                   $"{alarm.Address}\r\n\r\n" +
                   $"<i>{alarm.Time}</i>";
        }
    }
}
