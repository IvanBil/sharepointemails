﻿using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Generic;
using SharePointEmails.Core;
using Microsoft.SharePoint;

namespace SharepointEmails.ControlTemplates.SharepointEmails
{
    public partial class AddAssociation : UserControl
    {

        List<string> types = new List<string>() {  "By Id","By type"};
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                cb_Types.DataSource = types;
                cb_Types.DataBind();
                cb_ItemTypes.DataSource = Enum.GetNames(typeof(ItemType));
                cb_ItemTypes.DataBind();
            }
        }

        protected void cb_Types_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_Types.SelectedIndex < MultiView1.Views.Count)
            {
                MultiView1.SetActiveView(MultiView1.Views[cb_Types.SelectedIndex]);
            }
        }

        protected void btn_Add_Click(object sender, EventArgs e)
        {
            Association ass = null;
            if (cb_Types.SelectedIndex == 0)
            {
                ass = new TypeAssociation()
                {
                    Name = tb_Name.Text ?? "no name",
                    ItemType = (ItemType)Enum.Parse(typeof(ItemType), cb_ItemTypes.SelectedItem.Value),
                };
            }
            else if (cb_Types.SelectedIndex == 1)
            {
                ass = new IDAssociation()
                {
                    Name = tb_Name.Text ?? "no name",
                    ID = Guid.Empty
                };
            }
            var manager = ClassContainer.Instance.Resolve<ConfigurationManager>();
            if (manager != null)
            {
                manager.AddAssociation(SPContext.Current.Web, ass);
            }
        }
    }
}
