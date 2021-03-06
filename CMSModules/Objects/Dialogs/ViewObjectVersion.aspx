<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ViewObjectVersion.aspx.cs"
    Inherits="CMSModules_Objects_Dialogs_ViewObjectVersion" Theme="Default" MasterPageFile="~/CMSMasterPages/UI/Dialogs/ModalDialogPage.master"
    Title="View Object Version" %>

<%@ Register Src="~/CMSModules/Objects/Controls/ViewObjectVersion.ascx" TagName="ViewObjectVersion"
    TagPrefix="cms" %>
<asp:Content ID="cntBody" runat="server" ContentPlaceHolderID="plcContent">
    <cms:CMSUpdatePanel ID="pnlUpdate" runat="server">
        <ContentTemplate>
            <cms:ViewObjectVersion ID="viewVersion" runat="server" />
        </ContentTemplate>
    </cms:CMSUpdatePanel>
</asp:Content>
<asp:Content ID="cntFooter" ContentPlaceHolderID="plcFooter" runat="server">
    <div class="FloatRight">
        <cms:CMSButton ID="btnClose" runat="server" CssClass="SubmitButton" OnClientClick="window.close(); return false;" />
    </div>
</asp:Content>
