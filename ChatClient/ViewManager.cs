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
        Application.Current.Dispatcher.Invoke(() =>
        {
            MainWindow.ChatLogs.AppendLine(text);
            MainWindow.ChatLog.Text = MainWindow.ChatLogs.ToString();
        });
    }

    public void ShowRoomList(MapField<int, RoomInfo> roomInfos)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MainWindow.Rooms.Clear();
            foreach (var roomInfo in roomInfos)
            {
                MainWindow.Rooms.Add(new RoomInfoViewModel(roomInfo.Value));
            }
        });
    }

    public void ShowText(S_Chat s_Chat)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (RoomManager.Instance.CurrentRoom.UserInfos.TryGetValue(s_Chat.UserId, out UserInfo userInfo))
            {
                // 현재 방에 있는 유저의 메시지
                MainWindow.ChatLogs.AppendLine($"{userInfo.Nickname} ({s_Chat.UserId}) [{s_Chat.Timestamp}]: {s_Chat.Msg}");
            }
            else
            {
                // 현재 방에 없는 유저
                MainWindow.ChatLogs.AppendLine($"({s_Chat.UserId}) [{s_Chat.Timestamp}]: {s_Chat.Msg}");
            }
            MainWindow.ChatLog.Text = MainWindow.ChatLogs.ToString();
        });
    }


    public void ShowRoomUserList(MapField<int, UserInfo> userInfos)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // 현재 방에 있는 유저 목록 갱신
            MainWindow.ChatUsers.Clear();
            foreach (var user in userInfos.Values)
            {
                MainWindow.ChatUsers.Add(new UserInfoViewModel(user));
            }
        });
    }

    public void ShowLobbyUserList(MapField<int, UserInfo> userInfos)
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
                            room.UserInfos.Add(userInfo.UserId, userInfo);
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
        else if (RoomManager.Instance.CurrentRoom?.RoomId == roomId)
        {
            // 방에서 유저 제거
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.ChatUsers.Remove(MainWindow.ChatUsers.FirstOrDefault(u => u.Proto.UserId == userInfo.UserId));
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
                        room.UserInfos.Remove(userInfo.UserId);
                        break;
                    }
                }
            });
        }
    }

    public void ShowRemovedRoom(int roomId)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            foreach (var room in MainWindow.Rooms)
            {
                if (room.RoomId == roomId)
                {
                    MainWindow.Rooms.Remove(room);
                    break;
                }
            }
        });
    }
}