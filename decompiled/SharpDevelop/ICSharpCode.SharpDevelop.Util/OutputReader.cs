using System;
using System.IO;
using System.Text;
using System.Threading;

namespace ICSharpCode.SharpDevelop.Util;

public class OutputReader
{
	private StreamReader reader;

	private string output = string.Empty;

	private Thread thread;

	public string Output => output;

	public event LineReceivedEventHandler LineReceived;

	public OutputReader(StreamReader reader)
	{
		this.reader = reader;
	}

	public void Start()
	{
		thread = new Thread(ReadOutput);
		thread.Name = "OutputReader";
		thread.Start();
	}

	public void WaitForFinish()
	{
		if (thread != null)
		{
			thread.Join();
		}
	}

	protected void OnLineReceived(string line)
	{
		if (this.LineReceived != null)
		{
			this.LineReceived(this, new LineReceivedEventArgs(line));
		}
	}

	private void ReadOutput()
	{
		output = string.Empty;
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		while (!flag)
		{
			string text = reader.ReadLine();
			if (text != null)
			{
				stringBuilder.Append(text);
				stringBuilder.Append(Environment.NewLine);
				OnLineReceived(text);
			}
			else
			{
				flag = true;
			}
		}
		output = stringBuilder.ToString();
	}
}
