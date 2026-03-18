using System.Collections.Immutable;
using System.Reflection;

namespace ViennaDotNet.LauncherUI;

public static class Permissions
{
    [PermissionInfo("Server", "Start the server")]
    public const string StartServer = "server.start";
    [PermissionInfo("Server", "Restart the server")]
    public const string RestartServer = "server.restart";
    [PermissionInfo("Server", "Stop the server")]
    public const string StopServer = "server.stop";
    [PermissionInfo("Server", "Edit the server options")]
    public const string EditServerOptions = "server.options.edit";
    [PermissionInfo("Server", "View the logs of the server")]
    public const string ViewServerLogs = "server.view.logs";

    [PermissionInfo("Roles", "Manage roles")]
    public const string ManageRoles = "role.manage";

    public static readonly ImmutableArray<string> All;
    public static readonly ImmutableArray<PermissionDescriptor> AllWithInfo;

    static Permissions()
    {
        var fields = typeof(Permissions)
             .GetFields(BindingFlags.Public | BindingFlags.Static)
             .Where(f => f is { IsLiteral: true, IsInitOnly: false } &&
                         f.FieldType == typeof(string));

        All = [.. fields.Select(f => (string)f.GetRawConstantValue()!)];

        AllWithInfo = [.. fields.Select(f =>
        {
            var attr = f.GetCustomAttribute<PermissionInfoAttribute>();

            return new PermissionDescriptor(
                (string)f.GetRawConstantValue()!,
                attr?.Category ?? "Other",
                attr?.Description ?? ""
            );
        })];
    }

    public readonly record struct PermissionDescriptor(
        string Name,
        string Category,
        string Description
    );

    [AttributeUsage(AttributeTargets.Field)]
    public class PermissionInfoAttribute : Attribute
    {
        public string Category { get; }
        public string Description { get; }

        public PermissionInfoAttribute(string category, string description)
        {
            Category = category;
            Description = description;
        }
    }
}