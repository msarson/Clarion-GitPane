using System;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.ClassBrowser;
using ICSharpCode.SharpDevelop.Project;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.ClassBrowser;

public class ClaMemberNode : ExtTreeNode, IMemberNode
{
	private int line;

	private int column;

	private string fileName;

	private ModifierEnum modifiers;

	private IProject project;

	private bool isIncluded;

	private IMember member;

	private static readonly string getterName = "getter";

	private static readonly string setterName = "setter";

	private string FileName => fileName;

	public override bool Visible
	{
		get
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Invalid comparison between Unknown and I4
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Invalid comparison between Unknown and I4
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Invalid comparison between Unknown and I4
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Invalid comparison between Unknown and I4
			ClassBrowserFilter filter = ClassBrowserPad.Instance.Filter;
			if ((filter & 0x40) == 0 && isIncluded)
			{
				return false;
			}
			if ((modifiers & 8) != 0)
			{
				return (filter & 2) != 0;
			}
			if ((modifiers & 4) != 0)
			{
				return (filter & 4) != 0;
			}
			if ((modifiers & 1) != 0)
			{
				return (filter & 8) != 0;
			}
			return (filter & 0x10) != 0;
		}
	}

	public IMember Member => member;

	private void InitMemberNode(IMember member)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		this.member = member;
		modifiers = ((IDecoration)member).Modifiers;
		DomRegion region = member.Region;
		line = ((DomRegion)(ref region)).BeginLine;
		DomRegion region2 = member.Region;
		column = ((DomRegion)(ref region2)).BeginColumn;
	}

	public static string GetText(IMember member)
	{
		return ((TreeNode)(object)Create(null, member)).Text;
	}

	public static ClaMemberNode Create(IProject project, IMember member)
	{
		if (member is IMethod)
		{
			return new ClaMemberNode(project, (IMethod)(object)((member is IMethod) ? member : null));
		}
		if (member is IProperty)
		{
			return new ClaMemberNode(project, (IProperty)(object)((member is IProperty) ? member : null));
		}
		if (member is IField)
		{
			return new ClaMemberNode(project, (IField)(object)((member is IField) ? member : null));
		}
		if (member is IEvent)
		{
			return new ClaMemberNode(project, (IEvent)(object)((member is IEvent) ? member : null));
		}
		throw new ArgumentException("unknown member type");
	}

	private ClaMemberNode(IProject project)
	{
		this.project = project;
	}

	public ClaMemberNode(IProject project, IMethod method)
		: this(project)
	{
		InitMemberNode((IMember)(object)method);
		if (method is ClaMethod)
		{
			ClaMethod claMethod = (ClaMethod)(object)method;
			fileName = claMethod.ClaRegion.FileName;
			if (claMethod.DeclaringType != null)
			{
				string text = claMethod.DeclaringType.CompilationUnit.FileName;
				if (!text.Equals(claMethod.ClaRegion.FileName) && !text.Equals(claMethod.ClaBodyRegion.FileName))
				{
					isIncluded = true;
				}
			}
			if (method is ClaRoutine)
			{
				base.sortOrder = 20;
				((TreeNode)this).SelectedImageIndex = (((TreeNode)this).ImageIndex = ClaClassNode.RoutineIcon);
			}
			else if (method is ClaLocalMethod)
			{
				base.sortOrder = 21;
				((TreeNode)this).SelectedImageIndex = (((TreeNode)this).ImageIndex = ClassBrowserIconService.GetIcon(method));
			}
			else
			{
				base.sortOrder = 10;
				((TreeNode)this).SelectedImageIndex = (((TreeNode)this).ImageIndex = ClassBrowserIconService.GetIcon(method));
			}
			if (claMethod.IsAccessor)
			{
				((TreeNode)this).Text = (claMethod.IsGetter ? getterName : setterName);
				fileName = claMethod.ClaBodyRegion.FileName;
				line = claMethod.ClaBodyRegion.DeclBeginLine;
				column = claMethod.ClaBodyRegion.DeclBeginColumn;
			}
			else
			{
				((TreeNode)this).Text = AppendReturnType(GetAmbience().Convert(method), ((IMember)method).ReturnType);
				if (!string.IsNullOrEmpty(claMethod.IfaceImplDisplayName))
				{
					((TreeNode)this).Text = claMethod.IfaceImplDisplayName + "." + ((TreeNode)this).Text;
				}
			}
			if (claMethod.Routines.Count > 0 || claMethod.LocalMethods.Count > 0 || claMethod.LocalTypes.Count > 0 || claMethod.LocalVariables.Count > 0)
			{
				((TreeNode)this).Nodes.Add(new TreeNode());
			}
		}
		else
		{
			base.sortOrder = 10;
			((TreeNode)this).SelectedImageIndex = (((TreeNode)this).ImageIndex = ClassBrowserIconService.GetIcon(method));
			((TreeNode)this).Text = AppendReturnType(GetAmbience().Convert(method), ((IMember)method).ReturnType);
		}
	}

	public ClaMemberNode(IProject project, IProperty property)
		: this(project)
	{
		InitMemberNode((IMember)(object)property);
		if (property is ClaProperty)
		{
			ClaProperty claProperty = (ClaProperty)(object)property;
			fileName = claProperty.ClaRegion.FileName;
			if (claProperty.DeclaringType != null)
			{
				string text = claProperty.DeclaringType.CompilationUnit.FileName;
				if (!text.Equals(claProperty.ClaRegion.FileName) && !text.Equals(claProperty.ClaBodyRegion.FileName))
				{
					isIncluded = true;
				}
			}
			if ((claProperty.Getter != null && !claProperty.Getter.IsInline) || (claProperty.Setter != null && !claProperty.Setter.IsInline))
			{
				((TreeNode)this).Nodes.Add(new TreeNode());
			}
			((TreeNode)this).Text = AppendReturnType(GetAmbience().Convert(property), ((IMember)property).ReturnType);
			if (!string.IsNullOrEmpty(claProperty.IfaceImplDisplayName))
			{
				((TreeNode)this).Text = claProperty.IfaceImplDisplayName + "." + ((TreeNode)this).Text;
			}
		}
		else
		{
			((TreeNode)this).Text = AppendReturnType(GetAmbience().Convert(property), ((IMember)property).ReturnType);
		}
		base.sortOrder = 12;
		((TreeNode)this).SelectedImageIndex = (((TreeNode)this).ImageIndex = ClassBrowserIconService.GetIcon(property));
	}

	public ClaMemberNode(IProject project, IField field)
		: this(project)
	{
		InitMemberNode((IMember)(object)field);
		if (field is ClaField)
		{
			ClaField claField = (ClaField)(object)field;
			fileName = claField.ClaRegion.FileName;
			if (claField.DeclaringType != null)
			{
				string text = claField.DeclaringType.CompilationUnit.FileName;
				if (!text.Equals(claField.ClaRegion.FileName) && !text.Equals(claField.ClaBodyRegion.FileName))
				{
					isIncluded = true;
				}
			}
		}
		base.sortOrder = 11;
		((TreeNode)this).Text = AppendReturnType(GetAmbience().Convert(field), ((IMember)field).ReturnType);
		if (field is ClaKeyField)
		{
			((TreeNode)this).SelectedImageIndex = (((TreeNode)this).ImageIndex = ClaClassNode.KeyIcon);
		}
		else
		{
			((TreeNode)this).SelectedImageIndex = (((TreeNode)this).ImageIndex = ClassBrowserIconService.GetIcon(field));
		}
	}

	public ClaMemberNode(IProject project, IEvent e)
		: this(project)
	{
		InitMemberNode((IMember)(object)e);
		if (e is ClaEvent)
		{
			ClaEvent claEvent = (ClaEvent)(object)e;
			fileName = claEvent.ClaRegion.FileName;
			if (claEvent.DeclaringType != null)
			{
				string text = claEvent.DeclaringType.CompilationUnit.FileName;
				if (!text.Equals(claEvent.ClaRegion.FileName) && !text.Equals(claEvent.ClaBodyRegion.FileName))
				{
					isIncluded = true;
				}
			}
		}
		base.sortOrder = 14;
		((TreeNode)this).Text = AppendReturnType(GetAmbience().Convert(e), ((IMember)e).ReturnType);
		((TreeNode)this).SelectedImageIndex = (((TreeNode)this).ImageIndex = ClassBrowserIconService.GetIcon(e));
	}

	private static IAmbience GetAmbience()
	{
		IAmbience currentAmbience = (IAmbience)(object)AmbienceService.CurrentAmbience;
		currentAmbience.ConversionFlags = (ConversionFlags)0;
		return currentAmbience;
	}

	private static string AppendReturnType(string text, IReturnType rt)
	{
		string text2 = GetAmbience().Convert(rt);
		if (string.IsNullOrEmpty(text2))
		{
			return text;
		}
		return text + " : " + text2;
	}

	public override void ActivateItem()
	{
		if (FileName != null)
		{
			FileService.JumpToFilePosition(FileName, line - 1, column - 1);
		}
	}

	protected override void Initialize()
	{
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		((ExtTreeNode)this).Initialize();
		((TreeNode)this).Nodes.Clear();
		if (member is ClaMethod)
		{
			ClaMethod claMethod = (ClaMethod)(object)member;
			foreach (ClaRoutine routine in claMethod.Routines)
			{
				ClaMemberNode claMemberNode = Create(project, (IMember)(object)routine);
				((ExtTreeNode)claMemberNode).ContextmenuAddinTreePath = ((CommonClarionProject)(object)project).ClassBrowserMemberMenuPath;
				((ExtTreeNode)claMemberNode).AddTo((TreeNode)(object)this);
			}
			foreach (IClass localType in claMethod.LocalTypes)
			{
				((ExtTreeNode)new ClaClassNode(project, localType)).AddTo((TreeNode)(object)this);
			}
			foreach (IMethod localMethod in claMethod.LocalMethods)
			{
				ClaMemberNode claMemberNode2 = Create(project, (IMember)(object)localMethod);
				((ExtTreeNode)claMemberNode2).ContextmenuAddinTreePath = ((CommonClarionProject)(object)project).ClassBrowserMemberMenuPath;
				((ExtTreeNode)claMemberNode2).AddTo((TreeNode)(object)this);
			}
			foreach (IField localVariable in claMethod.LocalVariables)
			{
				ClaMemberNode claMemberNode3 = Create(project, (IMember)(object)localVariable);
				((ExtTreeNode)claMemberNode3).ContextmenuAddinTreePath = ((CommonClarionProject)(object)project).ClassBrowserMemberMenuPath;
				((ExtTreeNode)claMemberNode3).AddTo((TreeNode)(object)this);
			}
		}
		else if (member is ClaProperty)
		{
			ClaProperty claProperty = (ClaProperty)(object)member;
			if (claProperty.Getter != null && !claProperty.Getter.IsInline)
			{
				ClaMemberNode claMemberNode4 = Create(project, (IMember)(object)claProperty.Getter);
				((ExtTreeNode)claMemberNode4).ContextmenuAddinTreePath = ((CommonClarionProject)(object)project).ClassBrowserMemberMenuPath;
				((ExtTreeNode)claMemberNode4).AddTo((TreeNode)(object)this);
			}
			if (claProperty.Setter != null && !claProperty.Setter.IsInline)
			{
				ClaMemberNode claMemberNode5 = Create(project, (IMember)(object)claProperty.Setter);
				((ExtTreeNode)claMemberNode5).ContextmenuAddinTreePath = ((CommonClarionProject)(object)project).ClassBrowserMemberMenuPath;
				((ExtTreeNode)claMemberNode5).AddTo((TreeNode)(object)this);
			}
		}
		if (((TreeNode)this).TreeView is ExtTreeView)
		{
			((ExtTreeView)((TreeNode)this).TreeView).SortNodes(((TreeNode)this).Nodes, false);
		}
	}
}
