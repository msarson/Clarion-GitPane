using System;

namespace GitPane
{
    public class GitTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public string Content { get; set; }

        public GitTemplate()
        {
            Id = Guid.NewGuid().ToString();
            IsDefault = false;
        }
    }

    public enum TemplateType
    {
        Gitignore,
        Gitattributes
    }
}
