using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Duality.Editor;

namespace Output.Editor
{
	/// <summary>
	/// Defines a Duality editor plugin.
	/// </summary>
    public class OutputEditorPlugin : EditorPlugin
	{
		public override string Id
		{
			get { return "OutputEditorPlugin"; }
		}
	}
}
