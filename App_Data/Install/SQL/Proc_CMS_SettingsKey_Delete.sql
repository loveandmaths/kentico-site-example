CREATE PROCEDURE [Proc_CMS_SettingsKey_Delete]
	@ID int	
AS
BEGIN
	SET NOCOUNT ON;   
	
	DELETE FROM CMS_SettingsKey WHERE KeyID = @ID
END
