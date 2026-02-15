using System;
using System.Collections;
using System.Collections.Generic;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using Clarion.GEN;
using SoftVelocity.Generator.Properties;

namespace SoftVelocity.Generator;

internal class ApplicationTreeModel : ITreeModel
{
	private Application app;

	private bool AllowSelection = true;

	private AppTreeMode treeMode = AppTreeMode.AppModuleView;

	private bool categoryListNeedRefresh = true;

	private Dictionary<string, List<Procedure>> categoryList;

	private bool templateListNeedRefresh = true;

	private Dictionary<string, List<Procedure>> templateList;

	private bool procedureListNeedRefresh = true;

	private List<Procedure> procedureList;

	internal bool NeedRefresh
	{
		get
		{
			if (categoryListNeedRefresh && templateListNeedRefresh)
			{
				return procedureListNeedRefresh;
			}
			return false;
		}
		set
		{
			categoryListNeedRefresh = value;
			templateListNeedRefresh = value;
			procedureListNeedRefresh = value;
		}
	}

	private Dictionary<string, List<Procedure>> CategoryList
	{
		get
		{
			if (categoryListNeedRefresh)
			{
				if (categoryList != null)
				{
					foreach (List<Procedure> value in categoryList.Values)
					{
						value.Clear();
					}
					categoryList.Clear();
					categoryList = null;
				}
				categoryList = new Dictionary<string, List<Procedure>>();
				Module[] modules = app.Modules;
				foreach (Module module in modules)
				{
					if (module.Procedures == null)
					{
						continue;
					}
					Procedure[] procedures = module.Procedures;
					foreach (Procedure procedure in procedures)
					{
						string text = procedure.Category;
						if (string.IsNullOrEmpty(text))
						{
							text = procedure.Template;
							text = text.Split(new char[1] { '(' }, 2)[0];
						}
						if (string.IsNullOrEmpty(text))
						{
							text = "ToDo";
						}
						if (categoryList.ContainsKey(text))
						{
							categoryList[text].Add(procedure);
							continue;
						}
						List<Procedure> list = new List<Procedure>();
						list.Add(procedure);
						categoryList.Add(text, list);
					}
				}
				categoryListNeedRefresh = false;
			}
			return categoryList;
		}
	}

	private Dictionary<string, List<Procedure>> TemplateList
	{
		get
		{
			if (templateListNeedRefresh)
			{
				if (templateList != null)
				{
					foreach (List<Procedure> value in templateList.Values)
					{
						value.Clear();
					}
					templateList.Clear();
					templateList = null;
				}
				templateList = new Dictionary<string, List<Procedure>>();
				Module[] modules = app.Modules;
				foreach (Module module in modules)
				{
					if (module.Procedures == null)
					{
						continue;
					}
					Procedure[] procedures = module.Procedures;
					foreach (Procedure procedure in procedures)
					{
						string text = procedure.Template;
						if (string.IsNullOrEmpty(text))
						{
							text = "ToDo";
						}
						if (templateList.ContainsKey(text))
						{
							templateList[text].Add(procedure);
							continue;
						}
						List<Procedure> list = new List<Procedure>();
						list.Add(procedure);
						templateList.Add(text, list);
					}
				}
				templateListNeedRefresh = false;
			}
			return templateList;
		}
	}

	private List<Procedure> ProcedureList
	{
		get
		{
			if (procedureListNeedRefresh)
			{
				if (procedureList != null)
				{
					procedureList.Clear();
					procedureList = null;
				}
				procedureList = new List<Procedure>();
				Module[] modules = app.Modules;
				foreach (Module module in modules)
				{
					if (module.Procedures != null)
					{
						Procedure[] procedures = module.Procedures;
						foreach (Procedure item in procedures)
						{
							procedureList.Add(item);
						}
					}
				}
				procedureListNeedRefresh = false;
			}
			return procedureList;
		}
	}

	internal AppTreeMode TreeMode
	{
		get
		{
			return treeMode;
		}
		set
		{
			treeMode = value;
			this.StructureChanged(this, new TreePathEventArgs());
		}
	}

	public event EventHandler<TreeModelEventArgs> NodesChanged;

