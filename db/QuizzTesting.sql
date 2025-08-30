/* ============================================================
   QUIZZ APP – SQL SERVER 2014 STARTER SCHEMA & PROCEDURES
   ============================================================ */
/* ------------------------------
   1) TABLES
   ------------------------------ */

IF OBJECT_ID('dbo.Users','U') IS NULL
CREATE TABLE dbo.Users
(
UserId        INT IDENTITY(1,1) CONSTRAINT PK_Users PRIMARY KEY,
    Email         NVARCHAR(256) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,
    PasswordHash  NVARCHAR(256) NOT NULL, -- consider VARBINARY for hash+salt
    Role          TINYINT NOT NULL CONSTRAINT DF_Users_Role DEFAULT(1), -- 1=Host/Admin, 0=Player
    CreatedAt     DATETIME2(3) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_Users_Role CHECK (Role IN (0,1))
);
GO

IF OBJECT_ID('dbo.Quizzes','U') IS NULL
CREATE TABLE dbo.Quizzes
(
    QuizId        INT IDENTITY(1,1) CONSTRAINT PK_Quizzes PRIMARY KEY,
    Title         NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    IsPublished   BIT NOT NULL CONSTRAINT DF_Quizzes_IsPublished DEFAULT(0),
    CreatedBy     INT NOT NULL REFERENCES dbo.Users(UserId),
    CreatedAt     DATETIME2(3) NOT NULL CONSTRAINT DF_Quizzes_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt     DATETIME2(3) NULL
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Quizzes_CreatedBy' AND object_id=OBJECT_ID('dbo.Quizzes'))
    CREATE INDEX IX_Quizzes_CreatedBy ON dbo.Quizzes(CreatedBy);
GO


IF OBJECT_ID('dbo.Questions','U') IS NULL
CREATE TABLE dbo.Questions
(
    QuestionId    INT IDENTITY(1,1) CONSTRAINT PK_Questions PRIMARY KEY,
    QuizId        INT NOT NULL REFERENCES dbo.Quizzes(QuizId) ON DELETE CASCADE,
    [Text]        NVARCHAR(500) NOT NULL,
    QuestionType  TINYINT NOT NULL CONSTRAINT DF_Questions_Type DEFAULT(0), -- 0=MCQ single (extend later)
    TimeLimitSec  INT NOT NULL CONSTRAINT DF_Questions_Limit DEFAULT(20),
    Points        INT NOT NULL CONSTRAINT DF_Questions_Points DEFAULT(1000),
    MediaUrl      NVARCHAR(500) NULL,
    OrderNo       INT NOT NULL
);
GO

-- Per-quiz ordering unique
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_Questions_Quiz_Order' AND object_id=OBJECT_ID('dbo.Questions'))
    CREATE UNIQUE INDEX UX_Questions_Quiz_Order ON dbo.Questions(QuizId, OrderNo);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Questions_Quiz' AND object_id=OBJECT_ID('dbo.Questions'))
    CREATE INDEX IX_Questions_Quiz ON dbo.Questions(QuizId);
GO
ALTER TABLE dbo.Questions
    WITH CHECK ADD CONSTRAINT CK_Questions_Type CHECK (QuestionType IN (0));
GO

IF OBJECT_ID('dbo.Choices','U') IS NULL
CREATE TABLE dbo.Choices
(
    ChoiceId      INT IDENTITY(1,1) CONSTRAINT PK_Choices PRIMARY KEY,
    QuestionId    INT NOT NULL REFERENCES dbo.Questions(QuestionId) ON DELETE CASCADE,
    [Text]        NVARCHAR(200) NOT NULL,
    IsCorrect     BIT NOT NULL CONSTRAINT DF_Choices_IsCorrect DEFAULT(0),
    OrderNo       INT NOT NULL
);
GO


-- Per-question ordering unique
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_Choices_Q_Order' AND object_id=OBJECT_ID('dbo.Choices'))
    CREATE UNIQUE INDEX UX_Choices_Q_Order ON dbo.Choices(QuestionId, OrderNo);
GO
-- Exactly one correct choice per question
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_Choices_SingleCorrect' AND object_id=OBJECT_ID('dbo.Choices'))
    CREATE UNIQUE INDEX UX_Choices_SingleCorrect ON dbo.Choices(QuestionId) WHERE IsCorrect = 1;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Choices_Q' AND object_id=OBJECT_ID('dbo.Choices'))
    CREATE INDEX IX_Choices_Q ON dbo.Choices(QuestionId);
GO

/* Status: 0=Lobby,1=InProgress,2=Finished,3=Cancelled */
IF OBJECT_ID('dbo.GameSessions','U') IS NULL
CREATE TABLE dbo.GameSessions
(
    GameSessionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_GameSessions PRIMARY KEY DEFAULT NEWID(),
    QuizId        INT NOT NULL REFERENCES dbo.Quizzes(QuizId) ON DELETE CASCADE,
    HostUserId    INT NOT NULL REFERENCES dbo.Users(UserId),
    PinCode       CHAR(6) NOT NULL,
    [Status]      TINYINT NOT NULL CONSTRAINT DF_GameSessions_Status DEFAULT(0), -- 0=Lobby,1=InProgress,2=Finished,3=Cancelled
    StartedAt     DATETIME2(3) NULL,
    EndedAt       DATETIME2(3) NULL,
    CreatedAt     DATETIME2(3) NOT NULL CONSTRAINT DF_GameSessions_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_GameSessions_Status CHECK ([Status] IN (0,1,2,3))
);
GO
-- Unique active PIN (SQL 2014 supports filtered indexes)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_GameSessions_Quiz' AND object_id=OBJECT_ID('dbo.GameSessions'))
    CREATE INDEX IX_GameSessions_Quiz ON dbo.GameSessions(QuizId);
GO
-- Unique active PIN (SQL 2014 supports filtered indexes)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_GameSessions_Pin_Active' AND object_id=OBJECT_ID('dbo.GameSessions'))
    CREATE UNIQUE INDEX UX_GameSessions_Pin_Active ON dbo.GameSessions(PinCode) WHERE EndedAt IS NULL;
GO


IF OBJECT_ID('dbo.Players','U') IS NULL
CREATE TABLE dbo.Players
(
    PlayerId      INT IDENTITY(1,1) CONSTRAINT PK_Players PRIMARY KEY,
    GameSessionId UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.GameSessions(GameSessionId) ON DELETE CASCADE,
    DisplayName   NVARCHAR(50) NOT NULL,
    JoinedAt      DATETIME2(3) NOT NULL CONSTRAINT DF_Players_JoinedAt DEFAULT SYSUTCDATETIME(),
    IsKicked      BIT NOT NULL CONSTRAINT DF_Players_IsKicked DEFAULT(0)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Players_Session' AND object_id=OBJECT_ID('dbo.Players'))
    CREATE INDEX IX_Players_Session ON dbo.Players(GameSessionId);
GO

IF OBJECT_ID('dbo.PlayerAnswers','U') IS NULL
CREATE TABLE dbo.PlayerAnswers
(
    PlayerAnswerId BIGINT IDENTITY(1,1) CONSTRAINT PK_PlayerAnswers PRIMARY KEY,
    GameSessionId  UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.GameSessions(GameSessionId) ON DELETE CASCADE,
    PlayerId       INT NOT NULL REFERENCES dbo.Players(PlayerId) ON DELETE CASCADE,
    QuestionId     INT NOT NULL REFERENCES dbo.Questions(QuestionId),
    ChoiceId       INT NULL REFERENCES dbo.Choices(ChoiceId),
    IsCorrect      BIT NOT NULL CONSTRAINT DF_PlayerAnswers_IsCorrect DEFAULT(0),
    TimeTakenMs    INT NULL,
    ScoreAwarded   INT NOT NULL CONSTRAINT DF_PlayerAnswers_Score DEFAULT(0),
    SubmittedAt    DATETIME2(3) NOT NULL CONSTRAINT DF_PlayerAnswers_SubmittedAt DEFAULT SYSUTCDATETIME()
);
GO
-- Prevent multiple submissions per question by same player
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_PlayerAnswers_Session_Q_Player' AND object_id=OBJECT_ID('dbo.PlayerAnswers'))
    CREATE UNIQUE INDEX UX_PlayerAnswers_Session_Q_Player ON dbo.PlayerAnswers(GameSessionId, QuestionId, PlayerId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_PlayerAnswers_Session_Player' AND object_id=OBJECT_ID('dbo.PlayerAnswers'))
    CREATE INDEX IX_PlayerAnswers_Session_Player ON dbo.PlayerAnswers(GameSessionId, PlayerId) INCLUDE (ScoreAwarded, IsCorrect);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_PlayerAnswers_Session_Question' AND object_id=OBJECT_ID('dbo.PlayerAnswers'))
    CREATE INDEX IX_PlayerAnswers_Session_Question ON dbo.PlayerAnswers(GameSessionId, QuestionId);
GO


/* ------------------------------
   3) CREATE GAME SESSION (single, inline PIN)
   ------------------------------ */

IF OBJECT_ID('dbo.sp_CreateGameSession','P') IS NOT NULL DROP PROCEDURE dbo.sp_CreateGameSession;
GO
CREATE PROCEDURE dbo.sp_CreateGameSession
    @QuizId        INT,
    @HostUserId    INT,
    @GameSessionId UNIQUEIDENTIFIER OUTPUT,
    @PinCode       CHAR(6) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @pin CHAR(6), @tries INT = 0;

    WHILE 1=1
    BEGIN
        SET @pin = RIGHT('000000' + CAST(ABS(CHECKSUM(NEWID())) % 900000 + 100000 AS VARCHAR(6)), 6);

        IF NOT EXISTS (
            SELECT 1
            FROM dbo.GameSessions WITH (UPDLOCK, HOLDLOCK)
            WHERE PinCode = @pin AND EndedAt IS NULL
        ) BREAK;

        SET @tries += 1;
        IF @tries > 20
        BEGIN
            RAISERROR('Unable to generate unique PIN. Try again.',16,1);
            RETURN;
        END
    END

    DECLARE @id UNIQUEIDENTIFIER = NEWID();

    INSERT dbo.GameSessions(GameSessionId, QuizId, HostUserId, PinCode, [Status], CreatedAt)
    VALUES (@id, @QuizId, @HostUserId, @pin, 0, SYSUTCDATETIME());

    SET @GameSessionId = @id;
    SET @PinCode       = @pin;
END
GO


/* ------------------------------
   4) STORED PROCEDURES
   ------------------------------ */

-- 4.2 Player joins a session by PIN
IF OBJECT_ID('dbo.sp_JoinPlayerByPin','P') IS NOT NULL DROP PROCEDURE dbo.sp_JoinPlayerByPin;
GO
CREATE PROCEDURE dbo.sp_JoinPlayerByPin
    @PinCode       CHAR(6),
    @DisplayName   NVARCHAR(50),
    @PlayerId        INT OUTPUT,
    @GameSessionId   UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @sid UNIQUEIDENTIFIER =
    (
        SELECT TOP (1) GameSessionId
        FROM dbo.GameSessions
        WHERE PinCode=@PinCode AND [Status] IN (0,1) AND EndedAt IS NULL
        ORDER BY CreatedAt DESC
    );

    IF @sid IS NULL
    BEGIN
        RAISERROR('Invalid or finished session PIN.',16,1);
        RETURN;
    END

    INSERT dbo.Players(GameSessionId, DisplayName) VALUES (@sid, @DisplayName);

    SET @PlayerId = SCOPE_IDENTITY();
    SET @GameSessionId = @sid;
END
GO

-- 4.3 Submit answer (+ scoring, race-safe)
IF OBJECT_ID('dbo.sp_SubmitAnswer','P') IS NOT NULL
    DROP PROCEDURE dbo.sp_SubmitAnswer;
GO
CREATE PROCEDURE dbo.sp_SubmitAnswer
    @GameSessionId UNIQUEIDENTIFIER,
    @PlayerId      INT,
    @QuestionId    INT,
    @ChoiceId      INT,
    @TimeTakenMs   INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate player in session (and not kicked)
    IF NOT EXISTS (SELECT 1 FROM dbo.Players WHERE PlayerId=@PlayerId AND GameSessionId=@GameSessionId AND IsKicked=0)
    BEGIN
        RAISERROR('Player not in this session.', 16, 1);
        RETURN;
    END

    -- Validate session in progress
    IF NOT EXISTS (SELECT 1 FROM dbo.GameSessions WHERE GameSessionId=@GameSessionId AND [Status]=1 AND EndedAt IS NULL)
    BEGIN
        RAISERROR('Game is not in progress.', 16, 1);
        RETURN;
    END

    -- Load question & session quiz
    DECLARE @qQuizId INT, @points INT, @limitSec INT;
    SELECT @qQuizId = q.QuizId, @points = q.Points, @limitSec = q.TimeLimitSec
    FROM dbo.Questions q
    WHERE q.QuestionId = @QuestionId;

    IF @points IS NULL OR @limitSec IS NULL
    BEGIN
        RAISERROR('Question not found.', 16, 1);
        RETURN;
    END

    DECLARE @sessionQuizId INT;
    SELECT @sessionQuizId = QuizId FROM dbo.GameSessions WHERE GameSessionId = @GameSessionId;
    IF @sessionQuizId IS NULL OR @sessionQuizId <> @qQuizId
    BEGIN
        RAISERROR('Question does not belong to this session''s quiz.', 16, 1);
        RETURN;
    END

    -- Validate choice & compute correctness
    DECLARE @choiceQuestionId INT, @isCorrect BIT;
    SELECT @choiceQuestionId = c.QuestionId, @isCorrect = CASE WHEN c.IsCorrect=1 THEN 1 ELSE 0 END
    FROM dbo.Choices c WHERE c.ChoiceId = @ChoiceId;

    IF @choiceQuestionId IS NULL OR @choiceQuestionId <> @QuestionId
    BEGIN
        RAISERROR('Choice does not belong to the specified question.', 16, 1);
        RETURN;
    END

    -- Scoring
    DECLARE @base INT = CASE WHEN @isCorrect=1 THEN ISNULL(@points,1000) ELSE 0 END;
    DECLARE @speedBonusMax INT = 500;
    DECLARE @timeLeftMs INT = (@limitSec * 1000) - ISNULL(@TimeTakenMs, @limitSec*1000);
    IF @timeLeftMs < 0 SET @timeLeftMs = 0;
    DECLARE @bonus INT = CASE WHEN @isCorrect=1 THEN (@speedBonusMax * @timeLeftMs) / (@limitSec * 1000) ELSE 0 END;
    DECLARE @final INT = @base + @bonus;

    BEGIN TRY
        INSERT dbo.PlayerAnswers(GameSessionId, PlayerId, QuestionId, ChoiceId, IsCorrect, TimeTakenMs, ScoreAwarded)
        VALUES (@GameSessionId, @PlayerId, @QuestionId, @ChoiceId, @isCorrect, @TimeTakenMs, @final);
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() IN (2601, 2627)
        BEGIN
            RAISERROR('Answer already submitted for this question.', 16, 1);
            RETURN;
        END
        ELSE
        BEGIN
            DECLARE @msg NVARCHAR(2048) = ERROR_MESSAGE();
            RAISERROR(@msg, 16, 1);
            RETURN;
        END
    END CATCH
END
GO


-- 4.4 Leaderboard
IF OBJECT_ID('dbo.sp_GetLeaderboard','P') IS NOT NULL DROP PROCEDURE dbo.sp_GetLeaderboard;
GO
CREATE PROCEDURE dbo.sp_GetLeaderboard
    @GameSessionId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.PlayerId,
        p.DisplayName,
        COALESCE(SUM(pa.ScoreAwarded), 0) AS TotalScore,
        COALESCE(SUM(CASE WHEN pa.IsCorrect=1 THEN 1 ELSE 0 END), 0) AS CorrectCount,
        COALESCE(COUNT(pa.PlayerAnswerId), 0) AS AnsweredCount,
        MIN(p.JoinAt) AS JoinedAt
    FROM dbo.Players p
    LEFT JOIN dbo.PlayerAnswers pa
      ON pa.PlayerId = p.PlayerId
     AND pa.GameSessionId = p.GameSessionId
    WHERE p.GameSessionId = @GameSessionId
    GROUP BY p.PlayerId, p.DisplayName
    ORDER BY TotalScore DESC, CorrectCount DESC, AnsweredCount DESC, JoinedAt ASC;
END
GO

-- 4.5 Start / End (guarded transitions)
IF OBJECT_ID('dbo.sp_StartGame','P') IS NOT NULL DROP PROCEDURE dbo.sp_StartGame;
GO
CREATE PROCEDURE dbo.sp_StartGame
    @GameSessionId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.GameSessions
      SET [Status]=1, StartedAt = ISNULL(StartedAt, SYSUTCDATETIME())
    WHERE GameSessionId=@GameSessionId AND [Status]=0 AND EndedAt IS NULL;

    IF @@ROWCOUNT = 0 RAISERROR('Session cannot be started (wrong status or already ended).',16,1);
END
GO

IF OBJECT_ID('dbo.sp_EndGame','P') IS NOT NULL DROP PROCEDURE dbo.sp_EndGame;
GO
CREATE PROCEDURE dbo.sp_EndGame
    @GameSessionId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.GameSessions
      SET [Status]=2, EndedAt = SYSUTCDATETIME()
    WHERE GameSessionId=@GameSessionId AND [Status]=1 AND EndedAt IS NULL;

    IF @@ROWCOUNT = 0 RAISERROR('Session cannot be ended (wrong status or already ended).',16,1);
END
GO


/* ------------------------------
   5) VIEW: per-question answer stats
   ------------------------------ */
IF OBJECT_ID('dbo.vw_QuestionStats','V') IS NOT NULL
    DROP VIEW dbo.vw_QuestionStats;
GO
CREATE VIEW dbo.vw_QuestionStats
AS
SELECT
    pa.GameSessionId,
    pa.QuestionId,
    q.OrderNo,
    COUNT(*)                               AS TotalSubmissions,
    SUM(CASE WHEN pa.IsCorrect=1 THEN 1 ELSE 0 END) AS CorrectSubmissions,
    AVG(CAST(ISNULL(pa.TimeTakenMs,0) AS FLOAT))    AS AvgTimeTakenMs
FROM dbo.PlayerAnswers pa
JOIN dbo.Questions q ON q.QuestionId = pa.QuestionId
GROUP BY pa.GameSessionId, pa.QuestionId, q.OrderNo;
GO

/* ------------------------------
   6) Helpful indexes (optional)
   ------------------------------ */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_PlayerAnswers_Session_Player' AND object_id=OBJECT_ID('dbo.PlayerAnswers'))
CREATE INDEX IX_PlayerAnswers_Session_Player
ON dbo.PlayerAnswers(GameSessionId, PlayerId)
INCLUDE (ScoreAwarded, IsCorrect);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_PlayerAnswers_Session_Question' AND object_id=OBJECT_ID('dbo.PlayerAnswers'))
CREATE INDEX IX_PlayerAnswers_Session_Question
ON dbo.PlayerAnswers(GameSessionId, QuestionId);
GO


IF OBJECT_ID('dbo.sp_CancelGame','P') IS NOT NULL DROP PROCEDURE dbo.sp_CancelGame;
GO
CREATE PROCEDURE dbo.sp_CancelGame
    @GameSessionId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.GameSessions
       SET [Status] = 3, EndedAt = ISNULL(EndedAt, SYSUTCDATETIME())
     WHERE GameSessionId = @GameSessionId
       AND EndedAt IS NULL;
    IF @@ROWCOUNT = 0
        RAISERROR('Session cannot be cancelled (already ended or not found).', 16, 1);
END
GO


IF OBJECT_ID('dbo.sp_User_Create','P') IS NOT NULL DROP PROCEDURE dbo.sp_User_Create;
GO
CREATE PROCEDURE dbo.sp_User_Create
 @Email NVARCHAR(256), @PasswordHash NVARCHAR(256), @Role TINYINT = 1,
 @UserId INT OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  INSERT dbo.Users(Email,PasswordHash,Role) VALUES(@Email,@PasswordHash,@Role);
  SET @UserId = SCOPE_IDENTITY();
END
GO

/* ===== QUIZZES ===== */
IF OBJECT_ID('dbo.sp_Quiz_Create','P') IS NOT NULL DROP PROCEDURE dbo.sp_Quiz_Create;
GO
CREATE PROCEDURE dbo.sp_Quiz_Create
 @Title NVARCHAR(200), @Description NVARCHAR(500)=NULL, @CreatedBy INT, @IsPublished BIT = 0,
 @QuizId INT OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  INSERT dbo.Quizzes(Title,[Description],IsPublished,CreatedBy) VALUES(@Title,@Description,@IsPublished,@CreatedBy);
  SET @QuizId = SCOPE_IDENTITY();
END
GO

IF OBJECT_ID('dbo.sp_Quiz_Update','P') IS NOT NULL DROP PROCEDURE dbo.sp_Quiz_Update;
GO
CREATE PROCEDURE dbo.sp_Quiz_Update
 @QuizId INT, @Title NVARCHAR(200), @Description NVARCHAR(500)=NULL
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE dbo.Quizzes
     SET Title=@Title, [Description]=@Description, UpdatedAt=SYSUTCDATETIME()
   WHERE QuizId=@QuizId;
  IF @@ROWCOUNT=0 RAISERROR('Quiz not found.',16,1);
END
GO

IF OBJECT_ID('dbo.sp_Quiz_Delete','P') IS NOT NULL DROP PROCEDURE dbo.sp_Quiz_Delete;
GO
CREATE PROCEDURE dbo.sp_Quiz_Delete
 @QuizId INT
AS
BEGIN
  SET NOCOUNT ON;
  DELETE FROM dbo.Quizzes WHERE QuizId=@QuizId;
  IF @@ROWCOUNT=0 RAISERROR('Quiz not found.',16,1);
END
GO

IF OBJECT_ID('dbo.sp_Quiz_SetPublished','P') IS NOT NULL DROP PROCEDURE dbo.sp_Quiz_SetPublished;
GO
CREATE PROCEDURE dbo.sp_Quiz_SetPublished
 @QuizId INT, @IsPublished BIT
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE dbo.Quizzes SET IsPublished=@IsPublished, UpdatedAt=SYSUTCDATETIME()
  WHERE QuizId=@QuizId;
  IF @@ROWCOUNT=0 RAISERROR('Quiz not found.',16,1);
END
GO

/* ===== QUESTIONS ===== */
IF OBJECT_ID('dbo.sp_Question_Create','P') IS NOT NULL DROP PROCEDURE dbo.sp_Question_Create;
GO
CREATE PROCEDURE dbo.sp_Question_Create
 @QuizId INT, @Text NVARCHAR(500),
 @QuestionType TINYINT = 0, @TimeLimitSec INT = 20, @Points INT = 1000,
 @MediaUrl NVARCHAR(500)=NULL, @OrderNo INT=NULL,
 @QuestionId INT OUTPUT
AS
BEGIN
  SET NOCOUNT ON;

  IF @OrderNo IS NULL
     SELECT @OrderNo = ISNULL(MAX(OrderNo),0)+1 FROM dbo.Questions WITH (TABLOCKX) WHERE QuizId=@QuizId;

  INSERT dbo.Questions(QuizId,[Text],QuestionType,TimeLimitSec,Points,MediaUrl,OrderNo)
  VALUES(@QuizId,@Text,@QuestionType,@TimeLimitSec,@Points,@MediaUrl,@OrderNo);

  SET @QuestionId = SCOPE_IDENTITY();
END
GO

IF OBJECT_ID('dbo.sp_Question_Update','P') IS NOT NULL DROP PROCEDURE dbo.sp_Question_Update;
GO
CREATE PROCEDURE dbo.sp_Question_Update
 @QuestionId INT, @Text NVARCHAR(500), @QuestionType TINYINT, @TimeLimitSec INT, @Points INT, @MediaUrl NVARCHAR(500)=NULL
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE dbo.Questions
     SET [Text]=@Text, QuestionType=@QuestionType, TimeLimitSec=@TimeLimitSec, Points=@Points, MediaUrl=@MediaUrl
   WHERE QuestionId=@QuestionId;
  IF @@ROWCOUNT=0 RAISERROR('Question not found.',16,1);
END
GO

IF OBJECT_ID('dbo.sp_Question_Delete','P') IS NOT NULL DROP PROCEDURE dbo.sp_Question_Delete;
GO
CREATE PROCEDURE dbo.sp_Question_Delete
 @QuestionId INT
AS
BEGIN
  SET NOCOUNT ON;
  DELETE FROM dbo.Questions WHERE QuestionId=@QuestionId;
  IF @@ROWCOUNT=0 RAISERROR('Question not found.',16,1);
END
GO

IF OBJECT_ID('dbo.sp_Question_Reorder','P') IS NOT NULL DROP PROCEDURE dbo.sp_Question_Reorder;
GO
CREATE PROCEDURE dbo.sp_Question_Reorder
 @QuizId INT, @QuestionId INT, @NewOrderNo INT
AS
BEGIN
  SET NOCOUNT ON;
  DECLARE @OldOrder INT;
  SELECT @OldOrder = OrderNo FROM dbo.Questions WHERE QuestionId=@QuestionId AND QuizId=@QuizId;
  IF @OldOrder IS NULL RAISERROR('Question not found in quiz.',16,1);

  IF @NewOrderNo = @OldOrder RETURN;

  BEGIN TRAN;
    IF @NewOrderNo < @OldOrder
      UPDATE dbo.Questions SET OrderNo = OrderNo + 1
       WHERE QuizId=@QuizId AND OrderNo BETWEEN @NewOrderNo AND @OldOrder-1;
    ELSE
      UPDATE dbo.Questions SET OrderNo = OrderNo - 1
       WHERE QuizId=@QuizId AND OrderNo BETWEEN @OldOrder+1 AND @NewOrderNo;

    UPDATE dbo.Questions SET OrderNo=@NewOrderNo WHERE QuestionId=@QuestionId;
  COMMIT;
END
GO

/* ===== CHOICES ===== */
IF OBJECT_ID('dbo.sp_Choice_Create','P') IS NOT NULL DROP PROCEDURE dbo.sp_Choice_Create;
GO
CREATE PROCEDURE dbo.sp_Choice_Create
 @QuestionId INT, @Text NVARCHAR(200), @IsCorrect BIT = 0, @OrderNo INT = NULL,
 @ChoiceId INT OUTPUT
AS
BEGIN
  SET NOCOUNT ON;

  IF @OrderNo IS NULL
     SELECT @OrderNo = ISNULL(MAX(OrderNo),0)+1 FROM dbo.Choices WITH (TABLOCKX) WHERE QuestionId=@QuestionId;

  BEGIN TRAN;
    IF @IsCorrect = 1
      UPDATE dbo.Choices SET IsCorrect = 0 WHERE QuestionId=@QuestionId; -- keep "single correct"

    INSERT dbo.Choices(QuestionId,[Text],IsCorrect,OrderNo)
    VALUES(@QuestionId,@Text,@IsCorrect,@OrderNo);

    SET @ChoiceId = SCOPE_IDENTITY();
  COMMIT;
END
GO

IF OBJECT_ID('dbo.sp_Choice_Update','P') IS NOT NULL DROP PROCEDURE dbo.sp_Choice_Update;
GO
CREATE PROCEDURE dbo.sp_Choice_Update
 @ChoiceId INT, @Text NVARCHAR(200), @IsCorrect BIT, @OrderNo INT
AS
BEGIN
  SET NOCOUNT ON;

  DECLARE @QuestionId INT; SELECT @QuestionId = QuestionId FROM dbo.Choices WHERE ChoiceId=@ChoiceId;
  IF @QuestionId IS NULL RAISERROR('Choice not found.',16,1);

  BEGIN TRAN;
    IF @IsCorrect = 1
      UPDATE dbo.Choices SET IsCorrect=0 WHERE QuestionId=@QuestionId AND ChoiceId<>@ChoiceId;

    UPDATE dbo.Choices
       SET [Text]=@Text, IsCorrect=@IsCorrect, OrderNo=@OrderNo
     WHERE ChoiceId=@ChoiceId;
  COMMIT;
END
GO

IF OBJECT_ID('dbo.sp_Choice_Delete','P') IS NOT NULL DROP PROCEDURE dbo.sp_Choice_Delete;
GO
CREATE PROCEDURE dbo.sp_Choice_Delete
 @ChoiceId INT
AS
BEGIN
  SET NOCOUNT ON;
  DELETE FROM dbo.Choices WHERE ChoiceId=@ChoiceId;
  IF @@ROWCOUNT=0 RAISERROR('Choice not found.',16,1);
END
GO

IF OBJECT_ID('dbo.sp_Choice_Reorder','P') IS NOT NULL DROP PROCEDURE dbo.sp_Choice_Reorder;
GO
CREATE PROCEDURE dbo.sp_Choice_Reorder
 @QuestionId INT, @ChoiceId INT, @NewOrderNo INT
AS
BEGIN
  SET NOCOUNT ON;
  DECLARE @OldOrder INT;
  SELECT @OldOrder = OrderNo FROM dbo.Choices WHERE ChoiceId=@ChoiceId AND QuestionId=@QuestionId;
  IF @OldOrder IS NULL RAISERROR('Choice not found for question.',16,1);

  IF @NewOrderNo = @OldOrder RETURN;

  BEGIN TRAN;
    IF @NewOrderNo < @OldOrder
      UPDATE dbo.Choices SET OrderNo = OrderNo + 1
       WHERE QuestionId=@QuestionId AND OrderNo BETWEEN @NewOrderNo AND @OldOrder-1;
    ELSE
      UPDATE dbo.Choices SET OrderNo = OrderNo - 1
       WHERE QuestionId=@QuestionId AND OrderNo BETWEEN @OldOrder+1 AND @NewOrderNo;

    UPDATE dbo.Choices SET OrderNo=@NewOrderNo WHERE ChoiceId=@ChoiceId;
  COMMIT;
END
GO

IF OBJECT_ID('dbo.sp_Choice_SetCorrect','P') IS NOT NULL DROP PROCEDURE dbo.sp_Choice_SetCorrect;
GO
CREATE PROCEDURE dbo.sp_Choice_SetCorrect
 @QuestionId INT, @ChoiceId INT
AS
BEGIN
  SET NOCOUNT ON;
  BEGIN TRAN;
    UPDATE dbo.Choices SET IsCorrect=0 WHERE QuestionId=@QuestionId;
    UPDATE dbo.Choices SET IsCorrect=1 WHERE ChoiceId=@ChoiceId AND QuestionId=@QuestionId;
    IF @@ROWCOUNT=0 BEGIN ROLLBACK; RAISERROR('Choice not found for question.',16,1); RETURN; END
  COMMIT;
END
GO
/* ==============================
   END OF SCRIPT
   ============================== */


   /* ------------------------------
   2) SEED DATA (demo)
   ------------------------------ */
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email='host@example.com')
BEGIN
    INSERT dbo.Users(Email, PasswordHash, Role)
    VALUES (N'host@example.com', N'hashed-password', 1);
END
GO

DECLARE @HostId INT = (SELECT TOP 1 UserId FROM dbo.Users WHERE Email='host@example.com');

IF NOT EXISTS (SELECT 1 FROM dbo.Quizzes WHERE Title = N'General Knowledge Demo')
BEGIN
    INSERT dbo.Quizzes(Title, [Description], IsPublished, CreatedBy)
    VALUES (N'General Knowledge Demo', N'Demo quiz with two questions', 1, @HostId);

    DECLARE @QuizId INT = SCOPE_IDENTITY();

    INSERT dbo.Questions(QuizId, [Text], QuestionType, TimeLimitSec, Points, OrderNo)
    VALUES
    (@QuizId, N'What is the capital of India?', 0, 20, 1000, 1),
    (@QuizId, N'2 + 2 = ?',                    0, 15, 1000, 2);

    DECLARE @Q1 INT = (SELECT QuestionId FROM dbo.Questions WHERE QuizId=@QuizId AND OrderNo=1);
    DECLARE @Q2 INT = (SELECT QuestionId FROM dbo.Questions WHERE QuizId=@QuizId AND OrderNo=2);

    INSERT dbo.Choices(QuestionId, [Text], IsCorrect, OrderNo)
    VALUES
    (@Q1, N'New Delhi', 1, 1), (@Q1, N'Mumbai', 0, 2), (@Q1, N'Kolkata', 0, 3), (@Q1, N'Chennai', 0, 4),
    (@Q2, N'3',        0, 1), (@Q2, N'4',      1, 2), (@Q2, N'5',      0, 3), (@Q2, N'22',     0, 4);
END
GO



