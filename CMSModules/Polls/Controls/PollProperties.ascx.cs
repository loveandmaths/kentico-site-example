using System;

using CMS.GlobalHelper;
using CMS.Polls;
using CMS.UIControls;
using CMS.WebAnalytics;

public partial class CMSModules_Polls_Controls_PollProperties : CMSAdminEditControl
{
    #region "Variables"

    private int mSiteID = 0;
    private int mGroupID = 0;
    private PollInfo pollObj = null;

    #endregion


    #region "Properties"

    /// <summary>
    /// Gets or Sets site ID of the poll.
    /// </summary>
    public int SiteID
    {
        get
        {
            return mSiteID;
        }
        set
        {
            mSiteID = value;
        }
    }


    /// <summary>
    /// Gets or sets group ID.
    /// </summary>
    public int GroupID
    {
        get
        {
            return this.mGroupID;
        }
        set
        {
            mGroupID = value;
        }
    }

    #endregion


    #region "Methods"

    protected void Page_Load(object sender, EventArgs e)
    {
        // Hide code name editing in simple mode
        if (DisplayMode == ControlDisplayModeEnum.Simple)
        {
            plcCodeName.Visible = false;
        }

        // Required field validator error messages initialization
        rfvCodeName.ErrorMessage = GetString("general.requirescodename");
        rfvDisplayName.ErrorMessage = GetString("general.requiresdisplayname");
        rfvQuestion.ErrorMessage = GetString("Polls_New.QuestionError");
        rfvMaxLength.ErrorMessage = GetString("general.errortexttoolong");
        rfvMaxLengthResponse.ErrorMessage = GetString("general.errortexttoolong");

        // Set if it is live site
        txtDisplayName.IsLiveSite = txtTitle.IsLiveSite = txtQuestion.IsLiveSite = txtResponseMessage.IsLiveSite = this.IsLiveSite;

        if (!RequestHelper.IsPostBack())
        {
            // Show possible license limitation error relayed from new poll page
            string error = QueryHelper.GetText("error", null);
            if (!string.IsNullOrEmpty(error))
            {
                lblError.Text = error;
                lblError.Visible = true;
            }
        }

        if (pollObj == null)
        {
            pollObj = PollInfoProvider.GetPollInfo(this.ItemID);
            if ((this.ItemID > 0) && !IsLiveSite)
            {
                EditedObject = pollObj;
            }
        }

        if (pollObj != null)
        {
            // Fill editing form
            if (!RequestHelper.IsPostBack() && !IsLiveSite)
            {
                ReloadData();
            }
        }
    }


    /// <summary>
    /// Clears data.
    /// </summary>
    public override void ClearForm()
    {
        base.ClearForm();
        txtDisplayName.Text = null;
        txtCodeName.Text = null;
        txtTitle.Text = null;
        txtQuestion.Text = null;
        dtPickerOpenFrom.SelectedDateTime = DateTimeHelper.ZERO_TIME;
        dtPickerOpenTo.SelectedDateTime = DateTimeHelper.ZERO_TIME;
        txtResponseMessage.Text = null;
        chkAllowMultipleAnswers.Checked = false;
    }


    /// <summary>
    /// Reloads control with new data.
    /// </summary>
    public override void ReloadData()
    {
        this.ClearForm();
        pollObj = pollObj ?? PollInfoProvider.GetPollInfo(this.ItemID);

        if (pollObj == null)
        {
            return;
        }

        txtCodeName.Text = pollObj.PollCodeName;
        txtDisplayName.Text = pollObj.PollDisplayName;
        txtTitle.Text = pollObj.PollTitle;
        txtQuestion.Text = pollObj.PollQuestion;

        if (pollObj.PollOpenFrom != DataHelper.DATETIME_NOT_SELECTED)
        {
            dtPickerOpenFrom.SelectedDateTime = pollObj.PollOpenFrom;
        }

        if (pollObj.PollOpenTo != DataHelper.DATETIME_NOT_SELECTED)
        {
            dtPickerOpenTo.SelectedDateTime = pollObj.PollOpenTo;
        }

        txtResponseMessage.Text = pollObj.PollResponseMessage;
        chkAllowMultipleAnswers.Checked = pollObj.PollAllowMultipleAnswers;

        if (ActivitySettingsHelper.ActivitiesEnabledAndModuleLoaded(this.SiteID))
        {
            plcOnline.Visible = true;
            chkLogActivity.Checked = pollObj.PollLogActivity;
        }
    }


