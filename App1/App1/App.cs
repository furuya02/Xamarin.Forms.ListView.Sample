using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using LinqToTwitter;
using Xamarin.Forms;

namespace App1 {
    public class App {
        public static Page GetMainPage() {
            return new TweetPage();
        }
    }

    //１つのTweetを表現するクラス
    internal class Tweet {
        public string Name {get;set;}//表示名
        public string Text {get;set;}//メッセージ
        public string ScreenName {get;set;}//アカウント名
        public string CreatedAt {get;set;}//作成日時
        public string Icon {get;set;}//アイコン

    }
    internal class TweetPage : ContentPage {
        //データソース（class Tweetのコレクション）
        private readonly ObservableCollection<Tweet> _tweets = new ObservableCollection<Tweet>();

        public TweetPage() {
            //パディング(iPhone用)
            Padding = new Thickness(0, Device.OnPlatform(20, 0, 0), 0, 0);
            //ListViewの生成
            var listView = new ListView{
                ItemTemplate = new DataTemplate(typeof (MyCell)),//セルの指定
                ItemsSource = _tweets,//データソースの指定
                HasUnevenRows = true,//行の高さを可変とする
            };
            //このページのコンテンツとしてListView(のみ)を指定する
            Content = new StackLayout() {
                Children = {listView,
               }
            };
            //更新 ("xamarin"という文字列を検索する)
            Refresh("xamarin");
        }

        //セル用のテンプレート
        private class MyCell : ViewCell {
            public MyCell() {

                //アイコン
                var icon = new Image();
                icon.WidthRequest = icon.HeightRequest = 50;//アイコンのサイズ
                icon.VerticalOptions = LayoutOptions.Start;//アイコンを行の上に詰めて表示
                icon.SetBinding<Tweet>(Image.SourceProperty, x=>x.Icon);
                
                //名前
                var name = new Label{Font = Font.SystemFontOfSize(12)};
                name.SetBinding<Tweet>(Label.TextProperty, x=>x.Name);

                //アカウント名
                var screenName = new Label{Font = Font.SystemFontOfSize(12)};
                screenName.SetBinding<Tweet>(Label.TextProperty, x=>x.ScreenName);

                //作成日時
                var createAt = new Label{Font = Font.SystemFontOfSize(8),TextColor = Color.Gray};
                createAt.SetBinding<Tweet>(Label.TextProperty, x=>x.CreatedAt);

                //メッセージ本文
                var text = new Label{Font = Font.SystemFontOfSize(10)};
                text.SetBinding<Tweet>(Label.TextProperty, x=>x.Text);

                //名前行
                var layoutName = new StackLayout {
                    Orientation = StackOrientation.Horizontal, //横に並べる
                    Children = { name,screenName }//名前とアカウント名を横に並べる
                };

                //サブレイアウト
                var layoutSub = new StackLayout{
                    Spacing = 0,//スペースなし
                    Children ={layoutName,createAt, text}//名前行、作成日時、メッセージを縦に並べる
                };

                View = new StackLayout{
                    Padding = new Thickness(5),
                    Orientation = StackOrientation.Horizontal, //横に並べる
                    Children ={icon,layoutSub} //アイコンとサブレイアウトを横に並べる
                };
            }

            //テキストの長さに応じて行の高さを増やす
            protected override void OnBindingContextChanged() {
                base.OnBindingContextChanged();

                //メッセージ
                var text = ((Tweet)BindingContext).Text;
                //メッセージを改行で区切って、各行の最大文字数を27として計算する
                var row = text.Split('\n').Select(l => l.Length / 27).Select(c => c + 1).Sum();
                Height = 12 + 8 + row*10 + 20;//名前行、作成日時行、メッセージ行、パディングの合計値
                if (Height <60){
                    Height = 60;//列の高さは、最低でも60とする
                }
            }
        }

        private async void Refresh(string searchString) {
            //認証
            var auth = new ApplicationOnlyAuthorizer() {
                CredentialStore = new InMemoryCredentialStore {
                    ConsumerKey = "{API Key}",
                    ConsumerSecret = "{API secret}",
                },
            };
            await auth.AuthorizeAsync();
            //コンテキストの作成
            var context = new TwitterContext(auth);
            var response = await (from search in context.Search
                            where search.Type == SearchType.Search &&
                                  search.Query == searchString &&
                                  search.Count == 30
                            select search).SingleOrDefaultAsync();
            //取得データの解釈
            foreach (var a in response.Statuses) {
                _tweets.Add(new Tweet {
                    Text = a.Text,
                    Name = a.User.Name,
                    ScreenName = a.User.ScreenNameResponse,
                    CreatedAt = a.CreatedAt.ToString("f"),
                    Icon = a.User.ProfileImageUrl
                });
            }
        }
    }
}
