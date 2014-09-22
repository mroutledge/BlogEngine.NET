﻿using BlogEngine.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

public class BlogRollController : ApiController
{
    public BlogRollController()
    {
    }

    public IEnumerable<BlogRollRowItem> Get()
    {
        try
        {
            var items = new List<BlogRollRowItem>();
            var rolls = BlogRollItem.BlogRolls;

            foreach (var item in rolls)
            {
                items.Add(new BlogRollRowItem
                {
                    IsChecked = false,
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    BlogUrl = item.BlogUrl.ToString(),
                    FeedUrl = item.FeedUrl.ToString(),
                    Xfn = item.Xfn
                });
            }
            return items;
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
            var item = BlogRollItem.GetBlogRollItem(Guid.Parse(id));
            if (item == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var result = new BlogRollRowItem
                {
                    IsChecked = false,
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    BlogUrl = item.BlogUrl.ToString(),
                    FeedUrl = item.FeedUrl.ToString(),
                    Xfn = item.Xfn
                };

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

    public HttpResponseMessage Post(BlogRollRowItem item)
    {
        try
        {
            BlogRollItem br = new BlogRollItem();
            br.Title = item.Title.Replace(@"\", "'");
            br.Description = item.Description;
            br.BlogUrl = new Uri(getUrl(item.BlogUrl));
            br.FeedUrl = new Uri(getUrl(item.FeedUrl));
            br.Xfn = item.Xfn.TrimEnd();

            int largestSortIndex = -1;
            foreach (BlogRollItem brExisting in BlogRollItem.BlogRolls)
            {
                if (brExisting.SortIndex > largestSortIndex)
                    largestSortIndex = brExisting.SortIndex;
            }
            br.SortIndex = largestSortIndex + 1;
            br.Save();

            if (br == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            return Request.CreateResponse(HttpStatusCode.Created, br);
        }
        catch (UnauthorizedAccessException)
        {
            return Request.CreateResponse(HttpStatusCode.Unauthorized, Resources.labels.notAuthorized);
        }
        catch (Exception ex)
        {
            return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpPut]
    public HttpResponseMessage Update([FromBody]BlogRollRowItem item)
    {
        try
        {
            var br = BlogRollItem.GetBlogRollItem(item.Id);
            if (br == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            br.Title = item.Title;
            br.Description = item.Description;
            br.BlogUrl = new Uri(item.BlogUrl);
            br.FeedUrl = new Uri(item.FeedUrl);
            br.Xfn = item.Xfn.TrimEnd();
            br.Save();

            Resort();
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
    public HttpResponseMessage ProcessChecked([FromBody]List<BlogRollRowItem> items)
    {
        try
        {
            if (items == null || items.Count == 0)
                throw new HttpResponseException(HttpStatusCode.ExpectationFailed);

            foreach (var item in items)
            {
                if (item.IsChecked)
                {
                    BlogRollItem br = BlogRollItem.GetBlogRollItem(item.Id);
                    br.Delete();
                    br.Save();
                }
            }
            Resort();
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

    private string getUrl(string url)
    {
        if (!string.IsNullOrEmpty(url) && !url.Contains("://"))
            url = "http://" + url;
        return url;
    }

    private void Resort()
    {
        int sortIndex = -1;
        // Re-sort remaining items starting from zero to eliminate any possible gaps.
        // Need to cast BlogRollItem.BlogRolls to an array to
        // prevent errors with modifying a collection while enumerating it.
        foreach (BlogRollItem brItem in BlogRollItem.BlogRolls.ToArray())
        {
            brItem.SortIndex = ++sortIndex;
            brItem.Save();
        }
    }
}

public class BlogRollRowItem
{
    public bool IsChecked { get; set; }
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string BlogUrl { get; set; }
    public string FeedUrl { get; set; }
    public string Description { get; set; }
    public string Xfn { get; set; }
}