using SoftVelocity.BinaryToText;

namespace SoftVelocity.Generator;

internal class AppWatcherDetails : IBinaryWatcher
{
	public string AddDirectoryToolTip => "Clarion.Generator.ImportExport.SelectFolder.Tooltip";

	public string BinaryExtension => "app";

	public string DefaultTextExtension => "apv";

	public string FileDialogExtensionName => "Clarion.Generator.ImportExport.ExtensionName";

	public string FileDialogExtraExtensions => ".txa";

	public string FileDialogName => "Clarion.Generator.ImportExport.Name";

	public string MonitorAllText => "Clarion.Generator.ImportExport.UseAll";

	public string MonitorAllToolTip => "Clarion.Generator.ImportExport.UseAll.Tooltip";

	public string MonitorSelectedText => "Clarion.Generator.ImportExport.UseSelected";

	public string MonitorSelectedToolTip => "Clarion.Generator.ImportExport.UseSelected.Tooltip";

	public string Name => "Application";

	public string TabName => "Clarion.Generator.ImportExport.Tab";
}
