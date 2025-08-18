namespace Auth.Api.Application.DTOs.ResultDtos
{
    public class RegisterResult
    {
        public bool Succeeded { get; private set; }
        public string? Error { get; private set; }

        public static RegisterResult Success() =>
            new() { Succeeded = true };

        public static RegisterResult Failure(string error) =>
            new() { Succeeded = false, Error = error };
    }
}
