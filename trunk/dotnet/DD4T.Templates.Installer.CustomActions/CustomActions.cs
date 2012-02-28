﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Text;


namespace DD4T.Templates.Installer.CustomActions
{
    [RunInstaller(true)]
    public partial class CustomActions : System.Configuration.Install.Installer
    {
        protected IDictionary State;

        public CustomActions()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary stateSaver)
        {
            State = stateSaver;
            base.Install(stateSaver);
            UploadTemplatesToTridion();

        }

        #region CustomCode
        protected void UploadTemplatesToTridion()
        {
            const string TemplatesDllName = "DD4T.Templates.dll";
            const string relPathTcmUploadAssembly = @"bin\client\TcmUploadAssembly.exe";


            var tridionHomeDir = Tridion.ContentManager.ConfigurationSettings.GetTcmHomeDirectory();
            var tcmUploadAssembly = string.Format(@"{0}{1}", tridionHomeDir, relPathTcmUploadAssembly);
            //If we cannot find TcmUploadAssembly: exit
            if (!File.Exists(tcmUploadAssembly))
            {
                throw new Exception(Resources.CanNotFindTcmUploadAssembly);
            }

            string userName = Context.Parameters["USERNAME"];
            string passWord = Context.Parameters["PASSWORD"];
            string cmeUrl = Context.Parameters["CME_URL"];
            string folderUri = Context.Parameters["FOLDER_URI"];
            string rawAssemblyPath = Context.Parameters["ASSEMBLYPATH"];

            var templatesDllPath = string.Format(@"{0}\{1}",Path.GetDirectoryName(rawAssemblyPath), TemplatesDllName);

            //Debugger.Launch();
            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = string.Format(" \"{0}\" /targeturl:{1} /folder:{2} /username:{3} /password:{4} /verbose", templatesDllPath, cmeUrl, folderUri, userName, passWord);
            start.FileName = tcmUploadAssembly;
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            start.UseShellExecute = false;
            int exitCode = 0;
            StringBuilder output = new StringBuilder();
            using (Process proc = Process.Start(start))
            {                         
                using (StreamReader sr = proc.StandardOutput)
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        output.AppendLine(line);                       
                    }
                }
                proc.WaitForExit();
                exitCode = proc.ExitCode;                
            }

            if (exitCode != 0)
            {                               
                throw new Exception(string.Format(Resources.ErrorMessage, exitCode, output.ToString()));
            }
        }
        #endregion
    }
}