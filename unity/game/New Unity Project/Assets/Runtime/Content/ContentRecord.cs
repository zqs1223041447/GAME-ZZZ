namespace Game.Runtime.Content
{
    public sealed class ContentRecord
    {
        public ContentId Id { get; }
        public string DisplayName { get; }

        public ContentRecord(ContentId id, string displayName)
        {
            Id = id;
            DisplayName = displayName ?? string.Empty;
        }
    }
}
