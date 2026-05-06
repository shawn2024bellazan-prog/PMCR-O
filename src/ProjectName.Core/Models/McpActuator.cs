using System.Text.Json.Serialization;

namespace ProjectName.Core.Models;

/// <summary>
/// I AM the McpActuator enum.
/// I define the valid physical and virtual hands the Orchestrator can use to change the world.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum McpActuator 
{ 
    filesystem, 
    terminal, 
    playwright, 
    postman 
}