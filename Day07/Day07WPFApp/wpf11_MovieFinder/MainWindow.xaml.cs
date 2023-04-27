using MahApps.Metro.Controls;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using wpf11_MovieFinder.Logics;
using wpf11_MovieFinder.Models;

namespace wpf11_MovieFinder
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        bool isFavorite = false; // false->openApi 검색해온 결과, true-> 즐겨찾기 보기 클릭
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BtnNaverMovie_Click(object sender, RoutedEventArgs e)
        {
            await Commons.ShowMessageAsync("네이버 영화", "네이버 영화 사이트로 이동합니다!");
        }
        //네이버 API 사용 네이버 영화 검색 버튼
        private async void BtnSearchMovie_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtMovieName.Text))
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
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"오류 발생 : {ex.Message}");
            }
        }
        //textBox에서 엔터 누를 시 검색
        private void TxtMovieName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSearchMovie_Click(sender, e);
            }
        }
        //실제 검색 메서드
        private async void SearchMovie(string movieName)
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

                res = await req.GetResponseAsync(); //요청한 결과를 응답에 할당
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
            var jsonResult = JObject.Parse(result);
            var total = Convert.ToInt32(jsonResult["total_results"]);//전체 검색 결과 수
            var items = jsonResult["results"];
            //items를 데이터 그리드에 표시
            var json_array = items as JArray;
            var movieItems = new List<MovieItem>();//json에서 넘어온 배열을 담을 장소
            foreach (var val in json_array)
            {
                var MovieItem = new MovieItem()
                {
                    Adult = Convert.ToBoolean(val["adult"]),
                    Id = Convert.ToInt32(val["id"]),
                    Original_Language = Convert.ToString(val["original_language"]),
                    Original_Title = Convert.ToString(val["original_title"]),
                    Overview = Convert.ToString(val["overview"]),
                    Popularity = Convert.ToDouble(val["popularity"]),
                    Poster_Path = Convert.ToString(val["poster_path"]),
                    Release_Date = Convert.ToString(val["release_date"]),
                    Title = Convert.ToString(val["title"]),
                    Vote_Average = Convert.ToDouble(val["vote_average"])

                };
                movieItems.Add(MovieItem);
            }
            this.DataContext = movieItems;
            isFavorite = false;//즐겨찾기 아님을 명시
            StsResult.Content = $"OpenAPI {movieItems.Count}건 조회 완료";//상태바
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TxtMovieName.Focus();
        }

        // 그리드에서 셀 선택 시 이벤트
        private async void GrdResult_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                string posterPath = string.Empty; // 즐겨찾기 보기시 포스터 출력 위해 사용
                if (GrdResult.SelectedItem is MovieItem)//openAPI로 검색된 영화 포스터
                {
                    var movie = GrdResult.SelectedItem as MovieItem;
                    posterPath = movie.Poster_Path;
                }
                else if (GrdResult.SelectedItem is FavoriteMovieItem)//DB에서 가져온 즐겨찾기 포스터들
                {
                    var movie = GrdResult.SelectedItem as FavoriteMovieItem;
                    posterPath = movie.Poster_Path;
                }


                //var movie = GrdResult.SelectedItem as MovieItem;
                //Debug.WriteLine(posterPath);
                if (string.IsNullOrEmpty(posterPath))//포스터 이미지 없으면 No_Picture 출력
                {
                    ImgPoster.Source = new BitmapImage(new Uri("/No_Picture.png", UriKind.RelativeOrAbsolute));
                }
                else//포스터 이미지 경로가 있으면
                {
                    var base_url = "https://image.tmdb.org/t/p/w300_and_h450_bestv2";
                    ImgPoster.Source = new BitmapImage(new Uri($"{base_url}{posterPath}", UriKind.RelativeOrAbsolute));
                }
            }
            catch
            {
                await Commons.ShowMessageAsync("오류", $"이미지 로드 오류발생");
            }
        }
        // 영화 예고편 Youtube에서 보기
        private async void BtnWatchTrailer_Click(object sender, RoutedEventArgs e)
        {

            if (GrdResult.SelectedItems.Count == 0)
            {
                await Commons.ShowMessageAsync("Youtube", "영화를 선택해주세요!");
                return;
            }
            if (GrdResult.SelectedItems.Count > 1)
            {
                await Commons.ShowMessageAsync("Youtube", "영화를 하나만 선택해주세요!");
                return;
            }
            string movieName = string.Empty;

            if (GrdResult.SelectedItem is MovieItem)
            {
                var movie = GrdResult.SelectedItem as MovieItem;
                movieName = movie.Title;
            }
            else if (GrdResult.SelectedItem is FavoriteMovieItem)
            {
                var movie = GrdResult.SelectedItem as FavoriteMovieItem;
                movieName = movie.Title;
            }

            //await Commons.ShowMessageAsync("유튜브", $"예고편 볼 영화 : {movieName}");
            var trailerWindow = new TrailerWindow(movieName);
            trailerWindow.Owner = this;//trailerwindow의 부모는 MainWindow
            trailerWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;//부모창 중앙에 위치
            trailerWindow.ShowDialog();//모달창으로 열기. show()로 모달리스로 열면 부모창 건드려짐
        }

        //즐겨찾기 저장
        private async void BtnAddFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (GrdResult.SelectedItems.Count == 0)
            {
                await Commons.ShowMessageAsync("오류", "즐겨찾기에 추가할 영화를 선택하세요(복수선택 가능)");
                return;
            }

            if (isFavorite)
            {
                await Commons.ShowMessageAsync("오류", "이미 즐겨찾기한 영화입니다");
                return;
            }

            //List 만들 필요 없음
            //List<FavoriteMovieItem> list = new List<FavoriteMovieItem>();
            //foreach(MovieItem item in GrdResult.SelectedItems)
            //{
            //    var favoriteMovie = new FavoriteMovieItem
            //    {
            //        Id = item.Id,
            //        Title = item.Title,
            //        Original_Title = item.Original_Title,
            //        Adult = item.Adult,
            //        Overview = item.Overview,
            //        Release_Date = item.Release_Date,
            //        Original_Language = item.Original_Language,
            //        Vote_Average = item.Vote_Average,
            //        Popularity = item.Popularity,
            //        Poster_Path = item.Poster_Path,
            //        Reg_Date = DateTime.Now // 지금 저장하는 일시 생성
            //    };
            //    list.Add(favoriteMovie);
            //}

            //MySQL  Part!! 
            #region<MySQL Test>
            try
            {
                // MySQL DB 데이터 입력(테스트용)
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    if (conn.State == System.Data.ConnectionState.Closed) conn.Open();

                    var query = @"INSERT INTO FavoriteMovieItem
                                           (id
                                           ,Title
                                           ,Original_Title
                                           ,Realease_Date
                                           ,Original_Language
                                           ,Adult
                                           ,Popularity
                                           ,Vote_Average
                                           ,Poster_Path
                                           ,Overview
                                           ,Reg_Date)
                                     VALUES
                                           (@id
                                           ,@Title
                                           ,@Original_Title
                                           ,@Realease_Date
                                           ,@Original_Language
                                           ,@Adult
                                           ,@Popularity
                                           ,@Vote_Average
                                           ,@Poster_Path
                                           ,@Overview
                                           ,@Reg_Date)";
                    var insRes = 0;
                    foreach (MovieItem item in GrdResult.SelectedItems)
                    {
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@id", item.Id);
                        cmd.Parameters.AddWithValue("@Title", item.Title);
                        cmd.Parameters.AddWithValue("@Original_Title", item.Original_Title);
                        cmd.Parameters.AddWithValue("@Realease_Date", item.Release_Date);
                        cmd.Parameters.AddWithValue("@Original_Language", item.Original_Language);
                        cmd.Parameters.AddWithValue("@Adult", item.Adult);
                        cmd.Parameters.AddWithValue("@Popularity", item.Popularity);
                        cmd.Parameters.AddWithValue("@Vote_Average", item.Vote_Average);
                        cmd.Parameters.AddWithValue("@Poster_Path", item.Poster_Path);
                        cmd.Parameters.AddWithValue("@Overview", item.Overview);
                        cmd.Parameters.AddWithValue("@Reg_Date", DateTime.Now);

                        insRes += cmd.ExecuteNonQuery();
                    }
                }
                await Commons.ShowMessageAsync("저장", "DB 저장 성공");
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB저장 오류 {ex.Message}");
            }
            #endregion

            //try
            //{
            //    // DB 연결 확인
            //    using (SqlConnection conn = new SqlConnection(Commons.connString))
            //    {
            //        if (conn.State == System.Data.ConnectionState.Closed) conn.Open();

            //        var query = @"INSERT INTO [dbo].[FavoriteMovieItem]
            //                               ([id]
            //                               ,[Title]
            //                               ,[Original_Title]
            //                               ,[Realease_Date]
            //                               ,[Original_Language]
            //                               ,[Adult]
            //                               ,[Popularity]
            //                               ,[Vote_Average]
            //                               ,[Poster_Path]
            //                               ,[Overview]
            //                               ,[Reg_Date])
            //                         VALUES
            //                               (@id
            //                               ,@Title
            //                               ,@Original_Title
            //                               ,@Realease_Date
            //                               ,@Original_Language
            //                               ,@Adult
            //                               ,@Popularity
            //                               ,@Vote_Average
            //                               ,@Poster_Path
            //                               ,@Overview
            //                               ,@Reg_Date)";
            //        var insRes = 0; 
            //        foreach(MovieItem item in GrdResult.SelectedItems)//API로 조회된 결과
            //        {
            //            SqlCommand cmd = new SqlCommand(query, conn);
            //            cmd.Parameters.AddWithValue("@id", item.Id);
            //            cmd.Parameters.AddWithValue("@Title", item.Title);
            //            cmd.Parameters.AddWithValue("@Original_Title", item.Original_Title);
            //            cmd.Parameters.AddWithValue("@Realease_Date", item.Release_Date);
            //            cmd.Parameters.AddWithValue("@Original_Language", item.Original_Language);
            //            cmd.Parameters.AddWithValue("@Adult", item.Adult);
            //            cmd.Parameters.AddWithValue("@Popularity", item.Popularity);
            //            cmd.Parameters.AddWithValue("@Vote_Average", item.Vote_Average);
            //            cmd.Parameters.AddWithValue("@Poster_Path", item.Poster_Path);
            //            cmd.Parameters.AddWithValue("@Overview", item.Overview);
            //            cmd.Parameters.AddWithValue("@Reg_Date", DateTime.Now);

            //            insRes+=cmd.ExecuteNonQuery();
            //        }
            //        if(GrdResult.SelectedItems.Count == insRes)
            //        {
            //            await Commons.ShowMessageAsync("저장", "DB 저장 성공");
            //            StsResult.Content = $"즐겨찾기 {insRes}건 저장 완료!";
            //        }
            //        else
            //        {
            //            await Commons.ShowMessageAsync("저장", "DB 저장 오류 관리자에게 문의하세요!");
            //        }
            //    }
            //}
            //catch(Exception ex)
            //{
            //    await Commons.ShowMessageAsync("오류", $"DB저장 오류 {ex.Message}");
            //}
        }

        //즐겨찾기 보기
        private async void BtnViewFavorite_Click(object sender, RoutedEventArgs e)
        {
            this.DataContext = null;
            TxtMovieName.Text = string.Empty;

            List<FavoriteMovieItem> list = new List<FavoriteMovieItem>();
            #region<MySQL 즐겨찾기 보기>
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    if (conn.State == ConnectionState.Closed) conn.Open();
                    var query = @"SELECT id
                                      ,Title
                                      ,Original_Title
                                      ,Realease_Date
                                      ,Original_Language
                                      ,Adult
                                      ,Popularity
                                      ,Vote_Average
                                      ,Poster_Path
                                      ,Overview
                                      ,Reg_Date
                                  FROM FavoriteMovieItem
                                  ORDER BY id ASC";
                    var cmd = new MySqlCommand(query, conn);
                    var adapter = new MySqlDataAdapter(cmd);
                    var dSet = new DataSet();
                    adapter.Fill(dSet, "FavoriteMovieItem");
                    foreach (DataRow dr in dSet.Tables["FavoriteMovieItem"].Rows)
                    {
                        list.Add(new FavoriteMovieItem
                        {
                            Id = Convert.ToInt32(dr["id"]),
                            Title = Convert.ToString(dr["Title"]),
                            Original_Title = Convert.ToString(dr["Original_Title"]),
                            Original_Language = Convert.ToString(dr["Original_Language"]),
                            Adult = Convert.ToBoolean(dr["Adult"]),
                            Popularity = Convert.ToDouble(dr["Popularity"]),
                            Vote_Average = Convert.ToDouble(dr["Vote_Average"]),
                            Poster_Path = Convert.ToString(dr["Poster_Path"]),
                            Overview = Convert.ToString(dr["Overview"]),
                            Reg_Date = Convert.ToDateTime(dr["Reg_Date"])
                        });
                    }
                    this.DataContext = list;
                    isFavorite = true;
                    StsResult.Content = $"즐겨찾기 {list.Count}건 조회 완료!";
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB조회 오류 {ex.Message}");
            }
            #endregion
            #region<SSMS 즐겨찾기 보기>
            //try
            //{
            //    using(SqlConnection conn = new SqlConnection(Commons.connString))
            //    {
            //        if (conn.State == ConnectionState.Closed) conn.Open();
            //        var query = @"SELECT id
            //                          ,Title
            //                          ,Original_Title
            //                          ,Realease_Date
            //                          ,Original_Language
            //                          ,Adult
            //                          ,Popularity
            //                          ,Vote_Average
            //                          ,Poster_Path
            //                          ,Overview
            //                          ,Reg_Date
            //                      FROM FavoriteMovieItem
            //                      ORDER BY id ASC";
            //        var cmd = new SqlCommand(query, conn);
            //        var adapter = new SqlDataAdapter(cmd);
            //        var dSet = new DataSet();
            //        adapter.Fill(dSet, "FavoriteMovieItem");
            //        foreach(DataRow dr in dSet.Tables["FavoriteMovieItem"].Rows)
            //        {
            //            list.Add(new FavoriteMovieItem
            //            {
            //                Id = Convert.ToInt32(dr["id"]),
            //                Title = Convert.ToString(dr["Title"]),
            //                Original_Title = Convert.ToString(dr["Original_Title"]),
            //                Original_Language = Convert.ToString(dr["Original_Language"]),
            //                Adult = Convert.ToBoolean(dr["Adult"]),
            //                Popularity = Convert.ToDouble(dr["Popularity"]),
            //                Vote_Average = Convert.ToDouble(dr["Vote_Average"]),
            //                Poster_Path = Convert.ToString(dr["Poster_Path"]),
            //                Overview = Convert.ToString(dr["Overview"]),
            //                Reg_Date = Convert.ToDateTime(dr["Reg_Date"])
            //            });
            //        }
            //        this.DataContext = list;
            //        isFavorite = true;
            //        StsResult.Content = $"즐겨찾기 {list.Count}건 조회 완료!";
            //    }
            //}
            //catch(Exception ex)
            //{
            //    await Commons.ShowMessageAsync("오류", $"DB조회 오류 {ex.Message}");
            //}
            #endregion
        }

        private async void BtnDelFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (isFavorite == false)//DB에서 가져온 목록이 아니기에 삭제할수없도록
            {
                await Commons.ShowMessageAsync("오류", "즐겨찾기만 삭제할 수 있습니다!");
                return;
            }
            if (GrdResult.SelectedItems.Count == 0)//영화 선택 X
            {
                await Commons.ShowMessageAsync("오류", "삭제할 영화를 선택하세요.");
                return;
            }
            #region<MySQL 즐겨찾기 삭제>
            try // 삭제
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    if (conn.State == ConnectionState.Closed) conn.Open();

                    var query = "DELETE FROM FavoriteMovieItem WHERE id = @id";
                    var delRes = 0;
                    foreach (FavoriteMovieItem item in GrdResult.SelectedItems)
                    {
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@id", item.Id);

                        delRes += cmd.ExecuteNonQuery();
                    }

                    if (delRes == GrdResult.SelectedItems.Count)
                    {
                        await Commons.ShowMessageAsync("삭제", "DB삭제 성공!");
                        StsResult.Content = $"즐겨찾기 {delRes}건 삭제 완료!"; //삭제와 동시에 즐겨찾기 보기의 StsResult가 뿌려지기에 화면에 안나옴
                    }
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"Catch로 넘어옴 {ex.Message}");
            }
            #endregion
            #region<SSMS 즐겨찾기 삭제>
            //try // 삭제
            //{
            //    using(SqlConnection conn = new SqlConnection(Commons.connString))
            //    {
            //        if (conn.State == ConnectionState.Closed) conn.Open();

            //        var query = "DELETE FROM FavoriteMovieItem WHERE id = @id";
            //        var delRes = 0;
            //        foreach(FavoriteMovieItem item in GrdResult.SelectedItems)
            //        {
            //            SqlCommand cmd = new SqlCommand(query, conn);
            //            cmd.Parameters.AddWithValue("@id", item.Id);

            //            delRes += cmd.ExecuteNonQuery();
            //        }
            //        if(delRes==GrdResult.SelectedItems.Count)
            //        {
            //            await Commons.ShowMessageAsync("삭제", "DB삭제 성공!");
            //            StsResult.Content = $"즐겨찾기 {delRes}건 삭제 완료!"; //삭제와 동시에 즐겨찾기 보기의 StsResult가 뿌려지기에 화면에 안나옴
            //        }
            //        else
            //        {
            //            await Commons.ShowMessageAsync("삭제", "DB삭제 오류!");
            //        }
            //    }
            //}
            //catch(Exception ex)
            //{
            //    await Commons.ShowMessageAsync("오류", $"DB 삭제 오류 {ex.Message}");
            //}
            #endregion
            BtnViewFavorite_Click(sender, e);//즐겨찾기 보기 이벤트 핸들러 실행 -> 삭제와 동시에 다시 뿌려줌
        }
    }
}
