using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using wp09_caliburnApp.Models;

namespace wp09_caliburnApp.ViewModels
{
    public class MainViewModel : Screen
    {
        //Caliburn 버전업 되면서 사용방식 바뀐것
        private string firstName = "SuJin";
        public string FirstName 
        {
            get => firstName; 
            set
            {
                firstName = value;
                NotifyOfPropertyChange(() => FirstName);        // 속성값이 변경된걸 시스템에 알려주는 역할
                NotifyOfPropertyChange(nameof(CanClearName));
                NotifyOfPropertyChange(nameof(FullName));
            }
        }
        private string lastName = "Lee";
        public string LastName
        {
            get => lastName;
            set
            {
                lastName = value;
                NotifyOfPropertyChange(() => LastName);
                NotifyOfPropertyChange(() => FullName); //변화 통보
            }
        }
        public string FullName
        {
            get => $"{LastName} {FirstName}";
        }

        // 콤보박스에 바인딩 할 속성
        // 이런곳에선 var를 쓸 수 없음
        private BindableCollection<Person> managers = new BindableCollection<Person>();

        public BindableCollection<Person> Managers
        {
            get => managers;
            set => managers = value;
        }

        // 콤보박스에서 선택된 값을 지정할 속성
        private Person selectedManager;
        public Person SelectedManager
        {
            get => selectedManager;
            set
            {
                selectedManager = value;
                LastName = selectedManager.LastName;
                FirstName = selectedManager.FirstName;
                NotifyOfPropertyChange(nameof(SelectedManager));
            }
        }
        public MainViewModel()
        {
            // DB를 사용하면 여기서 DB 접속 => 데이터 Select까지
            Managers.Add(new Person { FirstName = "Tim", LastName = "Carmack" });
            Managers.Add(new Person { FirstName = "Steve", LastName = "Jobs" });
            Managers.Add(new Person { FirstName = "Bill", LastName = "Gates" });
            Managers.Add(new Person { FirstName = "Elon", LastName = "Musk" });
        }

        // 버튼 이벤트
        public void ClearName()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
        }

        //메서드와 이름 동일하게 하고 앞에 Can을 붙임
        public bool CanClearName
        {
            get => !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName);
        }
    }
}
