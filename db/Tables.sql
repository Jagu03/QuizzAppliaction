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