    /// <summary>
    /// Sets data to database.
    /// </summary>
    protected void btnOK_Click(object sender, EventArgs e)
    {
        if ((this.SiteID > 0) && !CheckPermissions("cms.polls", CMSAdminControl.PERMISSION_MODIFY))
        {
            return;
        }
        if ((this.SiteID <= 0) && !CheckPermissions("cms.polls", CMSAdminControl.PERMISSION_GLOBALMODIFY))
        {
            return;
        }

        // Get trimmed inputs
        string codeName = txtCodeName.Text.Trim();
        string displayName = txtDisplayName.Text.Trim();
        string question = txtQuestion.Text.Trim();

        // Validate the fields
        string errorMessage = new Validator()
            .NotEmpty(codeName, rfvCodeName.ErrorMessage)
            .NotEmpty(displayName, rfvDisplayName.ErrorMessage)
            .NotEmpty(question, rfvQuestion.ErrorMessage)
            .Result;

        if (!ValidationHelper.IsCodeName(codeName))
        {
            errorMessage = GetString("General.ErrorCodeNameInIdentificatorFormat");
        }
        else
        {
            // From/to date validation
            if (!dtPickerOpenFrom.IsValidRange() || !dtPickerOpenTo.IsValidRange())
            {
                errorMessage = GetString("general.errorinvaliddatetimerange");
            }

            if (string.IsNullOrEmpty(errorMessage) &&
                (!ValidationHelper.IsIntervalValid(dtPickerOpenFrom.SelectedDateTime, dtPickerOpenTo.SelectedDateTime, true)))
            {
                errorMessage = GetString("General.DateOverlaps");
            }
        }

        if (!string.IsNullOrEmpty(errorMessage))
        {
            // Error message - validation
            lblError.Visible = true;
            lblError.Text = errorMessage;
            return;
        }

        // Check uniqeness
        PollInfo secondPoll = null;
        if (this.SiteID <= 0)
        {
            // Try to find poll in global polls (leading period denotes global poll)
            secondPoll = PollInfoProvider.GetPollInfo("." + codeName, 0);
        }
        else
        {
            // Try to find poll in site polls
            secondPoll = PollInfoProvider.GetPollInfo(codeName, this.SiteID, this.GroupID);
        }
        if (secondPoll != null && (secondPoll.PollID != this.ItemID))
        {
            // Error message - Code name already exist
            lblError.Visible = true;
            lblError.Text = GetString("polls.codenameexists");
            return;
        }

        pollObj = pollObj ?? PollInfoProvider.GetPollInfo(this.ItemID);
        if (pollObj == null)
        {
            // Error message - Poll does not exist
            lblError.Visible = true;
            lblError.Text = GetString("polls.pollnotexist");
            return;
        }

        if (pollObj.PollSiteID != this.SiteID)
        {
            throw new Exception("[PollProperties.ascx]: Wrong poll object received since SiteID parameter wasn't provided.");
        }

        // Store the fields
        pollObj.PollCodeName = codeName;
        pollObj.PollDisplayName = displayName;
        pollObj.PollTitle = txtTitle.Text.Trim();
        pollObj.PollQuestion = question;
        pollObj.PollOpenFrom = dtPickerOpenFrom.SelectedDateTime;
        pollObj.PollOpenTo = dtPickerOpenTo.SelectedDateTime;
        pollObj.PollResponseMessage = txtResponseMessage.Text.Trim();
        pollObj.PollAllowMultipleAnswers = chkAllowMultipleAnswers.Checked;

        if (plcOnline.Visible)
        {
            pollObj.PollLogActivity = chkLogActivity.Checked;
        }
        // Save the data
        try
        {
            PollInfoProvider.SetPollInfo(pollObj);
        }
        catch (Exception ex)
        {
            lblError.Visible = true;
            lblError.Text = ex.Message;
            return;
        }

        lblInfo.Visible = true;
        lblInfo.Text = GetString("General.ChangesSaved");

        // Raise on saved event
        RaiseOnSaved();
    }

    #endregion
}