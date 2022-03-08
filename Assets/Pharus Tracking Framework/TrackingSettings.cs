using Assets.Pharus_Tracking_Framework.Enums;
using Assets.Pharus_Tracking_Framework.TransmissionFrameworks.Tracklink;
using System;

namespace Assets.Pharus_Tracking_Framework
{
    /// <summary>
    /// General Tracking Settings
    /// </summary>
    public class TrackingSettings
    {
        private bool tracklinkEnabled = true;
        private bool tuioEnabled = true;

        /// <summary>
        /// Protocol selection for the Tracklink service. TUIO always uses TCP.
        /// </summary>
        private EProtocolType tracklinkProtocol = EProtocolType.UDP;
        private string tracklinkTcpIp = "127.0.0.1";
        private int tracklinkTcpPort = 44345;
        private string tracklinkMulticastIp = "239.1.1.1";

        /// <summary>
        /// Udp port settings for both tracking protocols.
        /// </summary>
        private int tuioUdpPort = 3333;
        private int tracklinkUdpPort = 44345;

        // the resolution, in unity units
        private int trackingResolutionX = 1920;
        private int trackingResolutionY = 1080;

        // the real size of play space in cm
        private float stageSizeX = 1600f;
        private float stageSizeY = 900f;

        // Server reconnect intervall in milliseconds, not set in xml.
        private int checkServerReconnectIntervall = 5000;
        private TrackingXMLConfig xmlConfig;

        public bool TuioEnabled
        {
            get { return tuioEnabled; }
            set { tuioEnabled = value; }
        }
        public int TuioUdpPort
        {
            get { return tuioUdpPort; }
            set { this.tuioUdpPort = value; }
        }

        public bool TracklinkEnabled
        {
            get { return this.tracklinkEnabled; }
            set { this.tracklinkEnabled = value; }
        }

        public EProtocolType TracklinkProtocol
        {
            get { return this.tracklinkProtocol; }
            set { this.tracklinkProtocol = value; }
        }
        public string TracklinkTcpIp
        {
            get { return this.tracklinkTcpIp; }
            set { this.tracklinkTcpIp = value; }
        }
        public int TracklinkTcpPort
        {
            get { return this.tracklinkTcpPort; }
            set { this.tracklinkTcpPort = value; }
        }
        public string TracklinkMulticastIp
        {
            get { return this.tracklinkMulticastIp; }
            set { this.tracklinkMulticastIp = value; }
        }
        public int TracklinkUdpPort
        {
            get { return this.tracklinkUdpPort; }
            set { this.tracklinkUdpPort = value; }
        }
        public int TrackingResolutionX
        {
            get { return trackingResolutionX; }
            set { this.trackingResolutionX = value; }
        }
        public int TrackingResolutionY
        {
            get { return trackingResolutionY; }
            set { this.trackingResolutionY = value; }
        }
        public float StageSizeX
        {
            get { return this.stageSizeX; }
            set { this.stageSizeX = value; }
        }
        public float StageSizeY
        {
            get { return this.stageSizeY; }
            set { this.stageSizeY = value; }
        }
        public int CheckServerReconnectIntervall
        {
            get { return this.checkServerReconnectIntervall; }
        }

