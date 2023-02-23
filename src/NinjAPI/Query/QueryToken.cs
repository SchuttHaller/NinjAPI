namespace NinjAPI.Query
{
    public class QueryToken: Token
    {
        public string Value { get; set; } = null!;

        public static QueryToken New(TokenType tokenType, string value)
        {
            return new QueryToken() { Type = tokenType, Value = value };
        }
    }
}
