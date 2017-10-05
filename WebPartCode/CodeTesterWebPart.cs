using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.WebControls;
using Microsoft.CSharp;
using System.Web;
using System.Text;
using System.CodeDom.Compiler;
using System.Reflection;
using System.IO;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using Microsoft.SharePoint.WebPartPages;
using CodeTesterWebPart.WebPartCode;

namespace CodeTesterWebPart {
    [Guid("c0f81d16-a5f6-4169-ac50-acd7e2d5e91d")]
    public class CodeTesterWebPart : System.Web.UI.WebControls.WebParts.WebPart {

        public CodeTesterWebPart() {
            this.ExportMode = WebPartExportMode.All;
        }

        #region Fields

        protected TextBox txbReferences;
        protected TextBox txbUsings;
        protected TextBox txbCodeSnippet;
        protected Button btnRunSnippet;
        protected Button btnSaveInputs;
        protected Literal litOutput;
        protected GridView gvCompilerResults;
        protected HiddenField hfDisplayUsings;
        protected HiddenField hfDisplayCode;
        protected PlaceHolder ph;

        private CodeTesterProvider _provider = null;
        private CodeTesterLanguage _language = CodeTesterLanguage.CSharp;
        private String _references = null;
        private String _usings = null;
        private String _code = null;
        private Boolean _showUsings = true;
        private Boolean _showCode = true;

        #endregion

        #region Properties

        protected CodeTesterProvider Provider {
            get {
                if (_provider == null)
                    switch (Language) {
                        case CodeTesterLanguage.CSharp:
                            _provider = new CodeTesterProviderCSharp();
                            break;
                        case CodeTesterLanguage.VB:
                            _provider = new CodeTesterProviderVB();
                            break;
                    }
                return _provider;
            }
        }

        [Personalizable]
        [WebBrowsable(false)]
        public String References {
            get { return _references; }
            set { _references = value; }
        }

        [Personalizable]
        [WebBrowsable(false)]
        public String Usings {
            get { return _usings; }
            set { _usings = value; }
        }

        [Personalizable]
        [WebBrowsable(false)]
        public String Code {
            get { return _code; }
            set { _code = value; }
        }

        [Personalizable]
        [WebBrowsable(false)]
        public Boolean ShowUsings {
            get { return _showUsings; }
            set { _showUsings = value; }
        }

        [Personalizable]
        [WebBrowsable(false)]
        public Boolean ShowCode {
            get { return _showCode; }
            set { _showCode = value; }
        }

        [Personalizable]
        [Description("Coding language")]
        [Category("Configuration")]
        [WebBrowsable(true)]
        public CodeTesterLanguage Language {
            get { return _language; }
            set {
                _language = value;
                if (ChildControlsCreated) 
                    ResetUI();
            }
        }

        private void ResetUI() {
            Code = null;
            References = null;
            Usings = null;
            ShowCode = true;
            ShowUsings = true;
            _provider = null;
            Controls.Clear();
            CreateChildControls();
        }



        #endregion

        #region WebPart Overrides

        protected override void CreateChildControls() {
            base.CreateChildControls();

            txbReferences = new TextBox();
            txbReferences.ID = "txbReferences";
            txbReferences.TextMode = TextBoxMode.MultiLine;
            txbReferences.CssClass = "CTWP-References";
            txbReferences.Text = References == null ? Provider.GetDefaultReferences() : References;
            Controls.Add(txbReferences);

            txbUsings = new TextBox();
            txbUsings.ID = "txbUsings";
            txbUsings.TextMode = TextBoxMode.MultiLine;
            txbUsings.Text = Usings == null ? Provider.GetDefaultUsings() : Usings;
            txbUsings.CssClass = "CTWP-Usings";
            Controls.Add(txbUsings);

            txbCodeSnippet = new TextBox();
            txbCodeSnippet.ID = "txbCodeSnippet";
            txbCodeSnippet.TextMode = TextBoxMode.MultiLine;
            txbCodeSnippet.CssClass = "CTWP-CodeSnippet";
            txbCodeSnippet.Text = Code == null ? Provider.GetDefaultCode() : Code;
            Controls.Add(txbCodeSnippet);

            btnRunSnippet = new Button();
            btnRunSnippet.Text = "Run that method !";
            btnRunSnippet.UseSubmitBehavior = false;
            btnRunSnippet.Click += new EventHandler(btnRunSnippet_Click);
            Controls.Add(btnRunSnippet);

            btnSaveInputs = new Button();
            btnSaveInputs.Text = "Save Inputs";
            btnSaveInputs.UseSubmitBehavior = false;
            btnSaveInputs.Click += new EventHandler(btnSaveInputs_Click);
            Controls.Add(btnSaveInputs);

            hfDisplayUsings = new HiddenField();
            hfDisplayUsings.ID = "hfDisplayUsings";
            hfDisplayUsings.Value = ShowUsings ? "block" : "none";
            Controls.Add(hfDisplayUsings);

            hfDisplayCode = new HiddenField();
            hfDisplayCode.ID = "hfDisplayCode";
            hfDisplayCode.Value = ShowCode ? "block" : "none";
            Controls.Add(hfDisplayCode);

            litOutput = new Literal();
            litOutput.EnableViewState = false;
            Controls.Add(litOutput);

            ph = new PlaceHolder();
            Controls.Add(ph);

            gvCompilerResults = new GridView();
            gvCompilerResults.Width = Unit.Percentage(100);
            gvCompilerResults.AutoGenerateColumns = false;
            gvCompilerResults.EnableViewState = false;
            gvCompilerResults.HeaderStyle.CssClass = "CTWP-TasksListHeader";
            gvCompilerResults.RowStyle.CssClass = "CTWP-TasksListRow";
            gvCompilerResults.RowDataBound += new GridViewRowEventHandler(gvCompilerResults_RowDataBound);

            BoundField bfType = new BoundField();
            bfType.DataField = "IsWarning";
            gvCompilerResults.Columns.Add(bfType);

            BoundField bfLine = new BoundField();
            bfLine.HeaderText = "Line";
            bfLine.DataField = "Line";
            gvCompilerResults.Columns.Add(bfLine);

            BoundField bfColumn = new BoundField();
            bfColumn.HeaderText = "Col";
            bfColumn.DataField = "Column";
            gvCompilerResults.Columns.Add(bfColumn);

            BoundField bfErrorText = new BoundField();
            bfErrorText.HeaderText = "Error Message";
            bfErrorText.DataField = "ErrorText";
            bfErrorText.ItemStyle.CssClass = "CTWP-FullWidth";
            gvCompilerResults.Columns.Add(bfErrorText);

            Controls.Add(gvCompilerResults);

        }

