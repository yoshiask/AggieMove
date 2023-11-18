using OwlCore.Storage;
using OwlCore.ComponentModel;
using System;

namespace AggieMove.Services
{
    public class SettingsService : SettingsBase
    {
        public SettingsService(IModifiableFolder folder)
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
