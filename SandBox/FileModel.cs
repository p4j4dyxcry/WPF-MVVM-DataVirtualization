using System;
using System.IO;

namespace SandBox
{
    /// <summary>
    /// ファイルのデータモデルです。
    /// </summary>
    public class FileModel
    {
        public string Name { get; }
        public string Extension { get; }
        public string AbsolutePath { get; }
        
        public long FileSize { get; }

        private readonly string _lowerName;
        private readonly string _lowerExtension;
        
        public FileModel(string absolutePath)
        {
            AbsolutePath = absolutePath;
            Extension = Path.GetExtension(absolutePath);
            Name = Path.GetFileName(absolutePath);
            try
            {
                var info = new FileInfo(absolutePath);
                FileSize = info.Length;
            }
            catch
            {
                // ignored
            }

            _lowerName = Name.ToLower();
            _lowerExtension = Extension.ToLower();
        }

        /// <summary>
        /// 名前と拡張子でフィルタリングできるかを検証します。
        /// 要素が空の場合は常にtrueになります。
        /// 高速化のために小文字のOrdinalで比較するので入力文字列は小文字にしておく必要があります。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public bool Filter(string name, string extension)
        {
            bool result = true;
            
            if (string.IsNullOrWhiteSpace(name) is false)
                result &= _lowerName.IndexOf(name,StringComparison.Ordinal) != -1;

            if (string.IsNullOrWhiteSpace(extension) is false)
                result &= _lowerExtension.IndexOf(extension,StringComparison.Ordinal) != -1;

            return result;
        }
        
    }
}