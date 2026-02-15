using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public static class CodeCompletionDataUsageCache
{
	private struct UsageStruct
	{
		public int Uses;

		public int ShowCount;

		public UsageStruct(int Uses, int ShowCount)
		{
			this.Uses = Uses;
			this.ShowCount = ShowCount;
		}
	}

	private class SaveItemsComparer : IComparer<KeyValuePair<string, UsageStruct>>
	{
		public int Compare(KeyValuePair<string, UsageStruct> x, KeyValuePair<string, UsageStruct> y)
		{
			return -((double)x.Value.Uses / (double)x.Value.ShowCount).CompareTo((double)y.Value.Uses / (double)y.Value.ShowCount);
		}
	}

	private const long magic = 7306916068411589443L;

	private const short version = 1;

	private const int MinUsesForSave = 2;

	private static Dictionary<string, UsageStruct> dict;

	public static string CacheFilename => Path.Combine(PropertyService.ConfigDirectory, "CodeCompletionUsageCache.dat");

	private static void LoadCache()
	{
		dict = new Dictionary<string, UsageStruct>();
		ProjectService.SolutionClosed += delegate
		{
			SaveCache();
		};
		if (!File.Exists(CacheFilename))
		{
			return;
		}
		using (FileStream input = new FileStream(CacheFilename, FileMode.Open, FileAccess.Read))
		{
			using BinaryReader binaryReader = new BinaryReader(input);
			if (binaryReader.ReadInt64() != 7306916068411589443L)
			{
				LoggingService.Warn("CodeCompletionDataUsageCache: wrong file magic");
				return;
			}
			if (binaryReader.ReadInt16() != 1)
			{
				LoggingService.Warn("CodeCompletionDataUsageCache: unknown file version");
				return;
			}
			int num = binaryReader.ReadInt32();
			for (int num2 = 0; num2 < num; num2++)
			{
				string key = binaryReader.ReadString();
				int num3 = binaryReader.ReadInt32();
				int num4 = binaryReader.ReadInt32();
				if (num4 > 1000)
				{
					num4 /= 3;
					num3 /= 3;
				}
				dict.Add(key, new UsageStruct(num3, num4));
			}
		}
		LoggingService.Info("Loaded CodeCompletionDataUsageCache (" + dict.Count + " items)");
	}

	public static void SaveCache()
	{
		if (dict == null)
		{
			return;
		}
		int num;
		using (FileStream output = new FileStream(CacheFilename, FileMode.Create, FileAccess.Write))
		{
			using BinaryWriter writer = new BinaryWriter(output);
			num = SaveCache(writer);
		}
		LoggingService.Info("Saved CodeCompletionDataUsageCache (" + num + " of " + dict.Count + " items)");
	}

	private static int SaveCache(BinaryWriter writer)
	{
		writer.Write(7306916068411589443L);
		writer.Write((short)1);
		int dataUsageCacheItemCount = CodeCompletionOptions.DataUsageCacheItemCount;
		if (dict.Count < dataUsageCacheItemCount)
		{
			writer.Write(dict.Count);
			foreach (KeyValuePair<string, UsageStruct> item in dict)
			{
				writer.Write(item.Key);
				writer.Write(item.Value.Uses);
				writer.Write(item.Value.ShowCount);
			}
			return dict.Count;
		}
		List<KeyValuePair<string, UsageStruct>> list = new List<KeyValuePair<string, UsageStruct>>();
		foreach (KeyValuePair<string, UsageStruct> item2 in dict)
		{
			if (item2.Value.Uses > 2)
			{
				list.Add(item2);
			}
		}
		if (list.Count > dataUsageCacheItemCount)
		{
			list.Sort(new SaveItemsComparer());
		}
		int num = Math.Min(dataUsageCacheItemCount, list.Count);
		writer.Write(num);
		for (int i = 0; i < num; i++)
		{
			KeyValuePair<string, UsageStruct> keyValuePair = list[i];
			writer.Write(keyValuePair.Key);
			writer.Write(keyValuePair.Value.Uses);
			writer.Write(keyValuePair.Value.ShowCount);
		}
		return num;
	}

	public static void ResetCache()
	{
		dict = new Dictionary<string, UsageStruct>();
		try
		{
			if (File.Exists(CacheFilename))
			{
				File.Delete(CacheFilename);
			}
		}
		catch (Exception ex)
		{
			LoggingService.Warn("CodeCompletionDataUsageCache.ResetCache(): " + ex.Message);
		}
	}

	public static double GetPriority(string dotnetName, bool incrementShowCount)
	{
		if (!CodeCompletionOptions.DataUsageCacheEnabled)
		{
			return 0.0;
		}
		if (dict == null)
		{
			LoadCache();
		}
		if (!dict.TryGetValue(dotnetName, out var value))
		{
			return 0.0;
		}
		double num = (double)value.Uses / (double)value.ShowCount;
		if (value.Uses < 2)
		{
			num *= 0.2;
		}
		if (incrementShowCount)
		{
			value.ShowCount++;
			dict[dotnetName] = value;
		}
		return num;
	}

	public static void IncrementUsage(string dotnetName)
	{
		if (CodeCompletionOptions.DataUsageCacheEnabled)
		{
			if (dict == null)
			{
				LoadCache();
			}
			if (!dict.TryGetValue(dotnetName, out var value))
			{
				value = new UsageStruct(0, 2);
			}
			value.Uses++;
			dict[dotnetName] = value;
		}
	}
}