        void btnSaveInputs_Click(object sender, EventArgs e) {

            Code = txbCodeSnippet.Text;
            References = txbReferences.Text;
            Usings = txbUsings.Text;

            ShowUsings = hfDisplayUsings.Value == "block";
            ShowCode = hfDisplayCode.Value == "block";

            SetPersonalizationDirty();

        }

        protected override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            //CSS Registration
            CssRegistration cssControls = new CssRegistration();
            cssControls.Name = Page.ClientScript.GetWebResourceUrl(typeof(CodeTesterWebPart), "CodeTesterWebPart.Resources.CodeTesterWebPart.css");
            Controls.Add(cssControls);

            Page.ClientScript.RegisterClientScriptResource(typeof(CodeTesterWebPart), "CodeTesterWebPart.Resources.CodeTesterWebPart.js");

            txbUsings.Style[HtmlTextWriterStyle.Display] = hfDisplayUsings.Value;
        }

        protected override void RenderContents(HtmlTextWriter writer) {
            Provider.Render(writer, txbReferences, txbUsings, txbCodeSnippet, btnRunSnippet, btnSaveInputs, hfDisplayUsings, hfDisplayCode, litOutput, gvCompilerResults, ph);
        }

        #endregion

        #region Business

        void gvCompilerResults_RowDataBound(object sender, GridViewRowEventArgs e) {
            CompilerError err = e.Row.DataItem as CompilerError;
            if (err != null) {
                HtmlImage img = new HtmlImage();
                img.Src = Page.ClientScript.GetWebResourceUrl(typeof(CodeTesterWebPart), String.Format("CodeTesterWebPart.Resources.img{0}.PNG", err.IsWarning ? "Warning" : "Error"));
                DataControlFieldCell cell = e.Row.Controls[0] as DataControlFieldCell;
                cell.Controls.Add(img);
            }
        }

        void btnRunSnippet_Click(object sender, EventArgs e) {
            try {

                List<String> referencedAssemblies = new List<String>();

                StringReader sr = new StringReader(txbReferences.Text);
                while (sr.Peek() > -1) {
                    String ass = sr.ReadLine().Trim();
                    if (!String.IsNullOrEmpty(ass))
                        referencedAssemblies.Add(ass);
                }

                CompilerResults results = Provider.Compile(referencedAssemblies.ToArray(), txbUsings.Text, txbCodeSnippet.Text);
                gvCompilerResults.DataSource = results.Errors;
                gvCompilerResults.DataBind();

                if (results.Errors.HasErrors)
                    return;

                MethodInfo methodInfo = results.CompiledAssembly.GetType("TestNamespace.TestClass").GetMethod("TestMethod", BindingFlags.Static | BindingFlags.Public);
                Object[] args = new Object[2] { this.Page, this.ph };

                DateTime start = DateTime.Now;
                Object result = methodInfo.Invoke(null, args);
                TimeSpan duration = DateTime.Now.Subtract(start);

                litOutput.Text = String.Format("Duration : {0} ms<br /><b>Method returned : </b>{1}", Convert.ToInt32(duration.TotalMilliseconds).ToString(), (result == null) ? "<i>null value</i>" : (String)result);

            } catch (Exception ex) {
                litOutput.Text = "<b>An exception has been thrown : </b><br /><br />" + HttpUtility.HtmlEncode(ex.ToString());
            }
        }

        #endregion

    }
}
