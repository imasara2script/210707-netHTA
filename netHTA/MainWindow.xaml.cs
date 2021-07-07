using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Web.WebView2.Core;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly CountdownEvent condition = new CountdownEvent(1);

        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();
        }

        async void InitializeAsync()
        {
            await web.EnsureCoreWebView2Async(null);
            web.CoreWebView2.NavigationCompleted += Web_NavigationCompleted;
            web.CoreWebView2.WebMessageReceived += MessageReceived;

            string URL = get引数();
            URL変更(URL);
        }

        private string get引数()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int idx = 1; idx < args.Length; idx++) // idx = 0からにするとWpfApp1.exeがダウンロードされる感じになっちゃう
            {
                return args[idx];
            }
            return "c:/Public/index.html";
            return "http://google.co.jp/";
        }

        private async void URL変更(string URL)
        {
            web.CoreWebView2.Navigate(URL);

            //非同期実行
            string result = "";
            await Task.Run(() =>
            {
                //読み込み完了まで待機
                if (condition.Wait(1000 * 10))
                {
                    result = "ok";
                    GetTitle();
                }
                else
                {
                    result = "timeout";
                }
            });

            // MessageBox.Show(result);
        }

        private void Web_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            //読み込み結果を判定
            if (e.IsSuccess)
                Console.WriteLine("complete");
            else
                Console.WriteLine(e.WebErrorStatus);

            //シグナル初期化
            condition.Signal();
            System.Threading.Thread.Sleep(1);
            condition.Reset();
        }

        private void GetTitle()
        {
            // 非同期にすると別スレッドになり、別スレッドになるとメインスレッドが所有するコントロールにアクセスできないので
            // メインスレッドのDispatcher(キューを管理するクラス)に実行を依頼する。
            // https://araramistudio.jimdo.com/2017/05/02/c-%E3%81%A7%E5%88%A5%E3%82%B9%E3%83%AC%E3%83%83%E3%83%89%E3%81%8B%E3%82%89%E3%82%B3%E3%83%B3%E3%83%88%E3%83%AD%E3%83%BC%E3%83%AB%E3%82%92%E6%93%8D%E4%BD%9C%E3%81%99%E3%82%8B/
            this.Dispatcher.Invoke(async () =>
            {
                //検索欄の文字列を取得する
                var webTitle = await web.ExecuteScriptAsync("document.title");
                Title = webTitle;
                // MessageBox.Show(result);
            });
        }

        //JavaScriptからメッセージを受信したときに実行します。
        private void MessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs args)
        {
            String text = args.TryGetWebMessageAsString();
            Uri iconUri = new Uri(text, UriKind.RelativeOrAbsolute);

            Icon = BitmapFrame.Create(iconUri);
            // MessageBox.Show("JavaScriptから\n\n" + text);
        }
    }
}
