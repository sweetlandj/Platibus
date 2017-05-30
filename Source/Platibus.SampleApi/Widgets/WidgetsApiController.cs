using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;
using Platibus.Owin;
using Platibus.SampleMessages;
using Platibus.SampleMessages.Widgets;

namespace Platibus.SampleApi.Widgets
{
    [Authorize]
    [RoutePrefix("api/widgets")]
    public class WidgetsApiController : ApiController
    {
        private readonly IWidgetRepository _widgetRepository;

        private IBus Bus
        {
            get { return Request.GetOwinContext().GetBus(); }
        }
        
        public WidgetsApiController(IWidgetRepository widgetRepository)
        {
            _widgetRepository = widgetRepository;
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(ResponseDocument<WidgetResource>))]
        public async Task<IHttpActionResult> Post(RequestDocument<WidgetResource> request)
        {
            try
            {
                var widget = MapToEntity(request.Data ?? new WidgetResource());
                await _widgetRepository.Add(widget);
                var createdResource = MapToResource(widget);
                await Bus.Publish(new WidgetEvent("WidgetCreated", createdResource, GetRequestor()), "WidgetEvents");
                return ResourceCreated(createdResource);
            }
            catch (WidgetAlreadyExistsException)
            {
                return Conflict();
            }
        }

        [HttpPatch]
        [Route("{id}")]
        [ResponseType(typeof(ResponseDocument<WidgetResource>))]
        public async Task<IHttpActionResult> Patch(string id, [FromBody] RequestDocument<WidgetResource> request)
        {
            try
            {
                var resource = request.Data;
                if (resource == null || string.IsNullOrWhiteSpace(resource.Id))
                {
                    return BadRequest();
                }

                var widget = await _widgetRepository.Get(resource.Id);
                ApplyUpdates(widget, resource);
                await _widgetRepository.Update(widget);
                var updatedResource = MapToResource(widget);
                await Bus.Publish(new WidgetEvent("WidgetUpdated", updatedResource, GetRequestor()), "WidgetEvents");
                return Ok(ResponseDocument.Containing(resource));
            }
            catch (WidgetNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete]
        [Route("{id}")]
        [ResponseType(typeof(ResponseDocument<WidgetResource>))]
        public async Task<IHttpActionResult> Delete(string id)
        {
            try
            {
                await _widgetRepository.Remove(id);
                await Bus.Publish(new WidgetEvent("WidgetDeleted", null, GetRequestor()), "WidgetEvents");
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (WidgetNotFoundException)
            {
                return NotFound();
            }
        }
        
        [HttpGet]
        [Route("{id}")]
        [ResponseType(typeof(ResponseDocument<WidgetResource>))]
        public async Task<IHttpActionResult> Get(string id)
        {
            try
            {
                var widget = await _widgetRepository.Get(id);
                var resource = MapToResource(widget);
                return Ok(ResponseDocument.Containing(resource));
            }
            catch (WidgetNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("")]
        [ResponseType(typeof(ResponseDocument<IList<WidgetResource>>))]
        public async Task<IHttpActionResult> Get()
        {
            var widgets = await _widgetRepository.List();
            var resources = widgets.Select(MapToResource);
            return Ok(ResponseDocument.Containing(resources));
        }

        private IHttpActionResult ResourceCreated(WidgetResource resource)
        {
            var responseContent = ResponseDocument.Containing(resource);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(responseContent))
            };
            responseMessage.Headers.Location = new UriBuilder(Request.RequestUri)
            {
                Path = Request.RequestUri.AbsolutePath + "/" + resource.Id
            }.Uri;
            return ResponseMessage(responseMessage);
        }

        private string GetRequestor()
        {
            var principal = RequestContext.Principal;
            if (principal == null) return null;

            var identity = principal.Identity;
            if (identity == null) return null;

            return identity.Name;
        }
        
        private static Widget MapToEntity(WidgetResource resource)
        {
            if (resource == null) throw new ArgumentNullException("resource");
            var attributes = resource.Attributes ?? new WidgetAttributes();
            return new Widget
            {
                Id = resource.Id,
                PartNumber = attributes.PartNumber,
                Description = attributes.Description,
                Length = attributes.Length.GetValueOrDefault(),
                Width = attributes.Length.GetValueOrDefault(),
                Height = attributes.Height.GetValueOrDefault()
            };
        }

        private static void ApplyUpdates(Widget widget, WidgetResource updates)
        {
            if (updates == null) return;
            var attributeUpdates = updates.Attributes ?? new WidgetAttributes();
            ApplyUpdate(attributeUpdates.PartNumber, v => widget.PartNumber = v);
            ApplyUpdate(attributeUpdates.Description, v => widget.Description = v);
            ApplyUpdate(attributeUpdates.Length, v => widget.Length = v.GetValueOrDefault());
            ApplyUpdate(attributeUpdates.Width, v => widget.Width = v.GetValueOrDefault());
            ApplyUpdate(attributeUpdates.Height, v => widget.Height = v.GetValueOrDefault());
        }

        private static void ApplyUpdate<TValue>(TValue value, Action<TValue> applyUpdate)
        {
            if (value == null) return;
            applyUpdate(value);
        }

        private static WidgetResource MapToResource(Widget entity)
        {
            return new WidgetResource
            {
                Id = entity.Id,
                Attributes = new WidgetAttributes
                {
                    PartNumber = entity.PartNumber,
                    Description = entity.Description,
                    Length = entity.Length,
                    Width = entity.Width,
                    Height = entity.Height
                }
            };
        }
    }
}