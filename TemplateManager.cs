using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace GitPane
{
    public class TemplateManager
    {
        private string templatesPath;
        private string templatesFile;
        private List<GitTemplate> gitignoreTemplates;
        private List<GitTemplate> gitattributesTemplates;

        public TemplateManager(string addInPath)
        {
            templatesPath = Path.Combine(addInPath, "templates");
            templatesFile = Path.Combine(templatesPath, "templates.json");
            
            EnsureTemplatesFolder();
            LoadTemplates();
        }

        private void EnsureTemplatesFolder()
        {
            if (!Directory.Exists(templatesPath))
            {
                Directory.CreateDirectory(templatesPath);
            }
        }

        public List<GitTemplate> GetTemplates(TemplateType type)
        {
            return type == TemplateType.Gitignore 
                ? new List<GitTemplate>(gitignoreTemplates) 
                : new List<GitTemplate>(gitattributesTemplates);
        }

        public GitTemplate GetDefaultTemplate(TemplateType type)
        {
            var templates = type == TemplateType.Gitignore 
                ? gitignoreTemplates 
                : gitattributesTemplates;
            
            return templates.FirstOrDefault(t => t.IsDefault);
        }

        public void SaveTemplate(GitTemplate template, TemplateType type)
        {
            if (template == null)
                return;

            var templates = type == TemplateType.Gitignore 
                ? gitignoreTemplates 
                : gitattributesTemplates;

            // If setting as default, unset others
            if (template.IsDefault)
            {
                foreach (var t in templates)
                {
                    t.IsDefault = false;
                }
            }

            // Update or add
            var existing = templates.FirstOrDefault(t => t.Id == template.Id);
            if (existing != null)
            {
                existing.Name = template.Name;
                existing.Description = template.Description;
                existing.IsDefault = template.IsDefault;
                existing.Content = template.Content;
            }
            else
            {
                templates.Add(template);
            }

            SaveToFile();
        }

        public void DeleteTemplate(string id, TemplateType type)
        {
            var templates = type == TemplateType.Gitignore 
                ? gitignoreTemplates 
                : gitattributesTemplates;

            var template = templates.FirstOrDefault(t => t.Id == id);
            if (template != null)
            {
                templates.Remove(template);
                
                // Ensure at least one remains, and if we deleted the default, set a new one
                if (templates.Count > 0 && template.IsDefault)
                {
                    templates[0].IsDefault = true;
                }
                
                SaveToFile();
            }
        }

        private void LoadTemplates()
        {
            if (File.Exists(templatesFile))
            {
                try
                {
                    string json = File.ReadAllText(templatesFile);
                    var serializer = new JavaScriptSerializer();
                    var data = serializer.Deserialize<Dictionary<string, object>>(json);

                    gitignoreTemplates = DeserializeTemplates(data, "gitignoreTemplates");
                    gitattributesTemplates = DeserializeTemplates(data, "gitattributesTemplates");

                    // Validate loaded templates
                    if (gitignoreTemplates.Count == 0 || gitattributesTemplates.Count == 0)
                    {
                        CreateDefaultTemplates();
                    }
                    return;
                }
                catch
                {
                    // If JSON is corrupted, fall back to defaults
                }
            }

            CreateDefaultTemplates();
        }

        private List<GitTemplate> DeserializeTemplates(Dictionary<string, object> data, string key)
        {
            var templates = new List<GitTemplate>();
            
            if (data.ContainsKey(key) && data[key] is object[] array)
            {
                foreach (var item in array)
                {
                    if (item is Dictionary<string, object> dict)
                    {
                        var template = new GitTemplate
                        {
                            Id = dict.ContainsKey("id") ? dict["id"].ToString() : Guid.NewGuid().ToString(),
                            Name = dict.ContainsKey("name") ? dict["name"].ToString() : "",
                            Description = dict.ContainsKey("description") ? dict["description"].ToString() : "",
                            IsDefault = dict.ContainsKey("isDefault") && Convert.ToBoolean(dict["isDefault"]),
                            Content = dict.ContainsKey("content") ? dict["content"].ToString() : ""
                        };
                        templates.Add(template);
                    }
                }
            }
            
            return templates;
        }

        private void CreateDefaultTemplates()
        {
            gitignoreTemplates = new List<GitTemplate>();
            gitattributesTemplates = new List<GitTemplate>();

            // Default .gitignore templates
            gitignoreTemplates.Add(new GitTemplate
            {
                Name = "Clarion Basic",
                Description = "Standard Clarion project with typical ignore patterns",
                IsDefault = true,
                Content = @"# Compiled outputs
*.dll
*.exe
*.lib
*.obj

# Build directories
obj/
map/
bin/

# IDE files
*.sln.cache
*.suo
*.user

# Debug files
*.pdb
*.dbg"
            });

            gitignoreTemplates.Add(new GitTemplate
            {
                Name = "Clarion + Redirection",
                Description = "For projects using bin/obj folder redirection",
                IsDefault = false,
                Content = @"# Redirected output folders
bin/
obj/
map/

# Cache files
*.sln.cache
*.suo
*.user"
            });

            gitignoreTemplates.Add(new GitTemplate
            {
                Name = "Clarion Minimal",
                Description = "Minimal ignore for tracking most files",
                IsDefault = false,
                Content = @"# Executables only
*.exe
*.dll

# Cache
*.sln.cache"
            });

            gitignoreTemplates.Add(new GitTemplate
            {
                Name = "Clarion + C# Mixed",
                Description = "Mixed Clarion and C# projects",
                IsDefault = false,
                Content = @"# Clarion outputs
*.dll
*.exe
*.lib
obj/
map/
bin/

# C# outputs
[Bb]in/
[Oo]bj/
*.user
*.suo
.vs/

# Common
*.sln.cache"
            });

            // Default .gitattributes templates
            gitattributesTemplates.Add(new GitTemplate
            {
                Name = "Clarion Standard",
                Description = "Standard Clarion text file attributes",
                IsDefault = true,
                Content = @"# Clarion source files
*.clw text eol=crlf
*.inc text eol=crlf
*.int text eol=crlf
*.equ text eol=crlf

# Project files
*.cwproj text eol=crlf
*.sln text eol=crlf

# Binary files
*.dll binary
*.exe binary
*.lib binary"
            });

            gitattributesTemplates.Add(new GitTemplate
            {
                Name = "Clarion Basic",
                Description = "Basic Clarion files only",
                IsDefault = false,
                Content = @"*.clw text eol=crlf
*.cwproj text eol=crlf
*.sln text eol=crlf"
            });

            gitattributesTemplates.Add(new GitTemplate
            {
                Name = "Clarion + C# Mixed",
                Description = "Mixed Clarion and C# projects",
                IsDefault = false,
                Content = @"# Clarion
*.clw text eol=crlf
*.inc text eol=crlf
*.int text eol=crlf
*.equ text eol=crlf
*.cwproj text eol=crlf

# C#
*.cs text eol=crlf
*.csproj text eol=crlf

# Solution
*.sln text eol=crlf"
            });

            gitattributesTemplates.Add(new GitTemplate
            {
                Name = "Clarion Minimal",
                Description = "Minimal attributes, let Git auto-detect most files",
                IsDefault = false,
                Content = @"*.clw text eol=crlf"
            });

            SaveToFile();
        }

        private void SaveToFile()
        {
            try
            {
                var data = new Dictionary<string, object>
                {
                    { "version", "1.0" },
                    { "gitignoreTemplates", SerializeTemplates(gitignoreTemplates) },
                    { "gitattributesTemplates", SerializeTemplates(gitattributesTemplates) }
                };

                var serializer = new JavaScriptSerializer();
                string json = serializer.Serialize(data);
                
                // Pretty print JSON
                json = FormatJson(json);
                
                File.WriteAllText(templatesFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to save templates: " + ex.Message, ex);
            }
        }

        private List<Dictionary<string, object>> SerializeTemplates(List<GitTemplate> templates)
        {
            var result = new List<Dictionary<string, object>>();
            
            foreach (var template in templates)
            {
                result.Add(new Dictionary<string, object>
                {
                    { "id", template.Id },
                    { "name", template.Name },
                    { "description", template.Description },
                    { "isDefault", template.IsDefault },
                    { "content", template.Content }
                });
            }
            
            return result;
        }

        private string FormatJson(string json)
        {
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();

            for (var i = 0; i < json.Length; i++)
            {
                var ch = json[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            sb.Append(new string(' ', ++indent * 2));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            sb.Append(new string(' ', --indent * 2));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && json[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            sb.Append(new string(' ', indent * 2));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }

        public void EnsureDefaultTemplates()
        {
            if (gitignoreTemplates.Count == 0 || gitattributesTemplates.Count == 0)
            {
                CreateDefaultTemplates();
            }
        }
    }
}
