using System;
using System.Diagnostics;
using System.IO;

namespace GitPane
{
    public class GitRepository
    {
        private readonly string workingDirectory;
        private static bool? gitAvailable = null;

        public GitRepository(string directory)
        {
            workingDirectory = directory;
        }

        public string GetWorkingDirectory()
        {
            return workingDirectory;
        }

        public static bool IsGitAvailable()
        {
            // Cache the result to avoid repeated checks
            if (gitAvailable.HasValue)
                return gitAvailable.Value;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                    gitAvailable = process.ExitCode == 0;
                    return gitAvailable.Value;
                }
            }
            catch (Exception)
            {
                gitAvailable = false;
                return false;
            }
        }

        public static bool IsGitHubCLIAvailable()
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
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsRepository()
        {
            if (string.IsNullOrEmpty(workingDirectory) || !Directory.Exists(workingDirectory))
                return false;

            var result = ExecuteGitCommand("rev-parse --git-dir");
            return result.ExitCode == 0;
        }

        public GitCommandResult InitializeRepository()
        {
            if (string.IsNullOrEmpty(workingDirectory) || !Directory.Exists(workingDirectory))
            {
                return new GitCommandResult 
                { 
                    ExitCode = -1,
                    Output = "", 
                    Error = "Working directory is invalid or does not exist"
                };
            }

            // Initialize the repository
            var initResult = ExecuteGitCommand("init");
            if (initResult.ExitCode != 0)
                return initResult;

            // Create initial commit to establish master/main branch
            // This allows immediate branching and avoids "no branch" state
            var commitResult = ExecuteGitCommand("commit --allow-empty -m \"Initial commit\"");
            
            return commitResult;
        }

        public bool CreateGitignoreFile(string content)
        {
            try
            {
                string gitignorePath = Path.Combine(workingDirectory, ".gitignore");
                File.WriteAllText(gitignorePath, content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CreateGitattributesFile(string content)
        {
            try
            {
                string gitattributesPath = Path.Combine(workingDirectory, ".gitattributes");
                File.WriteAllText(gitattributesPath, content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ReplaceGitignoreFile(string content)
        {
            try
            {
                string gitignorePath = Path.Combine(workingDirectory, ".gitignore");
                string backupPath = Path.Combine(workingDirectory, ".gitignore.backup");
                
                if (File.Exists(gitignorePath))
                {
                    File.Copy(gitignorePath, backupPath, true);
                }
                
                File.WriteAllText(gitignorePath, content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ReplaceGitattributesFile(string content)
        {
            try
            {
                string gitattributesPath = Path.Combine(workingDirectory, ".gitattributes");
                string backupPath = Path.Combine(workingDirectory, ".gitattributes.backup");
                
                if (File.Exists(gitattributesPath))
                {
                    File.Copy(gitattributesPath, backupPath, true);
                }
                
                File.WriteAllText(gitattributesPath, content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool MergeGitignoreFile(string content, string templateName)
        {
            try
            {
                string gitignorePath = Path.Combine(workingDirectory, ".gitignore");
                string existing = File.Exists(gitignorePath) ? File.ReadAllText(gitignorePath) : "";
                
                string merged = existing;
                if (!merged.EndsWith("\n"))
                    merged += "\n";
                
                merged += $"\n# --- Added from template \"{templateName}\" on {DateTime.Now:yyyy-MM-dd} ---\n";
                merged += content;
                
                File.WriteAllText(gitignorePath, merged);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool MergeGitattributesFile(string content, string templateName)
        {
            try
            {
                string gitattributesPath = Path.Combine(workingDirectory, ".gitattributes");
                string existing = File.Exists(gitattributesPath) ? File.ReadAllText(gitattributesPath) : "";
                
                string merged = existing;
                if (!merged.EndsWith("\n"))
                    merged += "\n";
                
                merged += $"\n# --- Added from template \"{templateName}\" on {DateTime.Now:yyyy-MM-dd} ---\n";
                merged += content;
                
                File.WriteAllText(gitattributesPath, merged);
                return true;
            }
            catch
            {
                return false;
            }
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
                // No upstream configured — we cannot determine what has been pushed
                return 0;
            }

            // Count commits ahead of remote
            result = ExecuteGitCommand("rev-list --count @{u}..HEAD");
            if (result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output))
            {
                int count;
                if (int.TryParse(result.Output.Trim(), out count))
                    return count;
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
            var currentBranch = GetCurrentBranch();
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
                        var info = new BranchInfo
                        {
                            Name       = branchName,
                            LastCommit = lastCommit,
                            IsRemote   = isRemote,
                            IsCurrent  = branchName == currentBranch
                        };

                        // Ahead/behind counts for local branches that have a remote counterpart
                        if (!isRemote)
                        {
                            var countResult = ExecuteGitCommand(
                                $"rev-list --left-right --count origin/{EscapeGitArg(branchName)}...{EscapeGitArg(branchName)}");
                            if (countResult.ExitCode == 0 && !string.IsNullOrEmpty(countResult.Output))
                            {
                                var counts = countResult.Output.Trim().Split('\t');
                                if (counts.Length == 2)
                                {
                                    int.TryParse(counts[0], out int behind);
                                    int.TryParse(counts[1], out int ahead);
                                    info.BehindCount = behind;
                                    info.AheadCount  = ahead;
                                }
                            }
                        }

                        branches.Add(info);
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
                var result = ExecuteGitCommand($"checkout -b \"{EscapeGitArg(localName)}\" \"{EscapeGitArg(branchName)}\"");
                if (result.ExitCode != 0)
                {
                    // Branch might already exist locally, try regular checkout
                    result = ExecuteGitCommand($"checkout \"{EscapeGitArg(localName)}\"");
                }
                return result.ExitCode == 0;
            }
            
            var checkoutResult = ExecuteGitCommand($"checkout \"{EscapeGitArg(branchName)}\"");
            return checkoutResult.ExitCode == 0;
        }

        public GitCommandResult CreateBranch(string branchName, bool checkout = true)
        {
            if (checkout)
                return ExecuteGitCommand($"checkout -b \"{EscapeGitArg(branchName)}\"");
            else
                return ExecuteGitCommand($"branch \"{EscapeGitArg(branchName)}\"");
        }

        public GitCommandResult DeleteBranch(string branchName, bool force = false)
        {
            string flag = force ? "-D" : "-d";
            return ExecuteGitCommand($"branch {flag} \"{EscapeGitArg(branchName)}\"");
        }

        public GitCommandResult MergeBranch(string branchName)
        {
            return ExecuteGitCommand($"merge \"{EscapeGitArg(branchName)}\"");
        }

        /// <summary>Returns files with merge conflicts (unmerged paths).</summary>
        public string[] GetConflictedFiles()
        {
            var result = ExecuteGitCommand("diff --name-only --diff-filter=U");
            if (result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output))
                return result.Output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new string[0];
        }

        /// <summary>Returns true if a merge is currently in progress (MERGE_HEAD exists).</summary>
        public bool IsMergeInProgress()
        {
            return File.Exists(Path.Combine(workingDirectory, ".git", "MERGE_HEAD"));
        }

        public GitCommandResult AbortMerge()
        {
            return ExecuteGitCommand("merge --abort");
        }

        /// <summary>Commits the current merge with the default merge message.</summary>
        public GitCommandResult CommitMerge()
        {
            return ExecuteGitCommand("commit --no-edit");
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
            var result = ExecuteGitCommand($"stash push -m \"{EscapeGitArg(stashMessage)}\"");
            return result.ExitCode == 0;
        }

        public bool CommitChanges(string message)
        {
            // Stage all changes
            var addResult = ExecuteGitCommand("add -A");
            if (addResult.ExitCode != 0)
                return false;

            // Commit
            var commitResult = ExecuteGitCommand($"commit -m \"{EscapeGitArg(message)}\"");
            return commitResult.ExitCode == 0;
        }

        public GitCommandResult PushChanges()
        {
            // Get current branch
            string branch = GetCurrentBranch();
            if (string.IsNullOrEmpty(branch) || branch.Contains("HEAD"))
            {
                // Can't push from detached HEAD
                return new GitCommandResult { ExitCode = 1, Error = "Cannot push from detached HEAD state" };
            }
            
            // Try push with upstream set (works for first push and subsequent pushes)
            return ExecuteGitCommand($"push -u origin \"{EscapeGitArg(branch)}\"");
        }

        public GitCommandResult Fetch()
        {
            return ExecuteGitCommand("fetch");
        }

        public GitCommandResult Pull()
        {
            return ExecuteGitCommand("pull");
        }

        /// <remarks>
        /// DESTRUCTIVE: permanently discards all uncommitted changes and deletes
        /// untracked files. Callers must show a confirmation dialog before calling.
        /// </remarks>
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
                return result.Output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new string[0];
        }

        public StashEntry[] GetStashEntries()
        {
            var result = ExecuteGitCommand("stash list --format=%gd|%s|%cr");
            var entries = new System.Collections.Generic.List<StashEntry>();
            if (result.ExitCode != 0 || string.IsNullOrEmpty(result.Output))
                return entries.ToArray();

            int index = 0;
            foreach (var line in result.Output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.Split(new[] { '|' }, 3);
                entries.Add(new StashEntry
                {
                    Index    = index++,
                    Ref      = parts.Length > 0 ? parts[0].Trim() : $"stash@{{{index - 1}}}",
                    Message  = parts.Length > 1 ? parts[1].Trim() : string.Empty,
                    Relative = parts.Length > 2 ? parts[2].Trim() : string.Empty,
                });
            }
            return entries.ToArray();
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
            var result = ExecuteGitCommand($"add \"{EscapeGitArg(filePath)}\"");
            return result.ExitCode == 0;
        }

        public bool UnstageFile(string filePath)
        {
            var result = ExecuteGitCommand($"reset HEAD \"{EscapeGitArg(filePath)}\"");
            return result.ExitCode == 0;
        }

        public bool StageAllFiles()
        {
            var result = ExecuteGitCommand("add -A");
            return result.ExitCode == 0;
        }

        public bool UnstageAllFiles()
        {
            // Use git reset if repo has commits, otherwise just rm from index
            if (HasCommits())
            {
                var result = ExecuteGitCommand("reset HEAD");
                return result.ExitCode == 0;
            }
            else
            {
                // For new repos without commits, remove from index
                var result = ExecuteGitCommand("rm --cached -r .");
                return result.ExitCode == 0;
            }
        }

        public bool DiscardFile(string filePath)
        {
            var result = ExecuteGitCommand($"restore \"{EscapeGitArg(filePath)}\"");
            if (result.ExitCode != 0)
                result = ExecuteGitCommand($"checkout -- \"{EscapeGitArg(filePath)}\"");
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
            var result = ExecuteGitCommand($"config --get remote.{EscapeGitArg(remoteName)}.url");
            return result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output) ? result.Output.Trim() : null;
        }

        public bool AddRemote(string remoteName, string url)
        {
            var result = ExecuteGitCommand($"remote add \"{EscapeGitArg(remoteName)}\" \"{EscapeGitArg(url)}\"");
            return result.ExitCode == 0;
        }

        public bool RemoveRemote(string remoteName = "origin")
        {
            var result = ExecuteGitCommand($"remote remove \"{EscapeGitArg(remoteName)}\"");
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
            var detailsResult = ExecuteGitCommand($"show --stat --format=\"%H%n%an%n%ai%n%s%n%b\" \"{EscapeGitArg(commitHash)}\"");
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
            var statsResult = ExecuteGitCommand($"show --shortstat --format=\"\" \"{EscapeGitArg(commitHash)}\"");
            if (statsResult.ExitCode == 0 && !string.IsNullOrEmpty(statsResult.Output))
            {
                details.Stats = statsResult.Output.Trim();
            }
            
            // Get diff
            var diffResult = ExecuteGitCommand($"show --format=\"\" \"{EscapeGitArg(commitHash)}\"");
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
            string descArg = string.IsNullOrEmpty(description) ? "" : $"--description \"{EscapeGitArg(description)}\"";
            string arguments = $"repo create \"{EscapeGitArg(repoName)}\" {visibility} {descArg} --source=. --remote=origin";
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
                    string capturedError = null;
                    var stderrTask = System.Threading.Tasks.Task.Factory.StartNew(
                        () => capturedError = process.StandardError.ReadToEnd());
                    var output = process.StandardOutput.ReadToEnd();
                    stderrTask.Wait();
                    process.WaitForExit();

                    return new GitCommandResult
                    {
                        ExitCode = process.ExitCode,
                        Output = output,
                        Error = capturedError ?? string.Empty
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

        public static GitCommandResult ExecuteGitCommand(string arguments, string workingDirectory)
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
                    // Read both streams concurrently to avoid deadlock when either
                    // buffer fills while the other is being read synchronously.
                    string capturedError = null;
                    var stderrTask = System.Threading.Tasks.Task.Factory.StartNew(
                        () => capturedError = process.StandardError.ReadToEnd());
                    var output = process.StandardOutput.ReadToEnd();
                    stderrTask.Wait();
                    process.WaitForExit();

                    return new GitCommandResult
                    {
                        ExitCode = process.ExitCode,
                        Output = output,
                        Error = capturedError ?? string.Empty
                    };
                }
            }
            catch (Exception)
            {
                return new GitCommandResult { ExitCode = -1 };
            }
        }

        private GitCommandResult ExecuteGitCommand(string arguments)
        {
            return ExecuteGitCommand(arguments, workingDirectory);
        }

        /// <summary>
        /// Escapes a user-supplied string for safe inclusion inside a double-quoted
        /// git argument (e.g. commit message, stash message).
        /// Escapes backslashes first, then double-quotes.
        /// </summary>
        internal static string EscapeGitArg(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
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
