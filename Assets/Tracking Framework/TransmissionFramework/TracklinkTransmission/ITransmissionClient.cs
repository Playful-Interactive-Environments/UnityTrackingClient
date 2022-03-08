namespace Assets.Tracking_Framework.TransmissionFramework.TracklinkTransmission
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
