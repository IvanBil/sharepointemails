﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Xml.Linq;
using SharePointEmails.Core.Substitutions;
using System.Reflection;
using SharePointEmails.Logging;
using System.Globalization;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Administration;
using System.Configuration;
using SharePointEmails.Core.Interfaces;

namespace SharePointEmails.Core.Substitutions
{
    public class SubstitutionContext : ISubstitutionContext
    {
        ContextVars Vars;
        string m_eventData = string.Empty;
        SPList m_sourceList=null;
        string m_modifierName = string.Empty;
        ILogger Logger;
        SPEventType m_eventType = SPEventType.All;

        Guid eventID;

        public List<FieldChange> Changes {private set; get; }

        public SubstitutionContext(string eventData):this(eventData,null){}

        public SubstitutionContext(string eventData, SPList sourceList):this(eventData,sourceList,-1,null,null,-1,SPEventType.All){}

        public SubstitutionContext(string eventData, SPList sourceList, int itemId, string modifierName, string receiverEmail, int alertCreatorId, SPEventType eventType)
            : this(Guid.NewGuid(), eventData, sourceList, -1, null, null, -1, SPEventType.All)
        {
        }

        public SubstitutionContext(Guid eventID,string eventData, SPList sourceList, int itemId, string modifierName, string receiverEmail, int alertCreatorId, SPEventType eventType)
        {
            Logger = Application.Current.Logger;
            m_eventType = eventType;
            m_sourceList = sourceList;
            m_modifierName = modifierName;
            this.eventID = eventID;
            Vars = new ContextVars(sourceList, itemId, modifierName, receiverEmail, alertCreatorId);
            Changes = (!string.IsNullOrEmpty(eventData)) ? XDocument.Parse(eventData).Descendants("Field").Select(p => FieldChange.Create(p)).ToList() : new List<FieldChange>();
        }

        public string GetField(string fieldName, ModifiersCollection modifiers = null)
        {
            var change = Changes.Where(p => p.FieldDisplayName == fieldName || p.FieldName == fieldName).FirstOrDefault();
            return (change != null)?change.GetText(modifiers ?? ModifiersCollection.Empty):null;
        }

        public string GetContextValue(string value, ModifiersCollection modifiers)
        {
            return GetValueFromObjByPath(Vars, value);
        }

        static string ContextNameSpace = "urn:sharepointemail-context";
        static XNamespace nsp = XNamespace.Get(ContextNameSpace);
        public static XName L(string name)
        {
            return nsp + name;
        }
        public string GetXML()
        {
            XDocument res = new XDocument();
            
            var dataEl = new XElement(L("Data"));
            
            dataEl.SetAttributeValue("AdminEmail", GetAdminEmail(Vars.SList));
            dataEl.SetAttributeValue("EventID", eventID.ToString());
            res.Add(dataEl);
            var eventData = new XElement(L("EventData"));
            res.Root.Add(eventData);
            eventData.SetAttributeValue("EventType", (int)m_eventType);
            eventData.SetAttributeValue("EventTypeName", m_eventType.ToString());
            eventData.SetAttributeValue("ListEmail", GetListEmail(Vars.SList));
            eventData.SetAttributeValue("SUserName", m_modifierName);
            
            foreach (var change in Changes)
            {
                var el = new XElement(L("Field"));
                el.SetAttributeValue("Type", change.FieldType ?? string.Empty);
                el.SetAttributeValue("DisplayName", change.FieldDisplayName ?? change.FieldName ?? string.Empty);
                el.SetAttributeValue("Name", change.FieldName ?? string.Empty);
                el.SetAttributeValue("Changed", change.IsChanged);
                el.SetAttributeValue("New", (change.GetText(new ModifiersCollection { Modifier.New }) ?? string.Empty));
                el.SetAttributeValue("Old", (change.GetText(new ModifiersCollection { Modifier.Old }) ?? string.Empty));
                el.SetAttributeValue("Value", change.GetText(ModifiersCollection.Empty) ?? string.Empty);
                if (Vars.SList != null)
                {
                    if (Vars.SList.Fields.ContainsFieldWithStaticName(change.FieldName))
                    {
                        var hidden = Vars.SList.Fields.GetFieldByInternalName(change.FieldName).Hidden;
                        el.SetAttributeValue("Hidden", hidden);
                    }
                }
                eventData.Add(el);
            }

            AddApproveElement(eventData);
            AddDiscussionBoardElement(eventData);

            return res.ToString();
        }

