<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/CMSMasterPages/UI/SimplePage.master"
    Title="Mvtest properties" Inherits="CMSModules_OnlineMarketing_Pages_Content_MVTest_Edit"
    Theme="Default" CodeFile="Edit.aspx.cs" %>

<%@ Register Src="~/CMSModules/OnlineMarketing/Controls/UI/Mvtest/Edit.ascx" TagName="MvtestEdit"
    TagPrefix="cms" %>
<asp:Content ID="cntHeader" runat="server" ContentPlaceHolderID="plcBeforeContent">
    <asp:Panel runat="server" ID="pnlDisabled" CssClass="PageHeaderLine" Visible="false">
        <asp:Label runat="server" ID="lblDisabled" EnableViewState="false" />
        <asp:Label runat="server" ID="lblMVTestingDisabled" EnableViewState="false" />
    </asp:Panel>
</asp:Content>
<asp:Content ID="cntBody" runat="server" ContentPlaceHolderID="plcContent">
    <cms:MvtestEdit ID="editElem" runat="server" IsLiveSite="false" />
</asp:Content>
