using System.Threading.Tasks;

namespace SimpeConsumerSMBKafka.Services
{
    public interface IPeopleService
    {
        Task<byte[]> Get(string url);
    }
}
