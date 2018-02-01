﻿using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Groups
{
    public class GroupCreateMsgData : GroupBaseMsgData
    {
        /// <inheritdoc />
        internal GroupCreateMsgData() { }
        public override GroupMessageType GroupMessageType => GroupMessageType.CreateGroup;

        public string GroupName;

        public override string ClassName { get; } = nameof(GroupCreateMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(GroupName);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            GroupName = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + GroupName.GetByteCount();
        }
    }
}
