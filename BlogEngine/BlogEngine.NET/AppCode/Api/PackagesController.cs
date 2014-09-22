using BlogEngine.Core.Data.Contracts;
using BlogEngine.Core.Data.Models;
using BlogEngine.Core.Packaging;
using System.Collections.Generic;
using System.Web.Http;
using System;
using System.Net;
using System.Net.Http;

public class PackagesController : ApiController
{
    readonly IPackageRepository repository;

    public PackagesController(IPackageRepository repository)
    {
        this.repository = repository;
    }

    public IEnumerable<Package> Get(int take = 10, int skip = 0, string filter = "", string order = "")
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
            var result = repository.FindById(id);
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

    [HttpPut]
    public HttpResponseMessage ProcessChecked([FromBody]List<Package> items)
    {
        try
        {
            if (items == null || items.Count == 0)
                throw new HttpResponseException(HttpStatusCode.ExpectationFailed);

            var action = Request.GetRouteData().Values["id"].ToString();

            if (items != null && items.Count > 0)
            {
                try
                {
                    if (action == "install")
                    {
                        foreach (var item in items)
                        {
                            if (!repository.Install(item.Id))
                                throw new HttpResponseException(HttpStatusCode.InternalServerError);
                        }
                    }
                    if (action == "uninstall")
                    {
                        foreach (var item in items)
                        {
                            if (!repository.Uninstall(item.Id))
                                throw new HttpResponseException(HttpStatusCode.InternalServerError);
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                catch (System.Exception ex)
                {
                    BlogEngine.Core.Utils.Log("Error processing package", ex);
                    throw new HttpResponseException(HttpStatusCode.InternalServerError);
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

    [HttpPut]
    public bool Update([FromBody]Package item)
    {
        try
        {
            repository.Update(item);
            return true;
        }
        catch (Exception ex)
        {
            BlogEngine.Core.Utils.Log("Error updating package", ex);
            throw new HttpResponseException(HttpStatusCode.InternalServerError);
        }
    }

    [HttpPut]
    public HttpResponseMessage Install(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpResponseException(HttpStatusCode.ExpectationFailed);

            repository.Install(id);
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
    public HttpResponseMessage Uninstall(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpResponseException(HttpStatusCode.ExpectationFailed);

            repository.Uninstall(id);
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
    public bool Refresh()
    {
        try
        {
            BlogEngine.Core.Blog.CurrentInstance.Cache.Remove(Constants.CacheKey);
            return true;
        }
        catch (Exception ex)
        {
            BlogEngine.Core.Utils.Log("Error refreshing gallery cache", ex);
            throw new HttpResponseException(HttpStatusCode.InternalServerError);
        }
    }

}
