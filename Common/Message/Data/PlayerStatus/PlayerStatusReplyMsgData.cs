﻿using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.PlayerStatus
{
    public class PlayerStatusReplyMsgData: PlayerStatusBaseMsgData
    {
        /// <inheritdoc />
        internal PlayerStatusReplyMsgData() { }
        public override PlayerStatusMessageType PlayerStatusMessageType => PlayerStatusMessageType.Reply;

        public int PlayerStatusCount;
        public PlayerStatusInfo[] PlayerStatus = new PlayerStatusInfo[0];

        public override string ClassName { get; } = nameof(PlayerStatusReplyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(PlayerStatusCount);
            for (var i = 0; i < PlayerStatusCount; i++)
            {
                PlayerStatus[i].Serialize(lidgrenMsg, dataCompressed);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            PlayerStatusCount = lidgrenMsg.ReadInt32();
            PlayerStatus = ArrayPool<PlayerStatusInfo>.Claim(PlayerStatusCount);
            for (var i = 0; i < PlayerStatusCount; i++)
            {
                if (PlayerStatus[i] == null)
                    PlayerStatus[i] = new PlayerStatusInfo();

                PlayerStatus[i].Deserialize(lidgrenMsg, dataCompressed);
            }
        }

        public override void Recycle()
        {
            base.Recycle();

            ArrayPool<PlayerStatusInfo>.Release(ref PlayerStatus);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            var arraySize = 0;
            for (var i = 0; i < PlayerStatusCount; i++)
            {
                arraySize += PlayerStatus[i].GetByteCount(dataCompressed);
            }

            return base.InternalGetMessageSize(dataCompressed) + sizeof(int) + arraySize;
        }
    }
}
