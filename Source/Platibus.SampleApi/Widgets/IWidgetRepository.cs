using System.Collections.Generic;
using System.Threading.Tasks;

namespace Platibus.SampleApi.Widgets
{
    public interface IWidgetRepository
    {
        Task Add(Widget widget);

        Task Update(Widget widget);

        Task<Widget> Get(string id);

        Task<IEnumerable<Widget>> List();

        Task Remove(string id);
    }
}