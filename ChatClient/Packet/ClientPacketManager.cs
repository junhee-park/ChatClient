
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;

public class PacketManager
{
    #region Singleton
    static PacketManager _instance = new PacketManager();
    public static PacketManager Instance { get { return _instance; } }
    #endregion

    Dictionary<ushort, Action<Session, IMessage>> _handlers = new Dictionary<ushort, Action<Session, IMessage>> ();
    Dictionary<ushort, Func<ArraySegment<byte>, IMessage>> _makePacket = new Dictionary<ushort, Func<ArraySegment<byte>, IMessage>>();
        
    public PacketManager()
    {
        _handlers.Add((ushort)MsgId.SChat, PacketHandler.S_ChatHandler);
        _makePacket.Add((ushort)MsgId.SChat, MakePacket<S_Chat>);
        _handlers.Add((ushort)MsgId.SPing, PacketHandler.S_PingHandler);
        _makePacket.Add((ushort)MsgId.SPing, MakePacket<S_Ping>);
        _handlers.Add((ushort)MsgId.SSetNickname, PacketHandler.S_SetNicknameHandler);
        _makePacket.Add((ushort)MsgId.SSetNickname, MakePacket<S_SetNickname>);
        _handlers.Add((ushort)MsgId.SCreateRoom, PacketHandler.S_CreateRoomHandler);
        _makePacket.Add((ushort)MsgId.SCreateRoom, MakePacket<S_CreateRoom>);
        _handlers.Add((ushort)MsgId.SDeleteRoom, PacketHandler.S_DeleteRoomHandler);
        _makePacket.Add((ushort)MsgId.SDeleteRoom, MakePacket<S_DeleteRoom>);
        _handlers.Add((ushort)MsgId.SRoomList, PacketHandler.S_RoomListHandler);
        _makePacket.Add((ushort)MsgId.SRoomList, MakePacket<S_RoomList>);
        _handlers.Add((ushort)MsgId.SEnterRoom, PacketHandler.S_EnterRoomHandler);
        _makePacket.Add((ushort)MsgId.SEnterRoom, MakePacket<S_EnterRoom>);
        _handlers.Add((ushort)MsgId.SUserList, PacketHandler.S_UserListHandler);
        _makePacket.Add((ushort)MsgId.SUserList, MakePacket<S_UserList>);
        _handlers.Add((ushort)MsgId.SLeaveRoom, PacketHandler.S_LeaveRoomHandler);
        _makePacket.Add((ushort)MsgId.SLeaveRoom, MakePacket<S_LeaveRoom>);
        _handlers.Add((ushort)MsgId.SEnterLobby, PacketHandler.S_EnterLobbyHandler);
        _makePacket.Add((ushort)MsgId.SEnterLobby, MakePacket<S_EnterLobby>);
        _handlers.Add((ushort)MsgId.SEnterRoomAnyUser, PacketHandler.S_EnterRoomAnyUserHandler);
        _makePacket.Add((ushort)MsgId.SEnterRoomAnyUser, MakePacket<S_EnterRoomAnyUser>);
        _handlers.Add((ushort)MsgId.SEnterLobbyAnyUser, PacketHandler.S_EnterLobbyAnyUserHandler);
        _makePacket.Add((ushort)MsgId.SEnterLobbyAnyUser, MakePacket<S_EnterLobbyAnyUser>);
        _handlers.Add((ushort)MsgId.SLeaveRoomAnyUser, PacketHandler.S_LeaveRoomAnyUserHandler);
        _makePacket.Add((ushort)MsgId.SLeaveRoomAnyUser, MakePacket<S_LeaveRoomAnyUser>);
        _handlers.Add((ushort)MsgId.SDeleteAnyRoomInLobby, PacketHandler.S_DeleteAnyRoomInLobbyHandler);
        _makePacket.Add((ushort)MsgId.SDeleteAnyRoomInLobby, MakePacket<S_DeleteAnyRoomInLobby>);
        _handlers.Add((ushort)MsgId.SLeaveLobbyAnyUser, PacketHandler.S_LeaveLobbyAnyUserHandler);
        _makePacket.Add((ushort)MsgId.SLeaveLobbyAnyUser, MakePacket<S_LeaveLobbyAnyUser>);
        _handlers.Add((ushort)MsgId.SUserInfo, PacketHandler.S_UserInfoHandler);
        _makePacket.Add((ushort)MsgId.SUserInfo, MakePacket<S_UserInfo>);

    }

    public T MakePacket<T>(ArraySegment<byte> buffer) where T : IMessage, new()
    {
        T packet = new T();
        packet.MergeFrom(buffer);

        return packet;
    }

    public void InvokePacketHandler(Session session, ArraySegment<byte> buffer)
    {
        ushort size = BitConverter.ToUInt16(buffer.Array, 0);
        ushort packetId = BitConverter.ToUInt16(buffer.Array, 2);

        ArraySegment<byte> data = new ArraySegment<byte>(buffer.Array, 4, size - 4);
        bool result = _makePacket.TryGetValue(packetId, out var makePacketFunc);
        if (!result)
        {
            return;
        }
        IMessage packet = makePacketFunc.Invoke(data);

        result = _handlers.TryGetValue(packetId, out var handler);
        if (!result)
        {
            return;
        }
        handler?.Invoke(session, packet);
    }
}
