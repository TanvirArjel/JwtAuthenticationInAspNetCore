namespace JwtAuthentication.Api.ApiModels.AuthenticationModels
{
    public class JsonWebTokenModel
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public string AccessToken { get; set; }

        public string ExpireIn { get; set; }
    }
}
