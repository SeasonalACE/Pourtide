using ACE.Server.Managers;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ACE.Server.Features.Discord
{
    public enum DiscordChatChannel
    {
        Audit = 1,
        General = 2
    }

    public class ChatMessagePayload
    {
        public string content { get; set; }
    }

    public static class WebhookRepository
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task SendAuditChat(string message)
        {
            await SendWebhookChat(DiscordChatChannel.Audit, message, PropertyManager.GetString("turbine_chat_webhook_audit").Item);
        }
        public static async Task SendGeneralChat(string message)
        {
            await SendWebhookChat(DiscordChatChannel.Audit, message, PropertyManager.GetString("turbine_chat_webhook").Item);
        }
        private static async Task SendWebhookChat(DiscordChatChannel channel, string message, string webhookUrl)
        {
            await Task.Run(async () =>
            {
                using (var httpClient = new HttpClient())
                {
                    try
                    {
                        var payload = new ChatMessagePayload
                        {
                            content = message
                        };

                        var jsonPayload = JsonSerializer.Serialize(payload);

                        var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await httpClient.PostAsync(webhookUrl, httpContent);

                        response.EnsureSuccessStatusCode();
                    } catch (Exception ex)
                    {
                        log.Error("Error: an exception was thrown sending webhook payload to discord");
                        log.Error(ex.Message);
                        log.Error(ex.StackTrace);
                    }
                }
            });
        }
    }

}
