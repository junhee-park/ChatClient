using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ChatClient;
using Google.Protobuf.Protocol;
using ServerCore;

//namespace ChatClient
//{
//    public partial class MainWindow : Window
//    {
//        static ServerSession serverSession;

//        public MainWindow()
//        {
//            InitializeComponent();


//            return;
//            IPAddress[] iPAddress = Dns.GetHostAddresses(Dns.GetHostName());
//            IPEndPoint iPEndPoint = new IPEndPoint(iPAddress[1], 7777);

//            Connector connector = new Connector();
//            connector.Connect(iPEndPoint,
//                (saea) =>
//                {
//                    serverSession = new ServerSession(saea.ConnectSocket);
//                    serverSession.InitViewManager(new ViewManager());
//                    serverSession.OnConnect(saea.RemoteEndPoint);
//                    return serverSession;
//                });

//            // 방 목록 조회
//            RefreshRoomList();
//            RefreshUserList();
//        }

//        public void ShowText(string text)
//        {
//            ChatLog.AppendText(text + "\n");
//        }

//        public void RefreshRoomList()
//        {
//            RoomList.Items.Clear();
//            // 서버에서 방 목록을 받아와서 RoomList에 추가
//            C_RoomList c_RoomList = new C_RoomList();

//            serverSession.Send(c_RoomList);
//        }

//        public void RefreshUserList()
//        {
//            if (RoomManager.Instance.CurrentRoom == null)
//            {

//            }
//            else
//            {

//            }
                
//        }

//        private void OpenInputDialog_Click(object sender, RoutedEventArgs e)
//        {
//            var inputDialog = new InputDialog("닉네임 입력", "홍길동");
//            inputDialog.Owner = this;

//            if (inputDialog.ShowDialog() == true)
//            {
//                string nickname = inputDialog.InputText;
//                MessageBox.Show($"입력한 닉네임: {nickname}");
//            }
//            else
//            {
//                MessageBox.Show("입력이 취소되었습니다.");
//            }
//        }

//        // 방 입장
//        private void RoomList_SelectionChanged(object sender, SelectionChangedEventArgs e)
//        {
//            if (RoomList.SelectedItem != null)
//            {
//                string selectedRoom = RoomList.SelectedItem.ToString();

//                // 선택된 방 입장 처리 로직 추가 가능

//                // 화면 전환
//                LobbyView.Visibility = Visibility.Collapsed;
//                ChatRoomView.Visibility = Visibility.Visible;

//                ChatLog.AppendText($"'{selectedRoom}' 채팅방에 입장했습니다.\n");
//            }
//        }

//        // 나가기 버튼 클릭 시
//        private void ExitRoom_Click(object sender, RoutedEventArgs e)
//        {
//            // 서버에 방 나가기 통신 등을 여기에 추가 가능

//            // 화면 전환
//            ChatRoomView.Visibility = Visibility.Collapsed;
//            LobbyView.Visibility = Visibility.Visible;

//            RoomList.SelectedItem = null; // 선택 초기화
//        }

//        // 전송 버튼 클릭
//        private void SendButton_Click(object sender, RoutedEventArgs e)
//        {
//            string message = ChatInput.Text.Trim();

//            if (!string.IsNullOrEmpty(message))
//            {
//                ChatLog.AppendText($"[나] {message}\n");
//                ChatInput.Clear();

//                // 서버에 메시지 전송 추가 가능
//            }
//        }
//    }
//}

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
                    return serverSession;
                });

            LobbyUserList.ItemsSource = LobbyUsers;
            RoomListBox.ItemsSource = Rooms;
            ChatUserList.ItemsSource = ChatUsers;

            // Sample data
            LobbyUsers.Add("User1");
            LobbyUsers.Add("User2");
            Rooms.Add("Room A");
            Rooms.Add("Room B");

            RefreshRoomList();
            RefreshUserList();
        }

        private void RefreshRoomList()
        {
            Rooms.Clear();
            // 서버에서 방 목록을 받아와서 Rooms에 추가
            C_RoomList c_RoomList = new C_RoomList();
            serverSession.Send(c_RoomList);
        }

        private void RefreshUserList()
        {
            ChatUsers.Clear();
            C_UserList c_UserList = new C_UserList();
            serverSession.Send(c_UserList);
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

        private void NicknameButton_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputDialog("닉네임 변경", "새 닉네임을 입력하세요:");
            if (input.ShowDialog() == true)
            {
                MessageBox.Show($"닉네임이 '{input.Content}'로 변경되었습니다.");
            }
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

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ChatInput.Text))
            {
                ChatLogs.Add(ChatInput.Text);
                ChatInput.Clear();
            }
        }

        private void ChangeNicknameButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SendChatButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
