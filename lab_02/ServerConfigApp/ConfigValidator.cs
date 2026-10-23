namespace ServerConfigApp;

public enum Severity { Warning, Error }

public sealed record ConfigIssue(Severity Severity, string Message);

public enum StartStatus { Ready, Warning, Blocked }

public static class ConfigValidator
{
    public static IReadOnlyList<ConfigIssue> Check(ServerConfig config)
    {
        var issues = new List<ConfigIssue>();

        if (config.MaxPlayers < 1 || config.MaxPlayers > 100)
            issues.Add(new(Severity.Error, "Количество игроков должно быть от 1 до 100."));

        if (config.RamGb < 2)
            issues.Add(new(Severity.Error, "Для запуска требуется минимум 2 ГБ оперативной памяти."));
        else if (config.MaxPlayers > 50 && config.RamGb < 4)
            issues.Add(new(Severity.Error, "Для сервера более чем на 50 игроков необходимо минимум 4 ГБ ОЗУ."));

        if (config.Port < 1 || config.Port > 65535)
            issues.Add(new(Severity.Error, "Порт сервера должен быть в диапазоне от 1 до 65535."));

        if (config.PvpEnabled && !config.PvpAllowed)
            issues.Add(new(Severity.Error, "PvP включён, но запрещён правилами сервера. Измените одну из настроек."));

        if (config.WhitelistPlayers < 0)
            issues.Add(new(Severity.Error, "Число игроков в белом списке не может быть отрицательным."));
        else if (config.WhitelistEnabled && config.WhitelistPlayers == 0)
            issues.Add(new(Severity.Warning, "Белый список включён, но пуст: обычные игроки не смогут войти."));

        if (!config.OnlineMode)
            issues.Add(new(Severity.Warning, "Проверка подлинности аккаунтов отключена: возможен вход под чужим именем."));

        if (config.IsPublic && !config.DdosProtection)
            issues.Add(new(Severity.Warning, "Публичный сервер работает без DDoS-защиты."));

        return issues;
    }

    public static StartStatus GetStatus(IReadOnlyList<ConfigIssue> issues)
    {
        if (issues.Any(issue => issue.Severity == Severity.Error))
            return StartStatus.Blocked;
        if (issues.Count > 0)
            return StartStatus.Warning;
        return StartStatus.Ready;
    }
}
