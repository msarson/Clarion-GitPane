using System;
using System.Collections;
using System.Collections.Generic;

namespace ICSharpCode.Core;

public class AddInTreeNode
{
	public class TopologicalSort
	{
		private List<Codon> codons;

		private bool[] visited;

		private List<Codon> sortedCodons;

		private Dictionary<string, int> indexOfName;

		public TopologicalSort(List<Codon> codons)
		{
			this.codons = codons;
			visited = new bool[codons.Count];
			sortedCodons = new List<Codon>(codons.Count);
			indexOfName = new Dictionary<string, int>(codons.Count);
			for (int i = 0; i < codons.Count; i++)
			{
				visited[i] = false;
				indexOfName[codons[i].Id] = i;
			}
		}

		private void InsertEdges()
		{
			for (int i = 0; i < codons.Count; i++)
			{
				string insertBefore = codons[i].InsertBefore;
				if (insertBefore == null || !(insertBefore != ""))
				{
					continue;
				}
				if (indexOfName.ContainsKey(insertBefore))
				{
					string insertAfter = codons[indexOfName[insertBefore]].InsertAfter;
					if (insertAfter == null || insertAfter == "")
					{
						codons[indexOfName[insertBefore]].InsertAfter = codons[i].Id;
					}
					else
					{
						codons[indexOfName[insertBefore]].InsertAfter = insertAfter + ',' + codons[i].Id;
					}
				}
				else
				{
					LoggingService.WarnFormatted("Codon ({0}) specified in the insertbefore of the {1} codon does not exist!", insertBefore, codons[i]);
				}
			}
		}

		public List<Codon> Execute()
		{
			InsertEdges();
			for (int i = 0; i < codons.Count; i++)
			{
				Visit(i);
			}
			return sortedCodons;
		}

		private void Visit(int codonIndex)
		{
			if (visited[codonIndex])
			{
				return;
			}
			string[] array = codons[codonIndex].InsertAfter.Split(',');
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (text != null && text.Length != 0)
				{
					if (indexOfName.ContainsKey(text))
					{
						Visit(indexOfName[text]);
						continue;
					}
					LoggingService.WarnFormatted("Codon ({0}) specified in the insertafter of the {1} codon does not exist!", codons[codonIndex].InsertAfter, codons[codonIndex]);
				}
			}
			sortedCodons.Add(codons[codonIndex]);
			visited[codonIndex] = true;
		}
	}

	private Dictionary<string, AddInTreeNode> childNodes = new Dictionary<string, AddInTreeNode>();

	private List<Codon> codons = new List<Codon>();

	private bool isSorted;

	public Dictionary<string, AddInTreeNode> ChildNodes => childNodes;

	public List<Codon> Codons => codons;

	public List<T> BuildChildItems<T>(object caller)
	{
		List<T> list = new List<T>(codons.Count);
		if (!isSorted)
		{
			codons = new TopologicalSort(codons).Execute();
			isSorted = true;
		}
		foreach (Codon codon in codons)
		{
			ArrayList subItems = null;
			if (childNodes.ContainsKey(codon.Id))
			{
				subItems = childNodes[codon.Id].BuildChildItems(caller);
			}
			object obj = codon.BuildItem(caller, subItems);
			if (obj == null)
			{
				continue;
			}
			if (obj is IBuildItemsModifier buildItemsModifier)
			{
				buildItemsModifier.Apply(list);
				continue;
			}
			if (obj is T)
			{
				list.Add((T)obj);
				continue;
			}
			throw new InvalidCastException("The AddInTreeNode <" + codon.Name + " id='" + codon.Id + "' returned an instance of " + obj.GetType().FullName + " but the type " + typeof(T).FullName + " is expected.");
		}
		return list;
	}

	public ArrayList BuildChildItemsArrayList(object caller)
	{
		return BuildChildItems(caller);
	}

	public ArrayList BuildChildItems(object caller)
	{
		ArrayList arrayList = new ArrayList(codons.Count);
		if (!isSorted)
		{
			codons = new TopologicalSort(codons).Execute();
			isSorted = true;
		}
		foreach (Codon codon in codons)
		{
			ArrayList subItems = null;
			if (childNodes.ContainsKey(codon.Id))
			{
				subItems = childNodes[codon.Id].BuildChildItems(caller);
			}
			object obj = codon.BuildItem(caller, subItems);
			if (obj != null)
			{
				if (obj is IBuildItemsModifier buildItemsModifier)
				{
					buildItemsModifier.Apply(arrayList);
				}
				else
				{
					arrayList.Add(obj);
				}
			}
		}
		return arrayList;
	}

	public object BuildChildItem(string childItemID, object caller, ArrayList subItems)
	{
		foreach (Codon codon in codons)
		{
			if (codon.Id == childItemID)
			{
				return codon.BuildItem(caller, subItems);
			}
		}
		throw new TreePathNotFoundException(childItemID);
	}
}
