﻿using System;
using System.Collections;
using System.Data;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;

using CMS.CMSHelper;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.GlobalHelper;
using CMS.LicenseProvider;
using CMS.OnlineMarketing;
using CMS.SettingsProvider;
using CMS.SiteProvider;
using CMS.UIControls;
using System.Collections.Generic;
using System.Data.SqlClient;

public partial class CMSModules_ContactManagement_Pages_Tools_Account_Delete : CMSContactManagementAccountsPage
{
    #region "Private variables"

    private IList<string> accountIds = null;
    private int accountSiteId = 0;
    private static readonly Hashtable mErrors = new Hashtable();
    private Hashtable mParameters = null;
    private string mReturnScript = null;
    private int mSiteID = 0;
    private bool issitemanager = false;
    private int numberOfDeletedAccounts = 0;
    private static int SQL_TIMEOUT = 72000;

    #endregion


    #region "Properties"

    /// <summary>
    /// Current log context.
    /// </summary>
    public LogContext CurrentLog
    {
        get
        {
            return EnsureLog();
        }
    }


    /// <summary>
    /// Current Error.
    /// </summary>
    private string CurrentError
    {
        get
        {
            return ValidationHelper.GetString(mErrors["DeleteError_" + ctlAsync.ProcessGUID], string.Empty);
        }
        set
        {
            mErrors["DeleteError_" + ctlAsync.ProcessGUID] = value;
        }
    }


