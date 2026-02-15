using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;

namespace SoftVelocity.Generator.UI;

[DesignTimeVisible(true)]
[ToolboxItem(true)]
public sealed class ApplicationMainWindowControl : CWControl_Host
{
	private enum AppCommandIndex
	{
		CmdAppImportFromApplication = 1,
		CmdAppImportText,
		CmdAppExportText,
		CmdAppSelectiveExport,
		CmdAppExportProject,
		CmdAppChangeDictionary,
		CmdAppInsertModule,
		CmdAppSynchronize,
		CmdAppRefresh,
		CmdAppRedistributeProcs,
		CmdAppRepopulateModules,
		CmdAppRenumberModules,
		CmdAppDeleteEmptyModules,
		CmdAppDeleteEmptyLibs,
		CmdAppNewProcedure,
		CmdAppDeleteProcedure,
		CmdAppCopyProcedure,
		CmdAppSynchronizeProcedure,
		CmdAppRefreshProcedure,
		CmdAppTemplateUtility
	}

	public static readonly CommandID CommandFileImportFromApplication;

	public static readonly CommandID CommandFileImportText;

	public static readonly CommandID CommandFileExportText;

	public static readonly CommandID CommandFileSelectiveExport;

	public static readonly CommandID CommandFileExportProjectFile;

	public static readonly CommandID CommandApplicationChangeDictionary;

	public static readonly CommandID CommandApplicationInsertModule;

	public static readonly CommandID CommandApplicationSynchronize;

	public static readonly CommandID CommandApplicationRefresh;

	public static readonly CommandID CommandApplicationRedistributeProcedures;

	public static readonly CommandID CommandApplicationRepopulateModules;

	public static readonly CommandID CommandApplicationRenumberModules;

	public static readonly CommandID CommandApplicationDeleteEmptyModules;

	public static readonly CommandID CommandApplicationDeleteEmptylibraries;

	public static readonly CommandID CommandProcedureNew;

	public static readonly CommandID CommandEditDelete;

	public static readonly CommandID CommandProcedureCopy;

	public static readonly CommandID CommandProcedureSynchronize;

	public static readonly CommandID CommandProcedureRefresh;

	public static readonly CommandID CommandTemplateUtility;

	private static readonly Guid _CommandSet;

	public ApplicationMainWindowControl(ApplicationContainer container)
		: base(container)
	{
	}

	static ApplicationMainWindowControl()
	{
		_CommandSet = new Guid("{48FA276E-9598-534D-92C2-B0D754553A78}");
		CommandFileImportFromApplication = new CommandID(_CommandSet, 1);
		CommandFileImportText = new CommandID(_CommandSet, 2);
		CommandFileExportText = new CommandID(_CommandSet, 3);
		CommandFileSelectiveExport = new CommandID(_CommandSet, 4);
		CommandFileExportProjectFile = new CommandID(_CommandSet, 5);
		CommandApplicationChangeDictionary = new CommandID(_CommandSet, 6);
		CommandApplicationInsertModule = new CommandID(_CommandSet, 7);
		CommandApplicationSynchronize = new CommandID(_CommandSet, 8);
		CommandApplicationRefresh = new CommandID(_CommandSet, 9);
		CommandApplicationRedistributeProcedures = new CommandID(_CommandSet, 10);
		CommandApplicationRepopulateModules = new CommandID(_CommandSet, 11);
		CommandApplicationRenumberModules = new CommandID(_CommandSet, 12);
		CommandApplicationDeleteEmptyModules = new CommandID(_CommandSet, 13);
		CommandApplicationDeleteEmptylibraries = new CommandID(_CommandSet, 14);
		CommandProcedureNew = new CommandID(_CommandSet, 15);
		CommandEditDelete = new CommandID(_CommandSet, 16);
		CommandProcedureCopy = new CommandID(_CommandSet, 17);
		CommandProcedureSynchronize = new CommandID(_CommandSet, 18);
		CommandProcedureRefresh = new CommandID(_CommandSet, 19);
		CommandTemplateUtility = new CommandID(_CommandSet, 20);
	}

	public override void CommandInvoke(CommandID pCommandID)
	{
		if (pCommandID == null || pCommandID.Guid != _CommandSet)
		{
			throw new Exception();
		}
		ExecuteCommand(pCommandID);
	}

	protected override void Host_WindowOpened(object sender)
	{
		base.Host_WindowOpened(sender);
		ApplicationService.ApplicationFrameOpened();
	}

	internal override void DoClosingEvent(CancelEventArgs e)
	{
		if (_Container != null)
		{
			CWControl_ViewContent viewContent = _Container._ViewContent;
			if (viewContent is ApplicationMainWindowControl_ViewContent)
			{
				(viewContent as ApplicationMainWindowControl_ViewContent).DoClosingEvent(e);
				return;
			}
		}
		e.Cancel = true;
	}

	internal override void InitializeView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		this.Dock = System.Windows.Forms.DockStyle.Fill;
		base.Name = "ApplicationMainWindowControl";
		base.Location = new System.Drawing.Point(0, 0);
		base.Size = new System.Drawing.Size(745, 473);
		base.TabIndex = 0;
		base.Visible = false;
		base.ResumeLayout(false);
	}
}
