using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui.StartPage;
using WeifenLuo.WinFormsUI;

namespace ICSharpCode.SharpDevelop.Gui.CustomizableStrips;

public class CustomColorTable : ProfessionalColorTable, IDockTabCustomColor, IStartPageCustomColor, IApplicationHeaderCustomColor, IListCustomColor
{
	private AppearanceProperties ap;

	public override Color ButtonCheckedGradientBegin => ap.ButtonAppearance.CheckedAppearance.GradientBegin;

	public override Color ButtonCheckedGradientEnd => ap.ButtonAppearance.CheckedAppearance.GradientEnd;

	public override Color ButtonCheckedGradientMiddle => ap.ButtonAppearance.CheckedAppearance.GradientMiddle;

	public override Color ButtonCheckedHighlight => ap.ButtonAppearance.CheckedAppearance.Highlight;

	public override Color ButtonCheckedHighlightBorder => ap.ButtonAppearance.CheckedAppearance.BorderHighlight;

	public override Color ButtonPressedBorder => ap.ButtonAppearance.PressedAppearance.Border;

	public override Color ButtonPressedGradientBegin => ap.ButtonAppearance.PressedAppearance.GradientBegin;

	public override Color ButtonPressedGradientEnd => ap.ButtonAppearance.PressedAppearance.GradientEnd;

	public override Color ButtonPressedGradientMiddle => ap.ButtonAppearance.PressedAppearance.GradientMiddle;

	public override Color ButtonPressedHighlight => ap.ButtonAppearance.PressedAppearance.Highlight;

	public override Color ButtonPressedHighlightBorder => ap.ButtonAppearance.PressedAppearance.BorderHighlight;

	public override Color ButtonSelectedBorder => ap.ButtonAppearance.SelectedAppearance.Border;

	public override Color ButtonSelectedGradientBegin => ap.ButtonAppearance.SelectedAppearance.GradientBegin;

	public override Color ButtonSelectedGradientEnd => ap.ButtonAppearance.SelectedAppearance.GradientEnd;

	public override Color ButtonSelectedGradientMiddle => ap.ButtonAppearance.SelectedAppearance.GradientMiddle;

	public override Color ButtonSelectedHighlight => ap.ButtonAppearance.SelectedAppearance.Highlight;

	public override Color ButtonSelectedHighlightBorder => ap.ButtonAppearance.SelectedAppearance.BorderHighlight;

	public override Color CheckBackground => ap.ButtonAppearance.CheckedAppearance.Background;

	public override Color CheckPressedBackground => ap.ButtonAppearance.CheckedAppearance.PressedBackground;

	public override Color CheckSelectedBackground => ap.ButtonAppearance.CheckedAppearance.SelectedBackground;

	public override Color GripDark => ap.GripAppearance.Dark;

	public override Color GripLight => ap.GripAppearance.Light;

	public override Color ImageMarginGradientBegin => ap.ImageMarginAppearance.Normal.GradientBegin;

	public override Color ImageMarginGradientEnd => ap.ImageMarginAppearance.Normal.GradientEnd;

	public override Color ImageMarginGradientMiddle => ap.ImageMarginAppearance.Normal.GradientMiddle;

	public override Color ImageMarginRevealedGradientBegin => ap.ImageMarginAppearance.Revealed.GradientBegin;

	public override Color ImageMarginRevealedGradientEnd => ap.ImageMarginAppearance.Revealed.GradientEnd;

	public override Color ImageMarginRevealedGradientMiddle => ap.ImageMarginAppearance.Revealed.GradientMiddle;

	public override Color MenuBorder => ap.MenuStripAppearance.Border;

	public override Color MenuItemBorder => ap.MenuItemAppearance.Border;

	public override Color MenuItemPressedGradientBegin => ap.MenuItemAppearance.PressedGradientBegin;

	public override Color MenuItemPressedGradientEnd => ap.MenuItemAppearance.PressedGradientEnd;

	public override Color MenuItemPressedGradientMiddle => ap.MenuItemAppearance.PressedGradientMiddle;

	public override Color MenuItemSelected => ap.MenuItemAppearance.Selected;

	public override Color MenuItemSelectedGradientBegin => ap.MenuItemAppearance.SelectedGradientBegin;

	public override Color MenuItemSelectedGradientEnd => ap.MenuItemAppearance.SelectedGradientEnd;

	public override Color MenuStripGradientBegin => ap.MenuStripAppearance.GradientBegin;

	public override Color MenuStripGradientEnd => ap.MenuStripAppearance.GradientEnd;

