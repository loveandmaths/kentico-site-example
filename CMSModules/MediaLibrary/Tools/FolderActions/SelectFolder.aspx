<%@ Page Language="C#" AutoEventWireup="true" Inherits="CMSModules_MediaLibrary_Tools_FolderActions_SelectFolder"
    EnableEventValidation="false" CodeFile="SelectFolder.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Frameset//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-frameset.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server" enableviewstate="false">
    <title>Copy / Move</title>
</head>
<frameset border="0" rows="*, 43" id="rowsFrameset">
    <frame name="selectFolderContent" src="SelectFolder_Content.aspx<%=Request.Url.Query%>"
        scrolling="no" frameborder="0" id="content" />
    <frame name="selectFolderFooter" src="SelectFolder_Footer.aspx<%=Request.Url.Query%>"
        scrolling="no" frameborder="0" noresize="noresize" id="footer" />
    <cms:NoFramesLiteral ID="ltlNoFrames" runat="server" />
</frameset>
</html>
