CREATE PROCEDURE [dbo].[FileMeta_Create]
	@Name varchar(100),
	@Path varchar(50)
AS
BEGIN
	INSERT INTO dbo.FileMeta(Name, Path)
	VALUES (@Name, @Path); 
END
