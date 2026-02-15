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
