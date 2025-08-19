  --CREATE SCHEMA Quizz;

IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Quizz' AND TABLE_NAME = 'Categories')
BEGIN
 CREATE TABLE Quizz.Categories
 (
     CategoryId     INT IDENTITY(1,1) PRIMARY KEY,
     CategoryName   NVARCHAR(100) NOT NULL UNIQUE
 )
END
GO

IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Quizz' AND TABLE_NAME = 'Questions')
BEGIN
 CREATE TABLE Quizz.Questions
 (
     QuestionId      INT IDENTITY(1,1) PRIMARY KEY,
     QuestionText    NVARCHAR(400) NOT NULL, -- reduced size
     CorrectOptionId INT NULL,
     CategoryId      INT NOT NULL,
     FOREIGN KEY (CategoryId) REFERENCES Quizz.Categories(CategoryId),
     CONSTRAINT UQ_QuestionText_PerCategory UNIQUE (CategoryId, QuestionText)
 )
END
GO

IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Quizz' AND TABLE_NAME = 'Options')
BEGIN
 CREATE TABLE Quizz.Options
 (
     OptionId    INT IDENTITY(1,1) PRIMARY KEY,
     QuestionId  INT NOT NULL,
     OptionText  NVARCHAR(500) NOT NULL,
     FOREIGN KEY (QuestionId) REFERENCES Quizz.Questions(QuestionId)
 )
END
GO


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


IF OBJECT_ID(N'Quizz.InsertQuestionWithOptionsAndCategory',N'P') IS NULL
BEGIN
	EXEC sp_executesql N'CREATE PROCEDURE Quizz.InsertQuestionWithOptionsAndCategory AS SELECT 1'
END
GO

ALTER PROCEDURE Quizz.InsertQuestionWithOptionsAndCategory
(
    @CategoryId INT,
    @QuestionText VARCHAR(MAX),
    @Option1 VARCHAR(MAX),
    @Option2 VARCHAR(MAX),
    @Option3 VARCHAR(MAX),
    @Option4 VARCHAR(MAX),
    @CorrectOptionNumber INT -- 1 to 4
 )
AS
BEGIN
    SET NOCOUNT ON;

    -- Check for duplicate question in the same category
    IF EXISTS (
        SELECT 1 FROM Questions WHERE CategoryId = @CategoryId AND QuestionText = @QuestionText
    )
    BEGIN
        PRINT 'Duplicate QuestionText in the same category. Insertion skipped.';
        RETURN;
    END

    DECLARE @QuestionId INT, @OptionId1 INT, @OptionId2 INT, @OptionId3 INT, @OptionId4 INT, @CorrectOptionId INT;

    -- 1. Insert question
    INSERT INTO Questions (QuestionText, CategoryId)
    VALUES (@QuestionText, @CategoryId);

    SET @QuestionId = SCOPE_IDENTITY();

    -- 2. Insert options
    INSERT INTO Options (QuestionId, OptionText) VALUES (@QuestionId, @Option1);
    SET @OptionId1 = SCOPE_IDENTITY();

    INSERT INTO Options (QuestionId, OptionText) VALUES (@QuestionId, @Option2);
    SET @OptionId2 = SCOPE_IDENTITY();

    INSERT INTO Options (QuestionId, OptionText) VALUES (@QuestionId, @Option3);
    SET @OptionId3 = SCOPE_IDENTITY();

    INSERT INTO Options (QuestionId, OptionText) VALUES (@QuestionId, @Option4);
    SET @OptionId4 = SCOPE_IDENTITY();

    -- 3. Identify correct option
    SET @CorrectOptionId =
        CASE @CorrectOptionNumber
            WHEN 1 THEN @OptionId1
            WHEN 2 THEN @OptionId2
            WHEN 3 THEN @OptionId3
            WHEN 4 THEN @OptionId4
        END;

    -- 4. Update correct option in Questions table
    UPDATE Questions
    SET CorrectOptionId = @CorrectOptionId
    WHERE QuestionId = @QuestionId;

    PRINT 'Question inserted successfully.';
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
    FROM Categories
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



