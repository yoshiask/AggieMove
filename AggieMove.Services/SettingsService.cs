using OwlCore.AbstractStorage;
using OwlCore.Services;
using System;

namespace AggieMove.Services
{
    public class SettingsService : SettingsBase
    {
        public SettingsService(IFolderData folder)
            : base(folder, new NewtonsoftStreamSerializer())
        {
        }

        public DateTimeOffset TargetDate
        {
            get => GetSetting(GetDefaultTargetDate);
            set => SetSetting(value);
        }

        private DateTimeOffset GetDefaultTargetDate() => DateTimeOffset.Now;
    }
}
