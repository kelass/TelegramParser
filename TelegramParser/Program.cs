using OfficeOpenXml.Drawing.Chart;
using TeleSharp.TL;
using TLSharp.Core;

namespace TelegramParser
{
    class Program
    {

        static async Task Main()
        {
            List<TLMessage> _resultMessages = new List<TLMessage>();

            using (TelegramClient client = new TelegramClient("YOUR ID", "YOUR HASH"))
            {
                await DAL.Login(client);
                await DAL.FindGroup(client, _resultMessages);

                client.Dispose();
            }
            DAL.NumberRegex(_resultMessages);

            await Console.Out.WriteLineAsync("Numbers parsed and enter to xlsx file");
        }
            
        
    }


}
