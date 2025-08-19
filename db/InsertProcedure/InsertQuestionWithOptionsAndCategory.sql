IF OBJECT_ID(N'Quizz.InsertQuestionWithOptionsAndCategory',N'P') IS NULL
BEGIN
	EXEC sp_executesql N'CREATE PROCEDURE Quizz.InsertQuestionWithOptionsAndCategory AS SELECT 1'
END
GO

ALTER PROCEDURE Quizz.InsertQuestionWithOptionsAndCategory
(
    @CategoryId INT,
    @QuestionText NVARCHAR(400),
    @Option1 NVARCHAR(500),
    @Option2 NVARCHAR(500),
    @Option3 NVARCHAR(500),
    @Option4 NVARCHAR(500),
    @CorrectOptionNumber INT,         -- 1..4
    @Result NVARCHAR(200) OUTPUT
 )
AS
BEGIN
    SET NOCOUNT ON;

    -- Check for duplicate question in the same category
    IF EXISTS (
        SELECT 1 FROM Quizz.Questions WHERE CategoryId = @CategoryId AND QuestionText = @QuestionText
    )
    BEGIN
        PRINT 'Duplicate QuestionText in the same category. Insertion skipped.';
        RETURN;
    END

    DECLARE @QuestionId INT, @OptionId1 INT, @OptionId2 INT, @OptionId3 INT, @OptionId4 INT, @CorrectOptionId INT;

    -- 1. Insert question
    INSERT INTO Quizz.Questions (QuestionText, CategoryId)
    VALUES (@QuestionText, @CategoryId);

    SET @QuestionId = SCOPE_IDENTITY();

    -- 2. Insert options
    INSERT INTO Quizz.Options (QuestionId, OptionText) VALUES (@QuestionId, @Option1);
    SET @OptionId1 = SCOPE_IDENTITY();

    INSERT INTO Quizz.Options (QuestionId, OptionText) VALUES (@QuestionId, @Option2);
    SET @OptionId2 = SCOPE_IDENTITY();

    INSERT INTO Quizz.Options (QuestionId, OptionText) VALUES (@QuestionId, @Option3);
    SET @OptionId3 = SCOPE_IDENTITY();

    INSERT INTO Quizz.Options (QuestionId, OptionText) VALUES (@QuestionId, @Option4);
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
    UPDATE Quizz.Questions
    SET CorrectOptionId = @CorrectOptionId
    WHERE QuestionId = @QuestionId;

    PRINT 'Question inserted successfully.';
END

GO