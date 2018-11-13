﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MySchedule
{
    public partial class ResetPassword : Form
    {

        //パスワード変更用のメソッド

        public String userId { get; set; }

        public ResetPassword()
        {
            InitializeComponent();
        }

        private void ResetPassword_Load(object sender, EventArgs e)
        {
            //入力内容を隠す
            textBox1.PasswordChar = '*';
            textBox2.PasswordChar = '*';
            textBox3.PasswordChar = '*';
        }

        /// <summary>
        /// 「戻る」ボタンが押された場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, EventArgs e)
        {
            //このフォームを閉じる
            this.Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            //入力欄の値をそれぞれフィールド変数に格納
            String currentPassword = textBox1.Text;
            String newPassword = textBox2.Text;
            String reConfirmationNewPassword = textBox3.Text;

            //入力内容の正誤チェック
            String currentPasswordCheck = currentPassword.DoCheck("現在のパスワード", 5, 15);
            String newPasswordCheck = newPassword.DoCheck("新しいパスワード", 5, 15);
            String reConfirmarionNewPasswordCheck = reConfirmationNewPassword.DoCheck("新しいパスワード(再確認用)", 5, 15);
            
            //新しいパスワードと再確認用パスワードが等しいか調べる
            String passwordCompare = newPassword.ValueCompare(reConfirmationNewPassword,
                "新しいパスワード", "新しいパスワード(再確認用)");

            //すべてのチェックに問題がなければ次の処理へ
            if (String.IsNullOrWhiteSpace(currentPasswordCheck) && String.IsNullOrWhiteSpace(newPasswordCheck) && 
                String.IsNullOrWhiteSpace(reConfirmarionNewPasswordCheck) && String.IsNullOrWhiteSpace(passwordCompare))
            {
                //コモンユーティリティの呼び出し
                CommonUtility cu = new CommonUtility();
                //ログインIDをハッシュ関数化
                String userIdHash = cu.CreateHashKey(userId);
                //現在のパスワードをハッシュ関数化
                currentPassword = cu.CreateHashKey(currentPassword);
                //ログインIDのハッシュ関数と現在のパスワードのハッシュ関数を連結してさらにハッシュ関数化
                currentPassword = cu.CreateHashKey(userIdHash, currentPassword);

                //新しいパスワードをハッシュ関数化
                newPassword = cu.CreateHashKey(newPassword);
                //ログインIDのハッシュ関数と新しいパスワードのハッシュ関数を連結してさらにハッシュ関数化
                newPassword = cu.CreateHashKey(userIdHash, newPassword);

                UserInfoDAO uiDAO = new UserInfoDAO();

                //ログインIDとパスワードをもとに、まず該当するユーザーがいるかどうか調べる
                if (uiDAO.IsExistsUser(userId, currentPassword))
                {
                    //新しいパスワードを設定するメソッドの呼び出し、更新件数を格納したresultを受け取る
                    int result = uiDAO.ResetPassword(userId, currentPassword, newPassword);

                    //更新件数が1件以上なら
                    if (result > 0)
                    {
                        //メッセージ表示
                        MessageBox.Show("パスワードを変更しました", "パスワード再設定完了");
                        this.Close();
                    }
                    //更新されたものがなければ
                    else
                    {
                        //エラーメッセージ表示
                        MessageBox.Show("パスワードが変更できませんでした", "エラーが発生しました");
                    }
                }
                //該当するユーザーがいなかった倍
                else
                {
                    //エラーメッセージ表示
                    MessageBox.Show("パスワードが異なります", "入力内容を確認してください");
                }


            }
            //パスワードが空欄、あるいは文字数違反の場合
            if (!(String.IsNullOrWhiteSpace(currentPasswordCheck)))
            {
                //エラーメッセージ表示
                MessageBox.Show(currentPasswordCheck, "入力内容を確認してください");
            }
            //新しいパスワードが空欄、あるいは文字数違反の場合
            if (!(String.IsNullOrWhiteSpace(newPasswordCheck)))
            {
                //エラーメッセージ表示
                MessageBox.Show(newPasswordCheck, "入力内容を確認してください");
            }
            //新しいパスワード(再確認用)が空欄、あるいは文字列違反の場合
            if (!(String.IsNullOrWhiteSpace(reConfirmarionNewPasswordCheck)))
            {
                //エラーメッセージの表示
                MessageBox.Show(reConfirmarionNewPasswordCheck, "入力内容を確認してください");
            }
            //2つのパスワードが一致しない場合
            if (!(String.IsNullOrWhiteSpace(passwordCompare)))
            {
                //エラーメッセージ表示
                MessageBox.Show(passwordCompare, "入力内容を確認してください");
            }
        }


    }
}
