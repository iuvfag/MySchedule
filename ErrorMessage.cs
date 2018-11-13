using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MySchedule
{
    /// <summary>
    /// 画面にエラーメッセージを表示し、アプリケーションを閉じるためのメソッド
    /// </summary>
    internal static class ErrorMessage
    {
        /// <summary>
        /// エラーメッセージの表示とアプリケーションの終了を行うメソッド
        /// </summary>
        internal static void ApplicationClose()
        {
            MessageBox.Show("アプリケーションを終了します", "予期せぬエラーが発生しました");
            Application.Exit();     //アプリケーションの終了のための文
        }
    }
}
