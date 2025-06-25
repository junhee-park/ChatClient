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
                MainWindow.Rooms.Add(new RoomInfoViewModel(roomInfo));
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
            MainWindow.ChatUsers.Clear();
            foreach (var user in userInfos)
            {
                MainWindow.ChatUsers.Add(new UserInfoViewModel(user));
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

    public void ShowLobbyScreen()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MainWindow.ShowLobbyScreen();
        });
    }

    public void ShowRoomScreen()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MainWindow.ShowChatRoomScreen();
        });
    }

    public void ShowAddedRoom(RoomInfo roomInfo)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MainWindow.Rooms.Add(new RoomInfoViewModel(roomInfo));
        });
    }

    public void ShowAddedUser(int roomId, UserInfo userInfo)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (roomId == 0)
            {
                // 로비에 유저 추가
                MainWindow.LobbyUsers.Add(new UserInfoViewModel(userInfo));
                return;
            }
            else
            {
                if (RoomManager.Instance.CurrentRoom == null)
                {
                    // 로비에 있는 상태에서 방에 유저 추가
                    foreach (var room in MainWindow.Rooms)
                    {
                        if (room.RoomId == roomId)
                        {
                            room.UserInfos.Add(userInfo);
                            return;
                        }
                    }
                }
                else
                {
                    // 방에 있는 상태에서 방에 유저 추가
                    MainWindow.ChatUsers.Add(new UserInfoViewModel(userInfo));
                }
            }
        });
    }

    public void ShowRemovedUser(int roomId, UserInfo userInfo)
    {
        if (roomId == 0)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.LobbyUsers.Remove(MainWindow.LobbyUsers.FirstOrDefault(u => u.Proto.UserId == userInfo.UserId));
            });
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var room in MainWindow.Rooms)
                {
                    if (room.RoomId == roomId)
                    {
                        room.UserInfos.Remove(userInfo);
                        break;
                    }
                }
            });
        }
    }
}