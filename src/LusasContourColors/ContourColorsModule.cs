using KSharedEnums;
using Lusas.LPI;
using Lusas.Module;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LusasContourColors
{
    [ComVisible(true)]
    [Guid("568FC961-12D6-46DB-AD7C-C4F398FC3CEB"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IContourColorsModule
    {
    }

    [ComVisible(true)]
    [Guid("DB3BC9A7-D40C-413F-94F6-06688F78B534"), ClassInterface(ClassInterfaceType.None)]
    public class ContourColorsModule : LusasModuleClass, IContourColorsModule
    {
        private long _menuId;
        private ContourColorsDialog _dialog;

        protected override void onRefreshMainMenu()
        {
            var rootMenu = Modeller.getMainMenu();
            var modulesMenu = rootMenu.exists("Modules") ? rootMenu.getSubMenu("Modules") : rootMenu.appendMenu("Modules");
            _menuId = modulesMenu.appendItem("Contour Colours", "Change contour colours");
        }

        protected override bool onMenuClick(int menuID, object editingObj, object clientData = null)
        {
            if (menuID != _menuId) return false;

            try
            {
                if (_dialog != null && !_dialog.IsDisposed)
                {
                    _dialog.Activate();
                    _dialog.BringToFront();
                    return true;
                }

                var dialog = new ContourColorsDialog(Modeller);
                dialog.FormClosed += OnDialogClosed;
                _dialog = dialog;
                try
                {
                    dialog.Show(this);
                }
                catch
                {
                    dialog.FormClosed -= OnDialogClosed;
                    if (ReferenceEquals(_dialog, dialog)) _dialog = null;
                    dialog.Dispose();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Modeller.AfxMsgBox(ex.Message, TextOutStyles.textOutError_E);
            }

            return true;
        }

        private void OnDialogClosed(object sender, FormClosedEventArgs e)
        {
            var dialog = sender as ContourColorsDialog;
            if (dialog != null) dialog.FormClosed -= OnDialogClosed;
            if (ReferenceEquals(_dialog, dialog)) _dialog = null;
        }

        protected override void onExitModule()
        {
            if (_dialog != null && !_dialog.IsDisposed) _dialog.Close();
            base.onExitModule();
        }

        protected override bool onMenuUpdate(int menuID, object editingObj, ref bool enable, ref int isChecked, object clientData = null)
        {
            if (menuID != _menuId) return false;
            enable = true;
            return true;
        }
    }
}
