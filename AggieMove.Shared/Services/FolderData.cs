using OwlCore.AbstractStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace AggieMove.Services
{
    /// <summary>
    /// Wraps a <see cref="StorageFolder"/> for use with <c>OwlCore.AbstractStorage</c>.
    /// </summary>
    public class FolderData : IFolderData
    {
        public FolderData(StorageFolder folder)
        {
            StorageFolder = folder;
        }

        public StorageFolder StorageFolder { get; }

        public string Id => StorageFolder.FolderRelativeId;

        public string Name => StorageFolder.Name;

        public string Path => StorageFolder.Path;

        public async Task<IFileData> CreateFileAsync(string desiredName)
            => new FileData(await StorageFolder.CreateFileAsync(desiredName));

        public async Task<IFileData> CreateFileAsync(string desiredName, OwlCore.AbstractStorage.CreationCollisionOption options)
            => new FileData(await StorageFolder.CreateFileAsync(desiredName, (Windows.Storage.CreationCollisionOption)options));

        public async Task<IFolderData> CreateFolderAsync(string desiredName)
            => new FolderData(await StorageFolder.CreateFolderAsync(desiredName));

        public async Task<IFolderData> CreateFolderAsync(string desiredName, OwlCore.AbstractStorage.CreationCollisionOption options)
            => new FolderData(await StorageFolder.CreateFolderAsync(desiredName, (Windows.Storage.CreationCollisionOption)options));

        public Task DeleteAsync() => StorageFolder.DeleteAsync().AsTask();

        public Task EnsureExists() => Task.CompletedTask;

        public async Task<IFileData> GetFileAsync(string name)
            => new FileData(await StorageFolder.GetFileAsync(name));

        public async Task<IEnumerable<IFileData>> GetFilesAsync()
        {
            var files = await StorageFolder.GetFilesAsync();
            return files.Select(f => new FileData(f));
        }

        public async Task<IFolderData> GetFolderAsync(string name)
            => new FolderData(await StorageFolder.GetFolderAsync(name));

        public async Task<IEnumerable<IFolderData>> GetFoldersAsync()
        {
            var folders = await StorageFolder.GetFoldersAsync();
            return folders.Select(f => new FolderData(f));
        }

        public async Task<IFolderData> GetParentAsync()
            => new FolderData(await StorageFolder.GetParentAsync());
    }
}
