using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platibus.SampleMessages;
using Platibus.SampleMessages.Widgets;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Platibus.SampleApi.Widgets
{
    [Authorize]
    [Route("api/widgets")]
    public class WidgetsApiController : Controller
    {
        private readonly IWidgetRepository _widgetRepository;

        private readonly IBus _bus;

        public WidgetsApiController(IWidgetRepository widgetRepository, IBus bus)
        {
            _widgetRepository = widgetRepository;
            _bus = bus;
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Post([FromBody] RequestDocument<WidgetResource> request)
        {
            try
            {
                var widget = WidgetMap.ToEntity(request.Data ?? new WidgetResource());
                await _widgetRepository.Add(widget);
                var createdResource = WidgetMap.ToResource(widget);
                await _bus.Publish(new WidgetEvent("WidgetCreated", createdResource, GetRequestor()), "WidgetEvents");
                return CreatedAtAction("Post", createdResource);
            }
            catch (WidgetAlreadyExistsException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Conflict);
            }
        }

        [HttpPatch]
        [Route("{id}")]
        public async Task<IActionResult> Patch(string id, [FromBody] RequestDocument<WidgetResource> request)
        {
            try
            {
                var resource = request.Data;
                if (string.IsNullOrWhiteSpace(resource?.Id))
                {
                    return BadRequest();
                }

                var widget = await _widgetRepository.Get(resource.Id);
                WidgetMap.ApplyUpdates(widget, resource);
                await _widgetRepository.Update(widget);
                var updatedResource = WidgetMap.ToResource(widget);
                await _bus.Publish(new WidgetEvent("WidgetUpdated", updatedResource, GetRequestor()), "WidgetEvents");
                return Ok(updatedResource);
            }
            catch (WidgetNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _widgetRepository.Remove(id);
                await _bus.Publish(new WidgetEvent("WidgetDeleted", null, GetRequestor()), "WidgetEvents");
                return NoContent();
            }
            catch (WidgetNotFoundException)
            {
                return NotFound();
            }
        }
        
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var widget = await _widgetRepository.Get(id);
                var resource = WidgetMap.ToResource(widget);
                return Ok(ResponseDocument.Containing(resource));
            }
            catch (WidgetNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get()
        {
            var widgets = await _widgetRepository.List();
            var resources = widgets.Select(WidgetMap.ToResource);
            return Ok(ResponseDocument.Containing(resources));
        }
        
        private string GetRequestor()
        {
            var principal = ControllerContext.HttpContext.User;
            var identity = principal?.Identity;
            return identity?.Name;
        }
        
        
    }
}