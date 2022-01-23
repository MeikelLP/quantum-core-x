using System.Diagnostics;
using System.Threading.Tasks;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Packets.Quest;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Quest;

public abstract class Quest
{
    public QuestState State { get; }
    public IPlayerEntity Player { get; }

    private readonly PlayerEntity _player;
    private string _questScript = "";

    private TaskCompletionSource _currentNextTask;
    private TaskCompletionSource<byte> _currentChoiceTask;

    public Quest(QuestState state, IPlayerEntity player)
    {
        State = state;
        Player = player;
        _player = (PlayerEntity) player;
    }

    public abstract void Init();

    private void SendScript()
    {
        _player.Connection.Send(new QuestScript {
            Skin = 1,
            Source = _questScript,
            SourceSize = (ushort)(_questScript.Length + 1)
        });

        _questScript = "";
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

    protected void Done()
    {
        _questScript += "[ENTER][DONE]";
        
        SendScript();
    }
    
}