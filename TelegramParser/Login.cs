using System.Text;
using TeleSharp.TL.Messages;
using TeleSharp.TL;
using TLSharp.Core;
using System.Text.RegularExpressions;

namespace TelegramParser
{
    public static class DAL
    {
        public static async Task Login(TelegramClient client)
        {
            await client.ConnectAsync();

            Console.Write("Enter your number:");
            string number = Console.ReadLine();

            var hash = await client.SendCodeRequestAsync(number);

            Console.WriteLine("Enter your code:");
            string code = Console.ReadLine();

            var user = await client.MakeAuthAsync(number, hash, code);
        }
        public static async Task FindGroup(TelegramClient client, List<TLMessage> _resultMessages)
        {
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

                if (offset == _resultMessages.Last().Id) break;

                offset = _resultMessages.Last().Id;
            }
        }
        public static void NumberRegex(List<TLMessage> _resultMessages)
        {
            Regex regex = new Regex(@"\b(\+?(\d{1,3}))?[-. (]*(\d{2,4})[-. )]*(\d{2,4})[-. ]*(\d{2,4})[-. ]*(\d{2,4})[-. ]*(\d{2,4})\b");
            List<string> phoneNumbers = _resultMessages.SelectMany(m => regex.Matches(m.Message).Cast<Match>().Select(match => match.Value)).Distinct().ToList();

            SaveToTxt(phoneNumbers);
        }
        public static void SaveToTxt(List<string> phoneNumbers)
        {
            string path = Directory.GetCurrentDirectory() + "numbers.txt";
            using (StreamWriter stream = new StreamWriter(path, true, Encoding.ASCII))
            {
                foreach (string number in phoneNumbers)
                {
                    stream.WriteLine(number);
                }
            }
        }
    }
}
