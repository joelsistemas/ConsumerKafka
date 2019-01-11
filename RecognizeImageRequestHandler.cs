using SimpeConsumerSMBKafka.Services;
using SlimMessageBus;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpeConsumerSMBKafka
{
    public class RecognizeImageRequestHandler: IConsumer<RecognizeImageRequest>
    {
        private readonly HttpClient _client;
        private readonly IPeopleService _service;

        public RecognizeImageRequestHandler(HttpClient client, IPeopleService service)
        {
            _client = client;
            _service = service;
        }

        public async Task OnHandle(RecognizeImageRequest message, string topic)
        {
            var image = await _service.Get(message.Url);

            //await _client.PostAsync();
        }
    }
}
