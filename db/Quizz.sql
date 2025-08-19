  --CREATE SCHEMA Quizz;
IF OBJECT_ID(N'Quizz.GetQuestionsByCategory',N'P') IS NULL
BEGIN
	EXEC sp_executesql N'CREATE PROCEDURE Quizz.GetQuestionsByCategory AS SELECT 1'
END
GO
 --  EXEC Quizz.GetQuestionsByCategory @CategoryId = 1
ALTER PROCEDURE Quizz.GetQuestionsByCategory
(
    @CategoryId INT
)
WITH ENCRYPTION
AS
BEGIN
SET NOCOUNT ON
    SELECT 
        q.QuestionId,
        q.QuestionText,
        o.OptionId,
        o.OptionText,
        q.CorrectOptionId,
        c.CategoryName
    FROM Questions q
    INNER JOIN Options o ON q.QuestionId = o.QuestionId
    INNER JOIN Categories c ON c.CategoryId = q.CategoryId
    WHERE q.CategoryId = @CategoryId
    ORDER BY q.QuestionId, o.OptionId;

SET NOCOUNT OFF
END

GO

IF OBJECT_ID(N'Quizz.MergeCategory', N'P') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE Quizz.MergeCategory AS SELECT 1')
END
GO

ALTER PROCEDURE Quizz.MergeCategory
    @CategoryId INT,              -- 0 = Insert, >0 = Update
    @CategoryName NVARCHAR(100),
    @Result NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if update requested
    IF @CategoryId > 0
    BEGIN
        -- If category not found
        IF NOT EXISTS (SELECT 1 FROM Quizz.Categories WHERE CategoryId = @CategoryId)
        BEGIN
            SET @Result = 'Category not found for update.'
            RETURN
        END

        -- If another category with same name exists
        IF EXISTS (
            SELECT 1 FROM Quizz.Categories 
            WHERE CategoryName = @CategoryName AND CategoryId <> @CategoryId
        )
        BEGIN
            SET @Result = 'Another category with the same name already exists.'
            RETURN
        END

        UPDATE Quizz.Categories
        SET CategoryName = @CategoryName
        WHERE CategoryId = @CategoryId;

        SET @Result = 'Category updated successfully.'
        RETURN
    END
    ELSE
    BEGIN
        -- Insert flow
        IF EXISTS (SELECT 1 FROM Quizz.Categories WHERE CategoryName = @CategoryName)
        BEGIN
            SET @Result = 'Category already exists.'
            RETURN
        END

        INSERT INTO Quizz.Categories (CategoryName)
        VALUES (@CategoryName);

        SET @Result = 'Category inserted successfully.'
        RETURN
    END
END
GO



IF OBJECT_ID(N'Quizz.FetchCategories', N'P') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE Quizz.FetchCategories AS SELECT 1')
END
GO

ALTER PROCEDURE Quizz.FetchCategories
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        CategoryId,
        CategoryName
    FROM Quizz.Categories
    ORDER BY CategoryName;

    SET NOCOUNT OFF;
END
GO


IF OBJECT_ID(N'Quizz.DeleteCategory', N'P') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE Quizz.DeleteCategory AS SELECT 1');
END
GO

ALTER PROCEDURE Quizz.DeleteCategory
    @CategoryId INT,
    @Result NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- 1) Validate category exists
        IF NOT EXISTS (SELECT 1 FROM Quizz.Categories WHERE CategoryId = @CategoryId)
        BEGIN
            SET @Result = N'Category not found.';
            RETURN;
        END

        -- 2) Block delete if there are mapped questions
        IF EXISTS (SELECT 1 FROM Quizz.Questions WHERE CategoryId = @CategoryId)
        BEGIN
            SET @Result = N'Cannot delete: Category is already mapped to questions.';
            RETURN;
        END

        -- 3) Safe to delete
        DELETE FROM Quizz.Categories
        WHERE CategoryId = @CategoryId;

        SET @Result = N'Category deleted successfully.';
    END TRY
    BEGIN CATCH
        SET @Result = N'Error: ' + ERROR_MESSAGE();
    END CATCH
