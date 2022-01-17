using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
            session.Log($"REMOVE_DLG_SHOWN = {session["REMOVE_DLG_SHOWN"]}");


            string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            appdataPath = Path.Combine(appdataPath, APPDATA_FOLDER_NAME);
            session.Log($"AppDataPath '{appdataPath}'");

            bool removeDlgShown = session["REMOVE_DLG_SHOWN"] == "1";
            bool deleteUserData = session["REMOVEUSERDATA"] == "1";

            if (!removeDlgShown)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var frm = new Form
                {
                    ClientSize = new Size(493, 200),
                    Text = session["ProductName"] + " Setup",
                    ShowIcon = false,
                    //ShowInTaskbar = false,
                    MinimizeBox = false,
                    MaximizeBox = false,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterScreen
                };
                var banner = new PictureBox
                {
                    Image = GetImage(session, "WixUI_Bmp_Banner"),
                    Height = 58,
                    Dock = DockStyle.Top,
                };

                frm.Controls.Add(banner);
                var chk = new CheckBox
                {
                    Text = "Remove user data and settings",
                    Location = new Point(20, 90),
                    Width = 200,
                    TabIndex = 0
                };
                frm.Controls.Add(chk);

                banner.Controls.Add(new Label
                {
                    Text = "Removing " + session["ProductName"],
                    Font = new Font(frm.Font, FontStyle.Bold),
                    Location = new Point(25,20),
                    Width = 200,
                    BackColor = Color.Transparent
                });
                var btnCancel = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel, 
                    TabIndex = 2
                };
                frm.Controls.Add(btnCancel);
                btnCancel.Location = new Point(frm.ClientSize.Width - btnCancel.Width - 6, frm.ClientSize.Height - btnCancel.Height - 6);
                var btnNext = new Button
                {
                    Text = "Uninstall",
                    DialogResult = DialogResult.OK,
                    TabIndex = 1
                };
                btnNext.Location = new Point(btnCancel.Left - btnNext.Width - 6, btnCancel.Top);

                frm.Controls.Add(btnNext);
                

                
                var dlgResult = frm.ShowDialog();
                if (dlgResult == DialogResult.OK)
                    deleteUserData = chk.Checked;
                else
                {
                    return ActionResult.UserExit;
                }

            }

            if (deleteUserData)
            {
                session.Log($"Deleting folder '{appdataPath}'");
                if (Directory.Exists(appdataPath))
                    Directory.Delete(appdataPath, true);
            }


            return ActionResult.Success;
        }

        private static Image GetImage(Session session, string imagename)
        {
            try
            {
                var ms = GetBinaryStream(session, imagename);
                return Bitmap.FromStream(ms);
            }
            catch
            {
                return null;
            }
        }

        private static MemoryStream GetBinaryStream(Session session, string binaryName)
        {
            var v = session.Database.OpenView("SELECT `Data` FROM `Binary` WHERE `Name` = '{0}'", binaryName);

            v.Execute();

            Record r = v.Fetch();
            try
            {
                using (var stream = r.GetStream("Data"))
                {
                    var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    ms.Position = 0;
                    return ms;
                }
            }
            catch 
            {
                return null;
            }
        }
    }
}
