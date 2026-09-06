using System;

namespace Game.Runtime.Content
{
    public readonly struct ContentId : IEquatable<ContentId>
    {
        public string Value { get; }

        ContentId(string value)
        {
            Value = value;
        }

        public static bool TryParse(string raw, out ContentId id)
        {
            id = default;
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            string trimmed = raw.Trim();
            int dot = trimmed.IndexOf('.');
            if (dot <= 0 || dot >= trimmed.Length - 1)
                return false;

            id = new ContentId(trimmed);
            return true;
        }

        public bool Equals(ContentId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is ContentId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value != null ? StringComparer.Ordinal.GetHashCode(Value) : 0;
        }

        public override string ToString()
        {
            return Value ?? string.Empty;
        }
    }
}
