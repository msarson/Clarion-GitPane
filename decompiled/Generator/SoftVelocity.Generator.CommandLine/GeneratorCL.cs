using System.Collections.Generic;
using Clarion.Core.Redirection;
using Clarion.GEN;
using ICSharpCode.SharpDevelop;
using SoftVelocity.Generator.Commands;

namespace SoftVelocity.Generator.CommandLine;

internal abstract class GeneratorCL : ICommandLine
{
	internal class GenLogger : IRequestor
	{
		internal class GenErrorLogger : IErrorLog
		{
			private ICommandLineLogger logger;

			internal GenErrorLogger(ICommandLineLogger logger)
			{
				this.logger = logger;
				Win32Generator.CommandLineLogger = logger;
			}

			public void ClearErrors(bool ignored1, bool ignored2)
			{
			}

			public void SetError(string message)
			{
				logger.Error("GENE000", message);
			}

			public void SetError(string msg, string fileName, uint line, uint column)
			{
				logger.Error("GENE000", msg, fileName, (int)line, (int)column);
			}

			public void SetError(string msg, string fileName, uint offset)
			{
				logger.Error("GENE000", msg, fileName, 0, (int)offset);
			}

			public void SetError(GeneratorError errorCode)
			{
				ICommandLineLogger obj = logger;
				int num = (int)errorCode;
				obj.Error("GENE" + num.ToString("000"), errorCode.ToString());
			}

			public void StatusMessage(uint line, string message)
			{
			}

			public void Write(string message)
			{
				logger.Message(message);
			}
		}

		private GenErrorLogger errLogger;

		public IErrorLog ErrorLogger => errLogger;

		public bool QuietMode => true;

		public bool UseWideDialogs => true;

		internal GenLogger(ICommandLineLogger logger)
		{
			errLogger = new GenErrorLogger(logger);
		}
	}

	protected Win32GeneratorInstance gen;

	public virtual bool Enabled => true;

	protected abstract void DoRun(List<List<string>> parameters, ICommandLineLogger logger, RedirectionFile red);

	public void Run(List<List<string>> parameters, ICommandLineLogger logger, object red, bool forWindows, string version)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		Win32Generator.CommandLineLogger = logger;
		using (gen = Win32Generator.CreateGenerator(version, forWindows))
		{
			Win32Generator.SetRequestor(new GenLogger(logger));
			ClearCachedABCFilesMenuCommand.DoRun();
			DoRun(parameters, logger, (RedirectionFile)red);
		}
	}
}
