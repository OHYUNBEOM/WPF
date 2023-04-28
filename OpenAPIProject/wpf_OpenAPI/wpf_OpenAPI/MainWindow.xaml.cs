using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
using MahApps.Metro.Controls;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using wpf_OpenAPI.Logics;
using wpf_OpenAPI.Models;
using wpf_OpenAPI.Logics;
using wpf_OpenAPI.Models;
using System.Diagnostics;
using System.Data;

namespace wpf_OpenAPI
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
        // 부산 연극 정보 API 조회
        private async void BtnPlaySearch_Click(object sender, RoutedEventArgs e)
        {
            string key = "te5%2FahqxnGW00Gw1jJ92lJYLwkOvVrP9DZdSdffoIyZB8Jb%2BzHMrpMxU0VQOlxdvK%2BRzzcNsLTr%2BLaoLfFzUQg%3D%3D";
            string openApiUri = $"https://apis.data.go.kr/6260000/BusanCulturePlayService/getBusanCulturePlay?serviceKey={key}&numOfRows=734&resultType=json";
            string result = string.Empty;
            //WebRequest,WebResponse
            WebRequest req = null;
            WebResponse res = null;
            StreamReader reader = null;
            try
            {
                req = WebRequest.Create(openApiUri);
                res = await req.GetResponseAsync();
                reader = new StreamReader(res.GetResponseStream());
                result = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"OpenAPI 조회오류 {ex.Message}");
            }

            var jsonResult = JObject.Parse(result);
            var status = Convert.ToInt32(jsonResult["code"]);

            try
            {
                if(status==00)
                {
                    var data = jsonResult["getBusanCulturePlay"]["item"];
                    var json_array = data as JArray;

                    var playinfors = new List<PlayInfor>();
                    foreach (var infor in json_array)
                    {
                        playinfors.Add(new PlayInfor
                        {
                            Id=0,
                            Res_no = Convert.ToInt32(infor["res_no"]),
                            Title = Convert.ToString(infor["title"]),
                            Op_st_dt = Convert.ToDateTime(infor["op_st_dt"]),
                            Op_ed_dt = Convert.ToDateTime(infor["op_ed_dt"]),
                            Op_at = Convert.ToString(infor["op_at"]),
                            Place_nm = Convert.ToString(infor["place_nm"]),
                            Pay_at = Convert.ToString(infor["pay_at"])
                        });
                    }
                    this.DataContext= playinfors;
                    StsResult.Content = $"부산 연극 정보 {playinfors.Count}건 조회 완료";
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"JSON 처리오류 {ex.Message}");
            }

        }
        //조회된 연극정보 DB(MySQL)에 저장
        private async void BtnPlayInsertDB_Click(object sender, RoutedEventArgs e)
        {
            if (GrdResult.Items.Count == 0)
            {
                await (Commons.ShowMessageAsync("오류", "연극 정보 조회 이후 저장하세요!"));
                return;
            }
            //DB에 저장
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    if (conn.State == System.Data.ConnectionState.Closed) conn.Open();
                    var query = @"INSERT INTO playinfor
                                (
                                Res_no,
                                Title,
                                Op_st_dt,
                                Op_ed_dt,
                                Op_at,
                                Place_nm,
                                Pay_at)
                                VALUES
                                (
                                @Res_no,
                                @Title,
                                @Op_st_dt,
                                @Op_ed_dt,
                                @Op_at,
                                @Place_nm,
                                @Pay_at)";
                    var insRes = 0;
                    foreach(var temp in GrdResult.Items)
                    {
                        if(temp is PlayInfor)
                        {
                            var item = temp as PlayInfor;
                            MySqlCommand cmd = new MySqlCommand(query, conn);

                            cmd.Parameters.AddWithValue("@Res_no", item.Res_no);
                            cmd.Parameters.AddWithValue("@Title", item.Title);
                            cmd.Parameters.AddWithValue("@Op_st_dt", item.Op_st_dt);
                            cmd.Parameters.AddWithValue("@Op_ed_dt", item.Op_ed_dt);
                            cmd.Parameters.AddWithValue("@Op_at", item.Op_at);
                            cmd.Parameters.AddWithValue("@Place_nm", item.Place_nm);
                            cmd.Parameters.AddWithValue("@Pay_at", item.Pay_at);

                            insRes += cmd.ExecuteNonQuery();
                        }
                    }
                    await Commons.ShowMessageAsync("저장", "DB 저장 성공!");
                    StsResult.Content = $"DB 저장 {insRes}건 성공!";
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB저장 오류! {ex.Message}");
            }
        }

        private void TxtPlayName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSearchPlay_Click(sender, e);
            }
        }

        private async void BtnSearchPlay_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(TxtPlayName.Text))
            {
                await Commons.ShowMessageAsync("검색", "검색할 연극명 입력하세요.");
                return;
            }
            this.DataContext = null;
            var Search_playinfors = new List<PlayInfor>();
            //입력한 글자가 연극명에 포함된 연극 DB에서 찾아서 출력
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    if (conn.State == ConnectionState.Closed) conn.Open();
                    var query = $@"SELECT Id,
	                               Res_no,
	                               Title,
	                               Op_st_dt,
	                               Op_ed_dt,
	                               Op_at,
	                               Place_nm,
	                               Pay_at
                                   FROM playinfor
                               WHERE Title LIKE '%{TxtPlayName.Text}%'";
                    var cmd = new MySqlCommand(query, conn);
                    var adapter = new MySqlDataAdapter(cmd);
                    var dSet = new DataSet();
                    adapter.Fill(dSet, "PlayInfor");
                    foreach(DataRow dr in dSet.Tables["PlayInfor"].Rows)
                    {
                        Search_playinfors.Add(new PlayInfor
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            Res_no = Convert.ToInt32(dr["res_no"]),
                            Title = Convert.ToString(dr["title"]),
                            Op_st_dt = Convert.ToDateTime(dr["op_st_dt"]),
                            Op_ed_dt = Convert.ToDateTime(dr["op_ed_dt"]),
                            Op_at = Convert.ToString(dr["op_at"]),
                            Place_nm = Convert.ToString(dr["place_nm"]),
                            Pay_at = Convert.ToString(dr["pay_at"])
                        });
                    }
                    this.DataContext = Search_playinfors;
                    StsResult.Content = $"{TxtPlayName.Text} 가 포함된 연극명 {Search_playinfors.Count}건 조회 완료!";
                }

            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"오류 발생 : {ex.Message}");
            }
        }
    }
}