        private void AddDiscussionBoardElement(XElement eventData)
        {
            var disc = DiscussionBoardXml.Create().GetElement(Vars.SItem);
            if (disc != null)
            {
                eventData.Add(disc);
            }
        }

        private void AddApproveElement(XElement eventData)
        {
            if (Vars.SItem != null && Vars.DUser != null)
            {
                var approve = new XElement(L("Approve"));
                var url = "/_layouts/approve.aspx?List=" + Vars.SList.ID + "&ID=" + Vars.SItem.ID;
                approve.SetAttributeValue("Enabled", true);
                approve.SetAttributeValue("RejectUrl", url);
                approve.SetAttributeValue("ApproveUrl", url);
                approve.SetAttributeValue("PageUrl", url);
                approve.SetAttributeValue("CanApprove", Vars.SItem.DoesUserHavePermissions(Vars.DUser, SPBasePermissions.ApproveItems));                
                approve.SetAttributeValue("Status", (Vars.SItem.ModerationInformation != null) ? Vars.SItem.ModerationInformation.Status.ToString() : "");
                eventData.Add(approve);
            }
            else
            {
                Logger.WriteTrace("Approve info not generated", SeverityEnum.Trace);
            }
        }

        public CultureInfo GetDestinationCulture()
        {
            return CultureInfo.CurrentCulture;
        }

        public SPDocumentLibrary GetTemplateLibrary()
        {
            if (Vars.SSite != null)
            {
                return Vars.SSite.RootWeb.Lists.TryGetList(Constants.XsltLibrary) as SPDocumentLibrary;
            }
            return null;
        }

        #region privates

        string GetListEmail(SPList list)
        {
            try
            {
                if (list != null)
                {
                    var alias = list.EmailAlias;
                    if (!string.IsNullOrEmpty(alias))
                    {
                        var service = SPFarm.Local.GetChild<SPIncomingEmailService>();
                        if (service != null)
                        {
                            var serverAdress = service.ServerDisplayAddress;
                            if (!string.IsNullOrEmpty(serverAdress))
                            {
                                return string.Format("{0}@{1}", alias, serverAdress);
                            }
                            else
                            {
                                Logger.WriteTrace("No email server address for list ", SeverityEnum.Warning);
                            }
                        }
                        else
                        {
                            Logger.WriteTrace("Cannot get incoming email service", SeverityEnum.Warning);
                        }
                    }
                    else
                    {
                        Logger.WriteTrace("No email alias for list ", SeverityEnum.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteTrace(ex, SeverityEnum.CriticalError);
            }
            return string.Empty;
        }

        string GetAdminEmail(SPList list)
        {
            try
            {
                if (list != null)
                {
                    var web = list.ParentWeb;
                    var adminEmail = web.SiteAdministrators.OfType<SPUser>().Where(p => !string.IsNullOrEmpty(p.Email)).Select(p => p.Email).FirstOrDefault();
                    if (!string.IsNullOrEmpty(adminEmail))
                    {
                        return adminEmail;
                    }
                    else
                    {
                        Logger.WriteTrace("No admins with emails", SeverityEnum.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteTrace(ex, SeverityEnum.CriticalError);
            }
            return string.Empty;
        }

        string GetValueFromObjByPath(object obj, string path)
        {
            object temp = obj;
            foreach (var m in path.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if ((temp = temp.GetType().InvokeMember(m, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty, null, temp, new object[0])) == null)
                    return "";
            }
            return (temp == null) ? "" : temp.ToString();
        }

        #endregion

        
    }

}