	public override Color OverflowButtonGradientBegin => ap.OverflowButtonAppearance.GradientBegin;

	public override Color OverflowButtonGradientEnd => ap.OverflowButtonAppearance.GradientEnd;

	public override Color OverflowButtonGradientMiddle => ap.OverflowButtonAppearance.GradientMiddle;

	public override Color RaftingContainerGradientBegin => ap.RaftingContainerAppearance.GradientBegin;

	public override Color RaftingContainerGradientEnd => ap.RaftingContainerAppearance.GradientEnd;

	public override Color SeparatorDark => ap.SeparatorAppearance.Dark;

	public override Color SeparatorLight => ap.SeparatorAppearance.Light;

	public override Color StatusStripGradientBegin => ap.StatusStripAppearance.GradientBegin;

	public override Color StatusStripGradientEnd => ap.StatusStripAppearance.GradientEnd;

	public override Color ToolStripBorder => ap.ToolStripAppearance.Border;

	public override Color ToolStripContentPanelGradientBegin => ap.ToolStripAppearance.ContentPanelGradientBegin;

	public override Color ToolStripContentPanelGradientEnd => ap.ToolStripAppearance.ContentPanelGradientEnd;

	public override Color ToolStripDropDownBackground => ap.ToolStripAppearance.DropDownBackground;

	public override Color ToolStripGradientBegin => ap.ToolStripAppearance.GradientBegin;

	public override Color ToolStripGradientEnd => ap.ToolStripAppearance.GradientEnd;

	public override Color ToolStripGradientMiddle => ap.ToolStripAppearance.GradientMiddle;

	public override Color ToolStripPanelGradientBegin => ap.ToolStripAppearance.PanelGradientBegin;

	public override Color ToolStripPanelGradientEnd => ap.ToolStripAppearance.PanelGradientEnd;

	Color IDockTabCustomColor.TabActiveGradientBegin => ap.DockTabAppearance.Active.GradientBegin;

	Color IDockTabCustomColor.TabActiveGradientEnd => ap.DockTabAppearance.Active.GradientEnd;

	Color IDockTabCustomColor.TabActiveEdgeColor => ap.DockTabAppearance.Active.EdgeColor;

	Color IDockTabCustomColor.TabActiveTextColor => ap.DockTabAppearance.Active.TextColor;

	Color IDockTabCustomColor.TabInactiveGradientBegin => ap.DockTabAppearance.Inactive.GradientBegin;

	Color IDockTabCustomColor.TabInactiveGradientEnd => ap.DockTabAppearance.Inactive.GradientEnd;

	Color IDockTabCustomColor.TabInactiveEdgeColor => ap.DockTabAppearance.Inactive.EdgeColor;

	Color IDockTabCustomColor.TabInactiveTextColor => ap.DockTabAppearance.Inactive.TextColor;

	Color IDockTabCustomColor.PadTabActiveGradientBegin => ap.DockTabAppearance.PadActive.GradientBegin;

	Color IDockTabCustomColor.PadTabActiveGradientEnd => ap.DockTabAppearance.PadActive.GradientEnd;

	Color IDockTabCustomColor.PadTabActiveEdgeColor => ap.DockTabAppearance.PadActive.EdgeColor;

	Color IDockTabCustomColor.PadTabActiveTextColor => ap.DockTabAppearance.PadActive.TextColor;

	Color IDockTabCustomColor.PadTabHideGradientBegin => ap.DockTabAppearance.PadHide.GradientBegin;

	Color IDockTabCustomColor.PadTabHideGradientEnd => ap.DockTabAppearance.PadHide.GradientEnd;

	Color IDockTabCustomColor.PadTabHideEdgeColor => ap.DockTabAppearance.PadHide.EdgeColor;

	Color IDockTabCustomColor.PadTabHideTextColor => ap.DockTabAppearance.PadHide.TextColor;

	Color IDockTabCustomColor.PadTabHideOverGradientBegin => ap.DockTabAppearance.PadHideOver.GradientBegin;

	Color IDockTabCustomColor.PadTabHideOverGradientEnd => ap.DockTabAppearance.PadHideOver.GradientEnd;

	Color IDockTabCustomColor.PadTabHideOverEdgeColor => ap.DockTabAppearance.PadHideOver.EdgeColor;

	Color IDockTabCustomColor.PadTabHideOverTextColor => ap.DockTabAppearance.PadHideOver.TextColor;

