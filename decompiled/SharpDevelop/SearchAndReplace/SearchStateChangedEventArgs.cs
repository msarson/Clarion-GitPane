using System;

namespace SearchAndReplace;

internal class SearchStateChangedEventArgs : EventArgs
{
	private bool searching;

	public bool Searching => searching;

	public SearchStateChangedEventArgs(bool state)
	{
		searching = state;
	}
}
