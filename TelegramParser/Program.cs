using System.Text.RegularExpressions;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;

namespace TelegramParser
{
    class Program
    {
        static async Task Main()
        {
            List<TLMessage> _resultMessages = new List<TLMessage>();

            TelegramClient client = new TelegramClient(11414090, "13e1afeb6ce8a275b0d4b8496d46e6bb");
            await client.ConnectAsync();

            var hash = await client.SendCodeRequestAsync("+380661649464");
            string code = Console.ReadLine();
            var user = await client.MakeAuthAsync("+380661649464", hash, code);

            var dialogs = (TLDialogsSlice)await client.GetUserDialogsAsync();

            var offset = 0;
            var chat = dialogs.Chats.ToList().OfType<TLChannel>().FirstOrDefault(d => d.Title == "Робота за кордоном");

            if (chat == null)
                throw new Exception($"Channel {chat.Title} not found");

            while (true)
            {
                var req = new TLRequestGetHistory
                {
                    Peer = new TLInputPeerChannel
                    {
                        ChannelId = chat.Id,
                        AccessHash = chat.AccessHash.Value
                    },
                    Limit = 100,
                    OffsetId = offset,
                };

                var res = await client.SendRequestAsync<TLChannelMessages>(req);

                if (res.Messages.Count == 0) break;

                _resultMessages.AddRange(res.Messages.Select(m => m as TLMessage).Where(m => m != null));

                if (res.Messages.Count < 100) break;

                offset = _resultMessages.Last().Id;
            }
            var regex = new Regex(@"\b(?:\+?(\d{1,3}))?[-. (]*(\d{2,4})[-. )]*(\d{2,4})[-. ]*(\d{2,4})[-. ]*(\d{2,4})[-. ]*(\d{2,4})\b");
            var phoneNumbers = _resultMessages.SelectMany(m => regex.Matches(m.Message).Cast<Match>().Select(match => match.Value)).Distinct().ToList();
        }
    }


}
