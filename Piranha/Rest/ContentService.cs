﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Web;

using Piranha.Data;
using Piranha.Rest.DataContracts;
using Piranha.Web;

namespace Piranha.Rest
{
	/// <summary>
	/// ReST API for content.
	/// </summary>
	[ServiceContract()]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class ContentService
	{
		/// <summary>
		/// Gets the content specified by the given id.
		/// </summary>
		/// <param name="id">The content id</param>
		/// <returns>The content</returns>
		[OperationContract()]
		[WebGet(UriTemplate="get/{id}", ResponseFormat=WebMessageFormat.Json)]
		public Content Get(string id = "") {
			if (!String.IsNullOrEmpty(id)) {
				return Get(new Guid(id)) ;
			}
			return null ;
		}

		/// <summary>
		/// Gets the content specified by the given id as xml.
		/// </summary>
		/// <param name="id">The content id</param>
		/// <returns>The content</returns>
		[OperationContract()]
		[WebGet(UriTemplate="get/xml/{id}", ResponseFormat=WebMessageFormat.Xml)]
		public Content GetXml(string id) {
			return Get(id) ;
		}

		/// <summary>
		/// Gets the published folders in a recursive structure.
		/// </summary>
		/// <returns>The media folders</returns>
		[WebGet(UriTemplate="getfolders", ResponseFormat=WebMessageFormat.Json)]
		public IList<MediaFolder> GetFolders() {
			using (var db = new DataContext()) {
				var media = db.Media
					.Where(m => m.IsFolder == true)
					.OrderBy(m => m.ParentId)
					.ThenBy(m => m.Name) ;
				return Sort(media) ;
			}
		}

		/// <summary>
		/// Gets the published folders in a recursive structure.
		/// </summary>
		/// <returns>The media folders</returns>
		[WebGet(UriTemplate="getfolders/xml", ResponseFormat=WebMessageFormat.Xml)]
		public IList<MediaFolder> GetFoldersXml() {
			return GetFolders() ;
		}

		/// <summary>
		/// Gets the content specified by the given id.
		/// </summary>
		/// <param name="id">The id</param>
		/// <param name="draft">Whether to get the draft or not</param>
		/// <returns>The content</returns>
		internal Content Get(Guid id, bool draft = false) {
			try {
				Models.Content c = Models.Content.GetSingle(id, draft) ;

				if (c != null) {
					return new Content() {
						Id = c.Id,
						ParentId = c.ParentId,
						Filename = c.Filename,
						Name = c.Name,
						DisplayName = c.DisplayName,
						Description = c.Description,
						Type = c.Type,
						Size = c.Size,
						ThumbnailUrl = WebPages.WebPiranha.ApplicationPath + 
							(!draft ? Application.Current.Handlers.GetUrlPrefix("THUMBNAIL") :
							Application.Current.Handlers.GetUrlPrefix("THUMBNAILDRAFT")) + "/" + c.Id,
						ContentUrl = WebPages.WebPiranha.ApplicationPath + 
							(!draft ? Application.Current.Handlers.GetUrlPrefix("CONTENT") :
							Application.Current.Handlers.GetUrlPrefix("CONTENTDRAFT")) + "/" + c.Id,
						Created = c.Created.ToString(),
						Updated = c.Updated.ToString()
					};
				}
			} catch {}
			return null ;
		}

		private IList<MediaFolder> Sort(IEnumerable<Entities.Media> media, Guid? parentId = null) {
			var folders = new List<MediaFolder>() ;

			foreach (var m in media) {
				if (m.ParentId == parentId) {
					folders.Add(new MediaFolder() {
						 Id = m.Id,
						 Name = m.Name,
						 Folders = Sort(media, m.Id)
					}) ;
				}
			}
			return folders ;
		}
	}
}
