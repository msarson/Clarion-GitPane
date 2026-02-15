using ICSharpCode.SharpDevelop.Dom;

namespace SoftVelocity.Common.CodeCompletion;

public sealed class ClaExpressionContext
{
	private sealed class DefaultExpressionContext : ExpressionContext
	{
		private string name;

		public DefaultExpressionContext(string name)
		{
			this.name = name;
		}

		public override bool ShowEntry(object o)
		{
			return true;
		}

		public override string ToString()
		{
			return "[" + GetType().Name + ": " + name + "]";
		}
	}

	public static ExpressionContext LIKE = (ExpressionContext)(object)new DefaultExpressionContext("LIKE");
}
