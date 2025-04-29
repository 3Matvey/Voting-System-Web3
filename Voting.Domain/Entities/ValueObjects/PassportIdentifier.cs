using System.Text.RegularExpressions;

namespace Voting.Domain.Entities.ValueObjects
{
    public readonly partial record struct PassportIdentifier
    {
        public string Value { get; }

        public PassportIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !MyRegex1().IsMatch(value))
                throw new ArgumentException("Invalid passport format", nameof(value));
            Value = value;
        }

        public override string ToString() => Value;

        [GeneratedRegex("^[A-Z0-9]{6,10}$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex MyRegex1();
    }
}
