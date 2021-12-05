
# Grentry.SDK.Net

Grentry.SDK.Net is a Developer SDK for .Net to communicate with Grentry.Controller.




## Installation

Install my-project with npm

```bash
  Install-Package Grentry.SDK -Version 1.0.0
```
    

## Usage/Examples

```csharp
//Create a connection to the Grenty Controller
GrentryClient client = new GrentryClient("http://127.0.0.1:5000/");

//Register all Events
client.BarcodeReceived += Client_BarcodeReceived;
client.CardPresent += Client_CardPresent;
client.CardPresentChanged += Client_CardPresentChanged;
client.CardReceived += Client_CardReceived;
client.IOChanged += Client_IOChanged;
client.SerialDataReceived += Client_SerialDataReceived;

//Start listening for the events 
await client.Start();

//Trigger Relay 0 for 1500ms
await client.TriggerRelay(0, 1500);

//Open relay 1
await client.OpenRelay(1);

//Close relay 1
await client.CloseRelay(1);

//Take photo
var data = await client.TakePhoto();

```


## License

Grentry.SDK.Net is licensed under the MIT license. [MIT](https://choosealicense.com/licenses/mit/)

