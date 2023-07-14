using System.Text;
using TeleSharp.TL.Messages;
using TeleSharp.TL;
using TLSharp.Core;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;

namespace TelegramParser
{
    public static class DAL
    {
       
        public static async Task Login(TelegramClient client)
        {
            string hash;

            await client.ConnectAsync();
            await Console.Out.WriteAsync("Enter your number:");
            string number = Console.ReadLine();
            try
            {
                hash = await client.SendCodeRequestAsync(number);

                await Console.Out.WriteAsync("Enter your code:");
                string code = Console.ReadLine();

                var user = await client.MakeAuthAsync(number, hash, code);

            }
            catch
            {
                hash = await client.SendCodeRequestAsync(number);

                await Console.Out.WriteAsync("Enter your code:");
                string code = Console.ReadLine();

                var user = await client.MakeAuthAsync(number, hash, code);
            }

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
            List<string> phoneNumbers = _resultMessages.SelectMany(m => regex.Matches(m.Message).Cast<Match>().Select(match => '+' + match.Value)).Distinct().ToList();

            SaveToXlsx(phoneNumbers);
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
        public static void SaveToXlsx(List<string> phoneNumbers)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            string path = Directory.GetCurrentDirectory() + "numbers.xlsx";
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Numbers");

            //Add collection of numbers to the worksheet

            for (int i = 0; i < phoneNumbers.Count; i++)
            {
                worksheet.Cells["A" + (i + 1)].Value = phoneNumbers[i];
            }

            package.SaveAs(new FileInfo(path));
        }
    }
}
