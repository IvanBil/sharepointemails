﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Xml.Linq;

namespace SharePointEmails.Core
{
    public  class SearchContext:ISearchContext
    {

        public static ISearchContext Create(SPList list, int itemId, string eventData, SPEventType type)
        {
            return new SearchContext(list,itemId,eventData,type);
        }

        TemplateTypeEnum get(SPEventType type)
        {
            switch (type)
            {
                case SPEventType.All: return TemplateTypeEnum.AllItemEvents;
                case SPEventType.Add: return TemplateTypeEnum.ItemAdded;
                case SPEventType.Delete: return TemplateTypeEnum.ItemRemoved;
                case SPEventType.Modify: return TemplateTypeEnum.ItemUpdated;
            }
            return TemplateTypeEnum.AllItemEvents;
        }

        SearchContext(SPList list, int itemId, string eventData, SPEventType type)
        {
            List = list;
            Type = get(type);
            ItemId = itemId;
            try
            {
                var item = list.GetItemById(itemId);
                ItemContentTypeId = item.ContentTypeId;
            }
            catch
            {
                ItemContentTypeId = new SPContentTypeId(XDocument.Parse(eventData).Root.Elements()
                    .Where(p =>p.Attributes("Name")!=null&& p.Attribute("Name").Value == "ContentTypeId").First().Attributes()
                    .Where(p=>p.Name=="Old"||p.Name=="New").First().Value);
            }
        }

        SPList List { set; get; }

        public SPContentTypeId ItemContentTypeId { set; get; }

        public int ItemId { set; get; }

        public TemplateTypeEnum Type { set; get; }

        int CheckAsses(ITemplate template)
        {
            int res = SearchMatchLevel.NONE;
            foreach (var ass in template.Asses)
            {
                var m = ass.IsMatch(List, ItemContentTypeId, ItemId);
                if (m > res) res = m;
            }
            return res;
        }

        public int Match(ITemplate template)
        {
            if (template.State == TemplateStateEnum.Draft) return SearchMatchLevel.NONE;
            if ((Contains((int)Type, template.EventTypes)) || (Contains(template.EventTypes, Type)))
            {
                return CheckAsses(template);
            }
            else
            {
                return SearchMatchLevel.NONE;
            }
        }

        public bool Contains(int parent,int type)
        {
            return (((int)parent & type) != 0);
        }
        public bool Contains(int parent, TemplateTypeEnum type)
        {
            return (((int)parent & (int)type) != 0);
        }
        public bool Contains(TemplateTypeEnum parent, TemplateTypeEnum type)
        {
            return Contains((int)parent,(int)type);
        }

        public Guid SiteId
        {
            get
            {
                return List.ParentWeb.Site.ID;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Guid WebId
        {
            get
            {
                return List.ParentWeb.ID;
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public SPSite Site
        {
            get { return List.ParentWeb.Site; }
        }
    }
}
