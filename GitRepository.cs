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
            var result = ExecuteGitCommand("push");
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

        private class GitCommandResult
        {
            public int ExitCode { get; set; }
            public string Output { get; set; }
            public string Error { get; set; }
        }
    }
}
