using TeleSharp.TL;
using TLSharp.Core;

namespace TelegramParser
{
    class Program
    {

        static async Task Main()
        {
            TelegramClient client = new TelegramClient(11414090, "13e1afeb6ce8a275b0d4b8496d46e6bb");
            List<TLMessage> _resultMessages = new List<TLMessage>();

            await DAL.Login(client);
            await DAL.FindGroup(client, _resultMessages);
            DAL.NumberRegex(_resultMessages);

            client.Dispose();
        }
            
        
    }


}
