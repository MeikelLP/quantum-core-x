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

    protected async Task SendScript()
    {
        await _player.Connection.Send(new QuestScript {
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

    protected async Task Next()
    {
        _currentNextTask?.TrySetCanceled();
        _currentNextTask = new TaskCompletionSource();
        
        _questScript += "[NEXT]";
        await SendScript();
        
        _player.CurrentQuest = this;
    }

    protected async Task<byte> Choice(bool done = false, params string[] options)
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

        if (done)
        {
            _questScript += "[DONE]";
        }
        
        await SendScript();

        _player.CurrentQuest = this;
        return await _currentChoiceTask.Task;
    }

    protected async Task Done(bool silent = false)
    {
        if (!silent)
        {
            _questScript += "[ENTER]";
        }
        _questScript += "[DONE]";
        
        await SendScript();
    }
}