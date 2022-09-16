using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using QuantumCore.Database;
using QuantumCore.Game.Packets.Quest;

namespace QuantumCore.Game.Quest;

public class QuestLetter
{
    private ushort _id;
    private Quest _quest;
    
    public string Title { get; set; }
    public string CounterName { get; set; }
    public int? CounterValue { get; set; }
    public Action Callback { get; set; }
    
    public QuestLetter(ushort id, Quest quest, Action callback)
    {
        _id = id;
        _quest = quest;
        Callback = callback;
    }

    public void Invoke()
    {
        Callback();
    }

    public void Send()
    {
        var flags = QuestInfo.InfoFlags.Title;
        
        var buffer = new List<byte>();
        var encoded = Encoding.ASCII.GetBytes(Title.Length > 30 ? Title.Substring(0, 30) : Title);
        CopyTo(buffer, encoded, 31);

        if (!string.IsNullOrWhiteSpace(CounterName))
        {
            flags |= QuestInfo.InfoFlags.CounterName;
            var encodedCounterName = Encoding.ASCII.GetBytes(CounterName.Length > 16 ? CounterName.Substring(0, 16) : CounterName);
            CopyTo(buffer, encodedCounterName, 17);
        }

        if (CounterValue.HasValue)
        {
            flags |= QuestInfo.InfoFlags.CounterValue;
            // TODO: This is kinda janky for only to serialize an int
            var stream = new MemoryStream();
            var bw = new BinaryWriter(stream);
            bw.Write(CounterValue.Value);
            bw.Close();
            stream.Close();

            var data = stream.ToArray();
            CopyTo(buffer, data, 4);
        }
        
        var info = new QuestInfo {Index = _id, Flags = (byte) flags, Data = buffer.ToArray()};
        _quest.Player.Connection.Send(info);
    }

    public void End()
    {
        // todo
    }

    private void CopyTo(List<byte> buffer, byte[] data, ushort length)
    {
        for (var i = 0; i < length; i++)
        {
            buffer.Add(data.Length > i ? data[i] : (byte) 0x00);
        }
    }
}