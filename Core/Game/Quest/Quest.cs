using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Packets.Quest;
using QuantumCore.Game.World.Entities;
using Serilog;

namespace QuantumCore.Game.Quest;

public abstract class Quest
{
    public QuestState State { get; }
    public IPlayerEntity Player { get; }

    private readonly PlayerEntity _player;
    private string _questScript = "";
    private QuestSkin _currentSkin = QuestSkin.Normal;

    private TaskCompletionSource _currentNextTask;
    private TaskCompletionSource<byte> _currentChoiceTask;

    public Quest(QuestState state, IPlayerEntity player)
    {
        State = state;
        Player = player;
        _player = (PlayerEntity) player;
    }

    public abstract void Init();

    protected void SendScript()
    {
        _player.Connection.Send(new QuestScript {
            Skin = (byte) _currentSkin,
            Source = _questScript,
            SourceSize = (ushort)(_questScript.Length + 1)
        });

        _currentSkin = QuestSkin.Normal;
        _questScript = "";
    }

    protected void SetSkin(QuestSkin skin)
    {
        _currentSkin = skin;
    }

    protected void SendQuestLetter(string name, Action callback)
    {
        var buffer = new byte[31];
        var encoded = Encoding.ASCII.GetBytes(name);
        Array.Copy(encoded, buffer, encoded.Length > 30 ? 30 : encoded.Length);
        
        Log.Debug($"QuestInfo Data - {string.Join(" ", buffer.Select(n => n.ToString()))}");
        
        var info = new QuestInfo {Index = 0, Flags = (byte) (QuestInfo.InfoFlags.Title), Data = buffer};
        Player.Connection.Send(info);
    }

    protected void ExitQuest()
    {
        // todo
    }
    
    public void Answer(byte answer)
    {
        if (answer == 254)
        {
            _currentNextTask.SetResult();
            return;
        }
        
        _currentChoiceTask.SetResult(answer);
    }

    protected void Text(string str)
    {
        _questScript += str + "[ENTER]";
    }

    protected Task Next()
    {
        _currentNextTask?.TrySetCanceled();
        _currentNextTask = new TaskCompletionSource();
        
        _questScript += "[NEXT]";
        SendScript();
        
        _player.CurrentQuest = this;
        return _currentNextTask.Task;
    }

    protected Task<byte> Choice(params string[] options)
    {
        Debug.Assert(options.Length > 0);
        
        _currentChoiceTask?.TrySetCanceled();
        _currentChoiceTask = new TaskCompletionSource<byte>();

        _questScript += "[QUESTION ";
        
        for (var i = 0; i < options.Length; i++)
        {
            if (i != 0)
            {
                _questScript += "|";
            }
            
            Debug.Assert(!options[i].Contains(';'));
            Debug.Assert(!options[i].Contains('|'));

            _questScript += $"{i + 1};" + options[i];
        }

        _questScript += "]";
        
        SendScript();

        _player.CurrentQuest = this;
        return _currentChoiceTask.Task;
    }

    protected void Done(bool silent = false)
    {
        if (!silent)
        {
            _questScript += "[ENTER]";
        }
        _questScript += "[DONE]";
        
        SendScript();
    }
    
}