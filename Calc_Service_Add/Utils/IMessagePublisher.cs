using Events;

namespace Calc_Service_Add.Utils
{
    public interface IMessagePublisher
    {
        void PublishAddEvent(Result e);
    }
}
