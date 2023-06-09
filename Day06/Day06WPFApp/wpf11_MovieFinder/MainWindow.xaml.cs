﻿using MahApps.Metro.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using wpf11_MovieFinder.Logics;
using wpf11_MovieFinder.Models;

namespace wpf11_MovieFinder
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private  async void BtnNaverMovie_Click(object sender, RoutedEventArgs e)
        {
            await Commons.ShowMessageAsync("네이버 영화", "네이버 영화 사이트로 이동합니다!");
        }
        //네이버 API 사용 네이버 영화 검색 버튼
        private  async void BtnSearchMovie_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(TxtMovieName.Text))
            {
                await Commons.ShowMessageAsync("검색", "검색할 영화명을 입력하세요.");
                return;
            }
            //if(TxtMovieName.Text.Length<=1)//두글자는 써줘라
            //{
            //    await Commons.ShowMessageAsync("검색", "검색어를 2자이상 입력하세요.");
            //    return;
            //}
            try
            {// 실제 검색 메서드
                SearchMovie(TxtMovieName.Text);
            }
            catch(Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"오류 발생 : {ex.Message}");
            }
        }
        //textBox에서 엔터 누를 시 검색
        private void TxtMovieName_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Enter)
            {
                BtnSearchMovie_Click(sender, e);
            }
        }
        //실제 검색 메서드
        private  void SearchMovie(string movieName)
        {
            string tmdb_apiKey = "dd21bad55619f84e6e2823be028cbba2";
            string encoding_movieName = HttpUtility.UrlEncode(movieName, Encoding.UTF8);

            string openApiUri = $@"https://api.themoviedb.org/3/search/movie?api_key={tmdb_apiKey}&language=ko-KR&page=1&include_adult=false&query={encoding_movieName}";
            string result = string.Empty; //결과값

            //api 실행할 객체
            WebRequest req = null;
            WebResponse res = null;
            StreamReader reader = null;

            // TMDB API 요청
            try
            {
                req = WebRequest.Create(openApiUri);//URL을 넣어 객체 생성
                //Naver Api 헤더 설정
                res=req.GetResponse(); //요청한 결과를 응답에 할당
                reader = new StreamReader(res.GetResponseStream());
                result = reader.ReadToEnd();//json 결과 텍스트로 저장
                Debug.WriteLine(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                reader.Close();
                res.Close();
            }

            //result를 json으로 변경
            var jsonResult=JObject.Parse(result);
            var total = Convert.ToInt32(jsonResult["total_results"]);//전체 검색 결과 수
            var items = jsonResult["results"];
            //items를 데이터 그리드에 표시
            var json_array = items as JArray;
            var movieItems = new List<MovieItem>();//json에서 넘어온 배열을 담을 장소
            foreach(var val in json_array)
            {
                var MovieItem = new MovieItem()
                {
                    Id = Convert.ToInt32(val["id"]),
                    Title = Convert.ToString(val["title"]),
                    Original_Title = Convert.ToString(val["original_title"]),
                    Release_Date = Convert.ToString(val["release_date"]),
                    Vote_Average = Convert.ToDouble(val["vote_average"])
                };
                movieItems.Add(MovieItem);
            }
            this.DataContext=movieItems;
        }
    }
}
