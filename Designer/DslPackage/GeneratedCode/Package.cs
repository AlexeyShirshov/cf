﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using VSShellInterop = global::Microsoft.VisualStudio.Shell.Interop;
using VSShell = global::Microsoft.VisualStudio.Shell;
using DslShell = global::Microsoft.VisualStudio.Modeling.Shell;
using DslDesign = global::Microsoft.VisualStudio.Modeling.Design;
using VSTextTemplatingHost = global::Microsoft.VisualStudio.TextTemplating.VSHost;
	
namespace Worm.Designer
{
	/// <summary>
	/// This class implements the VS package that integrates this DSL into Visual Studio.
	/// </summary>
	[VSShell::DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\9.0")]
	[VSShell::PackageRegistration(RegisterUsing = VSShell::RegistrationMethod.Assembly, UseManagedResourcesOnly = true)]
	[VSShell::ProvideToolWindow(typeof(DesignerExplorerToolWindow), MultiInstances = false, Style = VSShell::VsDockStyle.Tabbed, Orientation = VSShell::ToolWindowOrientation.Right, Window = "{3AE79031-E1BC-11D0-8F78-00A0C9110057}")]
	[VSShell::ProvideToolWindowVisibility(typeof(DesignerExplorerToolWindow), Constants.DesignerEditorFactoryId)]
	[VSShell::ProvideEditorFactory(typeof(DesignerEditorFactory), 103, TrustLevel = VSShellInterop::__VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
	[VSShell::ProvideEditorExtension(typeof(DesignerEditorFactory), "." + Constants.DesignerFileExtension, 32)]
	[DslShell::ProvideRelatedFile("." + Constants.DesignerFileExtension, Constants.DefaultDiagramExtension,
		ProjectSystem = DslShell::ProvideRelatedFileAttribute.CSharpProjectGuid,
		FileOptions = DslShell::RelatedFileType.FileName)]
	[DslShell::ProvideRelatedFile("." + Constants.DesignerFileExtension, Constants.DefaultDiagramExtension,
		ProjectSystem = DslShell::ProvideRelatedFileAttribute.VisualBasicProjectGuid,
		FileOptions = DslShell::RelatedFileType.FileName)]
	[DslShell::RegisterAsDslToolsEditor]
	[global::System.Runtime.InteropServices.ComVisible(true)]
	internal abstract partial class DesignerPackageBase : DslShell::ModelingPackage
	{
		/// <summary>
		/// Initialization method called by the package base class when this package is loaded.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();

			// Register the editor factory used to create the DSL editor.
			this.RegisterEditorFactory(new DesignerEditorFactory(this));

			// Create the command set that handles menu commands provided by this package.
			DesignerCommandSet commandSet = new DesignerCommandSet(this);
			commandSet.Initialize();
			
			// Register the model explorer tool window for this DSL.
			this.AddToolWindow(typeof(DesignerExplorerToolWindow));

			if (this.DesignTimeRunMode)
			{
				// Toolbar registration doesn't work well under design run mode as the toolbox needs to be reset
				// Instead we'll dynamically zap and recreate our toolbox every time
				// Regular users of the finished tool will get the toolbox setup in the perfectly normal way
				this.SetupDynamicToolbox();
			}
		}
		
		/// <summary>
		/// Retrieves the toolbox items used by this DSL-based editor.
		/// </summary>
		[global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
		protected override global::System.Collections.Generic.IList<DslDesign::ModelingToolboxItem> CreateToolboxItems()
		{
			global::Worm.Designer.DesignerToolboxHelper toolboxHelper = new global::Worm.Designer.DesignerToolboxHelper(this);
			try
			{
				return toolboxHelper.CreateToolboxItems();
			}
			catch(global::System.Exception e)
			{
				global::System.Diagnostics.Debug.Fail("Exception thrown during toolbox item creation.  This may result in Package Load Failure:\r\n\r\n" + e);
				throw;
			}
		}
	}
}
//
// Package attributes which may need to change are placed on the partial class below, rather than in the main include file.
//
namespace Worm.Designer
{
	using System;
	using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.TextTemplating.VSHost;
    using Microsoft.Win32;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Resources;
	using System.Drawing; 
	using System.Windows.Forms; 
	using System.IO;
	using System.Xml;
	using EnvDTE;
	using Worm.CodeGen.Core;
	using Microsoft.VisualStudio.CommandBars;
	using Microsoft.VisualStudio.Modeling;
	using Microsoft.VisualStudio.Modeling.Shell;

	/// <summary>
	/// Double-derived class to allow easier code customization.
	/// </summary>

    [ProvideCodeGenerator(typeof(WormCodeGenerator), "WormCodeGenerator", "Code generator for .wxml files", true, ProjectSystem = ProvideCodeGeneratorAttribute.CSharpProjectGuid)]
    [ProvideCodeGenerator(typeof(WormCodeGenerator), "WormCodeGenerator", "Code generator for .wxml files", true, ProjectSystem = ProvideCodeGeneratorAttribute.VisualBasicProjectGuid)]
	[ProvideToolWindow(typeof(WormToolWindow), MultiInstances = false, Style = VSShell::VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Bottom)]
	[ProvideToolWindowVisibility(typeof(WormToolWindow), Constants.DesignerEditorFactoryId )]
	[VSShell::ProvideMenuResource("1000.ctmenu", 6)]
	[VSShell::ProvideToolboxItems(1)]
	[VSTextTemplatingHost::ProvideDirectiveProcessor(typeof(global::Worm.Designer.DesignerDirectiveProcessor), global::Worm.Designer.DesignerDirectiveProcessor.DesignerDirectiveProcessorName, "A directive processor that provides access to Designer files")]
	[global::System.Runtime.InteropServices.Guid(Constants.DesignerPackageId)]
	internal sealed partial class DesignerPackage : DesignerPackageBase
	{
		CommandBarEvents menuItemHandler;
        CommandBarControl menuItem;
        
		protected override void Initialize()
		{
			base.Initialize();
			
			this.AddToolWindow(typeof(Worm.Designer.WormToolWindow));
			
			System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            DTE dte = Helper.GetDTE(currentProcess.Id.ToString());
            ResourceManager manager = new ResourceManager("Worm.Designer.VSPackage", typeof(DesignerPackage).Assembly);
            Icon icon = new Icon((Icon)manager.GetObject("2671"), new Size(16, 16));
            string path = Path.Combine(this.UserLocalDataPath, "worm.ico");
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                icon.Save(fs);
            }

            Associate(".wxml", "ClassID.ProgID", "Worm Designer", path, dte.FileName);           
                    
            CommandBar commandBar = ((CommandBars)dte.Application.CommandBars)["Tools"];
            menuItem = commandBar.FindControl(MsoControlType.msoControlButton, 1, "", null, true);
            if (menuItem == null)
            {
                menuItem = commandBar.Controls.Add(MsoControlType.msoControlButton, System.Reflection.Missing.Value,
                                             System.Reflection.Missing.Value, 1, true);
                menuItem.Caption = "Import worm file";

            }


            menuItemHandler = (CommandBarEvents)dte.Application.Events.get_CommandBarEvents(menuItem);
            menuItemHandler.Click += new _dispCommandBarControlEvents_ClickEventHandler(menuItemHandler_Click);
        }
        
        private void menuItemHandler_Click(object CommandBarControl, ref bool handled, ref bool CancelDefault)
        {
            System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            DTE dte = Helper.GetDTE(currentProcess.Id.ToString());
            if (dte.ActiveDocument != null && Path.GetExtension(dte.ActiveDocument.Name) == ".wxml")
            {
                if (MessageBox.Show("Do you want to import data from worm file? Current model will be replaced by imported.", "Confirmation", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }
            }
            else
            {
                 MessageBox.Show("Worm designer file should be opened to import data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 return;
            }

           
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = "xml";
            dialog.Filter = "Xml files|*.xml|All files|*.*";
            dialog.RestoreDirectory = true;
            dialog.Title = "Load worm file";
		   if (dialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
					using (FileStream stream = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read))
					{
						using (XmlReader rdr = XmlReader.Create(stream))
						{
							OrmObjectsDef ormObjectsDef = OrmObjectsDef.LoadFromXml(rdr, new SXmlUrlResolver(System.IO.Path.GetDirectoryName(dialog.FileName)));
							if(DesignerDocView.ActiveWindow != null)
							{
								ModelingDocData data = DesignerDocView.ActiveWindow.DocData;
								if ((data != null) && data is DesignerDocData)
								{
									WormModel model = (WormModel)data.RootElement;
									XmlHelper.Import(model, ormObjectsDef);
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show("Cannot read file: " + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
        }
    
		class SXmlUrlResolver : XmlUrlResolver
        {
            private string _dir;

            public SXmlUrlResolver(string dir)
            {
                _dir = dir;
            }

            public override Uri ResolveUri(Uri baseUri, string relativeUri)
            {
                return new Uri(System.IO.Path.Combine(_dir, relativeUri));
            }
        }
		
		// Associate file extension with progID, description, icon and application
		 public static void Associate(string extension, string progID, string description, string icon, string application)
        {
            Registry.ClassesRoot.CreateSubKey(extension).SetValue("", progID);
            if (progID != null && progID.Length > 0)
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(progID))
                {
                    if (description != null)
                        key.SetValue("", description);
                    if (icon != null)
                        key.CreateSubKey("DefaultIcon").SetValue("", ToShortPathName(icon));
                    if (application != null)
                        key.CreateSubKey(@"Shell\Open\Command").SetValue("", ToShortPathName(application) + " \"%1\"");
                }
        }

        // Return true if extension already associated in registry
        public static bool IsAssociated(string extension)
        {
            return (Registry.ClassesRoot.OpenSubKey(extension, false) != null);
        }

        [DllImport("Kernel32.dll")]
        private static extern uint GetShortPathName(string lpszLongPath, [Out] StringBuilder lpszShortPath, uint cchBuffer);

        // Return short path format of a file name
        private static string ToShortPathName(string longName)
        {
            StringBuilder s = new StringBuilder(1000);
            uint iSize = (uint)s.Capacity;
            uint iRet = GetShortPathName(longName, s, iSize);
            return s.ToString();
        }
	}
}