--------------------------------------------09.08.2025 saturday night quizz assigned process and create process ------------------------------------------
IF OBJECT_ID(N'Quizz.CreateGroupForClass', N'P') IS NULL
    EXEC('CREATE PROCEDURE Quizz.CreateGroupForClass AS SELECT 1');
GO
ALTER PROCEDURE Quizz.CreateGroupForClass
(
    @ClassId INT,                     -- Academic.ClassId (உங்க schema-படி மாற்றிக்கலாம்)
    @GroupName NVARCHAR(200),
    @CreatedByStaffId INT,
    @GroupId INT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRAN;
    BEGIN TRY
        INSERT INTO Quizz.Groups(GroupType, GroupRefId, GroupName, CreatedByStaffId)
        VALUES('CLASS', @ClassId, @GroupName, @CreatedByStaffId);

        SET @GroupId = SCOPE_IDENTITY();

        -- Class-இலிருக்கும் மாணவர்கள் சேர்
        INSERT INTO Quizz.GroupMembers(GroupId, StudentId)
        SELECT @GroupId, sai.StudentId
        FROM Academic.StudAdmnInfo sai
        INNER JOIN Academic.StudClasses sc ON sc.StudentId = sai.StudentId
        WHERE sc.ClassId = @ClassId;  

        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK;
        THROW;
    END CATCH
END
GO


IF OBJECT_ID(N'Quizz.PublishAssignment', N'P') IS NULL
    EXEC('CREATE PROCEDURE Quizz.PublishAssignment AS SELECT 1');
GO
ALTER PROCEDURE Quizz.PublishAssignment
(
    @Title NVARCHAR(200),
    @CategoryId INT,
    @GroupId INT,
    @StartAt DATETIME2,
    @EndAt   DATETIME2,
    @TimeLimitSeconds INT = NULL,
    @ShuffleQuestions BIT = 1,
    @ShuffleOptions   BIT = 1,
    @MaxAttempts      INT = 1,
    @CreatedByStaffId INT,
    @AssignmentId INT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    IF @EndAt <= @StartAt
        THROW 51000, 'EndAt must be greater than StartAt', 1;

    BEGIN TRAN;
    BEGIN TRY
        INSERT INTO Quizz.Assignments
        (Title,CategoryId,GroupId,StartAt,EndAt,TimeLimitSeconds,ShuffleQuestions,ShuffleOptions,MaxAttempts,IsPublished,CreatedByStaffId)
        VALUES
        (@Title,@CategoryId,@GroupId,@StartAt,@EndAt,@TimeLimitSeconds,@ShuffleQuestions,@ShuffleOptions,@MaxAttempts,1,@CreatedByStaffId);

        SET @AssignmentId = SCOPE_IDENTITY();

        -- Snapshot questions (category லுள்ள எல்லா questions-யும் fix பண்ணி வைத்துக்கொள்ள)
        ;WITH Q AS
        (
            SELECT q.QuestionId, ROW_NUMBER() OVER(ORDER BY q.QuestionId) AS rn
            FROM Quizz.Questions q
            WHERE q.CategoryId = @CategoryId
        )
        INSERT INTO Quizz.AssignmentQuestions(AssignmentId, QuestionId, SortOrder)
        SELECT @AssignmentId, QuestionId, rn FROM Q;

        -- StudentAssignments உருவாக்கம்
        INSERT INTO Quizz.StudentAssignments(AssignmentId, StudentId)
        SELECT @AssignmentId, gm.StudentId
        FROM Quizz.GroupMembers gm
        WHERE gm.GroupId = @GroupId;

        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK;
        THROW;
    END CATCH
END
GO



IF OBJECT_ID(N'Quizz.FetchActiveAssignmentsForStudent', N'P') IS NULL
    EXEC('CREATE PROCEDURE Quizz.FetchActiveAssignmentsForStudent AS SELECT 1');
GO
ALTER PROCEDURE Quizz.FetchActiveAssignmentsForStudent
(
    @StudentId INT,
    @Now DATETIME2 = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    IF @Now IS NULL SET @Now = SYSDATETIME();

    SELECT sa.StudentAssignmentId, a.AssignmentId, a.Title, a.StartAt, a.EndAt,
           a.TimeLimitSeconds, a.MaxAttempts, sa.AttemptCount, a.CategoryId
    FROM Quizz.StudentAssignments sa
    INNER JOIN Quizz.Assignments a ON a.AssignmentId = sa.AssignmentId
    WHERE sa.StudentId = @StudentId
      AND a.IsPublished = 1
      AND @Now BETWEEN a.StartAt AND a.EndAt;
END
GO


IF OBJECT_ID(N'Quizz.StartAttempt', N'P') IS NULL
    EXEC('CREATE PROCEDURE Quizz.StartAttempt AS SELECT 1');
GO
ALTER PROCEDURE Quizz.StartAttempt
(
    @StudentId    INT,
    @AssignmentId INT,
    @AttemptId    INT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @saId INT, @maxAttempts INT, @attemptCount INT;
    SELECT TOP 1
        @saId = sa.StudentAssignmentId,
        @attemptCount = sa.AttemptCount,
        @maxAttempts = a.MaxAttempts
    FROM Quizz.StudentAssignments sa
    INNER JOIN Quizz.Assignments a ON a.AssignmentId = sa.AssignmentId
    WHERE sa.StudentId = @StudentId
      AND sa.AssignmentId = @AssignmentId;

    IF @saId IS NULL THROW 51001, 'Student not assigned to this assignment', 1;
    IF @attemptCount >= @maxAttempts THROW 51002, 'Max attempts reached', 1;

    BEGIN TRAN;
    BEGIN TRY
        INSERT INTO Quizz.Attempts(StudentAssignmentId) VALUES(@saId);
        SET @AttemptId = SCOPE_IDENTITY();

        -- ShuffleQuestions flag
        DECLARE @shuffle BIT = (SELECT ShuffleQuestions FROM Quizz.Assignments WHERE AssignmentId=@AssignmentId);

        -- ❗ORDER BY workaround: rank first, then order by rn in outer SELECT (with TOP)
        ;WITH Q AS
        (
            SELECT
                aq.QuestionId,
                rn = ROW_NUMBER() OVER (
                        ORDER BY CASE WHEN @shuffle=1
                                      THEN CHECKSUM(NEWID())
                                      ELSE aq.SortOrder END)
            FROM Quizz.AssignmentQuestions aq
            WHERE aq.AssignmentId = @AssignmentId
        )
        INSERT INTO Quizz.AttemptAnswers(AttemptId, QuestionId)
        SELECT TOP (1000000000) @AttemptId, QuestionId
        FROM Q
        ORDER BY rn;

        -- increment attempt count
        UPDATE Quizz.StudentAssignments
        SET AttemptCount = AttemptCount + 1
        WHERE StudentAssignmentId=@saId;

        COMMIT;

        -- Return paper
        DECLARE @shuffleOpt BIT = (SELECT ShuffleOptions FROM Quizz.Assignments WHERE AssignmentId=@AssignmentId);

        -- Questions (in the same order they were inserted via rn)
        SELECT aa.AttemptAnswerId, aa.QuestionId, q.QuestionText
        FROM Quizz.AttemptAnswers aa
        INNER JOIN Quizz.Questions q ON q.QuestionId = aa.QuestionId
        WHERE aa.AttemptId = @AttemptId
        ORDER BY aa.AttemptAnswerId;

        -- Options (per question; shuffle if enabled)
        SELECT o.QuestionId, o.OptionId, o.OptionText
        FROM Quizz.AttemptAnswers aa
        INNER JOIN Quizz.Options o ON o.QuestionId = aa.QuestionId
        WHERE aa.AttemptId = @AttemptId
        ORDER BY aa.AttemptAnswerId,
                 CASE WHEN @shuffleOpt=1 THEN CHECKSUM(NEWID()) ELSE o.OptionId END;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK;
        THROW;
    END CATCH
END
GO



IF OBJECT_ID(N'Quizz.FetchAttemptReview', N'P') IS NULL
    EXEC('CREATE PROCEDURE Quizz.FetchAttemptReview AS SELECT 1');
GO
ALTER PROCEDURE Quizz.FetchAttemptReview
(
    @AttemptId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.AttemptId, a.StartedAt, a.SubmittedAt, a.Score, a.Total
    FROM Quizz.Attempts a WHERE a.AttemptId=@AttemptId;

    SELECT aa.AttemptAnswerId, aa.QuestionId, q.QuestionText,
           aa.SelectedOptionId, aa.IsCorrect, q.CorrectOptionId
    FROM Quizz.AttemptAnswers aa
    INNER JOIN Quizz.Questions q ON q.QuestionId = aa.QuestionId
    WHERE aa.AttemptId=@AttemptId
    ORDER BY aa.AttemptAnswerId;
END
GO


-- SubmitAnswer
IF OBJECT_ID(N'Quizz.SubmitAnswer', N'P') IS NULL
    EXEC('CREATE PROCEDURE Quizz.SubmitAnswer AS SELECT 1');
GO
ALTER PROCEDURE Quizz.SubmitAnswer
(
    @AttemptId INT,
    @QuestionId INT,
    @SelectedOptionId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- validate mapping
    IF NOT EXISTS (
        SELECT 1 FROM Quizz.AttemptAnswers
        WHERE AttemptId=@AttemptId AND QuestionId=@QuestionId
    )
        THROW 51010, 'Invalid question for this attempt', 1;

    DECLARE @correctOptionId INT =
        (SELECT CorrectOptionId FROM Quizz.Questions WHERE QuestionId=@QuestionId);

    UPDATE Quizz.AttemptAnswers
    SET SelectedOptionId = @SelectedOptionId,
        IsCorrect = CASE WHEN @SelectedOptionId = @correctOptionId THEN 1 ELSE 0 END,
        AnsweredAt = SYSDATETIME()
    WHERE AttemptId=@AttemptId AND QuestionId=@QuestionId;
END
GO


-- FinishAttempt
IF OBJECT_ID(N'Quizz.FinishAttempt', N'P') IS NULL
    EXEC('CREATE PROCEDURE Quizz.FinishAttempt AS SELECT 1');
GO
ALTER PROCEDURE Quizz.FinishAttempt
(
    @AttemptId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @saId INT;
    SELECT @saId = a.StudentAssignmentId
    FROM Quizz.Attempts a
    WHERE a.AttemptId=@AttemptId;

    IF @saId IS NULL
        THROW 51020, 'Attempt not found', 1;

    DECLARE @score INT, @total INT;
    SELECT @score = COUNT(1) FROM Quizz.AttemptAnswers WHERE AttemptId=@AttemptId AND IsCorrect=1;
    SELECT @total = COUNT(1) FROM Quizz.AttemptAnswers WHERE AttemptId=@AttemptId;

    UPDATE Quizz.Attempts
    SET SubmittedAt = SYSDATETIME(), Score=@score, Total=@total
    WHERE AttemptId=@AttemptId;

    UPDATE sa
    SET LastAttemptScore = @score,
        LastAttemptTotal = @total,
        Status = 'COMPLETED'
    FROM Quizz.StudentAssignments sa
    INNER JOIN Quizz.Attempts a ON a.StudentAssignmentId = sa.StudentAssignmentId
    WHERE a.AttemptId = @AttemptId;

    -- return summary row (controller expects this)
    SELECT @score AS Score, @total AS Total;
END
GO


-- Groups = யாருக்கெல்லாம் assign? (Class/Section/Custom group எல்லாம் ஆதரிக்க)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='Groups' AND schema_id = SCHEMA_ID('Quizz'))
CREATE TABLE Quizz.Groups(
    GroupId            INT IDENTITY(1,1) PRIMARY KEY,
    GroupType          VARCHAR(20) NOT NULL CHECK (GroupType IN ('CLASS','SECTION','CUSTOM')),
    GroupRefId         INT NULL,              -- e.g., Academic.ClassId / SectionId (optional)
    GroupName          NVARCHAR(200) NOT NULL,
    CreatedByStaffId   INT NOT NULL,
    CreatedDate        DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='GroupMembers' AND schema_id = SCHEMA_ID('Quizz'))
CREATE TABLE Quizz.GroupMembers(
    GroupId   INT NOT NULL,
    StudentId INT NOT NULL,
    CONSTRAINT PK_Quizz_GroupMembers PRIMARY KEY(GroupId, StudentId),
    FOREIGN KEY (GroupId)  REFERENCES Quizz.Groups(GroupId)
    --,FOREIGN KEY (StudentId) REFERENCES Academic.StudAdmnInfo(StudentId)   -- if FK allowed
);

-- Assignment = ஒரு category-யை ஒரு group-க்கு quiz-ஆக publish பண்ணுறது
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='Assignments' AND schema_id = SCHEMA_ID('Quizz'))
CREATE TABLE Quizz.Assignments(
    AssignmentId       INT IDENTITY(1,1) PRIMARY KEY,
    Title              NVARCHAR(200) NOT NULL,
    CategoryId         INT NOT NULL,
    GroupId            INT NOT NULL,
    StartAt            DATETIME2 NOT NULL,
    EndAt              DATETIME2 NOT NULL,
    TimeLimitSeconds   INT NULL,              -- per attempt
    ShuffleQuestions   BIT NOT NULL DEFAULT(1),
    ShuffleOptions     BIT NOT NULL DEFAULT(1),
    MaxAttempts        INT NOT NULL DEFAULT(1),
    IsPublished        BIT NOT NULL DEFAULT(0),
    CreatedByStaffId   INT NOT NULL,
    CreatedDate        DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    FOREIGN KEY (CategoryId) REFERENCES Quizz.Categories(CategoryId),
    FOREIGN KEY (GroupId)    REFERENCES Quizz.Groups(GroupId)
);

-- Snapshot (optional): assignment உருவாக்கும்போது எத்தனை questions include?
-- future-ல் question மாற்றத்தால் quiz மாறாததற்கு snapshot நல்லது
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='AssignmentQuestions' AND schema_id = SCHEMA_ID('Quizz'))
CREATE TABLE Quizz.AssignmentQuestions(
    AssignmentId  INT NOT NULL,
    QuestionId    INT NOT NULL,
    SortOrder     INT NOT NULL,
    CONSTRAINT PK_Quizz_AssignmentQuestions PRIMARY KEY(AssignmentId, QuestionId),
    FOREIGN KEY (AssignmentId) REFERENCES Quizz.Assignments(AssignmentId),
    FOREIGN KEY (QuestionId)   REFERENCES Quizz.Questions(QuestionId)
);

-- Student ↔ Assignment status
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='StudentAssignments' AND schema_id = SCHEMA_ID('Quizz'))
CREATE TABLE Quizz.StudentAssignments(
    StudentAssignmentId INT IDENTITY(1,1) PRIMARY KEY,
    AssignmentId        INT NOT NULL,
    StudentId           INT NOT NULL,
    AttemptCount        INT NOT NULL DEFAULT(0),
    LastAttemptScore    INT NULL,
    LastAttemptTotal    INT NULL,
    Status              VARCHAR(20) NOT NULL DEFAULT('PENDING')  -- PENDING/COMPLETED
    ,UNIQUE(AssignmentId, StudentId),
    FOREIGN KEY (AssignmentId) REFERENCES Quizz.Assignments(AssignmentId)
);

-- Attempts & Answers
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='Attempts' AND schema_id = SCHEMA_ID('Quizz'))
CREATE TABLE Quizz.Attempts(
    AttemptId           INT IDENTITY(1,1) PRIMARY KEY,
    StudentAssignmentId INT NOT NULL,
    StartedAt           DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    SubmittedAt         DATETIME2 NULL,
    Score               INT NULL,
    Total               INT NULL,
    FOREIGN KEY (StudentAssignmentId) REFERENCES Quizz.StudentAssignments(StudentAssignmentId)
);

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='AttemptAnswers' AND schema_id = SCHEMA_ID('Quizz'))
CREATE TABLE Quizz.AttemptAnswers(
    AttemptAnswerId INT IDENTITY(1,1) PRIMARY KEY,
    AttemptId       INT NOT NULL,
    QuestionId      INT NOT NULL,
    SelectedOptionId INT NULL,
    IsCorrect       BIT NULL,
    AnsweredAt      DATETIME2 NULL,
    FOREIGN KEY (AttemptId)  REFERENCES Quizz.Attempts(AttemptId),
    FOREIGN KEY (QuestionId) REFERENCES Quizz.Questions(QuestionId)
);
