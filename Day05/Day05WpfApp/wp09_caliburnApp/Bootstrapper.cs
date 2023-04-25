using Caliburn.Micro;   // BootstrapperBase 사용하기위함
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using wp09_caliburnApp.ViewModels;      // MainViewModel클래스 쓰기 위함

namespace wp09_caliburnApp
{
    // Caliburn 으로 MVVM 실행할 때 주요설정 진행
    public class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize(); // Caliburn MVVM 초기화
        }

        // 시작한 후에 초기화 진행
        protected async override void OnStartup(object sender, StartupEventArgs e)
        {
            //base.OnStartup(sender, e);  // 부모 클래스 실행은 주석처리
            await DisplayRootViewForAsync<MainViewModel>();
        }
    }
}
