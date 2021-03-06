﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.GlobalHelper;
using CMS.OnlineMarketing;
using CMS.SettingsProvider;
using CMS.SiteProvider;
using CMS.UIControls;

public partial class CMSModules_ContactManagement_FormControls_ContactGroupDialog : CMSModalPage
{
    #region "Variables"

    private int siteId = -1;
    protected Hashtable mParameters;
    protected bool allowGlobalGroups = false;
    private bool isSitemanager = false;

    #endregion


    #region "Properties"

    /// <summary>
    /// Stop processing flag.
    /// </summary>
    public bool StopProcessing
    {
        get
        {
            return gridElem.StopProcessing;
        }
        set
        {
            gridElem.StopProcessing = value;
        }
    }


    /// <summary>
    /// Hashtable containing dialog parameters.
    /// </summary>
    private Hashtable Parameters
    {
        get
        {
            if (mParameters == null)
            {
                string identificator = QueryHelper.GetString("params", null);
                mParameters = (Hashtable)WindowHelper.GetItem(identificator);
            }
            return mParameters;
        }
    }

    #endregion


    #region "Methods"

    protected void Page_Load(object sender, EventArgs e)
    {
        CurrentMaster.Title.TitleText = GetString("om.contactgroup.select");
        CurrentMaster.Title.TitleImage = GetImageUrl("Objects/OM_ContactGroup/object.png");
        Page.Title = CurrentMaster.Title.TitleText;

        if (!QueryHelper.ValidateHash("hash") || Parameters == null)
        {
            StopProcessing = true;
            return;
        }

        siteId = ValidationHelper.GetInteger(Parameters["siteid"], 0);
        isSitemanager = ValidationHelper.GetBoolean(Parameters["issitemanager"], false);

        // Check permission
        if (ContactGroupHelper.AuthorizedReadContactGroup(siteId, true, isSitemanager))
        {
            if (siteId > 0)
            {
                gridElem.WhereCondition = "ContactGroupSiteID = " + siteId;
            }
            else
            {
                gridElem.WhereCondition = "ContactGroupSiteID IS NULL";
            }

            gridElem.OnExternalDataBound += gridElem_OnExternalDataBound;
            gridElem.OnBeforeDataReload += gridElem_OnBeforeDataReload;
        }
    }


    protected void gridElem_OnBeforeDataReload()
    {
        if (!gridElem.StopProcessing)
        {
            // Hide the last column if it is not necessary
            gridElem.NamedColumns["isglobal"].Visible = allowGlobalGroups;
        }
    }


    /// <summary>
    /// Unigrid external databound handler.
    /// </summary>
    protected object gridElem_OnExternalDataBound(object sender, string sourceName, object parameter)
    {
        switch (sourceName)
        {
            case "contactgroupdisplayname":
                LinkButton btn = new LinkButton();
                DataRowView drv = parameter as DataRowView;
                btn.Text = HTMLHelper.HTMLEncode(ValidationHelper.GetString(drv["ContactGroupDisplayName"], null));
                btn.Click += new EventHandler(btn_Click);
                btn.CommandArgument = ValidationHelper.GetString(drv["ContactGroupID"], null);
                btn.ToolTip = HTMLHelper.HTMLEncode(ValidationHelper.GetString(drv.Row["ContactGroupDescription"], null));
                return btn;

            case "isglobal":
                return (parameter is System.DBNull) ? UniGridFunctions.ColorLessSpanYesNo(true) : string.Empty;
        }
        return parameter;
    }


    /// <summary>
    /// Contact group selected event handler.
    /// </summary>
    protected void btn_Click(object sender, EventArgs e)
    {
        int groupId = ValidationHelper.GetInteger(((LinkButton)sender).CommandArgument, 0);
        string script = ScriptHelper.GetScript(@"
wopener.SelectValue_" + ValidationHelper.GetString(Parameters["clientid"], string.Empty) + @"(" + groupId + @");
window.close();
");

        ScriptHelper.RegisterStartupScript(this.Page, typeof(string), "CloseWindow", script);
    }

    #endregion
}
