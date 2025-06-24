using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ChatClient;
using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;

public interface IViewManager
{
    void ShowText(string text);
    void ShowText(S_Chat s_Chat);
    void ShowRoomList(RepeatedField<RoomInfo> roomInfos);
    void ShowRoomUserList(RepeatedField<UserInfo> userInfos);
    void ShowLobbyUserList(RepeatedField<UserInfo> userInfos);
    void ShowLobbyUserList(Dictionary<int, UserInfo> userInfos);
    void ShowChangedNickname(string oldName, string newName);
    void ShowChangedNickname(UserInfo userInfo, string newName);
}

public class ViewManager : IViewManager
{
    public MainWindow MainWindow { get; set; } // 메인 윈도우 참조
    public ViewManager()
    {
        MainWindow = App.Current.MainWindow as MainWindow;
        if (MainWindow == null)
            throw new InvalidOperationException("MainWindow is not set.");
    }

    public ViewManager(MainWindow mainWindow)
    {
        MainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
    }
    public void ShowText(string text)
    {
        
    }

    public void ShowRoomList(RepeatedField<RoomInfo> roomInfos)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MainWindow.Rooms.Clear();
            foreach (var roomInfo in roomInfos)
            {
                MainWindow.Rooms.Add(roomInfo.RoomName);
            }
        });
    }

    public void ShowText(S_Chat s_Chat)
    {
        throw new NotImplementedException();
    }


    public void ShowRoomUserList(RepeatedField<UserInfo> userInfos)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // 현재 방에 있는 유저 목록 갱신
            MainWindow.LobbyUsers.Clear();
            foreach (var user in userInfos)
            {
                MainWindow.LobbyUsers.Add(new UserInfoViewModel(user));
            }
        });
    }

    public void ShowLobbyUserList(RepeatedField<UserInfo> userInfos)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // 로비에 있는 유저 목록 갱신
            MainWindow.LobbyUsers.Clear();
            foreach (var user in userInfos)
            {
                MainWindow.LobbyUsers.Add(new UserInfoViewModel(user));
            }
        });
    }

    public void ShowLobbyUserList(Dictionary<int, UserInfo> userInfos)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // 로비에 있는 유저 목록 갱신
            MainWindow.LobbyUsers.Clear();
            foreach (var user in userInfos)
            {
                MainWindow.LobbyUsers.Add(new UserInfoViewModel(user.Value));
            }
        });
    }

    public void ShowChangedNickname(string oldName, string newName)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            //MainWindow.LobbyUsers.Remove(oldName);
            //MainWindow.LobbyUsers.Add(newName);
        });
    }

    public void ShowChangedNickname(UserInfo userInfo, string newName)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            foreach (var user in MainWindow.LobbyUsers)
            {
                if (user.Proto.UserId == userInfo.UserId)
                {
                    user.Nickname = newName;
                    break;
                }
            }
        });
    }
}