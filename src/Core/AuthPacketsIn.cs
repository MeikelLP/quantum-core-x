namespace QuantumCore.API.Packets;

public enum AuthPacketsIn
{
    Handshake = 0xff,
    LoginRequest = 0x6f,
    KeyAgreement = 0xfb,
    KeyAgreementCompleted = 0xfa,
}