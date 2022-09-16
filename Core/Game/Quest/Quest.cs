using System;
using System.Collections.Generic;
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

    private readonly Dictionary<ushort, QuestLetter> _questLetters = new();

    public Quest(QuestState state, IPlayerEntity player)
    {
        State = state;
        Player = player;
        _player = (PlayerEntity) player;
    }

    public abstract void Init();

    public QuestLetter GetQuestLetter(ushort id)
    {
        if (!_questLetters.ContainsKey(id))
        {
            return null;
        }

        return _questLetters[id];
    }

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

    protected QuestLetter CreateQuestLetter(string name, Action callback)
    {
        var id = _player.GetNextQuestLetterId();
        var letter = new QuestLetter(id, this, callback) {Title = name};
        _questLetters[id] = letter;

        return letter;
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