using ASP.App_Start;
using BlogEngine.Core.Data;
using BlogEngine.Core.Data.Contracts;
using Microsoft.Practices.Unity;

/// <summary>
/// Summary description for UnityConfig
/// </summary>
public class UnityConfig
{
	public UnityConfig() { }

    public static void Register(System.Web.Http.HttpConfiguration config)
    {
        var unity = new UnityContainer();
        
        unity.RegisterType<SettingsController>();
        unity.RegisterType<PostsController>();
        unity.RegisterType<PagesController>();
        unity.RegisterType<BlogsController>();
        unity.RegisterType<StatsController>();
        unity.RegisterType<PackagesController>();
        unity.RegisterType<LookupsController>();
        unity.RegisterType<CommentsController>();
        unity.RegisterType<TrashController>();
        unity.RegisterType<TagsController>();
        unity.RegisterType<CategoriesController>();
        unity.RegisterType<CustomFieldsController>();
        unity.RegisterType<UsersController>();
        unity.RegisterType<RolesController>();
        unity.RegisterType<FileManagerController>();
        unity.RegisterType<CommentFilterController>();

        unity.RegisterType<ISettingsRepository, SettingsRepository>(new HierarchicalLifetimeManager());
        unity.RegisterType<IPostRepository, PostRepository>(new HierarchicalLifetimeManager());
        unity.RegisterType<IPageRepository, PageRepository>(new HierarchicalLifetimeManager());
        unity.RegisterType<IBlogRepository, BlogRepository>(new HierarchicalLifetimeManager());
        unity.RegisterType<IStatsRepository, StatsRepository>(new HierarchicalLifetimeManager());
        unity.RegisterType<IPackageRepository, PackageRepository>(new HierarchicalLifetimeManager());
        unity.RegisterType<ILookupsRepository, LookupsRepository>(new HierarchicalLifetimeManager());
        unity.RegisterType<ICommentsRepository, CommentsRepository>(new HierarchicalLifetimeManager());
        unity.RegisterType<ITrashRepository, TrashRepository>(new HierarchicalLifetimeManager());
        unity.RegisterType<ITagRepository, TagRepository>(new HierarchicalLifetimeManager());
        unity.RegisterType<ICategoryRepository, CategoryRepository>(new HierarchicalLifetimeManager());
        unity.RegisterType<ICustomFieldRepository, CustomFieldRepository>(new HierarchicalLifetimeManager());
        unity.RegisterType<IUsersRepository, UsersRepository>(new HierarchicalLifetimeManager());
        unity.RegisterType<IRolesRepository, RolesRepository>(new HierarchicalLifetimeManager());
        unity.RegisterType<IFileManagerRepository, FileManagerRepository>(new HierarchicalLifetimeManager());
        unity.RegisterType<ICommentFilterRepository, CommentFilterRepository>(new HierarchicalLifetimeManager());

        config.DependencyResolver = new IoCContainer(unity);
    }
}