namespace AutoWeapons {

public interface ILogger
{
    void Log(string value);
    void LogError(string value);
    void LogSpaced(string value);
    void LogTabbed(params object[] value);
}
}