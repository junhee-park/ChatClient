using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ChatClient;
using Google.Protobuf.Protocol;
using ServerCore;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        static ServerSession serverSession;
        public ObservableCollection<string> LobbyUsers { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Rooms { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ChatUsers { get; set; } = new ObservableCollection<string>();
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

            C_EnterLobby c_EnterLobby = new C_EnterLobby();
            serverSession.Send(c_EnterLobby);
        }

        private void EnterLobby()
        {
            C_EnterLobby c_EnterLobby = new C_EnterLobby();
            serverSession.Send(c_EnterLobby);
        }

        private void ShowLobbyScreen()
        {
            LobbyScreen.Visibility = Visibility.Visible;
            ChatRoomScreen.Visibility = Visibility.Collapsed;
        }

        private void ShowChatRoomScreen()
        {
            LobbyScreen.Visibility = Visibility.Collapsed;
            ChatRoomScreen.Visibility = Visibility.Visible;
        }

        private void CreateRoomButton_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputDialog("방 생성", "방 이름을 입력하세요:");
            if (input.ShowDialog() == true)
            {
                Rooms.Add(input.Content.ToString());
            }
        }

        private void JoinRoomButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string roomName)
            {
                currentRoom = roomName;
                isRoomOwner = false;
                DeleteRoomButton.Visibility = isRoomOwner ? Visibility.Visible : Visibility.Collapsed;
                ChatUsers.Clear();
                ChatUsers.Add("User1");
                ChatUsers.Add("User2");
                ChatLogs.Clear();
                ChatLogs.Add($"[{currentRoom}] 채팅방에 입장했습니다.");
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
                Rooms.Remove(currentRoom);
                currentRoom = "";
                ShowLobbyScreen();
            }
        }


        private void ChangeNicknameButton_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputDialog("닉네임 변경", "새 닉네임을 입력하세요:");
            if (input.ShowDialog() == true)
            {
                MessageBox.Show($"닉네임이 '{input.Content}'로 변경되었습니다.");
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
    }
}
