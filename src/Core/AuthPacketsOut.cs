namespace QuantumCore.API.Packets;

public enum AuthPacketsOut
{
    Handshake = 0xff,
    LoginFailed = 0x07,
    LoginSuccess = 0x96,
    Phase = 0xfd,
    KeyAgreement = 0xfb,
    KeyAgreementCompleted = 0xfa,
}