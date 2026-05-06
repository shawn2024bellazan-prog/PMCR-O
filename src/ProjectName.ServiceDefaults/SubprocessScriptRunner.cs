using System.Diagnostics;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace ProjectName.ServiceDefaults;

public static class SubprocessScriptRunner
{
    /// <summary>
    /// Executes local PowerShell or Shell scripts defined in the skills folder.
    /// </summary>
    public static async Task<object?> RunAsync(
        AgentFileSkill skill,
        AgentFileSkillScript script,
        AIFunctionArguments arguments,
        CancellationToken ct)
    {
        // Resolve script environment
        string ext = Path.GetExtension(script.Name).ToLower();
        string shell = ext == ".ps1" ? "pwsh.exe" : "cmd.exe";
        string fullPath = Path.Combine(skill.Path, script.Name);

        var argList = new List<string>();
        if (ext == ".ps1")
        {
            argList.Add("-NoProfile");
            argList.Add("-NonInteractive");
            argList.Add("-File");
        }
        argList.Add(fullPath);

        // Append tool arguments from the LLM
        foreach (var arg in arguments)
        {
            if (arg.Value != null) argList.Add(arg.Value.ToString() ?? "");
        }

        var psi = new ProcessStartInfo(shell)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var a in argList) psi.ArgumentList.Add(a);

        try
        {
            using var process = Process.Start(psi);
            if (process == null) return "ERROR: Failed to start process.";

            string output = await process.StandardOutput.ReadToEndAsync(ct);
            string error = await process.StandardError.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);

            return process.ExitCode == 0 ? output.Trim() : $"ERROR (Exit {process.ExitCode}): {error}";
        }
        catch (Exception ex)
        {
            return $"CRITICAL EXECUTION FAILURE: {ex.Message}";
        }
    }
}