	Color IDockTabCustomColor.PadTabInactiveTextColor => ap.DockTabStripAppearance.TextColor;

	Color IDockTabCustomColor.PadTitleActiveGradientBegin => ap.DockPadTitleAppearance.ActiveBackColorGradientBegin;

	Color IDockTabCustomColor.PadTitleActiveGradientEnd => ap.DockPadTitleAppearance.ActiveBackColorGradientEnd;

	Color IDockTabCustomColor.PadTitleActiveTextColor => ap.DockPadTitleAppearance.ActiveTextColor;

	Color IDockTabCustomColor.PadTitleInactiveBackColor => ap.DockPadTitleAppearance.InactiveBackColor;

	Color IDockTabCustomColor.PadTitleInactiveTextColor => ap.DockPadTitleAppearance.InactiveTextColor;

	Color IDockTabCustomColor.StripGradientBegin => ap.DockTabStripAppearance.GradientBegin;

	Color IDockTabCustomColor.StripGradientEnd => ap.DockTabStripAppearance.GradientEnd;

	public Color StartPageSecondaryColor => ap.StartPageAppearance.SecondaryColor;

	public Color StartPagePrimaryColor => ap.StartPageAppearance.PrimaryColor;

	public Color StartPageBackgroundGradientBegin => ap.StartPageAppearance.BackgroundGradientBegin;

	public Color StartPageBackgroundGradientEnd => ap.StartPageAppearance.BackgroundGradientEnd;

	public Color StartPageButtonImageColor => ap.StartPageAppearance.ButtonImageColor;

	public Color StartPageGridHeaderColor => ap.StartPageAppearance.GridHeaderColor;

	public Color StartPageGridBodyColor => ap.StartPageAppearance.GridBodyColor;

	public Color StartPageGridAltBodyColor => ap.StartPageAppearance.GridAltBodyColor;

	public Color StartPageGridLineColor => ap.StartPageAppearance.GridLineColor;

	public Color StartPageGridHoverColor => ap.StartPageAppearance.GridHoverColor;

	public Color ApplicationHeaderGradientBegin => ap.ApplicationHeaderAppearance.GradientBegin;

	public Color ApplicationHeaderGradientEnd => ap.ApplicationHeaderAppearance.GradientEnd;

	Color IListCustomColor.Background => ap.ListAppearance.Background;

	Color IListCustomColor.Text => ap.ListAppearance.Text;

	Color IListCustomColor.BarActiveBackground => ap.ListAppearance.BarActiveBackground;

	Color IListCustomColor.BarActiveText => ap.ListAppearance.BarActiveText;

	Color IListCustomColor.BarInactiveBackground => ap.ListAppearance.BarInactiveBackground;

	Color IListCustomColor.BarInactiveText => ap.ListAppearance.BarInactiveText;

	public CustomColorTable(AppearanceProperties ap)
	{
		this.ap = ap;
	}

