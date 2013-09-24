﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Mvc;
using Coevery.Core.Admin;
using Coevery.FormDesigner.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Themes;

namespace Coevery.FormDesigner.Controllers {
    public class SystemAdminController : Controller {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public SystemAdminController(
            IOrchardServices orchardServices,
            IContentManager contentManager, IContentDefinitionManager contentDefinitionManager) {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            Services = orchardServices;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index(string id, string returnUrl) {
            if (string.IsNullOrEmpty(id)) {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var contentItem = _contentManager.New(id);
            dynamic model = _contentManager.BuildEditor(contentItem);
            var contentTypeDefinition = contentItem.TypeDefinition;
            string layout = contentTypeDefinition.Settings.ContainsKey("Layout")
                ? contentTypeDefinition.Settings["Layout"]
                : string.Empty;
            layout = string.IsNullOrEmpty(layout)
                ? "<fd-section section-columns=\"1\" section-columns-width=\"6:6\" section-title=\"Sample Title\"><fd-row><fd-column></fd-column></fd-row></fd-section>"
                : layout;
            var viewModel = Services.New.ViewModel();
            viewModel.Layout = layout;
            viewModel.DisplayName = contentItem.TypeDefinition.DisplayName;
            var templates = new List<dynamic>();
            var fields = _contentDefinitionManager.GetPartDefinition(id).Fields
                .Select(x => new FieldViewModel {DisplayName = x.DisplayName, Name = x.Name})
                .ToList();
            foreach (var item in model.Content.Items) {
                if (item.TemplateName != null && item.TemplateName.StartsWith("Fields/")) {
                    templates.Add(item);
                }
                else if (item.TemplateName == "Parts/Relationship.Edit") {
                    templates.Add(item);
                    var name = item.ContentPart.GetType().Name;
                    fields.Add(new FieldViewModel {
                        Name = name,
                        DisplayName = _contentDefinitionManager.GetPartDefinition(name).Settings["DisplayName"]
                    });
                }
            }
            viewModel.Templates = templates;
            viewModel.Fields = fields;

            return View((object) viewModel);
        }
    }
}