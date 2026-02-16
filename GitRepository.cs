using System;
using System.Diagnostics;
using System.IO;

namespace GitPane
{
    public class GitRepository
    {
        private readonly string workingDirectory;

        public GitRepository(string directory)
        {
            workingDirectory = directory;
        }

        public bool IsRepository()
        {
            if (string.IsNullOrEmpty(workingDirectory) || !Directory.Exists(workingDirectory))
                return false;

            var result = ExecuteGitCommand("rev-parse --git-dir");
            return result.ExitCode == 0;
        }

        public bool InitializeRepository()
        {
            if (string.IsNullOrEmpty(workingDirectory) || !Directory.Exists(workingDirectory))
                return false;

            var result = ExecuteGitCommand("init");
            return result.ExitCode == 0;
        }

        public bool HasCommits()
        {
            var result = ExecuteGitCommand("rev-list --count HEAD");
            if (result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output))
            {
                int count;
                if (int.TryParse(result.Output.Trim(), out count))
                {
                    return count > 0;
                }
            }
            return false;
        }

        public int GetUnpushedCommitsCount()
        {
            // Check if we have a remote tracking branch
            var result = ExecuteGitCommand("rev-parse --abbrev-ref @{u}");
            if (result.ExitCode != 0)
            {
                // No upstream branch set - check if we have a remote and local commits
                if (HasRemote() && HasCommits())
                {
                    // Return total commit count (all unpushed)
                    result = ExecuteGitCommand("rev-list --count HEAD");
                    if (result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output))
                    {
                        int count;
                        if (int.TryParse(result.Output.Trim(), out count))
                        {
                            return count;
                        }
                    }
                }
                return 0;
            }

