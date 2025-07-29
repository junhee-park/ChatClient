using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        public static ServerSession serverSession;
        public ObservableCollection<UserInfoViewModel> LobbyUsers { get; set; } = new ObservableCollection<UserInfoViewModel>();
        public ObservableCollection<RoomInfoViewModel> Rooms { get; set; } = new ObservableCollection<RoomInfoViewModel>();
        public ObservableCollection<UserInfoViewModel> ChatUsers { get; set; } = new ObservableCollection<UserInfoViewModel>();
        public StringBuilder ChatLogs { get; set; } = new StringBuilder();

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

            this.Closing += MainWindow_Closing;
        }

        private void EnterLobby()
        {
            C_EnterLobby c_EnterLobby = new C_EnterLobby();
            serverSession.Send(c_EnterLobby);
        }

        public void ShowLobbyScreen()
        {
            isRoomOwner = false;
            ChatLogs.Clear();
            ChatLog.Text = ChatLogs.ToString();
            DeleteRoomButton.Visibility = Visibility.Collapsed;
            LobbyScreen.Visibility = Visibility.Visible;
            ChatRoomScreen.Visibility = Visibility.Collapsed;
            ChangeNicknameButton.Visibility = Visibility.Visible;
        }

        public void ShowChatRoomScreen()
        {
            ChatLogs.Clear();
            ChatLog.Text = ChatLogs.ToString();
            DeleteRoomButton.Visibility = isRoomOwner ? Visibility.Visible : Visibility.Collapsed;
            LobbyScreen.Visibility = Visibility.Collapsed;
            ChatRoomScreen.Visibility = Visibility.Visible;
            ChangeNicknameButton.Visibility = Visibility.Collapsed;
            LeaveRoomButton.Visibility = !isRoomOwner ? Visibility.Visible : Visibility.Collapsed;
            ChatInput.Focus(); // 채팅 입력 박스에 포커스 설정
        }

        private void ChatInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // 버튼 Click 호출
                SendButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
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
                isRoomOwner = true;
            }
        }

        private void JoinRoomButton_Click(object sender, RoutedEventArgs e)
        {
            isRoomOwner = false;

            if (sender is Button btn && btn.Tag is RoomInfoViewModel roomInfoViewModel)
            {
                C_EnterRoom c_EnterRoom = new C_EnterRoom
                {
                    RoomId = roomInfoViewModel.RoomId
                };
                serverSession.Send(c_EnterRoom);
            }
        }

        private void LeaveRoomButton_Click(object sender, RoutedEventArgs e)
        {
            isRoomOwner = false;

            C_LeaveRoom c_LeaveRoom = new C_LeaveRoom();
            serverSession.Send(c_LeaveRoom);
        }

        private void DeleteRoomButton_Click(object sender, RoutedEventArgs e)
        {
            if (isRoomOwner)
            {
                C_DeleteRoom c_DeleteRoom = new C_DeleteRoom();
                serverSession.Send(c_DeleteRoom);
            }
            else
            {
                MessageBox.Show("방장만 방을 삭제할 수 있습니다.");
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
                if (ChatInput.Text.Contains("/"))
                {
                    // 명령어 처리 로직
                    string command = ChatInput.Text.Substring(1).Trim();
                    if (command.StartsWith("info"))
                    {
                        // /info 명령어 처리
                        if (serverSession.CurrentState == UserState.Room)
                        {
                            StringBuilder infoBuilder = new StringBuilder();
                            infoBuilder.AppendLine("현재 방 정보:");
                            infoBuilder.AppendLine($"방 ID: {serverSession.RoomManager.CurrentRoom.RoomId}");
                            infoBuilder.AppendLine($"방 이름: {serverSession.RoomManager.CurrentRoom.RoomName}");
                            infoBuilder.AppendLine($"방장 ID: {serverSession.RoomManager.CurrentRoom.RoomMasterUserId}");
                            infoBuilder.AppendLine("참여 중인 유저:");
                            foreach (var user in serverSession.RoomManager.CurrentRoom.UserInfos.Values)
                            {
                                infoBuilder.AppendLine($"- {user.Nickname} (ID: {user.UserId})");
                            }
                            MessageBox.Show(infoBuilder.ToString(), "방 정보");
                        }
                        else
                        {
                            MessageBox.Show("채팅방에 참여 중이 아닙니다.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("알 수 없는 명령어입니다.");
                    }
                }
                else
                {
                    C_Chat c_Chat = new C_Chat
                    {
                        Msg = ChatInput.Text
                    };
                    serverSession.Send(c_Chat);
                }
                // 입력 필드 초기화
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

    public class UserInfoViewModel : INotifyPropertyChanged
    {
        private string _nickname;
        private int _userId;
        public string Nickname { get => _nickname; set { _nickname = value; OnPropertyChanged(nameof(Nickname)); } }

        public int UserId { get => _userId; set { _userId = value; OnPropertyChanged(nameof(UserId)); } }
        public UserInfoViewModel(UserInfo userinfo)
        {
            Nickname = userinfo.Nickname;
            UserId = userinfo.UserId;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string prop)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class RoomInfoViewModel : INotifyPropertyChanged
    {
        private int _roomId;
        private string _roomName;
        private int _roomMasterUserId;
        private Dictionary<int, UserInfoViewModel> _userInfos;

        public int RoomId { get => _roomId; set { _roomId = value; OnPropertyChanged(nameof(RoomId)); } }

        public string RoomName { get => _roomName; set { _roomName = value; OnPropertyChanged(nameof(RoomName)); } }

        public int RoomMasterUserId { get => _roomMasterUserId; set { _roomMasterUserId = value; OnPropertyChanged(nameof(RoomMasterUserId)); } }

        public Dictionary<int, UserInfoViewModel> UserInfos { get; set; }

        public RoomInfoViewModel(RoomInfo roomInfo)
        {
            RoomId = roomInfo.RoomId;
            RoomName = roomInfo.RoomName;
            RoomMasterUserId = roomInfo.RoomMasterUserId;
            UserInfos = new Dictionary<int, UserInfoViewModel>();
            foreach (var userInfo in roomInfo.UserInfos)
            {
                UserInfos[userInfo.Key] = new UserInfoViewModel(userInfo.Value);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string prop)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
