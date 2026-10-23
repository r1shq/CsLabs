namespace ServerConfigApp;

public sealed record ServerConfig(
    int MaxPlayers,
    int RamGb,
    int Port,
    bool OnlineMode,
    bool PvpEnabled,
    bool PvpAllowed,
    bool WhitelistEnabled,
    int WhitelistPlayers,
    bool IsPublic,
    bool DdosProtection
);
