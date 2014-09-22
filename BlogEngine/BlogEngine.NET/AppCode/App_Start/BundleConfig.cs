using BlogEngine.Core;
using System;
using System.Web.Optimization;

/// <summary>
/// Summary description for BundleConfig
/// </summary>
public class BundleConfig
{
    public static void RegisterBundles(BundleCollection bundles)
    {
        // for anonymous users
        bundles.Add(new StyleBundle("~/Content/Auto/css").Include(
            "~/Content/Auto/*.css")
        );
        bundles.Add(new ScriptBundle("~/Scripts/Auto/js").Include(
            "~/Scripts/Auto/*.js")
        );

        // for authenticated users
        bundles.Add(new StyleBundle("~/Content/Auto/cssauth").Include(
            "~/Content/Auto/*.css",
            "~/Modules/QuickNotes/Qnotes.css")
        );  
        bundles.Add(new ScriptBundle("~/Scripts/Auto/jsauth").Include(
            "~/Scripts/Auto/*.js")
        );

        // syntax highlighter 
        var shRoot = "~/editors/tiny_mce_3_5_8/plugins/syntaxhighlighter/";
        bundles.Add(new StyleBundle("~/Content/highlighter").Include(
            shRoot + "styles/shCore.css",
            shRoot + "styles/shThemeDefault.css")
        );
        bundles.Add(new ScriptBundle("~/Scripts/highlighter").Include(
            shRoot + "scripts/XRegExp.js",
            shRoot + "scripts/shCore.js",
            shRoot + "scripts/shAutoloader.js",
            shRoot + "shActivator.js")
        );

        // syntax FileManager 
        bundles.Add(new StyleBundle("~/Content/filemanager").Include(
            "~/admin/FileManager/FileManager.css",
            "~/admin/uploadify/uploadify.css",
            "~/admin/FileManager/jqueryui/jquery-ui.css",
            "~/admin/FileManager/JCrop/css/jquery.Jcrop.css")
        );
        bundles.Add(new ScriptBundle("~/Scripts/filemanager").Include(
            "~/admin/uploadify/swfobject.js",
            "~/admin/uploadify/jquery.uploadify.v2.1.4.min.js",
            "~/admin/FileManager/jqueryui/jquery-ui.min.js",
            "~/admin/FileManager/jquery.jeegoocontext.min.js",
            "~/admin/FileManager/JCrop/js/jquery.Jcrop.min.js",
            "~/admin/FileManager/FileManager-mini.js")
        );

        // new admin bundles
        bundles.IgnoreList.Clear();
        AddDefaultIgnorePatterns(bundles.IgnoreList);

        bundles.Add(
          new StyleBundle("~/Content/css")
            .Include("~/Content/ie10mobile.css")
            .Include("~/Content/bootstrap.min.css")
            .Include("~/Content/toastr.css")
            .Include("~/Content/font-awesome.min.css")
            .Include("~/Content/editor.css")
            .Include("~/Content/app.css")
            .Include("~/editors/summernote/summernote.css")
          );

        bundles.Add(
          new ScriptBundle("~/scripts/blogadmin")
            .Include("~/scripts/jquery-2.1.0.js")
            .Include("~/scripts/jquery.form.js")
            .Include("~/scripts/jquery.validate.js")
            .Include("~/scripts/toastr.js")
            .Include("~/scripts/Q.js")
            .Include("~/Scripts/angular.min.js")
            .Include("~/Scripts/angular-route.min.js")
            .Include("~/Scripts/angular-animate.min.js")
            .Include("~/Scripts/angular-sanitize.min.js")
            .Include("~/admin/be-grid.js")
            .Include("~/admin/app.js")
            .Include("~/admin/controllers/dashboard.js")
            .Include("~/admin/controllers/blogs.js")
            .Include("~/admin/controllers/posts.js")
            .Include("~/admin/controllers/pages.js")
            .Include("~/admin/controllers/tags.js")
            .Include("~/admin/controllers/categories.js")
            .Include("~/admin/controllers/files.js")
            .Include("~/admin/controllers/comments.js")
            .Include("~/admin/controllers/users.js")
            .Include("~/admin/controllers/roles.js")
            .Include("~/admin/controllers/profile.js")
            .Include("~/admin/controllers/settings.js")
            .Include("~/admin/controllers/tools.js")
            .Include("~/admin/controllers/commentfilters.js")
            .Include("~/admin/controllers/blogroll.js")
            .Include("~/admin/controllers/pings.js")
            .Include("~/admin/controllers/packages.js")
            .Include("~/admin/controllers/common.js")
            .Include("~/admin/services.js")
            .Include("~/scripts/bootstrap.js")
            .Include("~/scripts/moment.js")
            .Include("~/editors/summernote/summernote.js")
          );

        bundles.Add(
          new ScriptBundle("~/scripts/wysiwyg")
            .Include("~/scripts/jquery-2.1.0.js")
            .Include("~/scripts/jquery.form.js")
            .Include("~/scripts/jquery.validate.js")
            .Include("~/scripts/toastr.js")
            .Include("~/scripts/Q.js")
            .Include("~/Scripts/angular.min.js")
            .Include("~/Scripts/angular-route.min.js")
            .Include("~/Scripts/angular-animate.min.js")
            .Include("~/Scripts/angular-sanitize.min.js")
            .Include("~/scripts/bootstrap.js")
            .Include("~/scripts/textext.js")
            .Include("~/scripts/moment.js")
            .Include("~/admin/app.js")
            .Include("~/admin/editor/editor.js")
            .Include("~/admin/editor/postcontroller.js")
            .Include("~/admin/editor/pagecontroller.js")
            .Include("~/admin/be-grid.js")
            .Include("~/admin/controllers/files.js")
            .Include("~/admin/services.js")
          );

        if (BlogConfig.DefaultEditor == "~/editors/bootstrap-wysiwyg/editor.cshtml")
        {
            bundles.GetBundleFor("~/scripts/wysiwyg").Include("~/editors/bootstrap-wysiwyg/jquery.hotkeys.js");
            bundles.GetBundleFor("~/scripts/wysiwyg").Include("~/editors/bootstrap-wysiwyg/bootstrap-wysiwyg.js");
            bundles.GetBundleFor("~/scripts/wysiwyg").Include("~/editors/bootstrap-wysiwyg/editor.js");
        }
        if (BlogConfig.DefaultEditor == "~/editors/tiny_mce_3_5_8/editor.cshtml")
        {
            bundles.GetBundleFor("~/scripts/wysiwyg").Include("~/editors/tiny_mce_3_5_8/tiny_mce.js");
            bundles.GetBundleFor("~/scripts/wysiwyg").Include("~/editors/tiny_mce_3_5_8/editor.js");
        }
        else
        {
            bundles.GetBundleFor("~/scripts/wysiwyg").Include("~/editors/summernote/summernote.js");
            // change language here if needed
            //bundles.GetBundleFor("~/scripts/wysiwyg").Include("~/editors/summernote/lang/summernote-ru-RU.js");
            bundles.GetBundleFor("~/scripts/wysiwyg").Include("~/editors/summernote/editor.js");            
        }
    }

    public static void AddDefaultIgnorePatterns(IgnoreList ignoreList)
    {
        if (ignoreList == null)
            throw new ArgumentNullException("ignoreList");

        ignoreList.Ignore("*.intellisense.js");
        ignoreList.Ignore("*-vsdoc.js");

        //ignoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);
        //ignoreList.Ignore("*.min.js", OptimizationMode.WhenDisabled);
        //ignoreList.Ignore("*.min.css", OptimizationMode.WhenDisabled);
    }
}