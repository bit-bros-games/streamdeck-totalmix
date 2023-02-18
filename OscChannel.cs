using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using SharpOSC;

namespace streamdeck_tentacle
{
    [PluginActionId("com.bitbros.tentacle.oscchannel.action")]
    public class OscChannel : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    Name = "/1/volume1",
                    SelectedAction = "1",
                    Port = 8000,
                    IP = "127.0.0.1",
                    Bus = "Input",
                    SelectedValue = String.Empty,
                    SelectedFunction = "1"
                };
                return instance;
            }

            [FilenameProperty]
            [JsonProperty(PropertyName = "Name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "Port")]
            public int Port { get; set; }

            [JsonProperty(PropertyName = "IP")]
            public string IP { get; set; }

            [JsonProperty(PropertyName = "SelectedAction")]
            public string SelectedAction { get; set; }

            [JsonProperty(PropertyName = "Bus")]
            public string Bus { get; set; }

            [JsonProperty(PropertyName = "SelectedValue")]
            public string SelectedValue { get; set; }

            [JsonProperty(PropertyName = "SelectedFunction")]
            public string SelectedFunction { get; set; }
        }

        #region Private Members

        private PluginSettings settings;
        private float dBInFloat;
        private string selectedValue;

        #endregion
        public OscChannel(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                this.settings = PluginSettings.CreateDefaultSettings();
                Connection.SetSettingsAsync(JObject.FromObject(settings));

                Logger.Instance.LogMessage(TracingLevel.INFO, $"OscOnOff: Settings initially set: {this.settings}");
            }
            else
            {
                this.settings = payload.Settings.ToObject<PluginSettings>();
            }
        }


        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "OscChannel: Destructor called");
        }

        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "OscChannel: Key Pressed");
            // volume
            if (settings.SelectedFunction == "1")
            {
                int selectedValue = 0;
                if (settings.SelectedValue != "")
                {
                    selectedValue = Int32.Parse(settings.SelectedValue);
                }
                dBInFloat = (float)Math.Round((float)selectedValue, 2);
                SetBus();
                SendOscCommand(settings.Name, dBInFloat, settings.IP, settings.Port);
                Logger.Instance.LogMessage(TracingLevel.INFO, $"OscChannel: Set Volume: {settings.Name} {dBInFloat}");
            }
        }

        public override void KeyReleased(KeyPayload payload)
        {

        }

        public override void OnTick() { }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            Logger.Instance.LogMessage(TracingLevel.INFO, $"OscChannel: Settings loaded: {payload.Settings}");
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #region Private Methods

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        #endregion


        public void SendOscCommand(string name, float value, string ip, int port)
        {
            var message = new OscMessage(name, value);
            var sender = new UDPSender(ip, port);
            sender.Send(message);
        }

        public void SetBus()
        {
            if (this.settings.Bus == "Input")
            {
                SendOscCommand("/1/busInput", 1.0f, settings.IP, settings.Port);
            }
            else if (this.settings.Bus == "Playback")
            {
                SendOscCommand("/1/busPlayback", 1.0f, settings.IP, settings.Port);
            }
            else if (this.settings.Bus == "Output")
            {
                SendOscCommand("/1/busOutput", 1.0f, settings.IP, settings.Port);
            }
        }
    }
}