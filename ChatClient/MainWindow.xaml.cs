using System.Windows;
using System.Windows.Controls;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 예시: 방 목록 채우기
            RoomList.Items.Add("방1");
            RoomList.Items.Add("방2");
        }

        // 방 입장
        private void RoomList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RoomList.SelectedItem != null)
            {
                string selectedRoom = RoomList.SelectedItem.ToString();

                // 선택된 방 입장 처리 로직 추가 가능

                // 화면 전환
                LobbyView.Visibility = Visibility.Collapsed;
                ChatRoomView.Visibility = Visibility.Visible;

                ChatLog.AppendText($"'{selectedRoom}' 채팅방에 입장했습니다.\n");
            }
        }

        // 나가기 버튼 클릭 시
        private void ExitRoom_Click(object sender, RoutedEventArgs e)
        {
            // 서버에 방 나가기 통신 등을 여기에 추가 가능

            // 화면 전환
            ChatRoomView.Visibility = Visibility.Collapsed;
            LobbyView.Visibility = Visibility.Visible;

            RoomList.SelectedItem = null; // 선택 초기화
        }

        // 전송 버튼 클릭
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = ChatInput.Text.Trim();

            if (!string.IsNullOrEmpty(message))
            {
                ChatLog.AppendText($"[나] {message}\n");
                ChatInput.Clear();

                // 서버에 메시지 전송 추가 가능
            }
        }
    }
}
