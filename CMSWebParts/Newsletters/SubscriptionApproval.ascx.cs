using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.PortalControls;
using CMS.GlobalHelper;

public partial class CMSWebParts_Newsletters_SubscriptionApproval : CMSAbstractWebPart
{
    #region "Public Properties"

    /// <summary>
    /// Gets or sets message which will be displayed when subscription was successful.
    /// </summary>
    public string SuccessfulApprovalText
    {
        get
        {
            return ValidationHelper.GetString(GetValue("SuccessfulApprovalText"), null); ;
        }
        set
        {
            SetValue("SuccessfulApprovalText", value);
        }
    }


    /// <summary>
    /// Gets or sets message which will be displayed when subscription was not successful.
    /// </summary>
    public string UnsuccessfulApprovalText
    {
        get
        {
            return ValidationHelper.GetString(GetValue("UnsuccessfulApprovalText"), null); ;
        }
        set
        {
            SetValue("UnsuccessfulApprovalText", value);
        }
    }

    #endregion


    #region "Methods"

    /// <summary>
    /// Content loaded event handler.
    /// </summary>
    public override void OnContentLoaded()
    {
        base.OnContentLoaded();
        SetupControl();
    }


    /// <summary>
    /// Initializes the control properties.
    /// </summary>
    protected void SetupControl()
    {
        if (this.StopProcessing)
        {
            // Stop processing
            this.subscriptionApproval.StopProcessing = true;
        }
        else
        {
            Guid subscriptionGuid = QueryHelper.GetGuid("subscriptionguid", Guid.Empty);

            if (subscriptionGuid != Guid.Empty)
            {
                subscriptionApproval.SuccessfulApprovalText = this.SuccessfulApprovalText;
                subscriptionApproval.UnsuccessfulApprovalText = this.UnsuccessfulApprovalText;
            }
            else
            {
                this.Visible = false;
            }
        }
    }

    #endregion
}
