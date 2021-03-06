<%@ Page Language="C#" AutoEventWireup="true" Inherits="CMSModules_MediaLibrary_Controls_MediaLibrary_FileList_FileUpload"
    Theme="Default" CodeFile="FileUpload.aspx.cs" %>

<%@ Register Src="~/CMSModules/MediaLibrary/Controls/MediaLibrary/FileUpload.ascx"
    TagName="LibraryFileUpload" TagPrefix="cms" %>
<%@ Register Src="~/CMSAdminControls/UI/System/PermissionMessage.ascx" TagName="PermissionMessage"
    TagPrefix="cms" %>
    
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server" enableviewstate="false">
    <title>Media library - New media file</title>
    <base target="_self" />
    <style type="text/css">
        body
        {
            margin: 0px;
            padding: 0px;
            height: 100%;
        }
    </style>
</head>
<body class="<%=mBodyClass%>">
    <form id="form1" runat="server">
        <cms:PermissionMessage ID="messageElem" runat="server" EnableViewState="false" DisplayMessage="false" />
        <cms:LibraryFileUpload ID="fileUpload" runat="server" />
    </form>
</body>
</html>
