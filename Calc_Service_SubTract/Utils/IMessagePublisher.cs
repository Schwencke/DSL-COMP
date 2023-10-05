using Events;

namespace Calc_Service_Sub.Utils
{
    public interface IMessagePublisher
    {
        void PublishSubEvent(Result e);
    }
}
