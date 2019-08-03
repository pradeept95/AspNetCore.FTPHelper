using System.Collections.Generic;

namespace AspNetCore.FTPHelper.Services
{
    public interface IFTPFileHelpers
    {
        bool CheckIfDirectoryExists(string localFile);
        bool DeleteFile(string folderSource);
        void DownloadFile(string SourceFolder, string targetFolder, string fileType = "");
        bool MakeDirectory(string directoryName);
        void UploadFile(List<string> sourceFiles, string targetFolder);
        void UploadFile(string sourceFile, string targetFolder);
        List<string> ListFileAndDirectory(string SourceFolder);
    }
}