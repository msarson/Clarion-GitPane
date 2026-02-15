using System;
using System.Collections.Generic;
using Clarion.Core.Options;

namespace SoftVelocity.Common;

public abstract class VersionInformation : IComparable
{
	private string name;

	private string m_directory;

	private string m_libsrc;

	private string m_redFile;

	private bool m_useInclude;

	private Dictionary<string, string> m_macros;

	protected ClarionVersion m_version;

	internal ClarionVersion Version => m_version;

	internal string Directory
	{
		get
		{
			return m_directory;
		}
		set
		{
			m_directory = value;
		}
	}

	internal string Libsrc
	{
		get
		{
			return m_libsrc;
		}
		set
		{
			m_libsrc = value;
		}
	}

	internal string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	internal string RedirectionFileName
	{
		get
		{
			return m_redFile;
		}
		set
		{
			m_redFile = value;
		}
	}

	internal bool UseInclude
	{
		get
		{
			return m_useInclude;
		}
		set
		{
			m_useInclude = value;
		}
	}

	internal Dictionary<string, string> Macros => m_macros;

	public int CompareTo(object obj)
	{
		return name.CompareTo(obj.ToString());
	}

	private void Setup()
	{
		name = m_version.Name;
		m_directory = m_version.Path;
		m_libsrc = m_version.Libsrc;
		RedirectionVersion redirectionFile = m_version.RedirectionFile;
		m_redFile = redirectionFile.Name;
		m_useInclude = redirectionFile.SupportsInclude;
		m_macros = new Dictionary<string, string>();
		foreach (KeyValuePair<string, string> macro in redirectionFile.Macros)
		{
			m_macros.Add(macro.Key, macro.Value);
		}
	}

	protected VersionInformation(string ver)
	{
		m_version = Versions.GetVersion(ver);
		Setup();
	}

	protected VersionInformation(string dir, bool forWindows)
	{
		m_version = Versions.NewVersion(dir, forWindows);
		Setup();
	}

	public virtual bool Store()
	{
		m_version.Path = m_directory;
		m_version.Name = name;
		m_version.Libsrc = m_libsrc;
		RedirectionVersion redirectionFile = m_version.RedirectionFile;
		redirectionFile.Name = m_redFile;
		redirectionFile.SupportsInclude = m_useInclude;
		redirectionFile.UpdateMacros(m_macros);
		return true;
	}

	public override string ToString()
	{
		return name;
	}

	internal bool Remove()
	{
		return Versions.Remove(m_version, true);
	}
}
