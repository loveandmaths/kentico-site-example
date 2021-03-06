CREATE TABLE [CMS_RoleUIElement] (
		[RoleID]        [int] NOT NULL,
		[ElementID]     [int] NOT NULL
) ON [PRIMARY]
ALTER TABLE [CMS_RoleUIElement]
	ADD
	CONSTRAINT [PK_CMS_RoleUIElement]
	PRIMARY KEY
	CLUSTERED
	([RoleID], [ElementID])
	WITH FILLFACTOR=80
	ON [PRIMARY]
ALTER TABLE [CMS_RoleUIElement]
	WITH CHECK
	ADD CONSTRAINT [FK_CMS_RoleUIElement_ElementID_CMS_UIElement]
	FOREIGN KEY ([ElementID]) REFERENCES [CMS_UIElement] ([ElementID])
ALTER TABLE [CMS_RoleUIElement]
	CHECK CONSTRAINT [FK_CMS_RoleUIElement_ElementID_CMS_UIElement]
ALTER TABLE [CMS_RoleUIElement]
	WITH CHECK
	ADD CONSTRAINT [FK_CMS_RoleUIElement_RoleID_CMS_Role]
	FOREIGN KEY ([RoleID]) REFERENCES [CMS_Role] ([RoleID])
ALTER TABLE [CMS_RoleUIElement]
	CHECK CONSTRAINT [FK_CMS_RoleUIElement_RoleID_CMS_Role]