END
GO


IF OBJECT_ID(N'Quizz.GetAllCategories', N'P') IS NULL
BEGIN
    EXEC sp_executesql N'CREATE PROCEDURE Quizz.GetAllCategories AS SELECT 1'
END 
GO
ALTER PROCEDURE Quizz.GetAllCategories
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CategoryId, CategoryName
    FROM Quizz.Categories
    ORDER BY CategoryName;

    SET NOCOUNT OFF;
END
GO

IF OBJECT_ID(N'Quizz.FetchQuestions', N'P') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE Quizz.FetchQuestions AS SELECT 1');
END
GO

ALTER PROCEDURE Quizz.FetchQuestions
(
    @CategoryId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        q.QuestionId,
        q.QuestionText,
        o.OptionId,
        o.OptionText,
        q.CorrectOptionId,
        c.CategoryName,
        q.CategoryId
    FROM Quizz.Questions q
    INNER JOIN Quizz.Options o ON q.QuestionId = o.QuestionId
    INNER JOIN Quizz.Categories c ON c.CategoryId = q.CategoryId
    WHERE (@CategoryId IS NULL OR q.CategoryId = @CategoryId)
    ORDER BY q.QuestionId, o.OptionId;

    SET NOCOUNT OFF;
END
GO

--exec Quizz.FetchQuestions


IF OBJECT_ID(N'Quizz.GetQuestionById', N'P') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE Quizz.GetQuestionById AS SELECT 1');
END
GO

ALTER PROCEDURE Quizz.GetQuestionById
(
    @QuestionId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT q.QuestionId, q.QuestionText, q.CorrectOptionId, q.CategoryId, c.CategoryName
    FROM Quizz.Questions q
    INNER JOIN Quizz.Categories c ON c.CategoryId = q.CategoryId
    WHERE q.QuestionId = @QuestionId;

    SELECT o.OptionId, o.OptionText
    FROM Quizz.Options o
    WHERE o.QuestionId = @QuestionId
    ORDER BY o.OptionId;

    SET NOCOUNT OFF;
END
GO

--EXEC Quizz.GetQuestionById @QuestionId = 2



IF OBJECT_ID(N'Quizz.UpdateQuestionWithOptionsAndCategory', N'P') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE Quizz.UpdateQuestionWithOptionsAndCategory AS SELECT 1');
END
GO

ALTER PROCEDURE Quizz.UpdateQuestionWithOptionsAndCategory
(
    @QuestionId INT,
    @CategoryId INT,
    @QuestionText NVARCHAR(400),
    @Option1 NVARCHAR(500),
    @Option2 NVARCHAR(500),
    @Option3 NVARCHAR(500),
    @Option4 NVARCHAR(500),
    @CorrectOptionNumber INT -- 1..4
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Quizz.Questions WHERE QuestionId = @QuestionId)
    BEGIN
        RAISERROR('Question not found', 16, 1);
        RETURN;
    END

    -- Prevent duplicate within same category
    IF EXISTS (
        SELECT 1
        FROM Quizz.Questions 
        WHERE CategoryId = @CategoryId AND QuestionText = @QuestionText AND QuestionId <> @QuestionId
    )
    BEGIN
        RAISERROR('Duplicate question in the same category', 16, 1);
        RETURN;
    END

    UPDATE Quizz.Questions
    SET QuestionText = @QuestionText,
        CategoryId = @CategoryId,
        CorrectOptionId = NULL -- reset, will set after reinserting options
    WHERE QuestionId = @QuestionId;

    -- Replace options (simpler than trying to map by ids)
    DELETE FROM Quizz.Options WHERE QuestionId = @QuestionId;

    DECLARE @o1 INT, @o2 INT, @o3 INT, @o4 INT;

    INSERT INTO Quizz.Options (QuestionId, OptionText) VALUES (@QuestionId, @Option1);
    SET @o1 = SCOPE_IDENTITY();

    INSERT INTO Quizz.Options (QuestionId, OptionText) VALUES (@QuestionId, @Option2);
    SET @o2 = SCOPE_IDENTITY();

    INSERT INTO Quizz.Options (QuestionId, OptionText) VALUES (@QuestionId, @Option3);
    SET @o3 = SCOPE_IDENTITY();

    INSERT INTO Quizz.Options (QuestionId, OptionText) VALUES (@QuestionId, @Option4);
    SET @o4 = SCOPE_IDENTITY();

    UPDATE Quizz.Questions
    SET CorrectOptionId = CASE @CorrectOptionNumber WHEN 1 THEN @o1 WHEN 2 THEN @o2 WHEN 3 THEN @o3 WHEN 4 THEN @o4 END
    WHERE QuestionId = @QuestionId;
END
GO



IF OBJECT_ID(N'Quizz.DeleteQuestion', N'P') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE Quizz.DeleteQuestion AS SELECT 1');
END
GO

ALTER PROCEDURE Quizz.DeleteQuestion
(
    @QuestionId INT,
    @Result NVARCHAR(200) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM Quizz.Questions WHERE QuestionId = @QuestionId)
        BEGIN
            SET @Result = N'Question not found.';
            RETURN;
        END

        DELETE FROM Quizz.Options WHERE QuestionId = @QuestionId;
        DELETE FROM Quizz.Questions WHERE QuestionId = @QuestionId;

        SET @Result = N'Question deleted successfully.';
    END TRY
    BEGIN CATCH
        SET @Result = N'Error: ' + ERROR_MESSAGE();
    END CATCH
END
GO


--------------------------------------Testing---------------------------------------
SELECT * FROM Quizz.Categories
SELECT * FROM Quizz.Questions
SELECT * FROM Quizz.Options

SELECT * FROM Academic.StudAdmnInfo WHERE DeptId = 1
SELECT * FROM IBase.Department WHERE DeptId = 1



INSERT INTO Quizz.Categories (CategoryName) VALUES
('Programming'),
('Data Structures'),
('DBMS'),
('Operating Systems');


IF OBJECT_ID('Quizz.Categories', 'U') IS NOT NULL
    DROP TABLE Quizz.Categories;
    IF OBJECT_ID('Quizz.Questions', 'U') IS NOT NULL
    DROP TABLE Quizz.Questions;
    IF OBJECT_ID('Quizz.Options', 'U') IS NOT NULL
    DROP TABLE Quizz.Options;


EXEC Quizz.InsertQuestionWithOptionsAndCategory
    @CategoryId = 1,
    @QuestionText = 'What is the output of: Console.WriteLine(1 + "1"); in C#?',
    @Option1 = '2',
    @Option2 = '11',
    @Option3 = 'Runtime Error',
    @Option4 = 'Compile Time Error',
    @CorrectOptionNumber = 2;






    -------------------------------------------------------------Testing 08.08.2025--------------------------------------------
    IF OBJECT_ID(N'Quizz.FetchQuestions', N'P') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE Quizz.FetchQuestions AS SELECT 1');
END
GO

ALTER PROCEDURE Quizz.FetchQuestions
(
    @CategoryId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        q.QuestionId,
        q.QuestionText,
        o.OptionId,
        o.OptionText,
        q.CorrectOptionId,
        c.CategoryName,
        q.CategoryId
    FROM Quizz.Questions q
    INNER JOIN Quizz.Options o ON q.QuestionId = o.QuestionId
    INNER JOIN Quizz.Categories c ON c.CategoryId = q.CategoryId
    WHERE (@CategoryId IS NULL OR q.CategoryId = @CategoryId)
    ORDER BY q.QuestionId, o.OptionId;

    SET NOCOUNT OFF;
END
GO

--exec Quizz.FetchQuestions


IF OBJECT_ID(N'Quizz.GetQuestionById', N'P') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE Quizz.GetQuestionById AS SELECT 1');
END
GO

ALTER PROCEDURE Quizz.GetQuestionById
(
    @QuestionId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT q.QuestionId, q.QuestionText, q.CorrectOptionId, q.CategoryId, c.CategoryName
    FROM Quizz.Questions q
    INNER JOIN Quizz.Categories c ON c.CategoryId = q.CategoryId
    WHERE q.QuestionId = @QuestionId;

    SELECT o.OptionId, o.OptionText
    FROM Quizz.Options o
    WHERE o.QuestionId = @QuestionId
    ORDER BY o.OptionId;

    SET NOCOUNT OFF;
END
GO

--EXEC Quizz.GetQuestionById @QuestionId = 2



IF OBJECT_ID(N'Quizz.UpdateQuestionWithOptionsAndCategory', N'P') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE Quizz.UpdateQuestionWithOptionsAndCategory AS SELECT 1');
END
GO

ALTER PROCEDURE Quizz.UpdateQuestionWithOptionsAndCategory
(
    @QuestionId INT,
    @CategoryId INT,
    @QuestionText NVARCHAR(400),
    @Option1 NVARCHAR(500),
    @Option2 NVARCHAR(500),
    @Option3 NVARCHAR(500),
    @Option4 NVARCHAR(500),
    @CorrectOptionNumber INT -- 1..4
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Quizz.Questions WHERE QuestionId = @QuestionId)
    BEGIN
        RAISERROR('Question not found', 16, 1);
        RETURN;
    END

    -- Prevent duplicate within same category
    IF EXISTS (
        SELECT 1
        FROM Quizz.Questions 
        WHERE CategoryId = @CategoryId AND QuestionText = @QuestionText AND QuestionId <> @QuestionId
    )
    BEGIN
        RAISERROR('Duplicate question in the same category', 16, 1);
        RETURN;
    END

    UPDATE Quizz.Questions
    SET QuestionText = @QuestionText,
        CategoryId = @CategoryId,
        CorrectOptionId = NULL -- reset, will set after reinserting options
    WHERE QuestionId = @QuestionId;

    -- Replace options (simpler than trying to map by ids)
    DELETE FROM Quizz.Options WHERE QuestionId = @QuestionId;

    DECLARE @o1 INT, @o2 INT, @o3 INT, @o4 INT;

    INSERT INTO Quizz.Options (QuestionId, OptionText) VALUES (@QuestionId, @Option1);
    SET @o1 = SCOPE_IDENTITY();

    INSERT INTO Quizz.Options (QuestionId, OptionText) VALUES (@QuestionId, @Option2);
    SET @o2 = SCOPE_IDENTITY();

    INSERT INTO Quizz.Options (QuestionId, OptionText) VALUES (@QuestionId, @Option3);
    SET @o3 = SCOPE_IDENTITY();

    INSERT INTO Quizz.Options (QuestionId, OptionText) VALUES (@QuestionId, @Option4);
    SET @o4 = SCOPE_IDENTITY();

    UPDATE Quizz.Questions
    SET CorrectOptionId = CASE @CorrectOptionNumber WHEN 1 THEN @o1 WHEN 2 THEN @o2 WHEN 3 THEN @o3 WHEN 4 THEN @o4 END
    WHERE QuestionId = @QuestionId;
END
GO



IF OBJECT_ID(N'Quizz.DeleteQuestion', N'P') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE Quizz.DeleteQuestion AS SELECT 1');
END
GO

ALTER PROCEDURE Quizz.DeleteQuestion
(
    @QuestionId INT,
    @Result NVARCHAR(200) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM Quizz.Questions WHERE QuestionId = @QuestionId)
        BEGIN
            SET @Result = N'Question not found.';
            RETURN;
        END

        DELETE FROM Quizz.Options WHERE QuestionId = @QuestionId;
        DELETE FROM Quizz.Questions WHERE QuestionId = @QuestionId;

        SET @Result = N'Question deleted successfully.';
    END TRY
    BEGIN CATCH
        SET @Result = N'Error: ' + ERROR_MESSAGE();
    END CATCH
END
GO




