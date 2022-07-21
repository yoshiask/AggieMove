using OwlCore.AbstractStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace AggieMove.Services
{
    /// <summary>
    /// Wraps a <see cref="StorageFile"/> for use with <c>OwlCore.AbstractStorage</c>.
    /// </summary>
    public class FileData : IFileData
    {
        public FileData(StorageFile file)
        {
            StorageFile = file;
        }

        public StorageFile StorageFile { get; }

        public string Id => StorageFile.FolderRelativeId;

        public string Path => StorageFile.Path;

        public string Name => StorageFile.Name;

        public string DisplayName => StorageFile.DisplayName;

        public string FileExtension => StorageFile.FileType;

        public IFileDataProperties Properties { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Task Delete() => StorageFile.DeleteAsync().AsTask();

        public async Task<IFolderData> GetParentAsync()
            => new FolderData(await StorageFile.GetParentAsync());

        public async Task<Stream> GetStreamAsync(OwlCore.AbstractStorage.FileAccessMode accessMode = OwlCore.AbstractStorage.FileAccessMode.Read)
        {
            var ras = await StorageFile.OpenAsync((Windows.Storage.FileAccessMode)accessMode);
            return ras.AsStream();
        }

        public async Task<Stream> GetThumbnailAsync(OwlCore.AbstractStorage.ThumbnailMode thumbnailMode, uint requiredSize)
        {
            var ras = await StorageFile.GetThumbnailAsync((Windows.Storage.FileProperties.ThumbnailMode)thumbnailMode, requiredSize);
            return ras.AsStream();
        }

        public Task WriteAllBytesAsync(byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}
