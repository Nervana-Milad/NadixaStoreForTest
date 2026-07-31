namespace Nadixa.Application.DTOS.Auth
{
    public class AuthResult
    {
        public bool Succeeded { get; set; }

        public List<string> Errors { get; set; } = new();

        public static AuthResult Success()
        {
            return new AuthResult
            {
                Succeeded = true
            };
        }

        public static AuthResult Fail(string error)
        {
            return new AuthResult
            {
                Succeeded = false,
                Errors = new List<string>
                {
                    error
                }
            };
        }

        public static AuthResult Fail(
            IEnumerable<string> errors)
        {
            return new AuthResult
            {
                Succeeded = false,
                Errors = errors.ToList()
            };
        }
    }
}