	public event EventHandler<TreeModelEventArgs> NodesInserted;

	public event EventHandler<TreeModelEventArgs> NodesRemoved;

	public event EventHandler<TreePathEventArgs> StructureChanged;

	public ApplicationTreeModel(Application app)
	{
		this.app = app;
		NeedRefresh = true;
	}

	public IEnumerable GetChildren(TreePath treePath)
	{
		if (treePath.IsEmpty())
		{
			switch (TreeMode)
			{
			case AppTreeMode.AppTreeView:
				return null;
			case AppTreeMode.AppModuleView:
				return app.Modules;
			case AppTreeMode.AppTemplateView:
				return TemplateList.Keys;
			case AppTreeMode.AppAlphaView:
			{
				List<Procedure> list = ProcedureList;
				list.Sort((Procedure p1, Procedure p2) => (!(p2.Name == p1.Name)) ? p1.Name.CompareTo(p2.Name) : 0);
				return list;
			}
			case AppTreeMode.AppCategoryView:
				return CategoryList.Keys;
			case AppTreeMode.AppModifiedView:
				return ProcedureList;
			}
		}
		else
		{
			switch (TreeMode)
			{
			case AppTreeMode.AppTreeView:
				return null;
			case AppTreeMode.AppModuleView:
				if (treePath.LastNode is Module)
				{
					return ((Module)treePath.LastNode).Procedures;
				}
				return null;
			case AppTreeMode.AppTemplateView:
			{
				string text2 = null;
				if (treePath.LastNode is string)
				{
					text2 = treePath.LastNode as string;
				}
				if (text2 != null && TemplateList.ContainsKey(text2))
				{
					return TemplateList[text2];
				}
				return null;
			}
			case AppTreeMode.AppAlphaView:
				return null;
			case AppTreeMode.AppCategoryView:
			{
				string text = null;
				if (treePath.LastNode is string)
				{
					text = treePath.LastNode as string;
				}
				if (text != null && CategoryList.ContainsKey(text))
				{
					return CategoryList[text];
				}
				return null;
			}
			case AppTreeMode.AppModifiedView:
				return null;
			}
		}
		return null;
	}

	public bool IsLeaf(TreePath treePath)
	{
		if (treePath.LastNode is Procedure)
		{
			return true;
		}
		return false;
	}

	public void IsCheckBoxVisibleValueNeeded(object sender, NodeControlValueEventArgs e)
	{
		if (e.Node.Tag == null)
		{
			return;
		}
		if (AllowSelection)
		{
			if (e.Node.Tag is Module)
			{
				e.Value = true;
			}
			else if (e.Node.Tag is Procedure)
			{
				e.Value = true;
			}
			else
			{
				e.Value = false;
			}
		}
		else
		{
			e.Value = false;
		}
	}

	public void TextNeeded(object sender, NodeControlValueEventArgs e)
	{
		if (e.Node.Tag == null)
		{
			return;
		}
		if (e.Node.Tag is Module)
		{
			e.Value = ((Module)e.Node.Tag).Name;
		}
		else if (e.Node.Tag is Procedure)
		{
			string text = ((Procedure)e.Node.Tag).Category;
			if (string.IsNullOrEmpty(text))
			{
				text = ((Procedure)e.Node.Tag).Template;
				text = text.Split(new char[1] { '(' }, 2)[0];
			}
			if (string.IsNullOrEmpty(text))
			{
				text = "ToDo";
			}
			if (string.IsNullOrEmpty(text))
			{
				e.Value = ((Procedure)e.Node.Tag).Name;
			}
			else
			{
				e.Value = ((Procedure)e.Node.Tag).Name + "(" + text + ")";
			}
		}
		else
		{
			e.Value = e.Node.Tag.ToString();
		}
	}

	public void IconNeeded(object sender, NodeControlValueEventArgs e)
	{
		if (e.Node.Tag != null)
		{
			if (e.Node.Tag is Module)
			{
				e.Value = Resources.TREEMOD;
			}
			else if (e.Node.Tag is Procedure)
			{
				e.Value = Resources.TREEPROC;
			}
			else
			{
				e.Value = Resources.TREELABEL;
			}
		}
	}
}
