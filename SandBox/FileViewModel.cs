namespace SandBox
{
    public class FileViewModel
    {
        public string Name         => _fileModel.Name;
        public string Extension    => _fileModel.Extension;
        public string AbsolutePath => _fileModel.AbsolutePath;
        public string FileSize { get; }
            
        private readonly FileModel _fileModel;
        public FileViewModel(FileModel fileModel)
        {
            _fileModel = fileModel;

            var gb = 1024d * 1024d;
            var mb = 1024d * 1024d;
            var kb = 1024d;

            if (fileModel.FileSize > gb)
                FileSize = $"{(long)(fileModel.FileSize / gb)}GB";                
            else if (fileModel.FileSize > mb)
                FileSize = $"{(long)(fileModel.FileSize / mb)}MB";                
            else if (fileModel.FileSize > kb)
                FileSize = $"{(long)(fileModel.FileSize / kb)}KB";
            else
                FileSize = $"{fileModel.FileSize}B";
        }
    }
}