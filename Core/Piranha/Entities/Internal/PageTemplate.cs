/*
 * Copyright (c) 2011-2015 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 * 
 * http://github.com/piranhacms/piranha
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;

using Piranha.Data;

namespace Piranha.Models
{
	/// <summary>
	/// Active record for a page template.
	/// 
	/// Changes made to records of this type are logged.
	/// </summary>
	[PrimaryKey(Column = "pagetemplate_id")]
	[Serializable]
	public class PageTemplate : PiranhaRecord<PageTemplate>, ICacheRecord<PageTemplate>
	{
		#region Fields
		/// <summary>
		/// Gets/sets the id.
		/// </summary>
		[Column(Name = "pagetemplate_id")]
		public override Guid Id { get; set; }

		/// <summary>
		/// Gets/sets the name.
		/// </summary>
		[Column(Name = "pagetemplate_name")]
		[Display(ResourceType = typeof(Piranha.Resources.Global), Name = "Name")]
		[Required(ErrorMessageResourceType = typeof(Piranha.Resources.Global), ErrorMessageResourceName = "NameRequired")]
		public string Name { get; set; }

		/// <summary>
		/// Gets/sets the template's description.
		/// </summary>
		[Column(Name = "pagetemplate_description")]
		[Display(ResourceType = typeof(Piranha.Resources.Page), Name = "Description")]
		public string Description { get; set; }

		/// <summary>
		/// Gets/sets the template's html preview.
		/// </summary>
		[Column(Name = "pagetemplate_preview")]
		[Display(ResourceType = typeof(Piranha.Resources.Page), Name = "HtmlPreview")]
		public HtmlString Preview { get; set; }

		/// <summary>
		/// Gets/sets the regions associated with this page.
		/// </summary>
		[Column(Name = "pagetemplate_page_regions", Json = true)]
		internal List<string> PageRegions { get; set; }

		/// <summary>
		/// Gets/sets the associated properties.
		/// </summary>
		[Column(Name = "pagetemplate_properties", Json = true)]
		public List<string> Properties { get; set; }

		/// <summary>
		/// Gets/sets the optional controller for the template.
		/// </summary>
		[Column(Name = "pagetemplate_controller")]
		[Display(ResourceType = typeof(Piranha.Resources.Page), Name = "Template")]
		public string Controller { get; set; }

		/// <summary>
		/// Gets/sets whether the controller can be overridden by the implementing page.
		/// </summary>
		[Column(Name = "pagetemplate_controller_show")]
		public bool ShowController { get; set; }

		/// <summary>
		/// Gets/sets the optional view for the template.
		/// </summary>
		[Column(Name = "pagetemplate_view")]
		[Display(ResourceType = typeof(Piranha.Resources.Page), Name = "View")]
		public string View { get; set; }

		/// <summary>
		/// Gets/sets whether the view can be overridden by the implementing page.
		/// </summary>
		[Column(Name = "pagetemplate_view_show")]
		public bool ShowView { get; set; }

		/// <summary>
		/// Gets/sets the optional permalink of a page this sould redirect to.
		/// </summary>
		[Column(Name = "pagetemplate_redirect")]
		[Display(ResourceType = typeof(Piranha.Resources.Page), Name = "Redirect")]
		public string Redirect { get; set; }

		/// <summary>
		/// Gets/sets if the redirect can be overriden by the implementing page.
		/// </summary>
		[Column(Name = "pagetemplate_redirect_show")]
		public bool ShowRedirect { get; set; }

		/// <summary>
		/// Gets/sets if this is a site template.
		/// </summary>
		[Column(Name = "pagetemplate_site_template")]
		public bool IsSiteTemplate { get; set; }

		/// <summary>
		/// Gets/sets the type that created this template if it was create by code.
		/// </summary>
		[Column(Name = "pagetemplate_type")]
		public string Type { get; set; }

		/// <summary>
		/// Gets/sets the created date.
		/// </summary>
		[Column(Name = "pagetemplate_created")]
		public override DateTime Created { get; set; }

		/// <summary>
		/// Gets/sets the updated date.
		/// </summary>
		[Column(Name = "pagetemplate_updated")]
		public override DateTime Updated { get; set; }

		/// <summary>
		/// Gets/sets the user id that created the record.
		/// </summary>
		[Column(Name = "pagetemplate_created_by")]
		public override Guid CreatedBy { get; set; }

		/// <summary>
		/// Gets/sets the user id that created the record.
		/// </summary>
		[Column(Name = "pagetemplate_updated_by")]
		public override Guid UpdatedBy { get; set; }
		#endregion

		#region Properties
		/// <summary>
		/// Gets if the template is locked from editing in the manager interface.
		/// </summary>
		public bool IsLocked {
			get {
				if (!String.IsNullOrEmpty(Type))
					return Extend.ExtensionManager.Current.PageTypes.Where(pt => pt.GetType().FullName == Type).SingleOrDefault() != null;
				return false;
			}
		}
		#endregion

		/// <summary>
		/// Default constructor.
		/// </summary>
		public PageTemplate()
			: base() {
			PageRegions = new List<string>();
			Properties = new List<string>();
			LogChanges = true;
		}

		/// <summary>
		/// Gets a single page template.
		/// </summary>
		/// <param name="id">The template id</param>
		/// <returns>The page</returns>
		public static PageTemplate GetSingle(Guid id) {
			if (id != Guid.Empty) {
				if (!Application.Current.CacheProvider.Contains(id.ToString()))
					Application.Current.CacheProvider[id.ToString()] = PageTemplate.GetSingle((object)id);
				return (PageTemplate)Application.Current.CacheProvider[id.ToString()];
			}
			return null;
		}

		/// <summary>
		/// Invalidate the cache for the given record.
		/// </summary>
		/// <param name="record">The record.</param>
		public void InvalidateRecord(PageTemplate record) {
			Application.Current.CacheProvider.Remove(record.Id.ToString());
            Application.Current.CacheProvider.Remove("RT_" + record.Id);
		}
	}
}
