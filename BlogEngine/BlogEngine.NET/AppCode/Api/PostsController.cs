using App_Code;
using BlogEngine.Core.Data.Contracts;
using BlogEngine.Core.Data.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

public class PostsController : ApiController
{
    readonly IPostRepository repository;

    public PostsController(IPostRepository repository)
    {
        this.repository = repository;
    }

    public IEnumerable<PostItem> Get(int take = 10, int skip = 0, string filter = "", string order = "")
    {
        try
        {
            return repository.Find(take, skip, filter, order);
        }
        catch (UnauthorizedAccessException)
        {
            throw new HttpResponseException(HttpStatusCode.Unauthorized);
        }
        catch (Exception)
        {
            throw new HttpResponseException(HttpStatusCode.InternalServerError);
        }
    }

    public HttpResponseMessage Get(string id)
    {
        try
        {
            var result = repository.FindById(new Guid(id));
            if (result == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
        catch (UnauthorizedAccessException)
        {
            return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }
        catch (Exception)
        {
            return Request.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    public HttpResponseMessage Post([FromBody]PostDetail item)
    {
        try
        {
            var result = repository.Add(item);
            if (result == null)
                return Request.CreateResponse(HttpStatusCode.NotModified);

            return Request.CreateResponse(HttpStatusCode.Created, result);
        }
        catch (UnauthorizedAccessException)
        {
            return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }
        catch (Exception)
        {
            return Request.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    [HttpPut]
    public HttpResponseMessage Update([FromBody]PostDetail item)
    {
        try
        {
            repository.Update(item, "update");
            return Request.CreateResponse(HttpStatusCode.OK);
        }
        catch (UnauthorizedAccessException)
        {
            return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }
        catch (Exception)
        {
            return Request.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
	
	[HttpPut]
    public HttpResponseMessage ProcessChecked([FromBody]List<PostDetail> items)
    {
        try
        {
            if (items == null || items.Count == 0)
                throw new HttpResponseException(HttpStatusCode.ExpectationFailed);

            var action = Request.GetRouteData().Values["id"].ToString();
      
            foreach (var item in items)
            {
                if (item.IsChecked)
                {
                    if (action.ToLower() == "delete")
                    {
                        repository.Remove(item.Id);
                    }
                    else
                    {
                        repository.Update(item, action);
                    }	
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
        catch (UnauthorizedAccessException)
        {
            return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }
        catch (Exception)
        {
            return Request.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    public HttpResponseMessage Delete(string id)
    {
        try
        {
            Guid gId;
            if (Guid.TryParse(id, out gId))
                repository.Remove(gId);

            return Request.CreateResponse(HttpStatusCode.OK);
        }
        catch (UnauthorizedAccessException)
        {
            return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }
        catch (Exception)
        {
            return Request.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}