	public void SetFromProfessionalColorTable(ProfessionalColorTable colors)
	{
		ap.ApplicationHeaderAppearance.GradientBegin = colors.MenuItemPressedGradientBegin;
		ap.ApplicationHeaderAppearance.GradientEnd = colors.MenuItemPressedGradientEnd;
		ap.ButtonAppearance.CheckedAppearance.GradientBegin = colors.ButtonCheckedGradientBegin;
		ap.ButtonAppearance.CheckedAppearance.GradientEnd = colors.ButtonCheckedGradientEnd;
		ap.ButtonAppearance.CheckedAppearance.GradientMiddle = colors.ButtonCheckedGradientMiddle;
		ap.ButtonAppearance.CheckedAppearance.Highlight = colors.ButtonCheckedHighlight;
		ap.ButtonAppearance.CheckedAppearance.BorderHighlight = colors.ButtonCheckedHighlightBorder;
		ap.ButtonAppearance.PressedAppearance.Border = colors.ButtonPressedBorder;
		ap.ButtonAppearance.PressedAppearance.GradientBegin = colors.ButtonPressedGradientBegin;
		ap.ButtonAppearance.PressedAppearance.GradientEnd = colors.ButtonPressedGradientEnd;
		ap.ButtonAppearance.PressedAppearance.GradientMiddle = colors.ButtonPressedGradientMiddle;
		ap.ButtonAppearance.PressedAppearance.Highlight = colors.ButtonPressedHighlight;
		ap.ButtonAppearance.PressedAppearance.BorderHighlight = colors.ButtonPressedHighlightBorder;
		ap.ButtonAppearance.SelectedAppearance.Border = colors.ButtonSelectedBorder;
		ap.ButtonAppearance.SelectedAppearance.GradientBegin = colors.ButtonSelectedGradientBegin;
		ap.ButtonAppearance.SelectedAppearance.GradientEnd = colors.ButtonSelectedGradientEnd;
		ap.ButtonAppearance.SelectedAppearance.GradientMiddle = colors.ButtonSelectedGradientMiddle;
		ap.ButtonAppearance.SelectedAppearance.Highlight = colors.ButtonSelectedHighlight;
		ap.ButtonAppearance.SelectedAppearance.BorderHighlight = colors.ButtonSelectedHighlightBorder;
		ap.ButtonAppearance.CheckedAppearance.Background = colors.CheckBackground;
		ap.ButtonAppearance.CheckedAppearance.PressedBackground = colors.CheckPressedBackground;
		ap.ButtonAppearance.CheckedAppearance.SelectedBackground = colors.CheckSelectedBackground;
		ap.GripAppearance.Dark = colors.GripDark;
		ap.GripAppearance.Light = colors.GripLight;
		ap.ImageMarginAppearance.Normal.GradientBegin = colors.ImageMarginGradientBegin;
		ap.ImageMarginAppearance.Normal.GradientEnd = colors.ImageMarginGradientEnd;
		ap.ImageMarginAppearance.Normal.GradientMiddle = colors.ImageMarginGradientMiddle;
		ap.ImageMarginAppearance.Revealed.GradientBegin = colors.ImageMarginRevealedGradientBegin;
		ap.ImageMarginAppearance.Revealed.GradientEnd = colors.ImageMarginRevealedGradientEnd;
		ap.ImageMarginAppearance.Revealed.GradientMiddle = colors.ImageMarginRevealedGradientMiddle;
		ap.MenuStripAppearance.Border = colors.MenuBorder;
		ap.MenuItemAppearance.Border = colors.MenuItemBorder;
		ap.MenuItemAppearance.PressedGradientBegin = colors.MenuItemPressedGradientBegin;
		ap.MenuItemAppearance.PressedGradientEnd = colors.MenuItemPressedGradientEnd;
		ap.MenuItemAppearance.PressedGradientMiddle = colors.MenuItemPressedGradientMiddle;
		ap.MenuItemAppearance.Selected = colors.MenuItemSelected;
		ap.MenuItemAppearance.SelectedGradientBegin = colors.MenuItemSelectedGradientBegin;
		ap.MenuItemAppearance.SelectedGradientEnd = colors.MenuItemSelectedGradientEnd;
		ap.MenuStripAppearance.GradientBegin = colors.MenuStripGradientBegin;
		ap.MenuStripAppearance.GradientEnd = colors.MenuStripGradientEnd;
		ap.OverflowButtonAppearance.GradientBegin = colors.OverflowButtonGradientBegin;
		ap.OverflowButtonAppearance.GradientEnd = colors.OverflowButtonGradientEnd;
		ap.OverflowButtonAppearance.GradientMiddle = colors.OverflowButtonGradientMiddle;
		ap.RaftingContainerAppearance.GradientBegin = colors.RaftingContainerGradientBegin;
		ap.RaftingContainerAppearance.GradientEnd = colors.RaftingContainerGradientEnd;
		ap.SeparatorAppearance.Dark = colors.SeparatorDark;
		ap.SeparatorAppearance.Light = colors.SeparatorLight;
		ap.StatusStripAppearance.GradientBegin = colors.StatusStripGradientBegin;
		ap.StatusStripAppearance.GradientEnd = colors.StatusStripGradientEnd;
		ap.ToolStripAppearance.Border = colors.ToolStripBorder;
		ap.ToolStripAppearance.ContentPanelGradientBegin = colors.ToolStripContentPanelGradientBegin;
		ap.ToolStripAppearance.ContentPanelGradientEnd = colors.ToolStripContentPanelGradientEnd;
		ap.ToolStripAppearance.DropDownBackground = colors.ToolStripDropDownBackground;
		ap.ToolStripAppearance.GradientBegin = colors.ToolStripGradientBegin;
		ap.ToolStripAppearance.GradientEnd = colors.ToolStripGradientEnd;
		ap.ToolStripAppearance.GradientMiddle = colors.ToolStripGradientMiddle;
		ap.ToolStripAppearance.PanelGradientBegin = colors.ToolStripPanelGradientBegin;
		ap.ToolStripAppearance.PanelGradientEnd = colors.ToolStripPanelGradientEnd;
	}
}
