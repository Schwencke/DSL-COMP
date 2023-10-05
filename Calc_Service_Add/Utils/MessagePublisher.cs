using EasyNetQ;
using Events;
using Helpers;

namespace Calc_Service_Add.Utils
{
    public class MessagePublisher : IMessagePublisher
    {

        public void PublishAddEvent(Result e)
        {
            using (var _bus = ConnectionHelper.GetRMQConnection())
            {
                _bus.PubSub.PublishAsync(e, "Result");
            }
        }


    }
}
