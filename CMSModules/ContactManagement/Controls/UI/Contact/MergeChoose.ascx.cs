﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections;
using System.Text;

using CMS.UIControls;
using CMS.OnlineMarketing;
using CMS.CMSHelper;
using CMS.SettingsProvider;
using CMS.GlobalHelper;

public partial class CMSModules_ContactManagement_Controls_UI_Contact_MergeChoose : CMSAdminListControl
{
    #region "Variables and constants"

    /// <summary>
    /// URL of collision dialog.
    /// </summary>
    private const string MERGE_DIALOG = "~/CMSModules/ContactManagement/Pages/Tools/Contact/CollisionDialog.aspx";

    private ContactInfo ci = null;

    #endregion


    #region "Properties"

    /// <summary>
    /// Indicates if the control should perform the operations.
    /// </summary>
    public override bool StopProcessing
    {
        get
        {
            return base.StopProcessing;
        }
        set
        {
            base.StopProcessing = value;
            this.gridElem.StopProcessing = value;
        }
    }


    /// <summary>
    /// Indicates if the control is used on the live site.
    /// </summary>
    public override bool IsLiveSite
    {
        get
        {
            return base.IsLiveSite;
        }
        set
        {
            base.IsLiveSite = value;
            gridElem.IsLiveSite = value;
            filter.IsLiveSite = value;
        }
    }


    /// <summary>
    /// Current contact.
    /// </summary>
    public ContactInfo Contact
    {
        get
        {
            if (ci == null)
            {
                if (CMSPage.EditedObject != null)
                {
                    ci = (ContactInfo)CMSPage.EditedObject;
                }
            }
            return ci;
        }
    }


    /// <summary>
    /// Modal dialog identificator.
    /// </summary>
    private string Identificator
    {
        get
        {
            // Try to load data from control viewstate
            string identificator = ValidationHelper.GetString(hdnIdentificator.Value, String.Empty);
            if (string.IsNullOrEmpty(identificator))
            {
                // Create new Guid
                identificator = Guid.NewGuid().ToString();
                hdnIdentificator.Value = identificator;
            }

            return identificator;
        }
    }

    #endregion


    #region "Methods"

    protected void Page_Load(object sender, EventArgs e)
    {
        gridElem.OnExternalDataBound += new OnExternalDataBoundEventHandler(CMControlsHelper.UniGridOnExternalDataBound);
        if (this.Contact != null)
        {
            // Current contact is global object
            if (this.Contact.ContactSiteID == 0)
            {
                // Display site selector in site manager
                if (ContactHelper.IsSiteManager)
                {
                    filter.DisplaySiteSelector = true;
                }
                // Display 'site or global' selector in CMS desk for global objects
                else if (ContactHelper.AuthorizedReadContact(CMSContext.CurrentSiteID, false) && ContactHelper.AuthorizedModifyContact(CMSContext.CurrentSiteID,false))
                {
                    filter.DisplayGlobalOrSiteSelector = true;
                }
                filter.HideMergedIntoGlobal = true;
            }

            filter.SiteID = this.Contact.ContactSiteID;
            gridElem.WhereCondition = filter.WhereCondition;
            gridElem.WhereCondition = SqlHelperClass.AddWhereCondition(gridElem.WhereCondition, "ContactID <> " + this.Contact.ContactID);
            gridElem.ZeroRowsText = GetString("om.contact.nocontacts");
            btnMergeSelected.Click += new EventHandler(btnMerge_Click);
            btnMergeAll.Click += new EventHandler(btnMergeAll_Click);

            if (QueryHelper.GetBoolean("saved", false))
            {
                lblInfo.Visible = true;
            }
        }
        else
        {
            this.StopProcessing = true;
            this.Visible = false;
        }
    }


    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);
        pnlButton.Visible = !DataHelper.DataSourceIsEmpty(gridElem.GridView.DataSource);
        gridElem.NamedColumns["sitename"].Visible = ((filter.SelectedSiteID < 0) && (filter.SelectedSiteID != UniSelector.US_GLOBAL_RECORD));
    }


    /// <summary>
    /// Button merge selected click.
    /// </summary>
    void btnMerge_Click(object sender, EventArgs e)
    {
        if (ContactHelper.AuthorizedModifyContact(this.Contact.ContactSiteID, true))
        {
            if (gridElem.SelectedItems.Count > 0)
            {
                SetDialogParameters(false);
                OpenWindow();
            }
            else
            {
                lblError.Visible = true;
            }
        }
    }


    /// <summary>
    /// Button merge all click.
    /// </summary>
    void btnMergeAll_Click(object sender, EventArgs e)
    {
        if (ContactHelper.AuthorizedModifyContact(this.Contact.ContactSiteID, true))
        {
            if (!DataHelper.DataSourceIsEmpty(gridElem.GridView.DataSource))
            {
                SetDialogParameters(true);
                OpenWindow();
            }
            else
            {
                lblError.Visible = true;
            }
        }
    }


    /// <summary>
    /// Sets the dialog parameters to the context.
    /// </summary>
    private void SetDialogParameters(bool mergeAll)
    {
        Hashtable parameters = new Hashtable();
        DataSet ds;

        if (mergeAll)
        {
            ds = new ContactListInfo().Generalized.GetData(null, gridElem.WhereCondition, null, -1, null, false);
        }
        else
        {
            string[] array = new string[gridElem.SelectedItems.Count];
            gridElem.SelectedItems.CopyTo(array);
            ds = new ContactListInfo().Generalized.GetData(null, SqlHelperClass.GetWhereCondition("ContactID", array), null, -1, null, false);
        }

        parameters["MergedContacts"] = ds;
        parameters["ParentContact"] = this.Contact;
        parameters["issitemanager"] = ContactHelper.IsSiteManager;
        WindowHelper.Add(Identificator, parameters);
    }


    /// <summary>
    /// Registers JS for opening window.
    /// </summary>
    private void OpenWindow()
    {
        ScriptHelper.RegisterDialogScript(this.Page);

        string url = MERGE_DIALOG + "?params=" + Identificator;
        url += "&hash=" + QueryHelper.GetHash(url, false);

        StringBuilder script = new StringBuilder();
        script.Append(@"modalDialog('" + ResolveUrl(url) + @"', 'mergeDialog', 700, 700, null, null, true);");

        ScriptHelper.RegisterStartupScript(this, typeof(string), "MergeDialog" + ClientID, ScriptHelper.GetScript(script.ToString()));
        ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "RefreshPageScript", ScriptHelper.GetScript("function RefreshPage() { window.location.replace('" + URLHelper.AddParameterToUrl(URLHelper.CurrentURL, "saved", "true") + "'); }"));
    }

    #endregion
}