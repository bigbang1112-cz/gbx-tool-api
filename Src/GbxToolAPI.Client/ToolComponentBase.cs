using Microsoft.AspNetCore.Components;

namespace GbxToolAPI.Client;

public abstract class ToolComponentBase<T> : ComponentBase where T : class, ITool
{
}
