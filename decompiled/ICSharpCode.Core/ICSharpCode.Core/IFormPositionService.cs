using System.Windows.Forms;

namespace ICSharpCode.Core;

public interface IFormPositionService
{
	void StorePosition(Form form);

	void StorePosition(Form form, string formName);

	void RestorePosition(Form form);

	void RestorePosition(Form form, string formName);

	void Apply(Form form);

	void Apply(Form form, string formName);
}
