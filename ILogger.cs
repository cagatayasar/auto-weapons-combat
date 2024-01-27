namespace AutoWeapons {

public interface ILogger
{
    void Log(string value);
    void LogError(string value);
    void LogSpaced(params object[] values);
    void LogTabbed(params object[] values);
}
}