/*
 * Copyright (c) 2014 Håkan Edling
 *
 * See the file LICENSE for copying permission.
 */

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Xml.Linq;
using Newtonsoft.Json;
using Piranha.Models.Manager.PageModels;
using Piranha.Web;
using WebActivatorEx;
using Piranha.WebPages;

[assembly: PreApplicationStartMethod(typeof(Piranha.Localization.Startup), "PreInit")]
[assembly: PostApplicationStartMethod(typeof(Piranha.Localization.Startup), "Init")]

namespace Piranha.Localization
{
	/// <summary>
	/// Starts the localization module
	/// </summary>
	public sealed class Startup
	{
		/// <summary>
		/// Preforms pre-startup initialization.
		/// </summary>
		public static void PreInit() {
			//
			// Register the HTTP module
			//
			Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(typeof(Module));
		}
	
		/// <summary>
		/// Initializes the module and attaches application hooks.
		/// </summary>
		public static void Init()
		{

		    Hooks.Manager.PageListModelLoaded += (controller, menu, model) =>
		    {
                Localizer.LocalizePageListModel(model);
            };
            //
            // Page translations hooks
            //
            Hooks.Model.PageModelLoaded += (model) => {
				Localizer.LocalizePageModel(model);
			};
			Hooks.Manager.PageEditModelLoaded += (controller, menu, model) => {
				Localizer.LocalizePageOnLoad(model);

				// Reset culture
				ResetCulture(controller.HttpContext);

				// Reset title
				if (model.Page.IsNew)
					controller.ViewBag.Title = Piranha.Resources.Page.EditTitleNew;
				else controller.ViewBag.Title = Piranha.Resources.Page.EditTitleExisting;
			};
			Hooks.Manager.PageEditModelBeforeSave += (controller, menu, model, publish) => {
				Localizer.LocalizePageBeforeSave(model, publish);

				// Reset culture
				//ResetCulture(controller.HttpContext); -- Appears to break the Manager when page initially loads after a POST

				// Reset title
				if (model.Page.IsNew)
					controller.ViewBag.Title = Piranha.Resources.Page.EditTitleNew;
				else controller.ViewBag.Title = Piranha.Resources.Page.EditTitleExisting;
			};

			//
			// Post translation hooks
			//
			Hooks.Model.PostModelLoaded += (model) => {
				// Do something
			};
			Hooks.Manager.PostEditModelLoaded += (controller, menu, model) => {
				// Do something
			};
			Hooks.Manager.PostEditModelBeforeSave += (controller, menu, model, publish) => { 
				// Do something
			};

			//
			// Category hooks are currently missing in the core framework.
			//

			//
			// Page edit toolbar
			//
            Hooks.Manager.Toolbar.PageEditToolbarRender += (url, str, model) =>
            {
                str.Append(String.Format("<li {2}><a href=\"{0}\"><span class=\"flag flag-gb\"></span>English</a>{1}{3}{4}</li>",
                    url.Action("edit", new { id = model.Page.Id }), Previews(model, "en-gb", url), (Utils.GetDefaultCulture().Name == Utils.GetCurrentCulture().Name ? "style=\"background-color: #b2dfe7;\"":""), (model.Page.Published != DateTime.MinValue && model.Page.Updated > model.Page.LastPublished? "<span style=\"color:#DDC000\" class=\"hint--bottom\" data-hint=\"Draft\"><i class=\"icon-draft\"></i><span>" : ""), (model.Page.Published == DateTime.MinValue ? "<span class=\"info-unpublished\"></span>" : "")));
                if (model.Page.Created != DateTime.MinValue)
                {
                    var translations = Localizer.GetPageTranslations(model);

                    foreach (var lang in Module.Languages)
                    {
                        var trans = translations.FirstOrDefault(t => t.Culture == lang.Culture);
                        bool? draft = null;
                        bool published = false;
                        if (trans != null)
                        {
                            draft = trans.Updated > trans.LastPublished || trans.LastPublished == null;
                            published = trans.Published != null;
                        }
                        str.Append(
                            String.Format(
                                "<li {4}><a href=\"{0}\"><span class=\"flag flag-{2}\"></span> {1}</a>{3}{5}{6}</li>",
                                "/" + lang.UrlPrefix + url.Action("edit", new {id = model.Page.Id}),
                                lang.Name, lang.Culture.Split('-')[1].ToLower(), Previews(model, lang.Culture, url),
                                (lang.Culture == Utils.GetCurrentCulture().Name
                                    ? "style=\"background-color: #b2dfe7;\""
                                    : ""),
                                (published
                                    ? ""
                                    : "<span style=\"color:#ccc\" class=\"hint--bottom\" data-hint=\"Unpublished (same as English)\"><i class=\"icon-copy\"></i></span>"),
                                (draft != null && (bool) draft
                                    ? "<span style=\"color:#DDC000\" class=\"hint--bottom\" data-hint=\"Draft\"><i class=\"icon-draft\"></i><span>"
                                    : "")));
                    }

                    //
                    // Modify the post action to the currently selected language.
                    //
                    if (Utils.GetDefaultCulture().Name != Utils.GetCurrentCulture().Name)
                    {
                        var lang =
                            Module.Languages.Where(l => l.Culture == Utils.GetCurrentCulture().Name).SingleOrDefault();

                        if (lang != null)
                        {
                            str.Append(
                                "<script>" +
                                "  $(document).ready(function() {" +
                                "    var form = $($('form')[0]);" +
                                "    form.attr('action', '/" + lang.UrlPrefix + "' + form.attr('action'));" +
                                "  });" +
                                "</script>"
                                );
                        }
                    }
                }
            };
		}

	    private static string Previews(EditModel model, string culture, UrlHelper url)
	    {
	        if (model.SiteTree.HostNames == null)
	            return string.Empty;
	        var hostName = model.SiteTree.HostNames.Split(',').FirstOrDefault();
	        if (model.Page.Permalink == null)
	            return "";
            return  string.Format("<a href=\"{0}/{1}{2}\" class=\"preview\" target=\"preview\">Preview</a>", WebPiranha.GetSiteUrl(hostName), culture.ToLower(),url.GetPermalink(model.Page.Permalink, true));
	    }

	    private static void ResetCulture(System.Web.HttpContextBase context) {
			var def = Utils.GetDefaultCulture();

			if (def.Name != CultureInfo.CurrentUICulture.Name) {
				context.Items.Add("LOCALIZATION_CULTURE", CultureInfo.CurrentUICulture.Name);
				Thread.CurrentThread.CurrentCulture =
					Thread.CurrentThread.CurrentUICulture = def;
			}
		}

        private static string PrettyPrint(string serialisedInput)
        {
            if (string.IsNullOrEmpty(serialisedInput))
            {
                return serialisedInput;
            }

            try
            {
                return XDocument.Parse(serialisedInput).ToString();
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                var t = JsonConvert.DeserializeObject<object>(serialisedInput);
                return JsonConvert.SerializeObject(t, Formatting.Indented);
            }
            catch (Exception)
            {
                // ignored
            }

            return serialisedInput;
        }
    }
}