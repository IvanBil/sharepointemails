﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.SharePoint;

namespace SharePointEmails.Core.Associations
{
    [Serializable]
    [XmlInclude(typeof(GroupAssociation))]
    [XmlInclude(typeof(IDAssociation))]
    public abstract class Association
    {
        public string ID { set; get; }
        public string Name { set; get; }
        public string Description { set; get; }

        public Association()
        {
            ID = Guid.NewGuid().ToString();
            Name = "Ass" + ID;
        }
        public abstract int IsMatch(SPList list, SPContentTypeId ctId, int ItemId);
        public abstract AssType Type { get; }
    }

    public enum AssType
    {
        Group = 1, ID = 2//don't change ids
    }
}
