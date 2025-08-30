namespace QuizApi.Dtos
{
    public record ChoiceDto(int ChoiceId, string Text, bool IsCorrect, int OrderNo);

    public record QuestionDto(
        int QuestionId,
        int QuizId,
        string Text,
        byte QuestionType,
        int TimeLimitSec,
        int Points,
        string? MediaUrl,
        int OrderNo,
        IEnumerable<ChoiceDto> Choices
    );
}