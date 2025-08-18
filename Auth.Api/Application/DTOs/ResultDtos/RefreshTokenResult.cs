namespace Auth.Api.Application.DTOs.ResultDtos
{
    public class RefreshTokenResult
    {

        public bool Success { get; private set; }
        public string? Error { get; private set; }
        public string? AccessToken { get; private set; }
        public string? RefreshToken { get; private set; }

        private RefreshTokenResult() { }

        public static RefreshTokenResult SuccessResult(string accessToken, string refreshToken) =>
            new RefreshTokenResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

        public static RefreshTokenResult Failure(string error) =>
            new RefreshTokenResult
            {
                Success = false,
                Error = error
            };
    }
}
