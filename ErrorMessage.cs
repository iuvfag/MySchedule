using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MySchedule
{
    internal static class ErrorMessage
    {
        internal static void errorMessage()
        {
            MessageBox.Show("アプリケーションを終了します", "予期せぬエラーが発生しました");
            Application.Exit();
        }
    }
}
