using System.Diagnostics;

namespace QuantumCore.Core.Networking;

internal class SubHeaderField : FieldCache
{
    public byte SubHeader { get; set; }
        
    public SubHeaderField(byte subHeader, int position)
    {
        SubHeader = subHeader;
        Position = position;
        FieldType = typeof(byte);
    }

    public override object GetValue(object obj)
    {
        return SubHeader;
    }

    public override void SetValue(object obj, object value)
    {
        Debug.Assert(false);
    }
}