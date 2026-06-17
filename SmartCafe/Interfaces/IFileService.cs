namespace SmartCafe.Interfaces
{
    public interface IFileService
    {
        Task<bool> WriteImageDocker(IFormFile file, string name, string folder);
    }
}
