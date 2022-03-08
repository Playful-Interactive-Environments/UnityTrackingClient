namespace Assets.Pharus_Tracking_Framework.TransmissionFrameworks.Tracklink
{
    public interface ITransmissionClient
    {
        bool Connected { get; }

        void Connect();
        void Disconnect();

        void RegisterTransmissionReceiver(ITransmissionReceiver newReceiver);
        void UnregisterTransmissionReceiver(ITransmissionReceiver oldReceiver);
    }
}
