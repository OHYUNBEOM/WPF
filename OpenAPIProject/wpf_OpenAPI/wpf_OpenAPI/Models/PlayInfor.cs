using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpf_OpenAPI.Models
{
    public class PlayInfor
    {
        public int Id { get; set; }
        public int Res_no { get; set; }
        public string Title { get; set; }
        public DateTime Op_st_dt { get; set; }
        public DateTime Op_ed_dt { get; set; }
        public string Op_at { get; set; }
        public string Place_nm { get; set; }
        public string Pay_at { get; set; }
    }
}
