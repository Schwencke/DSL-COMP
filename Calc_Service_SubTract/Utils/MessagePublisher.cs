using EasyNetQ;
using Events;
using Helpers;

namespace Calc_Service_Sub.Utils
{
    public class MessagePublisher : IMessagePublisher
    {

        public void PublishSubEvent(Result e)
        {
            using (var _bus = ConnectionHelper.GetRMQConnection())
            {
                _bus.PubSub.PublishAsync(e, "Result");
            }
        }
    }
}
