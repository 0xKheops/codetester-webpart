using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.CodeDom.Compiler;

namespace CodeTesterWebPart.WebPartCode
{
    public abstract class CodeTesterProvider
    {

        public abstract void Render(HtmlTextWriter writer, TextBox txbReferences, TextBox txbUsings, TextBox txbCodeSnippet, Button btnRunSnippet, Button btnSaveInputs, HiddenField hfDisplayUsings, HiddenField hfDisplayCode, Literal litOutput, GridView gvCompilerResults, PlaceHolder ph);

        public abstract String GetDefaultReferences();

        public abstract String GetDefaultUsings();

        public abstract String GetDefaultCode();

        public abstract CompilerResults Compile( String[] referencedAssemblies, String usingsBlock, String methodContent);

    }
}
