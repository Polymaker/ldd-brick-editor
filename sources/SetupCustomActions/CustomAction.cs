using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;

namespace SetupCustomActions
{
    public class CustomActions
    {
        public const string APPDATA_FOLDER_NAME = "LDD Brick Editor";

        [CustomAction]
        public static ActionResult RemoveUserData(Session session)
        {

            //Debugger.Launch();
            session.Log("Begin RemoveUserData");
            session.Log($"REMOVEUSERDATA = {session["REMOVEUSERDATA"]}");

            string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            appdataPath = Path.Combine(appdataPath, APPDATA_FOLDER_NAME);
            session.Log($"AppDataPath '{appdataPath}'");

            if (session["REMOVEUSERDATA"] == "1")
            {
                session.Log($"Deleting folder '{appdataPath}'");
                if (Directory.Exists(appdataPath))
                    Directory.Delete(appdataPath, true);
            }


            return ActionResult.Success;
        }


    }
}