            // Count commits ahead of remote
            result = ExecuteGitCommand("rev-list --count @{u}..HEAD");
            if (result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output))
            {
                int count;
                if (int.TryParse(result.Output.Trim(), out count))
                {
                    return count;
                }
            }
            return 0;
        }

        public string GetCurrentBranch()
        {
            var result = ExecuteGitCommand("rev-parse --abbrev-ref HEAD");
            if (result.ExitCode == 0)
            {
                var branch = result.Output.Trim();
                // Handle detached HEAD state
                if (branch == "HEAD")
                {
                    // Get the short commit SHA instead
                    var shaResult = ExecuteGitCommand("rev-parse --short HEAD");
                    if (shaResult.ExitCode == 0)
                        return $"HEAD (detached at {shaResult.Output.Trim()})";
                    return "HEAD (detached)";
                }
                return branch;
            }
            return null;
        }

        public BranchInfo[] GetAllBranchesWithInfo()
        {
            // Get all branches sorted by last commit date
            var result = ExecuteGitCommand("for-each-ref --sort=-committerdate --format=%(refname:short)|%(committerdate:relative) refs/heads/ refs/remotes/");
            if (result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output))
            {
                var lines = result.Output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var branches = new System.Collections.Generic.List<BranchInfo>();
                
                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length == 2)
                    {
                        var branchName = parts[0].Trim();
                        var lastCommit = parts[1].Trim();
                        var isRemote = branchName.StartsWith("origin/") || branchName.StartsWith("remotes/");
                        
                        branches.Add(new BranchInfo
                        {
                            Name = branchName,
                            LastCommit = lastCommit,
                            IsRemote = isRemote
                        });
                    }
                }
                
                return branches.ToArray();
            }
            return new BranchInfo[0];
        }

        public bool CheckoutBranch(string branchName)
        {
            // Handle remote branches - create local tracking branch
            if (branchName.StartsWith("origin/"))
            {
                var localName = branchName.Substring(7); // Remove "origin/"
                var result = ExecuteGitCommand($"checkout -b \"{localName}\" \"{branchName}\"");
                if (result.ExitCode != 0)
                {
                    // Branch might already exist locally, try regular checkout
                    result = ExecuteGitCommand($"checkout \"{localName}\"");
                }
                return result.ExitCode == 0;
            }
            
            var checkoutResult = ExecuteGitCommand($"checkout \"{branchName}\"");
            return checkoutResult.ExitCode == 0;
        }

        public bool HasUncommittedChanges()
        {
            var result = ExecuteGitCommand("status --porcelain");
            return result.ExitCode == 0 && !string.IsNullOrWhiteSpace(result.Output);
        }

        public string GetUncommittedChangesStatus()
        {
            var result = ExecuteGitCommand("status --short");
            return result.ExitCode == 0 ? result.Output : string.Empty;
        }

        public bool StashChanges(string message = null)
        {
            var stashMessage = string.IsNullOrEmpty(message) ? "GitPane auto-stash" : message;
            var result = ExecuteGitCommand($"stash push -m \"{stashMessage}\"");
            return result.ExitCode == 0;
        }

        public bool CommitChanges(string message)
        {
            // Stage all changes
            var addResult = ExecuteGitCommand("add -A");
            if (addResult.ExitCode != 0)
                return false;

            // Commit
            var commitResult = ExecuteGitCommand($"commit -m \"{message}\"");
            return commitResult.ExitCode == 0;
        }

        public bool PushChanges()
        {
            // Get current branch
            string branch = GetCurrentBranch();
            if (string.IsNullOrEmpty(branch) || branch.Contains("HEAD"))
            {
                // Can't push from detached HEAD
                return false;
            }
            
            // Try push with upstream set (works for first push and subsequent pushes)
            var result = ExecuteGitCommand($"push -u origin {branch}");
            return result.ExitCode == 0;
        }

        public bool DiscardChanges()
        {
            // Reset tracked files
            var resetResult = ExecuteGitCommand("reset --hard HEAD");
            if (resetResult.ExitCode != 0)
                return false;

            // Clean untracked files
            var cleanResult = ExecuteGitCommand("clean -fd");
            return cleanResult.ExitCode == 0;
        }

        public string[] GetStashList()
        {
            var result = ExecuteGitCommand("stash list");
            if (result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output))
            {
                return result.Output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
            return new string[0];
        }

        public bool HasStashes()
        {
            var stashes = GetStashList();
            return stashes.Length > 0;
        }

        public bool ApplyStash(int stashIndex = 0)
        {
            var result = ExecuteGitCommand($"stash apply stash@{{{stashIndex}}}");
            return result.ExitCode == 0;
        }

        public bool PopStash(int stashIndex = 0)
        {
            var result = ExecuteGitCommand($"stash pop stash@{{{stashIndex}}}");
            return result.ExitCode == 0;
        }

        public bool DropStash(int stashIndex = 0)
        {
            var result = ExecuteGitCommand($"stash drop stash@{{{stashIndex}}}");
            return result.ExitCode == 0;
        }

        public string[] GetStagedFiles()
        {
            var result = ExecuteGitCommand("diff --name-status --cached");
            if (result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output))
            {
                return result.Output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
            return new string[0];
        }

        public string[] GetUnstagedFiles()
        {
            var result = ExecuteGitCommand("diff --name-status");
            if (result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output))
            {
                return result.Output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
            return new string[0];
        }

        public string[] GetUntrackedFiles()
        {
            var result = ExecuteGitCommand("ls-files --others --exclude-standard");
            if (result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output))
            {
                return result.Output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
            return new string[0];
        }

        public bool StageFile(string filePath)
        {
            var result = ExecuteGitCommand($"add \"{filePath}\"");
            return result.ExitCode == 0;
        }

        public bool UnstageFile(string filePath)
        {
            var result = ExecuteGitCommand($"reset HEAD \"{filePath}\"");
            return result.ExitCode == 0;
        }

        public bool StageAllFiles()
        {
            var result = ExecuteGitCommand("add -A");
            return result.ExitCode == 0;
        }

        public bool UnstageAllFiles()
        {
            var result = ExecuteGitCommand("reset HEAD");
            return result.ExitCode == 0;
        }

        public bool DiscardFile(string filePath)
        {
            // Use git restore (Git 2.23+) or fall back to checkout
            var result = ExecuteGitCommand($"restore \"{filePath}\"");
            if (result.ExitCode != 0)
            {
                // Fall back to checkout for older git versions
                result = ExecuteGitCommand($"checkout -- \"{filePath}\"");
            }
            return result.ExitCode == 0;
        }

        public bool DiscardAllFiles()
        {
            // Discard tracked file changes
            var result = ExecuteGitCommand("restore .");
            if (result.ExitCode != 0)
            {
                // Fall back to checkout for older git versions
                result = ExecuteGitCommand("checkout -- .");
            }
            
            // Also remove untracked files
            if (result.ExitCode == 0)
            {
                ExecuteGitCommand("clean -fd");
            }
            
            return result.ExitCode == 0;
        }

        public string GetRepositoryName()
        {
            // Try to get from origin URL first
            var result = ExecuteGitCommand("config --get remote.origin.url");
            if (result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output))
            {
                var url = result.Output.Trim();
                // Extract name from URL (handle both HTTPS and SSH)
                var name = url;
                if (name.EndsWith(".git"))
                    name = name.Substring(0, name.Length - 4);
                
                var lastSlash = name.LastIndexOf('/');
                if (lastSlash >= 0)
                    name = name.Substring(lastSlash + 1);
                
                return name;
            }

            // Fallback to directory name
            return Path.GetFileName(workingDirectory);
        }

        public bool HasRemote(string remoteName = "origin")
        {
            var result = ExecuteGitCommand("remote -v");
            return result.ExitCode == 0 && result.Output.Contains(remoteName);
        }

        public string GetRemoteUrl(string remoteName = "origin")
        {
            var result = ExecuteGitCommand($"config --get remote.{remoteName}.url");
            return result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output) ? result.Output.Trim() : null;
        }

        public bool AddRemote(string remoteName, string url)
        {
            var result = ExecuteGitCommand($"remote add {remoteName} \"{url}\"");
            return result.ExitCode == 0;
        }

        public bool RemoveRemote(string remoteName = "origin")
        {
            var result = ExecuteGitCommand($"remote remove {remoteName}");
            return result.ExitCode == 0;
        }

        public bool AddToGitignore(string pattern)
        {
            try
            {
                string gitignorePath = Path.Combine(workingDirectory, ".gitignore");
                
                // Read existing content to check for duplicates
                string existingContent = "";
                if (File.Exists(gitignorePath))
                {
                    existingContent = File.ReadAllText(gitignorePath);
                    
                    // Check if pattern already exists
                    var lines = existingContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        if (line.Trim() == pattern.Trim())
                            return true; // Already exists, consider it success
                    }
                }
                
                // Append pattern
                using (var writer = File.AppendText(gitignorePath))
                {
                    // Add newline before if file exists and doesn't end with newline
                    if (!string.IsNullOrEmpty(existingContent) && !existingContent.EndsWith("\n"))
                        writer.WriteLine();
                    
                    writer.WriteLine(pattern);
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public GitCommitInfo[] GetCommitHistory(int maxCount = 100)
        {
            // Get commit log with custom format: hash|author|date|subject
            string format = "%H|%an|%ai|%s";
            var result = ExecuteGitCommand($"log --max-count={maxCount} --pretty=format:\"{format}\"");
            
            if (result.ExitCode != 0 || string.IsNullOrEmpty(result.Output))
                return new GitCommitInfo[0];
            
            var lines = result.Output.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var commits = new GitCommitInfo[lines.Length];
            
            for (int i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split('|');
                if (parts.Length >= 4)
                {
                    commits[i] = new GitCommitInfo
                    {
                        Hash = parts[0],
                        ShortHash = parts[0].Substring(0, Math.Min(7, parts[0].Length)),
                        Author = parts[1],
                        Date = parts[2],
                        Subject = parts[3]
                    };
                }
            }
            
            return commits;
        }

        public GitCommitDetails GetCommitDetails(string commitHash)
        {
            if (string.IsNullOrEmpty(commitHash))
                return null;
            
            // Get commit details
            var detailsResult = ExecuteGitCommand($"show --stat --format=\"%H%n%an%n%ai%n%s%n%b\" {commitHash}");
            if (detailsResult.ExitCode != 0)
                return null;
            
            var lines = detailsResult.Output.Split(new[] { '\n' }, StringSplitOptions.None);
            if (lines.Length < 4)
                return null;
            
            // Parse the output
            var details = new GitCommitDetails
            {
                Hash = lines[0].Trim(),
                Author = lines[1].Trim(),
                Date = lines[2].Trim(),
                Subject = lines[3].Trim()
            };
            
            // Find message body (lines after subject until first empty line or stats)
            int bodyStart = 4;
            int bodyEnd = bodyStart;
            for (int i = bodyStart; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]) || lines[i].Contains(" file") || lines[i].Contains(" changed"))
                    break;
                bodyEnd = i + 1;
            }
            
            if (bodyEnd > bodyStart)
            {
                details.Body = string.Join("\n", lines, bodyStart, bodyEnd - bodyStart).Trim();
            }
            
            // Get stats summary
            var statsResult = ExecuteGitCommand($"show --shortstat --format=\"\" {commitHash}");
            if (statsResult.ExitCode == 0 && !string.IsNullOrEmpty(statsResult.Output))
            {
                details.Stats = statsResult.Output.Trim();
            }
            
            // Get diff
            var diffResult = ExecuteGitCommand($"show --format=\"\" {commitHash}");
            if (diffResult.ExitCode == 0)
            {
                details.Diff = diffResult.Output;
            }
            
            return details;
        }

        public bool IsGitHubCLIInstalled()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "gh",
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool IsGitHubCLIAuthenticated()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "gh",
                    Arguments = "auth status",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public GitCommandResult CreateGitHubRepo(string repoName, bool isPrivate, string description = "")
        {
            string visibility = isPrivate ? "--private" : "--public";
            string descArg = string.IsNullOrEmpty(description) ? "" : $"--description \"{description}\"";
            
            // Create repo and set as remote origin
            string arguments = $"repo create {repoName} {visibility} {descArg} --source=. --remote=origin";
            
            return ExecuteGitHubCLICommand(arguments);
        }

        private GitCommandResult ExecuteGitHubCLICommand(string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "gh",
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    return new GitCommandResult
                    {
                        ExitCode = process.ExitCode,
                        Output = output,
                        Error = error
                    };
                }
            }
            catch (Exception ex)
            {
                return new GitCommandResult 
                { 
                    ExitCode = -1,
                    Error = ex.Message
                };
            }
        }

        private GitCommandResult ExecuteGitCommand(string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    return new GitCommandResult
                    {
                        ExitCode = process.ExitCode,
                        Output = output,
                        Error = error
                    };
                }
            }
            catch (Exception)
            {
                return new GitCommandResult { ExitCode = -1 };
            }
        }

        public class GitCommandResult
        {
            public int ExitCode { get; set; }
            public string Output { get; set; }
            public string Error { get; set; }
        }

        public class GitCommitInfo
        {
            public string Hash { get; set; }
            public string ShortHash { get; set; }
            public string Author { get; set; }
            public string Date { get; set; }
            public string Subject { get; set; }
        }

        public class GitCommitDetails
        {
            public string Hash { get; set; }
            public string Author { get; set; }
            public string Date { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
            public string Stats { get; set; }
            public string Diff { get; set; }
        }
    }
}
