using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grentry.SDK.TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GrentryClient client = new GrentryClient("http://192.168.0.20:5000/");
            client.BarcodeReceived += Client_BarcodeReceived;
            client.CardPresent += Client_CardPresent;
            //client.CardPresentChanged += Client_CardPresentChanged;
            client.CardReceived += Client_CardReceived;
            client.IOChanged += Client_IOChanged;
            client.SerialDataReceived += Client_SerialDataReceived;

            await client.Start();

            Console.WriteLine("Trigger relay 0, 1500ms");
            await client.TriggerRelay(0, 1500);

            Console.WriteLine("Open relay 1");
            await client.OpenRelay(1);

            Thread.Sleep(2000);
            Console.WriteLine("Close relay 1");
            await client.CloseRelay(1);

            Console.WriteLine("Take photo");
            var data = await client.TakePhoto();

            Console.WriteLine($"Got photo with size {data.Length}");

            Console.WriteLine($"Wait 3 min play with events");
            await Task.Delay(180000);


        }

        private static void Client_SerialDataReceived(object sender, byte[] data)
        {
            Console.WriteLine($"Serial Data received {data}");
        }

        private static void Client_IOChanged(object sender, int ioNbr, bool high)
        {
            Console.WriteLine($"IO {ioNbr} changed to  {high}");
        }

        private static void Client_CardReceived(object sender, Models.CardInfoDto cardInfo)
        {
            Console.WriteLine($"Card received to {cardInfo.Uid}");
        }

        //private static void Client_CardPresentChanged(object sender, bool isPresent)
        //{
        //    Console.WriteLine($"CardPresent Changed to {isPresent}");
        //}

        private static void Client_CardPresent(object sender)
        {
            Console.WriteLine("CardPresent");
        }

        private static void Client_BarcodeReceived(object sender, Models.BarcodeDto barcodeInfo)
        {
            Console.WriteLine(barcodeInfo.Barcode);
        }
    }
}
