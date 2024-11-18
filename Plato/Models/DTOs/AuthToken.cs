namespace Plato.Models.DTOs
{
    public sealed record AuthToken
    {
        public AuthToken(string accessToken, string? refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public string AccessToken { get; set; }

        public string? RefreshToken { get; set; }
    }
}