        /// <summary>
        /// Initializes tracking settings by loading data from an external xml config file.
        /// </summary>
        /// <param name="config">The xml config.</param>
        public void Initialize(TrackingXMLConfig config)
        {
            this.xmlConfig = config;

            string configTuioEnabled = null;
            string configTrackLinkEnabled = null;
            string configTuioPort = null;
            string configTracklinkProtocol = null;
            string configTracklinkTcpIp = null;
            string configTracklinkTcpPort = null;
            string configTracklinkUdpMulticastIp = null;
            string configTracklinkUdpPort = null;
            string interpolationX = null;
            string interpolationY = null;
            string stageX = null;
            string stageY = null;

            if (xmlConfig != null)
            {

                for (int i = 0; i < xmlConfig.ConfigNodes.Length; i++)
                {
                    switch (xmlConfig.ConfigNodes[i].Name)
                    {
                        case "tuio-enabled":
                            configTuioEnabled = xmlConfig.ConfigNodes[i].Value;
                            break;

                        case "tracklink-enabled":
                            configTrackLinkEnabled = xmlConfig.ConfigNodes[i].Value;
                            break;

                        case "tuio-udp-port":
                            configTuioPort = xmlConfig.ConfigNodes[i].Value;
                            break;

                        case "tracklink-protocol":
                            configTracklinkProtocol = xmlConfig.ConfigNodes[i].Value;
                            break;

                        case "tracklink-tcp-ip":
                            configTracklinkTcpIp = xmlConfig.ConfigNodes[i].Value;
                            break;

                        case "tracklink-tcp-port":
                            configTracklinkTcpPort = xmlConfig.ConfigNodes[i].Value;
                            break;

                        case "tracklink-multicast-ip":
                            configTracklinkUdpMulticastIp = xmlConfig.ConfigNodes[i].Value;
                            break;

                        case "tracklink-udp-port":
                            configTracklinkUdpPort = xmlConfig.ConfigNodes[i].Value;
                            break;
                        case "trackingInterpolationX":
                            interpolationX = xmlConfig.ConfigNodes[i].Value;
                            break;

                        case "trackingInterpolationY":
                            interpolationY = xmlConfig.ConfigNodes[i].Value;
                            break;

                        case "trackingStageX":
                            stageX = xmlConfig.ConfigNodes[i].Value;
                            break;

                        case "trackingStageY":
                            stageY = xmlConfig.ConfigNodes[i].Value;
                            break;

                        default:
                            break;
                    }
                }

                bool configTrackingEnabledBool;
                if (configTuioEnabled != null && Boolean.TryParse(configTuioEnabled, out configTrackingEnabledBool))
                {
                    this.TuioEnabled = configTrackingEnabledBool;
                    Console.WriteLine(string.Format("TUIO XML config: TUIO tracking enabled: {0}", this.tuioEnabled));
                }
                //else
                //{
                //	Debug.Log(string.Format("TUIO XML config: couldn't load enabled config. Tracking enabled: {0}", m_tuioSettings.TuioEnabled));
                //}

                bool configTracklinkBool;
                if (configTrackLinkEnabled != null && Boolean.TryParse(configTrackLinkEnabled, out configTracklinkBool))
                {
                    this.TracklinkEnabled = configTracklinkBool;
                    Console.WriteLine(string.Format("TrackLink XML config: TrackLink enabled: {0}", this.TracklinkEnabled));
                }

                int configUDPPortInt;
                if (configTuioPort != null && int.TryParse(configTuioPort, out configUDPPortInt))
                {
                    this.TuioUdpPort = configUDPPortInt;
                    Console.WriteLine(string.Format("TUIO XML config: TUIO using UDP Port: {0}", configTuioPort));
                }
                else
                {
                    Console.WriteLine("TUIO XML config: invalid TUIO Port config");
                }

                int configInterpolationIntX;
                int configInterpolationIntY;
                if (interpolationX != null && int.TryParse(interpolationX, out configInterpolationIntX) &&
                    interpolationY != null && int.TryParse(interpolationY, out configInterpolationIntY))
                {
                    this.TrackingResolutionX = configInterpolationIntX;
                    this.TrackingResolutionY = configInterpolationIntY;
                    Console.WriteLine(string.Format("TUIO XML config: tracking interpolation: {0}x{1}", this.TrackingResolutionX, this.TrackingResolutionY));
                }
                //else
                //{
                //	Debug.Log(string.Format("TUIO XML config: invalid interpolation config. Using: {0}x{1}", m_tuioSettings.TrackingResolutionX, m_tuioSettings.TrackingResolutionY));
                //}

                float configStageFloatX;
                float configStageFloatY;
                if (stageX != null && float.TryParse(stageX, out configStageFloatX) &&
                    stageY != null && float.TryParse(stageY, out configStageFloatY))
                {
                    this.StageSizeX = configStageFloatX;
                    this.StageSizeY = configStageFloatY;
                    Console.WriteLine(string.Format("TUIO XML config: stage size: {0}x{1}", this.stageSizeX, this.stageSizeY));
                }
            }
            else
            {
                Console.WriteLine("no TUIO config xml file found in resources: Disable and Destroy TuioTrackingService");
            }

            if (configTracklinkProtocol != null)
            {
                configTracklinkProtocol = configTracklinkProtocol.ToUpper();
                switch (configTracklinkProtocol)
                {
                    case "UDP":
                        int configUDPPortInt;
                        if (configTracklinkUdpMulticastIp != null &&
                            configTracklinkUdpPort != null && int.TryParse(configTracklinkUdpPort, out configUDPPortInt))
                        {
                            this.tracklinkProtocol = EProtocolType.UDP;
                            this.tracklinkMulticastIp = configTracklinkUdpMulticastIp;
                            this.tracklinkUdpPort = configUDPPortInt;
                            Console.WriteLine(string.Format("TrackLink XML config: using UDP: {0}:{1}", configTracklinkUdpMulticastIp, configTracklinkUdpPort));
                        }
                        else
                        {
                            Console.WriteLine("TrackLink XML config: invalid UDP config data");
                        }
                        break;

                    case "TCP":
                        int configTCPPortInt;
                        if (configTracklinkTcpIp != null &&
                            configTracklinkTcpPort != null && int.TryParse(configTracklinkTcpPort, out configTCPPortInt))
                        {
                            this.tracklinkProtocol = EProtocolType.TCP;
                            this.tracklinkTcpIp = configTracklinkTcpIp;
                            this.tracklinkTcpPort = configTCPPortInt;
                            Console.WriteLine(string.Format("TrackLink XML config: using TCP: {0}:{1}", configTracklinkTcpIp, configTracklinkTcpPort));
                        }
                        else
                        {
                            Console.WriteLine("TrackLink XML config: invalid TCP config data");
                        }
                        break;

                    default:
                        Console.WriteLine("TrackLink XML config: invalid protocol specification");
                        break;
                }
            }
            else
            {
                Console.WriteLine("TrackLink XML config: invalid protocol specification");
            }
        }
    }
}