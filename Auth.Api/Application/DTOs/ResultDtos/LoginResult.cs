namespace Auth.Api.Application.DTOs.ResultDtos
{
    public class LoginResult
    {
         
        public bool Succeeded { get; private set; }
        public string? Error { get; private set; }
        public string? AccessToken { get; private set; }
        public string? RefreshToken { get; private set; }

        public static LoginResult SuccessResult(string accessToken, string refreshToken) =>
            new() { Succeeded = true, AccessToken = accessToken, RefreshToken = refreshToken };

        public static LoginResult Failure(string error) =>
            new() { Succeeded = false, Error = error };
    }
}