    /// <summary>
    /// Where condition used for multiple actions.
    /// </summary>
    private string WhereCondition
    {
        get
        {
            string where = string.Empty;
            if (Parameters != null)
            {
                where = ValidationHelper.GetString(Parameters["where"], string.Empty);
            }
            return where;
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


    /// <summary>
    /// Returns script for returning back to list page.
    /// </summary>
    private string ReturnScript
    {
        get
        {
            if (string.IsNullOrEmpty(mReturnScript) && (Parameters != null))
            {
                mReturnScript = "document.location.href = 'List.aspx?siteid=" + SiteID + (issitemanager ? "&issitemanager=1" : string.Empty) + "';";
            }

            return mReturnScript;
        }
    }


    /// <summary>
    /// Returns script for returning back to list page with information that deleting proces has been started.
    /// </summary>
    private string ReturnScriptDeleteAsync
    {
        get
        {
            if (string.IsNullOrEmpty(mReturnScript) && (Parameters != null))
            {
                mReturnScript = "document.location.href = 'List.aspx?siteid=" + SiteID + (issitemanager ? "&issitemanager=1" : string.Empty) + "&deleteasync=1';";
            }

            return mReturnScript;
        }
    }


    /// <summary>
    /// Site ID retrieved from dialog parameters.
    /// </summary>
    public override int SiteID
    {
        get
        {
            if ((mSiteID == 0) && (Parameters != null))
            {
                mSiteID = ValidationHelper.GetInteger(Parameters["siteid"], 0);
            };
            return mSiteID;
        }
    }

    #endregion


    #region "Page events"

    protected void Page_Load(object sender, EventArgs e)
    {
        // Check hash validity
        if (QueryHelper.ValidateHash("hash"))
        {
            // Initialize events
            ctlAsync.OnFinished += ctlAsync_OnFinished;
            ctlAsync.OnError += ctlAsync_OnError;
            ctlAsync.OnRequestLog += ctlAsync_OnRequestLog;
            ctlAsync.OnCancel += ctlAsync_OnCancel;

            issitemanager = ValidationHelper.GetBoolean(Parameters["issitemanager"], false);

            if (!RequestHelper.IsCallback())
            {
                // Setup page title text and image
                CurrentMaster.Title.TitleText = GetString("om.account.deletetitle");
                CurrentMaster.Title.TitleImage = GetImageUrl("Objects/OM_Account/delete.png");

                btnCancel.Attributes.Add("onclick", ctlAsync.GetCancelScript(true) + "return false;");

                titleElemAsync.TitleText = GetString("om.account.deleting");
                titleElemAsync.TitleImage = GetImageUrl("Objects/OM_Account/delete.png");

                // Set visibility of panels
                pnlContent.Visible = true;
                pnlLog.Visible = false;

                // Get names of the accounts that are to be deleted
                DataSet ds = AccountInfoProvider.GetAccounts(WhereCondition, "AccountName", 1000, "AccountID, AccountName, AccountSiteID");

                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    DataRowCollection rows = ds.Tables[0].Rows;

                    // Data set contains only one item
                    if (rows.Count == 1)
                    {
                        CurrentMaster.Title.TitleText += " \"" + HTMLHelper.HTMLEncode(ValidationHelper.GetString(DataHelper.GetDataRowValue(rows[0], "AccountName"), "N/A")) + "\"";
                        accountIds = new List<string>(1);
                        accountIds.Add(ValidationHelper.GetString(DataHelper.GetDataRowValue(rows[0], "AccountID"), string.Empty));
                        accountSiteId = ValidationHelper.GetInteger(DataHelper.GetDataRowValue(rows[0], "AccountSiteID"), 0);
                        numberOfDeletedAccounts = 1;
                    }
                    else if (rows.Count > 1)
                    {
                        // Modify title and question for multiple items
                        CurrentMaster.Title.TitleText = GetString("om.account.deletetitlemultiple");
                        lblQuestion.ResourceString = "om.account.deletemultiplequestion";

                        // Display list with names of deleted items
                        pnlAccountList.Visible = true;

                        string name = null;
                        StringBuilder builder = new StringBuilder();

                        for (int i = 0; i < rows.Count; i++)
                        {
                            name = ValidationHelper.GetString(DataHelper.GetDataRowValue(rows[i], "AccountName"), string.Empty);
                            builder.Append(HTMLHelper.HTMLEncode(name));
                            builder.Append("<br />");
                        }
                        // Display three dots after last record
                        if (rows.Count == 1000)
                        {
                            builder.Append("...");
                        }

                        lblAccounts.Text = builder.ToString();

                        accountSiteId = SiteID;
                        // Get all IDs of deleted items
                        ds = AccountInfoProvider.GetAccounts(WhereCondition, "AccountID", 0, "AccountID");
                        accountIds = SqlHelperClass.GetStringValues(ds.Tables[0], "AccountID");
                        numberOfDeletedAccounts = ds.Tables[0].Rows.Count;
                    }
                }
                else
                {
                    // Hide everything
                    pnlContent.Visible = false;
                }
            }
        }
        else
        {
            pnlDelete.Visible = false;
            lblError.Text = GetString("dialogs.badhashtext");
        }
    }


    protected override void OnPreRender(EventArgs e)
    {
        // Set visibility of controls
        lblError.Visible = (!string.IsNullOrEmpty(lblError.Text));
        brSeparator.Visible = pnlAccountList.Visible;

        btnNo.OnClientClick = ReturnScript + "return false;";

        base.OnPreRender(e);
    }

    #endregion


    #region "Button actions"

    protected void btnOK_Click(object sender, EventArgs e)
    {
        // Check permissions
        if (AccountHelper.AuthorizedModifyAccount(accountSiteId, true, issitemanager))
        {
            EnsureAsyncLog();
            RunAsyncDelete();
        }
    }

    #endregion


    #region "Methods"

    /// <summary>
    /// Delete contacts on SQL server.
    /// </summary>
    private void DeleteOnSql()
    {
        // Start deleting contacts
        AccountInfoProvider.DeleteAccountInfos(WhereCondition, chkBranches.Checked);

        // Return to the list page with info label displayed
        ltlScript.Text += ScriptHelper.GetScript(this.ReturnScriptDeleteAsync);
    }


    /// <summary>
    /// Ensures log for asynchronous control
    /// </summary>
    private void EnsureAsyncLog()
    {
        pnlLog.Visible = true;
        pnlContent.Visible = false;

        CurrentError = string.Empty;
        CurrentLog.Close();
        EnsureLog();
    }


    /// <summary>
    /// Starts asycnhronous deleting of contacts.
    /// </summary>
    private void RunAsyncDelete()
    {
        // Run the async method
        ctlAsync.Parameter = ReturnScript;
        ctlAsync.RunAsync(Delete, WindowsIdentity.GetCurrent());
    }


    /// <summary>
    /// Deletes document(s).
    /// </summary>
    private void Delete(object parameter)
    {
        if (parameter == null || accountIds.Count < 1)
        {
            return;
        }

        try
        {
            // Begin log
            AddLog(GetString("om.account.deleting"));
            AddLog(string.Empty);

            // When deleting children and not removing relations then we can run
            if (chkChildren.Checked && (numberOfDeletedAccounts > 1))
            {
                DeleteOnSql();
            }
            else
            {
                DeleteItems();
            }
        }
        catch (ThreadAbortException ex)
        {
            string state = ValidationHelper.GetString(ex.ExceptionState, string.Empty);
            if (state == CMSThread.ABORT_REASON_STOP)
            {
                // When canceled
                AddError(GetString("om.deletioncanceled"));
            }
            else
            {
                // Log error
                LogExceptionToEventLog(ex);
            }
        }
        catch (Exception ex)
        {
            // Log error
            LogExceptionToEventLog(ex);
        }
    }


    /// <summary>
    /// Delete items.
    /// </summary>
    private void DeleteItems()
    {
        // Set long timeout so that mass delete can finish successfully
        SqlConnectionStringBuilder connectionString = new SqlConnectionStringBuilder(ConnectionHelper.GetConnection().DataConnection.ConnectionString);
        connectionString.ConnectTimeout = SQL_TIMEOUT;
        using (var cs = new CMSConnectionScope(connectionString.ToString(), true))
        {
            // Delete the accounts
            AccountInfo ai = null;
            foreach (string accountId in accountIds)
            {
                ai = AccountInfoProvider.GetAccountInfo(ValidationHelper.GetInteger(accountId, 0));
                if (ai != null)
                {
                    // Display name of deleted account
                    AddLog(ai.AccountName);

                    // Delete account with its dependencies
                    AccountHelper.Delete(ai, chkChildren.Checked, chkBranches.Checked);
                }
            }
        }
    }

    #endregion


    #region "Async methods"

    private void ctlAsync_OnCancel(object sender, EventArgs e)
    {
        ctlAsync.Parameter = null;
        AddError(GetString("om.deletioncanceled"));
        ltlScript.Text += ScriptHelper.GetScript("var __pendingCallbacks = new Array();RefreshCurrent();");
        lblError.Text = CurrentError;
        CurrentLog.Close();
    }


    private void ctlAsync_OnRequestLog(object sender, EventArgs e)
    {
        ctlAsync.Log = CurrentLog.Log;
    }


    private void ctlAsync_OnError(object sender, EventArgs e)
    {
        if (ctlAsync.Status == AsyncWorkerStatusEnum.Running)
        {
            ctlAsync.Stop();
        }
        ctlAsync.Parameter = null;
        lblError.Text = CurrentError;
        CurrentLog.Close();
    }


    private void ctlAsync_OnFinished(object sender, EventArgs e)
    {
        lblError.Text = CurrentError;
        CurrentLog.Close();

        if (!string.IsNullOrEmpty(CurrentError))
        {
            ctlAsync.Parameter = null;
            lblError.Text = CurrentError;
        }

        if (ctlAsync.Parameter != null)
        {
            // Return to the list page after successful deletion
            ltlScript.Text += ScriptHelper.GetScript(ctlAsync.Parameter.ToString());

            // Do not set the window title anymore
            CurrentMaster.Title.SetWindowTitle = false;
        }
    }


    /// <summary>
    /// Ensures the logging context.
    /// </summary>
    protected LogContext EnsureLog()
    {
        LogContext log = LogContext.EnsureLog(ctlAsync.ProcessGUID);
        log.Reversed = true;
        log.LineSeparator = "<br />";
        return log;
    }


    /// <summary>
    /// Adds the log information.
    /// </summary>
    /// <param name="newLog">New log information</param>
    protected void AddLog(string newLog)
    {
        EnsureLog();
        LogContext.AppendLine(newLog);
    }


    /// <summary>
    /// Adds the error to collection of errors.
    /// </summary>
    /// <param name="error">Error message</param>
    protected void AddError(string error)
    {
        AddLog(error);
        CurrentError = (error + "<br />" + CurrentError);
    }


    /// <summary>
    /// When exception occures, log it to event log.
    /// </summary>
    /// <param name="ex">Exception to log</param>
    private void LogExceptionToEventLog(Exception ex)
    {
        EventLogProvider.LogException("Contact management", "DELETEACCOUNT", ex);
        AddError(GetString("om.account.deletefailed") + ": " + ex.Message);
    }

    #endregion
}