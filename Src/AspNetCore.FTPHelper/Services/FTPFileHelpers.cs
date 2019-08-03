using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace AspNetCore.FTPHelper.Services
{
    public class FTPFileHelpers : IFTPFileHelpers
    {
        private readonly IOptions<FTPSettings> _ftpSettings;

        public FTPFileHelpers(IOptions<FTPSettings> ftpSettings)
        {
            this._ftpSettings = ftpSettings;
        } 

        /// <summary>
        /// Download the files and folder form the FTP Server
        /// </summary>
        /// <param name="SourceFolder">file location at ftp</param>
        /// <param name="TargetFolder">download location at local machine</param>
        /// <param name="fileType">file with extension need to be downloaded, leave empty if you need to download all files</param>
        public void DownloadFile(string SourceFolder,string targetFolder, string fileType = "")
        {
            try
            {
                Uri uri = new Uri(_ftpSettings.Value.FTPAddress + "/" + SourceFolder + "/"); // new Uri("ftp://172.18.46.62:21/InSettlementFileFromCrane/");

                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(uri);
                ftpRequest.Credentials = new NetworkCredential(_ftpSettings.Value.FTPUsername, _ftpSettings.Value.FTPPassword);//new NetworkCredential("ftpuser", "P@ssw0rd");
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                ftpRequest.Timeout = Timeout.Infinite;
                ftpRequest.KeepAlive = true;
                ftpRequest.UseBinary = true;
                ftpRequest.Proxy = null;
                ftpRequest.UsePassive = true;

                var response = ftpRequest.GetResponse();

                List<string> directories = new List<string>();
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    string line = streamReader.ReadLine();
                    while (!string.IsNullOrEmpty(line))
                    {
                        directories.Add(line);
                        line = streamReader.ReadLine();
                    }
                    streamReader.Close();
                }
                using (WebClient ftpClient = new WebClient())
                {
                    ftpClient.Credentials = new System.Net.NetworkCredential(_ftpSettings.Value.FTPUsername, _ftpSettings.Value.FTPPassword);
                    ftpClient.Proxy = null;
                    for (int i = 0; i <= directories.Count - 1; i++)
                    {
                        if (string.IsNullOrEmpty(fileType) || directories[i].ToLower().EndsWith(fileType.ToLower()))
                        {
                            string sourcePath = uri + directories[i].ToString();
                            string transferPath = targetFolder + "\\" + directories[i].ToString(); // @"D:\\New folder\" + directories[i].ToString();
                            ftpClient.DownloadFile(sourcePath, transferPath);

                            response.Close();
                            //deleting a file from source FTP

                            FtpWebRequest ftpDeleteRequest = (FtpWebRequest)WebRequest.Create(uri + directories[i].ToString());
                            ftpDeleteRequest.Credentials = ftpClient.Credentials;
                            ftpDeleteRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                            ftpDeleteRequest.Timeout = Timeout.Infinite;
                            ftpDeleteRequest.KeepAlive = true;
                            ftpDeleteRequest.UseBinary = true;
                            ftpDeleteRequest.Proxy = null;
                            ftpDeleteRequest.UsePassive = true;

                            FtpWebResponse ftpDeleteResponse = (FtpWebResponse)ftpDeleteRequest.GetResponse();

                            ftpDeleteResponse.Close();

                        }
                    }
                } 
            }
            catch (Exception ex)
            {
                throw new Exception("Error while downloading file. Details: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Upload list of files to the specified directory in ftp
        /// </summary>
        /// <param name="sourceFiles">list of files to be upload</param>
        /// <param name="targetFolder">ftp folder where file need to upload</param>
        public void UploadFile(List<string> sourceFiles, string targetFolder)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var sourceFile in sourceFiles)
            {
                FileInfo file = new FileInfo(sourceFile);
                try
                {
                    Uri uri = new Uri(_ftpSettings.Value.FTPAddress + "//" + targetFolder + "//" + file.Name); // new Uri("ftp://172.18.46.62:21/InSettlementFileFromCrane/");

                    FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(uri);
                    ftpRequest.Credentials = new NetworkCredential(_ftpSettings.Value.FTPUsername, _ftpSettings.Value.FTPPassword);//new NetworkCredential("ftpuser", "P@ssw0rd");
                    ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                    ftpRequest.Timeout = Timeout.Infinite;
                    ftpRequest.KeepAlive = true;
                    ftpRequest.UseBinary = true;
                    ftpRequest.Proxy = null;
                    ftpRequest.UsePassive = true;

                    // Copy the contents of the file to the request stream.
                    using (StreamReader sourceStream = new StreamReader(sourceFile))
                    {
                        byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                        sourceStream.Close();
                        ftpRequest.ContentLength = fileContents.Length;

                        using (Stream requestStream = ftpRequest.GetRequestStream())
                        {
                            requestStream.Write(fileContents, 0, fileContents.Length);
                            requestStream.Close();

                            using (FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse())
                            {
                                bool Success = response.StatusCode == FtpStatusCode.CommandOK || response.StatusCode == FtpStatusCode.ClosingData || response.StatusCode == FtpStatusCode.FileActionOK;
                                if (Success)
                                {
                                    file.Delete();
                                }
                                else
                                {
                                    sb.AppendLine(file.Name);
                                }
                                response.Close();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine(file.Name);
                }
            }
            if (!string.IsNullOrWhiteSpace(sb.ToString()))
            {
                throw new Exception("Following CSV files not moved to SAP folder :" + sb.ToString());
            }
        }

        /// <summary>
        /// Upload file to the specified directory in ftp
        /// </summary>
        /// <param name="sourceFiles">file to be upload</param>
        /// <param name="targetFolder">ftp folder where file need to upload</param>
        public void UploadFile(string sourceFile, string targetFolder)
        {
            StringBuilder sb = new StringBuilder();
            FileInfo file = new FileInfo(sourceFile);
            try
            {
                Uri uri = new Uri(_ftpSettings.Value.FTPAddress + "//" + targetFolder + "//" + file.Name); // new Uri("ftp://172.18.46.62:21/InSettlementFileFromCrane/");

                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(uri);
                ftpRequest.Credentials = new NetworkCredential(_ftpSettings.Value.FTPUsername, _ftpSettings.Value.FTPPassword);//new NetworkCredential("ftpuser", "P@ssw0rd");
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                ftpRequest.Timeout = Timeout.Infinite;
                ftpRequest.KeepAlive = true;
                ftpRequest.UseBinary = true;
                ftpRequest.Proxy = null;
                ftpRequest.UsePassive = true;

                // Copy the contents of the file to the request stream.
                using (StreamReader sourceStream = new StreamReader(sourceFile))
                {
                    byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                    sourceStream.Close();
                    ftpRequest.ContentLength = fileContents.Length;

                    using (Stream requestStream = ftpRequest.GetRequestStream())
                    {
                        requestStream.Write(fileContents, 0, fileContents.Length);
                        requestStream.Close();

                        using (FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse())
                        {
                            bool Success = response.StatusCode == FtpStatusCode.CommandOK || response.StatusCode == FtpStatusCode.ClosingData || response.StatusCode == FtpStatusCode.FileActionOK;
                            if (Success)
                            {
                                file.Delete();
                            }
                            else
                            {
                                sb.AppendLine(file.Name);
                            }
                            response.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            } 
        } 

        /// <summary>
        /// delete file from ftp
        /// </summary>
        /// <param name="folderSource"></param>
        /// <returns></returns>
        public bool DeleteFile(string folderSource)
        {
            Uri uri = new Uri(_ftpSettings.Value.FTPAddress + "/" + folderSource + "/");  

            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(uri);
            ftpRequest.Credentials = new NetworkCredential(_ftpSettings.Value.FTPUsername, _ftpSettings.Value.FTPPassword);
            ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            try
            {
                FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                response.Close();
            }
            catch (Exception e)
            {
                ftpRequest.Abort();
                throw e;
            }
            ftpRequest.Abort(); 
            return true;
        }  

        /// <summary>
        /// check if the directory exists
        /// </summary>
        /// <param name="localFile"></param>
        /// <returns></returns>
        public bool CheckIfDirectoryExists(string localFile)
        {
            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create($"{_ftpSettings.Value.FTPAddress}/{localFile}");
            ftpRequest.Credentials = new NetworkCredential(_ftpSettings.Value.FTPUsername, _ftpSettings.Value.FTPPassword);
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            try
            {
                FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                response.Close();
            }
            catch (Exception e)
            {
                ftpRequest.Abort();
                throw e;
            }
            ftpRequest.Abort();
            return true;
        } 

        /// <summary>
        /// create directory in the ftp
        /// </summary>
        /// <param name="directoryName"></param>
        /// <returns></returns>
        public bool MakeDirectory(string directoryName)
        {
            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create($"{_ftpSettings.Value.FTPAddress}/{directoryName}");
            ftpRequest.Credentials = new NetworkCredential(_ftpSettings.Value.FTPUsername, _ftpSettings.Value.FTPPassword);
            ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
            try
            {
                FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                response.Close();
            }
            catch (Exception e)
            {
                ftpRequest.Abort();
                throw e;
            }
            ftpRequest.Abort();
            return true;
        }

        public List<string> ListFileAndDirectory(string SourceFolder)
        {
            try
            {
                Uri uri = new Uri(_ftpSettings.Value.FTPAddress + "/" + SourceFolder + "/"); // new Uri("ftp://172.18.46.62:21/InSettlementFileFromCrane/");

                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(uri);
                ftpRequest.Credentials = new NetworkCredential(_ftpSettings.Value.FTPUsername, _ftpSettings.Value.FTPPassword);//new NetworkCredential("ftpuser", "P@ssw0rd");
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                ftpRequest.Timeout = Timeout.Infinite;
                ftpRequest.KeepAlive = true;
                ftpRequest.UseBinary = true;
                ftpRequest.Proxy = null;
                ftpRequest.UsePassive = true;

                var response = ftpRequest.GetResponse();

                List<string> directories = new List<string>();
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    string line = streamReader.ReadLine();
                    while (!string.IsNullOrEmpty(line))
                    {
                        directories.Add(line);
                        line = streamReader.ReadLine();
                    }
                    streamReader.Close();
                }
                return directories;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while getting file/directory list. Details: " + ex.Message);
            }
        }
    }
} 