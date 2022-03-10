using System.Threading.Tasks;

namespace AggieMove.Services
{
    public interface IDialog
    {
        public object GetResult();

        public Task<object> ShowAsync();
    }
}
