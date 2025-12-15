namespace RusticaPortal_PRMVAN.Api.Swagger
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string input)
        {
            if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
            {
                return input;
            }
            var chars = input.ToCharArray();
            chars[0] = char.ToLowerInvariant(input[0]);

            return new(chars);
        }
    }
}
