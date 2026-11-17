namespace SpecAudit.Application;
public interface IFolderPicker
{
    Task<string?> PickAsync(string title);
}
