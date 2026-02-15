using System;
using System.IO;
using log4net;
using log4net.Config;

namespace ICSharpCode.Core;

public static class LoggingService
{
	private static ILog log;

	public static bool IsDebugEnabled => log.IsDebugEnabled;

	public static bool IsInfoEnabled => log.IsInfoEnabled;

	public static bool IsWarnEnabled => log.IsWarnEnabled;

	public static bool IsErrorEnabled => log.IsErrorEnabled;

	public static bool IsFatalEnabled => log.IsFatalEnabled;

	static LoggingService()
	{
		log = LogManager.GetLogger(typeof(LoggingService));
		XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile));
	}

	public static void SetLoggerDebug()
	{
		LogManager.GetRepository().Threshold = LogManager.GetRepository().LevelMap["DEBUG"];
	}

	public static void SetLoggerAll()
	{
		LogManager.GetRepository().Threshold = LogManager.GetRepository().LevelMap["ALL"];
	}

	public static void SetLoggerOff()
	{
		LogManager.GetRepository().Threshold = LogManager.GetRepository().LevelMap["OFF"];
	}

	public static void Debug(object message)
	{
		log.Debug(message);
	}

	public static void DebugFormatted(string format, params object[] args)
	{
		log.DebugFormat(format, args);
	}

	public static void Info(object message)
	{
		log.Info(message);
	}

	public static void InfoFormatted(string format, params object[] args)
	{
		log.InfoFormat(format, args);
	}

	public static void Warn(object message)
	{
		log.Warn(message);
	}

	public static void Warn(object message, Exception exception)
	{
		log.Warn(message, exception);
	}

	public static void WarnFormatted(string format, params object[] args)
	{
		log.WarnFormat(format, args);
	}

	public static void Error(object message)
	{
		log.Error(message);
	}

	public static void Error(object message, Exception exception)
	{
		log.Error(message, exception);
	}

	public static void ErrorFormatted(string format, params object[] args)
	{
		log.ErrorFormat(format, args);
	}

	public static void Fatal(object message)
	{
		log.Fatal(message);
	}

	public static void Fatal(object message, Exception exception)
	{
		log.Fatal(message, exception);
	}

	public static void FatalFormatted(string format, params object[] args)
	{
		log.FatalFormat(format, args);
	}
}
