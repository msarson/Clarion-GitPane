using System;
using ICSharpCode.Core;

namespace SoftVelocity.Generator.Commands;

internal class CancelGeneration : AbstractMenuCommand
{
	private static bool _GenerationProcessCancelled = false;

	public static bool IsGenerationProcessCancelled => _GenerationProcessCancelled;

	public static event EventHandler GenerationCancelled;

	public override void Run()
	{
		OnGenerationCancelled();
	}

	public static void ResetGenerationProcessCancelled()
	{
		_GenerationProcessCancelled = false;
	}

	private static void OnGenerationCancelled()
	{
		_GenerationProcessCancelled = true;
		if (CancelGeneration.GenerationCancelled != null)
		{
			CancelGeneration.GenerationCancelled(null, null);
		}
	}
}
