using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom.Compiler;
using Microsoft.VisualBasic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CodeTesterWebPart.WebPartCode {
    public class CodeTesterProviderVB : CodeTesterProvider {

        public override string GetDefaultReferences() {
            return @"System.dll
System.Data.dll
System.Web.dll
System.Xml.dll
C:\Program Files\Common Files\Microsoft Shared\Web Server Extensions\12\ISAPI\Microsoft.SharePoint.dll";
        }

        public override string GetDefaultUsings() {
            return @"Imports System
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports Microsoft.SharePoint";
        }

        public override string GetDefaultCode() {
            return "Return SPContext.Current.Web.Title";
        }

        public override void Render(HtmlTextWriter writer, TextBox txbReferences, TextBox txbUsings, TextBox txbCodeSnippet, Button btnRunSnippet, Button btnSaveInputs, HiddenField hfDisplayUsings, HiddenField hfDisplayCode, Literal litOutput, GridView gvCompilerResults, PlaceHolder ph) {

            Page page = txbCodeSnippet.Page;
            String clientId = txbCodeSnippet.Parent.ClientID;

            String usingsId = clientId + "usings";
            String toggleUsings = String.Format("CTWP_ToggleVisibility('{0}', '{1}', '{2}')", txbUsings.ClientID, usingsId, hfDisplayUsings.ClientID);

            String codeImageId = clientId + "imgCode";
            String codeTableId = clientId + "tableCode";
            String toggleCode = String.Format("CTWP_ToggleVisibility('{0}', '{1}', '{2}')", codeTableId, codeImageId, hfDisplayCode.ClientID);

            writer.Write(@"<table cellpadding=""2"" cellspacing=""0"" class=""CTWP-FullWidth"">");
            writer.Write(String.Format(@"<tr><td>Show/Hide : <a href=""javascript:CTWP_ToggleRefVisibility('{0}');"">References</a>&nbsp;<a href=""javascript:{1}"">Usings</a>&nbsp;<a href=""javascript:{2}"">Code</a></td></tr><td colspan=""4"">", txbReferences.ClientID, toggleUsings, toggleCode));
            txbReferences.RenderControl(writer);
            writer.Write(@"</td></tr><tr><td>");
            txbUsings.RenderControl(writer);
            writer.Write(@"<img id=""{0}"" src=""{1}"" style=""display:{3};cursor:hand;"" onclick=""{2}"" />", usingsId, page.ClientScript.GetWebResourceUrl(typeof(CodeTesterWebPart), "CodeTesterWebPart.Resources.Imports.PNG"), toggleUsings, hfDisplayUsings.Value == "block" ? "none" : "block");
            writer.Write(@"</td></tr><tr><td style=""padding:0px;"">");

            //Code
            writer.Write(@"<img id=""{0}"" src=""{1}"" style=""display:{3};cursor:hand;margin:2px;"" onclick=""{2}"" />", codeImageId, page.ClientScript.GetWebResourceUrl(typeof(CodeTesterWebPart), "CodeTesterWebPart.Resources.Code.PNG"), toggleCode, hfDisplayCode.Value == "block" ? "none" : "block");
            writer.Write(@"<table id=""{0}"" cellpadding=""2"" cellspacing=""0"" class=""CTWP-Table CTWP-FullWidth"" style=""display:{1};"">", codeTableId, hfDisplayCode.Value);
            writer.Write(@"<tr><td colspan=""4"">");
            writer.Write(@"<font color=""#0000ff"">Namespace</font> TestNamespace</td></tr>");
            writer.Write(@"<tr><td><div class=""CTWP-Indent"">&nbsp;</div></td><td colspan=""3""><font color=""#0000ff"">Public Class</font> TestClass</td></tr>");
            writer.Write(@"<tr><td><div class=""CTWP-Indent"">&nbsp;</div></td><td><div class=""CTWP-Indent"">&nbsp;</div></td><td colspan=""2""><font color=""#0000ff"">Public Shared Function</font> TestMethod(<font color=""#0000ff"">ByVal</font> page <font color=""#0000ff"">As</font> <font color=""#2b91af"">Page</font>, <font color=""#0000ff"">ByVal</font> ph <font color=""#0000ff"">As</font> <font color=""#2b91af"">PlaceHolder</font>) <font color=""#0000ff"">As</font> <font color=""#2b91af"">String</font></td></tr>");
            writer.Write(@"<tr><td><div class=""CTWP-Indent"">&nbsp;</div></td><td><div class=""CTWP-Indent"">&nbsp;</div></td><td><div class=""CTWP-Indent"">&nbsp;</div></td><td class=""CTWP-CodeSnippetTD"">");
            txbCodeSnippet.RenderControl(writer);
            writer.Write(@"</td></tr>");
            writer.Write(@"<tr><td><div class=""CTWP-Indent"">&nbsp;</div></td><td><div class=""CTWP-Indent"">&nbsp;</div></td><td colspan=""2""><font color=""#0000ff"">End Function</font></td></tr>");
            writer.Write(@"<tr><td><div class=""CTWP-Indent"">&nbsp;</div></td><td colspan=""3""><font color=""#0000ff"">End Class</font></td></tr>");
            writer.Write(@"<tr><td colspan=""4""><font color=""#0000ff"">End Namespace</font></td></tr></table></td></tr>");

            writer.Write(@"<tr><td align=""center"">");
            btnRunSnippet.RenderControl(writer);
            writer.Write(@"&nbsp;");
            btnSaveInputs.RenderControl(writer);
            writer.Write(@"</td></tr><tr><td>");
            litOutput.RenderControl(writer);
            writer.Write(@"</td></tr><tr><td>");
            gvCompilerResults.RenderControl(writer);
            writer.Write(@"</td></tr></table>");
            hfDisplayUsings.RenderControl(writer);
            hfDisplayCode.RenderControl(writer);
            ph.RenderControl(writer);
        }

        public override CompilerResults Compile(String[] referencedAssemblies, String usingsBlock, String methodContent) {

            StringBuilder source = new StringBuilder();
            source.AppendLine(usingsBlock);
            source.AppendLine(@"Namespace TestNamespace 
Public Class TestClass
Public Shared Function TestMethod(ByVal page As Page, ByVal ph As PlaceHolder) As String");
            source.AppendLine(methodContent);
            source.AppendLine(@"End Function
End Class
End Namespace");

            CompilerParameters options = new CompilerParameters();
            options.GenerateInMemory = true;
            options.GenerateExecutable = false;
            options.IncludeDebugInformation = true;

            foreach (String assemblyPath in referencedAssemblies)
                options.ReferencedAssemblies.Add(assemblyPath);

            VBCodeProvider codeProvider = new VBCodeProvider();
            return codeProvider.CompileAssemblyFromSource(options, source.ToString());

        }

    }
}
