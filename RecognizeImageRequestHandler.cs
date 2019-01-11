using System;
using SimpeConsumerSMBKafka.Services;
using SlimMessageBus;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpeConsumerSMBKafka
{
    public class RecognizeImageRequestHandler: IConsumer<RecognizeImageRequest>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly IPeopleService _service;

        public RecognizeImageRequestHandler(IHttpClientFactory clientFactory, IPeopleService service)
        {
            _client = clientFactory.CreateClient();
            _service = service;
        }

        public async Task OnHandle(RecognizeImageRequest message, string topic)
        {
            var image = await _service.Get(message.Url);

            //await _client.PostAsync();
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose the client
                _client?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
