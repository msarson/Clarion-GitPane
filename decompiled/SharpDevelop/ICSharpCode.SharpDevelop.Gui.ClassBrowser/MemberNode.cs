using System;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class MemberNode : ExtTreeNode, IMemberNode
{
	private int line;

	private int column;

	private ModifierEnum modifiers;

	private IClass declaringType;

	private IMember member;

	private string FileName
	{
		get
		{
			if (declaringType == null || declaringType.CompilationUnit == null)
			{
				return null;
			}
			return declaringType.CompilationUnit.FileName;
		}
	}

	public override bool Visible
	{
		get
		{
			ClassBrowserFilter filter = ClassBrowserPad.Instance.Filter;
			if ((modifiers & ModifierEnum.Public) != ModifierEnum.None)
			{
				return (filter & ClassBrowserFilter.ShowPublic) != 0;
			}
			if ((modifiers & ModifierEnum.Protected) != ModifierEnum.None)
			{
				return (filter & ClassBrowserFilter.ShowProtected) != 0;
			}
			if ((modifiers & ModifierEnum.Private) != ModifierEnum.None)
			{
				return (filter & ClassBrowserFilter.ShowPrivate) != 0;
			}
			return (filter & ClassBrowserFilter.ShowOther) != 0;
		}
	}

	public IMember Member => member;

	private void InitMemberNode(IMember member)
	{
		this.member = member;
		ContextmenuAddinTreePath = "/SharpDevelop/Pads/ClassBrowser/MemberContextMenu";
		declaringType = member.DeclaringType;
		modifiers = member.Modifiers;
		line = member.Region.BeginLine;
		column = member.Region.BeginColumn;
	}

	public static string GetText(IMember member)
	{
		return Create(member).Text;
	}

	public static MemberNode Create(IMember member)
	{
		if (member is IMethod)
		{
			return new MemberNode(member as IMethod);
		}
		if (member is IProperty)
		{
			return new MemberNode(member as IProperty);
		}
		if (member is IField)
		{
			return new MemberNode(member as IField);
		}
		if (member is IEvent)
		{
			return new MemberNode(member as IEvent);
		}
		throw new ArgumentException("unknown member type");
	}

	public MemberNode(IMethod method)
	{
		InitMemberNode(method);
		sortOrder = 10;
		base.Text = AppendReturnType(GetAmbience().Convert(method), method.ReturnType);
		base.SelectedImageIndex = (base.ImageIndex = ClassBrowserIconService.GetIcon(method));
	}

	public MemberNode(IProperty property)
	{
		InitMemberNode(property);
		sortOrder = 12;
		base.Text = AppendReturnType(GetAmbience().Convert(property), property.ReturnType);
		base.SelectedImageIndex = (base.ImageIndex = ClassBrowserIconService.GetIcon(property));
	}

	public MemberNode(IField field)
	{
		InitMemberNode(field);
		sortOrder = 11;
		base.Text = AppendReturnType(GetAmbience().Convert(field), field.ReturnType);
		base.SelectedImageIndex = (base.ImageIndex = ClassBrowserIconService.GetIcon(field));
	}

	public MemberNode(IEvent e)
	{
		InitMemberNode(e);
		sortOrder = 14;
		base.Text = AppendReturnType(GetAmbience().Convert(e), e.ReturnType);
		base.SelectedImageIndex = (base.ImageIndex = ClassBrowserIconService.GetIcon(e));
	}

	protected virtual IAmbience GetAmbience()
	{
		IAmbience currentAmbience = AmbienceService.CurrentAmbience;
		currentAmbience.ConversionFlags = ConversionFlags.None;
		return currentAmbience;
	}

	private string AppendReturnType(string text, IReturnType rt)
	{
		return text + " : " + GetAmbience().Convert(rt);
	}

	public override void ActivateItem()
	{
		if (FileName != null)
		{
			FileService.JumpToFilePosition(FileName, line - 1, column - 1);
		}
	}
}
