using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ChatClient;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using ServerCore;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        static ServerSession serverSession;
        public ObservableCollection<UserInfoViewModel> LobbyUsers { get; set; } = new ObservableCollection<UserInfoViewModel>();
        public ObservableCollection<RoomInfoViewModel> Rooms { get; set; } = new ObservableCollection<RoomInfoViewModel>();
        public ObservableCollection<UserInfoViewModel> ChatUsers { get; set; } = new ObservableCollection<UserInfoViewModel>();
        public ObservableCollection<string> ChatLogs { get; set; } = new ObservableCollection<string>();

        private string currentRoom = "";
        private bool isRoomOwner = false;

        public MainWindow()
        {
            InitializeComponent();

            IPAddress[] iPAddress = Dns.GetHostAddresses(Dns.GetHostName());
            IPEndPoint iPEndPoint = new IPEndPoint(iPAddress[1], 7777);

            Connector connector = new Connector();
            connector.Connect(iPEndPoint,
                (saea) =>
                {
                    serverSession = new ServerSession(saea.ConnectSocket);
                    serverSession.InitViewManager(new ViewManager(this));
                    serverSession.OnConnect(saea.RemoteEndPoint);
                    EnterLobby();
                    return serverSession;
                });

            LobbyUserList.ItemsSource = LobbyUsers;
            RoomListBox.ItemsSource = Rooms;
            ChatUserList.ItemsSource = ChatUsers;

            //RoomInfoViewModel roomInfoViewModel = new RoomInfoViewModel(new RoomInfo());
            //roomInfoViewModel.RoomName = "기본 방";
            //roomInfoViewModel.RoomId = 1;
            //roomInfoViewModel.RoomMasterUserId = 1;
            //Rooms.Add(roomInfoViewModel);
            //LobbyUsers.Add(new UserInfoViewModel(new UserInfo { Nickname = "User1", UserId = 1 }));

            this.Closing += MainWindow_Closing;
        }

        private void EnterLobby()
        {
            C_EnterLobby c_EnterLobby = new C_EnterLobby();
            serverSession.Send(c_EnterLobby);
        }

        public void ShowLobbyScreen()
        {
            LobbyScreen.Visibility = Visibility.Visible;
            ChatRoomScreen.Visibility = Visibility.Collapsed;
        }

        public void ShowChatRoomScreen()
        {
            LobbyScreen.Visibility = Visibility.Collapsed;
            ChatRoomScreen.Visibility = Visibility.Visible;
        }

        private void CreateRoomButton_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputDialog("방 생성", "방 이름을 입력하세요:");
            if (input.ShowDialog() == true)
            {
                C_CreateRoom c_CreateRoom = new C_CreateRoom
                {
                    RoomName = input.InputText
                };
                serverSession.Send(c_CreateRoom);
            }
        }

        private void JoinRoomButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is RoomInfoViewModel roomInfoViewModel)
            {
                C_EnterRoom c_EnterRoom = new C_EnterRoom
                {
                    RoomId = roomInfoViewModel.RoomId
                };
                serverSession.Send(c_EnterRoom);
                ChatLogs.Clear();
                isRoomOwner = false;
                DeleteRoomButton.Visibility = isRoomOwner ? Visibility.Visible : Visibility.Collapsed;
                ShowChatRoomScreen();
            }
        }

        private void LeaveRoomButton_Click(object sender, RoutedEventArgs e)
        {
            currentRoom = "";
            isRoomOwner = false;
            ShowLobbyScreen();
        }

        private void DeleteRoomButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(currentRoom))
            {
                //Rooms.Remove(currentRoom);
                currentRoom = "";
                ShowLobbyScreen();
            }
        }


        private void ChangeNicknameButton_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputDialog("닉네임 변경", "새 닉네임을 입력하세요:");
            if (input.ShowDialog() == true)
            {
                serverSession.TempNickname = input.InputText;

                // 서버에 닉네임 변경 요청을 보내는 로직 추가
                C_SetNickname c_SetNickname = new C_SetNickname
                {
                    Nickname = input.InputText
                };
                serverSession.Send(c_SetNickname);
            }
        }

        private void SendChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ChatInput.Text))
            {
                ChatLogs.Add(ChatInput.Text);
                ChatInput.Clear();
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            // 세션 정리 코드
            try
            {
                if (serverSession != null)
                {
                    serverSession.OnDisconnect(serverSession.Socket.RemoteEndPoint);
                    serverSession.Disconnect();
                    serverSession = null;
                }
                // 추가적인 정리 작업이 필요하다면 여기에 작성
            }
            catch (Exception ex)
            {
                // 로그 출력 또는 예외 처리
                Console.WriteLine("종료 중 오류: " + ex.Message);
            }
        }
    }

    public abstract class ProtoViewModelBase<TProto> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected TProto _proto;

        public TProto Proto => _proto;

        public ProtoViewModelBase(TProto proto)
        {
            _proto = proto;
        }

        protected bool SetProperty<T>(Func<T> getter, Action<T> setter, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(getter(), value))
            {
                setter(value);
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class UserInfoViewModel : ProtoViewModelBase<UserInfo>
    {
        public UserInfoViewModel(UserInfo proto) : base(proto) { }

        public string Nickname
        {
            get => _proto.Nickname;
            set => SetProperty(() => _proto.Nickname, v => _proto.Nickname = v, value);
        }

        public int UserId
        {
            get => _proto.UserId;
            set => SetProperty(() => _proto.UserId, v => _proto.UserId = v, value);
        }
    }


    public class RoomInfoViewModel : ProtoViewModelBase<RoomInfo>
    {
        public RoomInfoViewModel(RoomInfo proto) : base(proto) { }

        public string RoomName
        {
            get => _proto.RoomName;
            set => SetProperty(() => _proto.RoomName, v => _proto.RoomName = v, value);
        }

        public int RoomId
        {
            get => _proto.RoomId;
            set => SetProperty(() => _proto.RoomId, v => _proto.RoomId = v, value);
        }

        public int RoomMasterUserId
        {
            get => _proto.RoomMasterUserId;
            set => SetProperty(() => _proto.RoomMasterUserId, v => _proto.RoomMasterUserId = v, value);
        }

        public RepeatedField<UserInfo> UserInfos
        {
            get => _proto.UserInfos;
            set => SetProperty(() => _proto.UserInfos, v => {
                _proto.UserInfos.Clear();
                foreach (var item in v)
                {
                    _proto.UserInfos.Add(item);
                }
            }, value);
        }
    }
}
