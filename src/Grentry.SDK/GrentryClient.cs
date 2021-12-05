using Grentry.SDK.Models;
using Grentry.SKD.Api;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Grentry.SDK
{
    public delegate void BarcodeEventHandler(object sender, BarcodeDto barcodeInfo);
    public delegate void IOChangedEventHandler(object sender, int ioNbr, bool high);
    public delegate void CardPresentEventHandler(object sender);
    public delegate void CardReceivedEventHandler(object sender, CardInfoDto cardInfo);
    public delegate void SerialDataReceivedEventHandler(object sender, byte[] data);
    public class GrentryClient
    {
        private string baseurl;
        HubConnection connection;
        private readonly IHttpClientFactory _httpClientFactory;

        #region events
        /// <summary>
        /// Fires if a new barcode is received
        /// </summary>
        public event BarcodeEventHandler BarcodeReceived;
        /// <summary>
        /// Fires if the input of an IO changed
        /// </summary>
        public event IOChangedEventHandler IOChanged;
        /// <summary>
        /// Fires as long as a card is present on the reader
        /// </summary>
        public event CardPresentEventHandler CardPresent;
        /// <summary>
        /// Fires if the Uid of a card is read
        /// </summary>
        public event CardReceivedEventHandler CardReceived;
        /// <summary>
        /// Fires if data from the Serial Port (RS485) is received
        /// </summary>
        public event SerialDataReceivedEventHandler SerialDataReceived;
        #endregion

        /// <summary>
        /// Use this only if you do not have the ability to provide IHttpClientFactory
        /// </summary>
        /// <param name="baseurl">Base url of the Grentry Controll. Example http://127.0.0.1:8080</param>
        public GrentryClient(
            string baseurl) :
            this(baseurl, null)
        {
        }

        /// <summary>
        /// Create a object to communicate with the Grentry Controll
        /// </summary>
        /// <param name="baseurl">Base url of the Grentry Controll. Example http://127.0.0.1:8080</param>
        /// <param name="httpClientFactory">httpClientFactory</param>
        public GrentryClient(
            string baseurl,
            IHttpClientFactory httpClientFactory)
        {
            this._httpClientFactory = httpClientFactory;
            this.baseurl = baseurl;
            var hubConnectionUrl = $"{this.baseurl.TrimEnd('/')}/devicecontroller";
            this.connection = new HubConnectionBuilder()
                .WithUrl(hubConnectionUrl)
                .WithAutomaticReconnect()
                .Build();

            this.RegisterEvents();
        }

        /// <summary>
        /// Starts the connection to the reader, retrys until successfull connection,
        /// If you want to cancel the connection retry you can provide a CancellationToken and cancel the retrying
        /// </summary>
        /// <param name="token">CancellationToken to cancel retrying</param>
        /// <returns>True if success, False if CancellationToken is used to cancel</returns>
        public async Task<bool> Start(CancellationToken token = default)
        {
            // Keep trying to until we can start or the token is canceled.
            while (true)
            {
                try
                {
                    await connection.StartAsync(token);
                    return true;
                }
                catch when (token.IsCancellationRequested)
                {
                    return false;
                }
                catch
                {
                    // Failed to connect, trying again in 5000 ms.
                    await Task.Delay(5000);
                }
            }
        }

        private void RegisterEvents()
        {
            this.connection.On<BarcodeDto>("barcode", (barcode) =>
            {
                this.BarcodeReceived?.Invoke(this, barcode);
            });

            this.connection.On<bool>("io1-changed", (high) =>
            {
                this.IOChanged?.Invoke(this, 1, high);
            });

            this.connection.On<bool>("io2-changed", (high) =>
            {
                this.IOChanged?.Invoke(this, 2, high);
            });

            this.connection.On("card-present", () =>
            {
                this.CardPresent?.Invoke(this);
            });

            this.connection.On<CardInfoDto>("card-received", (cardInfo) =>
            {
                this.CardReceived?.Invoke(this, cardInfo);
            });

            this.connection.On<byte[]>("serial", (info) =>
            {
                this.SerialDataReceived?.Invoke(this, info);
            });

        }

        /// <summary>
        /// Take a photo and get the photo as byte[]
        /// </summary>
        /// <returns>Photo data as byte[]</returns>
        public async Task<byte[]> TakePhoto()
        {
            var result = await GetGrentryControllerApiClient().GetPhotoAsync();

            return result.Data;
        }

        /// <summary>
        /// Opens the relay and closes the relay after given milliseconds, (relays numbering starting at 0)
        /// </summary>
        /// <param name="relayNr">Relay number, starting at 0</param>
        /// <param name="ms">time in milliseconds until the relay will be closed</param>
        /// <returns></returns>
        public async Task TriggerRelay(int relayNr, int ms)
        {
            await GetGrentryControllerApiClient().TriggerRelayAsync(new RelayTriggerDto
            {
                RelayNr = relayNr,
                Milliseconds = ms
            });
        }

        /// <summary>
        /// Opens a relay. (relays numbering starting at 0)
        /// </summary>
        /// <param name="relayNr">Relay number, starting at 0</param>
        /// <returns></returns>
        public async Task OpenRelay(int relayNr)
        {
            await GetGrentryControllerApiClient().OpenRelayAsync(relayNr);
        }

        /// <summary>
        /// Closes a relay. (relays numbering starting at 0)
        /// </summary>
        /// <param name="relayNr">Relay number, starting at 0</param>
        /// <returns></returns>
        public async Task CloseRelay(int relayNr)
        {
            await GetGrentryControllerApiClient().CloseRelayAsync(relayNr);
        }

        /// <summary>
        /// Write Serial data to the RS485
        /// </summary>
        /// <param name="data">data to write</param>
        /// <returns></returns>
        public async Task WriteSerial(byte[] data)
        {
            await GetGrentryControllerApiClient().WriteAsync(new SerialWriteDto
            {
                Data = data
            });
        }

        private GrentryControllerApiClient GetGrentryControllerApiClient()
        {
            HttpClient client;
            if (this._httpClientFactory == null)
            {
                client = new HttpClient();
            }
            else
            {
                client = this._httpClientFactory.CreateClient();
            }

            return new GrentryControllerApiClient(this.baseurl, client);
        }
    }
}
