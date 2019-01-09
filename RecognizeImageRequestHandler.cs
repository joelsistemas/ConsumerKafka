using SlimMessageBus;
using System.Threading.Tasks;

namespace SimpeConsumerSMBKafka
{
    public class RecognizeImageRequestHandler: IRecognizeImageRequestHandler
    {

        public RecognizeImageRequestHandler()
        {

        }

        public Task OnHandle(RecognizeImageRequest message, string topic)
        {

            return Task.Delay(1000);
        }
    }
}
